using System.Collections.Generic;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {
    /// <summary>
    /// 連結ポリゴンの情報を保持するクラス
    /// 連結ポリゴンは切断辺の始点から終点の順を正方向として連結する
    /// </summary>
    public class LinkedPolygon : AbstractNodeSequence<NewPolygon> {

        public LinkedPolygon() : base(new AllMergeStrategy<NewPolygon>()) { }
        public LinkedPolygon(INodeSequenceMergeStrategy<NewPolygon> mergeStrategy) : base(mergeStrategy) { }

        /// <summary>
        /// 対象ポリゴンを連結ポリゴンに対して後ろに追加するメソッド
        /// </summary>
        /// <param name="args"> 追加するポリゴン情報を含む引数 </param>
        /// <returns> 追加に成功した場合は true, 失敗した場合は false </returns>
        public override bool TryAppend(params object[] args) {
            if (args.Length != 1 || !(args[0] is NewPolygon polygon)) {
                Debug.LogError("TryAppend for LinkedPolygon requires one NewPolygon argument.");
                return false;
            }

            if (_nodeSequence.Last?.Value.AwayVertex.Domein == polygon.TowardVertex.Domein || First == null) {
                _nodeSequence.AddLast(polygon);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 対象ポリゴンを連結ポリゴンに対して前に追加するメソッド
        /// </summary>
        /// <param name="args"> 追加するポリゴン情報を含む引数 </param>
        /// <returns> 追加に成功した場合は true, 失敗した場合は false </returns>
        public override bool TryPrepend(params object[] args) {
            if (args.Length != 1 || !(args[0] is NewPolygon polygon)) {
                Debug.LogError("TryPrepend for LinkedPolygon requires one NewPolygon argument.");
                return false;
            }

            if (_nodeSequence.First?.Value.TowardVertex.Domein == polygon.AwayVertex.Domein) {
                _nodeSequence.AddFirst(polygon);
                return true;
            }
            return false;
        }

        /// <summary>
        /// ノードを辿りながらポリゴン情報を生成するメソッド
        /// 切断平面の法線方向を "A", ポリゴンの法線方向を "B", 切断辺 (NewVertex, NewVertex) の方向を "C" とする
        /// このとき、A, B, C のベクトルが、フレミングの左手の法則に従う向きになるように仮置きし、
        /// C は front, back によって向きが可変のベクトルなので、とくに深い意味はなく、A,B,C の 3 ベクトル空間上での始点を "toward", 終点を "away" と仮置きしている
        /// front, back によらず、"start", "end" は共通の方向定義を持っており、start -> end で構成される辺は、最終的なポリゴンにおいて頂点右回りである
        /// </summary>
        /// <param name="localPlane"> 切断平面 </param>
        /// <param name="trackerArray"> 切断後の配属頂点インデックスを格納する配列 </param>
        /// <param name="originMesh"> 切断前メッシュ (MeshContainer) </param>
        /// <param name="frontsideMesh"> 切断後法線側メッシュ (MeshContainer) </param>
        /// <param name="backsideMesh"> 切断後反法線側メッシュ (MeshContainer) </param>
        public void MakePolygon(
            Plane localPlane,
            int[] trackerArray,
            MeshContainer originMesh,
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh,
            CutSurfacePolygonBuffer cutSurfacePolygonBuffer
        ) {
            NewVertex toward = First.Value.TowardVertex;
            NewVertex away = Last.Value.AwayVertex;

            var (towardPosition, towardNormal, towardUV) = toward.CalcVertexInfo(localPlane, originMesh);
            var (awayPosition, awayNormal, awayUV) = away.CalcVertexInfo(localPlane, originMesh);

            frontsideMesh.AddVertex(towardPosition, towardNormal, towardUV, out int frontsideNewStartIndex);
            frontsideMesh.AddVertex(awayPosition, awayNormal, awayUV, out int frontsideNewEndIndex);
            backsideMesh.AddVertex(awayPosition, awayNormal, awayUV, out int backsideNewStartIndex);
            backsideMesh.AddVertex(towardPosition, towardNormal, towardUV, out int backsideNewEndIndex);

            cutSurfacePolygonBuffer.AddVertex(towardPosition, awayPosition);

            // frontside 用に連結ポリゴンのノードを辿りながらポリゴン情報を生成する
            ScanNode(
                First,
                _nodeSequence.Count,
                frontsideMesh,
                frontsideNewStartIndex,
                frontsideNewEndIndex,
                trackerArray,
                isFrontside: true
            );

            // backside 用に連結ポリゴンのノードを辿りながらポリゴン情報を生成する
            ScanNode(
                Last,
                _nodeSequence.Count,
                backsideMesh,
                backsideNewStartIndex,
                backsideNewEndIndex,
                trackerArray,
                isFrontside: false
            );
        }

        /// <summary>
        /// 連結ポリゴンのノードを辿りながら対象の MeshContainer にポリゴン情報を生成するメソッド
        /// </summary>
        /// <param name="startPolygonNode"> 切断辺を構築する向きに連結ポリゴンを辿ることにする。このときの先頭のポリゴン </param>
        /// <param name="linkCount"> 連結ポリゴンの連結数 </param>
        /// <param name="targetsideMesh"> ポリゴン情報を構成させる対象の MeshContainer </param>
        /// <param name="newStartIndex"> 切断辺の始点 (ポリゴンの頂点が右回りになる向き) </param>
        /// <param name="newEndIndex"> 切断辺の終点 (ポリゴンの頂点が右回りになる向き) </param>
        /// <param name="trackerArray"> 振り分け情報と、元頂点データのインデックスが対応している配列 </param>
        /// <param name="isFrontside"> 走査を frontside 用に行うなら true, backside なら false </param>
        private void ScanNode(
            LinkedListNode<NewPolygon> startPolygonNode,
            int linkCount,
            MeshContainer targetsideMesh,
            int newStartIndex,
            int newEndIndex,
            int[] trackerArray,
            bool isFrontside
        ) {
            var node = startPolygonNode;

            // 連結ポリゴンの中間までは、「切断辺の始点」「側辺二頂点」で三角形を構成する
            for (int i = 0; i < linkCount / 2; i++) {
                if (node?.Value is TrianglePolygon polygon) {
                    CreateTriangle(targetsideMesh, polygon, newStartIndex, trackerArray, isFrontside);
                }
                node = isFrontside ? node?.Next : node?.Previous;
            }

            // 中間は、「切断辺の始点」「側辺二頂点」と、「切断辺の始点」「切断辺の終点」「側辺の終点」で三角形を構成する
            if (node?.Value is TrianglePolygon midPolygon) {
                CreateTriangle(targetsideMesh, midPolygon, newStartIndex, trackerArray, isFrontside);
                CreateTriangle(targetsideMesh, midPolygon, newStartIndex, newEndIndex, trackerArray, isFrontside);
                node = isFrontside ? node?.Next : node?.Previous;
            }

            // 残りは、「切断辺の終点」「側辺の二頂点」で三角形を構成する
            while (node != null) {
                if (node.Value is TrianglePolygon polygon) {
                    CreateTriangle(targetsideMesh, polygon, newEndIndex, trackerArray, isFrontside);
                }
                node = isFrontside ? node.Next : node.Previous;
            }
        }

        /// <summary>
        /// 三角形を構成し、MesnhContainer に追加するメソッド
        /// </summary>
        /// <param name="targetsideMesh"> 追加対象の MeshContainer </param>
        /// <param name="polygon"> 連結ポリゴン中の一つの要素 </param>
        /// <param name="edgeIndex"> 構成される三角形のうち、一つの頂点を担当する、切断辺のどちらか一方の頂点 </param>
        /// <param name="trackerArray"> 振り分け情報と、元頂点データのインデックスが対応している配列 </param>
        /// <param name="isFrontside"> 走査を frontside 用に行うなら true, backside なら false </param>
        private void CreateTriangle(
            MeshContainer targetsideMesh,
            TrianglePolygon polygon,
            int edgeIndex,
            int[] trackerArray,
            bool isFrontside
        ) {
            if (isFrontside) {
                // 辺を所持している場合のみ作成する
                if (!polygon.IsFrontsideEdge)
                    return;
                targetsideMesh.MakeTriangle(
                    polygon.SubmeshIndex,
                    edgeIndex,
                    trackerArray[polygon.AwayVertex.FrontsideVertexIndex],
                    trackerArray[polygon.TowardVertex.FrontsideVertexIndex]
                );
            }
            else {
                // 辺を所持している場合のみ作成する
                if (polygon.IsFrontsideEdge)
                    return;
                targetsideMesh.MakeTriangle(
                    polygon.SubmeshIndex,
                    edgeIndex,
                    trackerArray[polygon.TowardVertex.BacksideVertexIndex],
                    trackerArray[polygon.AwayVertex.BacksideVertexIndex]
                );
            }
        }

        /// <summary>
        /// 三角形を構成し、MesnhContainer に追加するメソッド (中間地点)
        /// </summary>
        /// <param name="targetsideMesh"> 追加対象の MeshContainer </param>
        /// <param name="polygon"> 連結ポリゴン中の一つの要素 </param>
        /// <param name="edgeStartIndex"> 構成される三角形の、二つの頂点を担当する切断辺のうちの始点 </param>
        /// <param name="edgeEndIndex"> 構成される三角形の、二つの頂点を担当する切断辺のうちの始点 </param>
        /// <param name="trackerArray"> 振り分け情報と、元頂点データのインデックスが対応している配列 </param>
        /// <param name="isFrontside"> 走査を frontside 用に行うなら true, backside なら false </param>
        private void CreateTriangle(
            MeshContainer targetsideMesh,
            TrianglePolygon polygon,
            int edgeStartIndex,
            int edgeEndIndex,
            int[] trackerArray,
            bool isFrontside
        ) {
            int middleIndex = isFrontside
                ? trackerArray[polygon.AwayVertex.FrontsideVertexIndex]
                : trackerArray[polygon.TowardVertex.BacksideVertexIndex];

            targetsideMesh.MakeTriangle(
                polygon.SubmeshIndex,
                edgeStartIndex,
                edgeEndIndex,
                middleIndex
            );
        }
    }
}