using System;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 切断平面上の頂点に関する情報を保持するクラス
    /// </summary>
    public abstract class AbstractCutSurfaceVertex : IEquatable<AbstractCutSurfaceVertex> {

        /// <summary>
        /// 頂点のローカル座標 (unity)
        /// </summary>
        public readonly Vector3 LocalPosition;

        /// <summary>
        /// 切断平面上を座標系とした頂点の座標 2D 座標 (直交座標系)
        /// </summary>
        public readonly Vector2 PlanePosition;

        /// <summary>
        /// 法線側メッシュでの頂点インデックス (切断後メッシュ頂点リストでのインデックス)
        /// </summary>
        public int NewFrontsideVertexIndex {
            get; private set;
        }

        /// <summary>
        /// 反法線側メッシュでの頂点インデックス (切断後メッシュ頂点リストでのインデックス)
        /// </summary>
        public int NewBacksideVertexIndex {
            get; private set;
        }

        /// <summary>
        /// 切断平面上の頂点情報を初期化するコンストラクタ
        /// </summary>
        /// <param name="localPlane"> 切断平面 (ローカル座標) </param>
        /// <param name="localPosition"> 頂点座標 </param>
        public AbstractCutSurfaceVertex(Plane localPlane, Vector3 localPosition) {
            LocalPosition = localPosition;

            Vector3 axisX = Vector3.Cross(localPlane.normal, Vector3.up).normalized;
            if (axisX.sqrMagnitude < 0.0001f) {
                axisX = Vector3.Cross(localPlane.normal, Vector3.right).normalized;
            }
            Vector3 axisY = Vector3.Cross(localPlane.normal, axisX).normalized;
            Vector3 anchor = localPlane.normal * localPlane.distance;

            PlanePosition = new Vector2(
                Vector3.Dot(localPosition - anchor, axisX),
                Vector3.Dot(localPosition - anchor, axisY)
            );
        }

		/// <summary>
		/// 切断平面上の頂点インデックスを設定するメソッド
		/// </summary>
		/// <param name="newFrontsideVertexIndex"> 切断後の法線側メッシュでの頂点インデックス </param>
		/// <param name="newBucksideVertexIndex"> 切断後の反法線側メッシュでの頂点インデックス </param>
		public void SetIndex(
            int newFrontsideVertexIndex,
            int newBucksideVertexIndex
        ) {
            NewFrontsideVertexIndex = newFrontsideVertexIndex;
            NewBacksideVertexIndex = newBucksideVertexIndex;
        }

        /// <summary>
        /// Equals メソッドのオーバーライド
        /// 内容比較
        /// </summary>
        /// <param name="other"> AbstractCutSurfaceVertex 型オブジェクト </param>
        /// <returns> 等しければ true, そうでなければ false </returns>
        public abstract bool Equals(AbstractCutSurfaceVertex? other);

        /// <summary>
        /// Equals メソッドのオーバーライド
        /// 型比較
        /// </summary>
        /// <param name="obj"> オブジェクト型 </param>
        /// <returns> 等しければ true, そうでなければ false </returns>
        public override bool Equals(object? obj) {
            if (obj is AbstractCutSurfaceVertex other)
                return Equals(other);
            return false;
        }

        /// <summary>
        /// GetHashCode メソッドのオーバーライド
        /// Equala メソッドで利用されるハッシュコードを返す
        /// </summary>
        /// <returns> インスタンスの参照に基づいた数値 </returns>
        public override int GetHashCode() {
            return PlanePosition.GetHashCode();
        }
    }
}