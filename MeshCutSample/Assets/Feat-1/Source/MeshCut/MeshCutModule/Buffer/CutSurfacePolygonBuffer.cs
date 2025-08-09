using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 切断平面のポリゴンバッファクラス
    /// </summary>
    public class CutSurfacePolygonBuffer {

        /// <summary>
        /// ローカル座標系の切断平面
        /// </summary>
        private Plane _localPlane;

        /// <summary>
        /// 切断平面上の連結頂点のリスト
        /// 図形的には非単調である可能性のある多角形がリストとして保存される
        /// </summary>
        private LinkedVertexList _linkedVertexList;

        ///// <summary>
        ///// 切断平面上の図形を y 単調な多角形に分割した後の連結頂点のリスト
        ///// 図形的には y 単調な多角形がリストとして保存される
        ///// </summary>
        //private LinkedMonotoneGeometryVertexList _linkedMonotoneGeometryVertexList;

        private MonotoneGeometryPathList _monotoneGeometryPathList;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="localPlane"> ローカル座標系の切断平面 </param>
        public CutSurfacePolygonBuffer(Plane localPlane) {
            _localPlane = localPlane;
            _linkedVertexList = new LinkedVertexList();
            //_linkedMonotoneGeometryVertexList = new LinkedMonotoneGeometryVertexList();
        }

        /// <summary>
        /// 切断平面上の頂点を連結頂点リストに追加するメソッド
        /// </summary>
        /// <param name="towardPosition"> 辺の始点のローカル空間座標 </param>
        /// <param name="awayPosition"> 辺の終点のローカル空間座標 </param>
        public void AddVertex(
            Vector3 towardPosition,
            Vector3 awayPosition
        ) {
            NonConvexMonotoneCutSurfaceVertex towardCutSurfaceVertex = new(_localPlane, towardPosition);
            NonConvexMonotoneCutSurfaceVertex awayCutSurfaceVertex = new(_localPlane, awayPosition);

            _linkedVertexList.Add(towardCutSurfaceVertex, awayCutSurfaceVertex);
        }

        /// <summary>
        /// 切断平面上のポリゴンを生成するメソッド
        /// </summary>
        /// <param name="frontsideMesh"> 切断後の法線側メッシュコンテナ </param>
        /// <param name="backsideMesh"> 切断後の反法線側メッシュコンテナ </param>
        /// <param name="hasCutSurfaceMaterial"> 切断平面用のマテリアルの有無 </param>
        public void MakeCutSurfacePolygon(
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh,
            bool hasCutSurfaceMaterial = false
        ) {
            if (_linkedVertexList.Count == 0) {
                Debug.LogWarning("CutSurfacePolygonBuffer: No vertices to process.");
                return;
            }
            // すべての非単調な多角形が閉じたパス (図形) として保存しているリストから，y に単調な多角形に分割するための対角線を生成して，すべてのパスを保存する生成系をインスタンス化する
            DiagonalEdgeGenerator diagonalEdgeGenerator = new DiagonalEdgeGenerator(_linkedVertexList);

            HashSet<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> diagonalSet = diagonalEdgeGenerator.GetDiagonalSet();

            _monotoneGeometryPathList = new MonotoneGeometryPathList(_linkedVertexList, diagonalSet);
            _monotoneGeometryPathList.MakePolygon(
                _localPlane,
                hasCutSurfaceMaterial,
                frontsideMesh,
                backsideMesh
            );
        }
    }
}