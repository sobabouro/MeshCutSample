using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 頂点情報を管理するためのクラス
    /// </summary>
    public class NewVertex {

        /// <summary>
        /// 新頂点に隣接した、切断平面法線側の頂点のインデックスのプロパティ (切断前メッシュ頂点リストでのインデックス)
        /// </summary>
        public readonly int FrontsideVertexIndex;

        /// <summary>
        /// 新頂点に隣接した、切断平面反法線側の頂点のインデックスのプロパティ (切断前メッシュ頂点リストでのインデックス)
        /// </summary>
        public readonly int BacksideVertexIndex;

        /// <summary>
        /// 新頂点のドメインのプロパティ (切断前メッシュ頂点リストでの法線側インデックスを 16bit 左シフトして、反法線側インデックスと結合した値)
        /// unity における Mesh オブジェクトの最大頂点数は 65535 なので、16ビットで十分に表現できる
        /// </summary>
        public readonly uint Domein;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="frontsideVertexIndex"> 新頂点の切断平面法線側の頂点のインデックス (切断前メッシュ頂点リストでのインデックス) </param>
        /// <param name="backsideVertexIndex"> 新頂点の切断平面反法線側の頂点のインデックス (切断前メッシュ頂点リストでのインデックス) </param>
        public NewVertex(
            int frontsideVertexIndex,
            int backsideVertexIndex
        ) {
            FrontsideVertexIndex = frontsideVertexIndex;
            BacksideVertexIndex = backsideVertexIndex;
            Domein = ((uint)frontsideVertexIndex << 16) | (uint)(backsideVertexIndex);
        }

        /// <summary>
        /// 新頂点の位置、法線、UV座標を計算するメソッド
        /// </summary>
        /// <param name="localPlane"> 切断平面 </param>
        /// <param name="originMesh"> 切断前のメッシュ情報 </param>
        /// <returns> 新頂点の位置、法線、UV座標のタプル </returns>
        public (Vector3 position, Vector3 normal, Vector2 uv) CalcVertexInfo(
            Plane localPlane,
            MeshContainer originMesh
        ) {
            float frontGetDistanceToPoint = localPlane.GetDistanceToPoint(originMesh.Vertices[FrontsideVertexIndex]);
            float backGetDistanceToPoint = localPlane.GetDistanceToPoint(originMesh.Vertices[BacksideVertexIndex]);
            float ratio = frontGetDistanceToPoint / (frontGetDistanceToPoint - backGetDistanceToPoint);

            Vector3 position = Vector3.Lerp(
                originMesh.Vertices[FrontsideVertexIndex],
                originMesh.Vertices[BacksideVertexIndex],
                ratio
            );

            Vector3 normal = Vector3.Lerp(
                originMesh.Normals[FrontsideVertexIndex],
                originMesh.Normals[BacksideVertexIndex],
                ratio
            ).normalized;

            Vector2 uv = Vector2.Lerp(
                originMesh.UVs[FrontsideVertexIndex],
                originMesh.UVs[BacksideVertexIndex],
                ratio
            );

            return (position, normal, uv);
        }
    }
}