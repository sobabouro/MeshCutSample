using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 切断平面上の y 単調な多角形 (閉パス) のリストを管理するクラス
    /// </summary>
    public class MonotoneGeometryPathList {

        /// <summary>
        /// すべての頂点に対しての，接続する辺へのマッピング
        /// </summary>
        private Map _map;

        /// <summary>
        /// 切断平面上の y 単調な多角形のパスリスト
        /// </summary>
        private List<MonotoneGeometryPath> _pathList;

        /// <summary>
        /// すべての図形の中で最も高い X 座標を持つ頂点の位置
        /// UV 座標の構築に使用する
        /// </summary>
        private BoundingBox _boundingBox;

        /// <summary>
        /// コンストラクタ
        /// ここでパスの生成を行う
        /// </summary>
        /// <param name="linkedVertexList"> 切断平面上の非単調である可能性のある多角形リスト </param>
        /// <param name="diagonalSet"> 切断平面上のすべての非単調多角形を y に単調な多角形に分割するための対角線集合 </param>
        public MonotoneGeometryPathList(
            LinkedVertexList linkedVertexList,
            HashSet<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> diagonalSet
        ) {
            _map = new();
            _pathList = new();
            _boundingBox = new();

            CreateMap(linkedVertexList, diagonalSet);
            ScanPath();
        }

        /// <summary>
        /// 元の辺と対角線をマップに追加するメソッド
        /// </summary>
        /// <param name="linkedVertexList"> 元の連結辺シーケンスのリスト </param>
        /// <param name="diagonalSet"> 対角線集合 </param>
        private void CreateMap(
            LinkedVertexList linkedVertexList,
            HashSet<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> diagonalSet
        ) {
            // 元の辺を追加する
            foreach (var linkedVertex in linkedVertexList) {

                if (linkedVertex.First == null || linkedVertex.First.Next == null)
                    continue;

                var firstNode = linkedVertex.First;
                var currNode = firstNode;

                do {
                    var nextNode = linkedVertex.TorusNext(currNode);

                    /*デバッグ用*/
                    var edge = new NonConvexMonotoneCutSurfaceEdge(currNode.Value, nextNode.Value);
                    edge.Address = $"e{currNode.Value.Address}";
                    _map.AddEdge(edge);

                    //_map.AddEdge(new NonConvexMonotoneCutSurfaceEdge(currNode.Value, nextNode.Value));
                    UpdateMostHighestLowestPosition(currNode.Value);
                    currNode = nextNode;
                }
                while (!currNode.Equals(linkedVertex.First));
            }

            /*デバッグ用*/
            int diagonalCount = 0;

            // 対角線を追加する
            foreach (var diagonal in diagonalSet) {

                Debug.Log($"MonotoneGeometryPathList: AddEdgeToMap() Diagonals {diagonal.Item1.Address} <-> {diagonal.Item2.Address}.");

                /*デバッグ用*/
                var edge = new NonConvexMonotoneCutSurfaceEdge(diagonal.Item1, diagonal.Item2);
                edge.Address = $"diagonal[{diagonalCount++}]";
                _map.AddEdge(edge);
                _map.AddEdge(edge.GetReverseEdge());

                //_map.AddEdge(new NonConvexMonotoneCutSurfaceEdge(diagonal.Item1, diagonal.Item2));
            }
        }

        /// <summary>
        /// マップから，切断平面上の y 単調な多角形のパスを探索するメソッド
        /// </summary>
        private void ScanPath() {

            // パス探索で訪問済みの辺を追跡するための集合
            HashSet<NonConvexMonotoneCutSurfaceEdge> visitedEdgeSet = new();
            HashSet<NonConvexMonotoneCutSurfaceVertex> visitedVertexSet = new(); // ループの始点として使用済みか追跡

            foreach (var keyVertex in _map.GetAllKeys().Where(v => !visitedVertexSet.Contains(v)).ToList()) {

                // このキー頂点から始まるパスを全て試す
                foreach (var currEdge in _map.GetSortedEdgesFromIncomingVector(keyVertex, null)) {

                    // 既に訪問済みの辺はスキップする
                    if (visitedEdgeSet.Contains(currEdge))
                        continue;

                    MonotoneGeometryPath currPath = new MonotoneGeometryPath();
                    NonConvexMonotoneCutSurfaceVertex startVertex = currEdge.Start;
                    NonConvexMonotoneCutSurfaceVertex currVertex = currEdge.Start;
                    NonConvexMonotoneCutSurfaceVertex prevVertex = null;

                    bool isClosedPath = false;

                    // パス探索を開始する
                    while (!isClosedPath) {

                        bool foundNext = false;
                        var sortedEdges = _map.GetSortedEdgesFromIncomingVector(currVertex, prevVertex);

                        foreach (var nextEdge in sortedEdges) {

                            // 直前の頂点に戻る辺 (頂点)、または既に使われた辺 (頂点) ではない，接続する頂点であれば更新する
                            if (!nextEdge.End.Equals(prevVertex) && !visitedEdgeSet.Contains(nextEdge)) {

                                // 開始点に戻る辺が見つかった場合は閉パスとする
                                if (nextEdge.End.Equals(startVertex))
                                    isClosedPath = true;

                                visitedEdgeSet.Add(nextEdge);

                                // パスに頂点を追加
                                if (!currPath.Contains(nextEdge.End)) {
                                    currPath.AddLast((nextEdge.End, currVertex));
                                }
                                else {
                                    // 既に含まれていて、それが始点でない場合はパスが交差している可能性があるため、ループを抜ける
                                    if (!isClosedPath) {
                                        Debug.LogWarning("MonotoneGeometryPathList: Path self-intersection detected. Abandoning path.");
                                        foundNext = false;
                                        break;
                                    }
                                }

                                prevVertex = currVertex;
                                currVertex = nextEdge.End;
                                foundNext = true;
                                break;
                            }
                        }

                        if (!foundNext) {
                            Debug.LogWarning($"MonotoneGeometryPathList: No valid next edge found for {currVertex.Address}. Path might be incomplete.");
                            break;
                        }
                    }

                    // パスの最終チェックを行い、パスリストに閉パスを追加する
                    if (isClosedPath && currPath.Count > 2) {
                        _pathList.Add(currPath);

                        Debug.Log($"MonotoneGeometryPathList: Closed path found starting from {startVertex.Address} with {currPath.Count} vertices.");

                        Debug.Log($"--------");

                        foreach (var pair in currPath) {
                            visitedVertexSet.Add(pair.Item1);

                            Debug.Log($"{pair.Item1.Address}");
                        }
                        Debug.Log($"--------");
                    }
                    else {
                        Debug.LogWarning($"MonotoneGeometryPathList: Path starting from {startVertex.Address} could not be closed or was too short.");
                    }
                }
            }
        }

        /// <summary>
        /// 切断平面上の y 単調な多角形のパスからポリゴンを生成するメソッド
        /// </summary>
        /// <param name="localPlane"> ローカル座標系の切断平面 </param>
        /// <param name="hasCutSurfaceMaterial"> 切断面に新規マテリアルを割り当てるかどうか </param>
        /// <param name="frontsideMesh"> 切断後の法線側メッシュ </param>
        /// <param name="backsideMesh"> 切断後の反法線側メッシュ </param>
        public void MakePolygon(
            Plane localPlane,
            bool hasCutSurfaceMaterial,
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh
        ) {
            foreach (var path in _pathList) {
                if (path.Count < 3)
                    continue;

                path.MakePolygon(
                    _boundingBox,
                    localPlane,
                    hasCutSurfaceMaterial,
                    frontsideMesh,
                    backsideMesh
                );
            }
        }

        /// <summary>
        /// 切断面のバウンディングボックスを更新するためのメソッド
        /// x, y 座標の最も高い位置と最も低い位置を更新する
        /// </summary>
        /// <param name="vertex"> 追加する頂点 </param>
        private void UpdateMostHighestLowestPosition(NonConvexMonotoneCutSurfaceVertex vertex) {
            _boundingBox.TryUpdate(vertex.PlanePosition.x, vertex.PlanePosition.y);
        }
    }
}