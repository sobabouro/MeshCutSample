using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 切断処理が適用されるデータのバッファクラス
    /// </summary>
    public class SubdivideDataBuffer {

        private bool _isReady = false;

        /// <summary>
        /// 法線方向ごとにポリゴンの情報を保持するバッファ
        /// </summary>
        private EquivalentNormalPolygonBuffer _polygonBuffer;

        /// <summary>
        /// 切断面のポリゴン情報を保持するバッファ
        /// </summary>
        private CutSurfacePolygonBuffer _cutSurfacePolygonBuffer;

        public SubdivideDataBuffer(Plane localPlane) {
            _polygonBuffer = new EquivalentNormalPolygonBuffer(localPlane);
            _cutSurfacePolygonBuffer = new CutSurfacePolygonBuffer(localPlane);
        }

        /// <summary>
        /// 新しいポリゴン情報を追加するメソッド
        /// </summary>
        /// <param name="submeshGroupNumber"> 所属しているサブメッシュのグループ番号 </param>
        /// <param name="polygonNormal"> 追加するポリゴンの法線 </param>
        /// <param name="input"> 新情報生成用に整形されたデータ (SideIndexInfo) </param>
        public void AddData(
            int submeshGroupNumber,
            Vector3 polygonNormal,
            SideIndexInfo input
        ) {
            NewVertex toward = new(
                input.FrontAwayIndex,
                input.BackTowardIndex
            );
            NewVertex away = new(
                input.FrontTowardIndex,
                input.BackAwayIndex
            );
            NewPolygon polygon = new TrianglePolygon(
                submeshGroupNumber,
                toward,
                away,
                input.IsFrontSideEdge
            );
            _polygonBuffer.Add(polygonNormal, polygon);
        }

		/// <summary>
		/// 切断面のポリゴンを生成するメソッド
		/// </summary>
		/// <param name="trackerArray"> 振分情報と元頂点インデックスが対応している配列 </param>
		/// <param name="originMesh"> 切断前のメッシュコンテナ </param>
		/// <param name="frontsideMesh"> 切断後の法線側メッシュコンテナ </param>
		/// <param name="backsideMesh"> 切断後の反法線側のメッシュコンテナ </param>
		/// <param name="hasCutSurfaceMaterial"> 新規マテリアルがあるかどうか </param>
		public void MakeAllPolygon(
            int[] trackerArray,
            MeshContainer originMesh,
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh,
            bool hasCutSurfaceMaterial = false
        ) {
            _polygonBuffer.MakeBaseSurfacePolygon(
                trackerArray,
                originMesh,
                frontsideMesh,
                backsideMesh,
                _cutSurfacePolygonBuffer
            );

            // 新規マテリアルがある場合、新規マテリアル用のサブメッシュ配列を追加する
            if (hasCutSurfaceMaterial) {
                frontsideMesh.AddNewSubmesh();
				backsideMesh.AddNewSubmesh();
			}

            _cutSurfacePolygonBuffer.MakeCutSurfacePolygon(
                frontsideMesh,
                backsideMesh,
                hasCutSurfaceMaterial
            );
        }
    }
}