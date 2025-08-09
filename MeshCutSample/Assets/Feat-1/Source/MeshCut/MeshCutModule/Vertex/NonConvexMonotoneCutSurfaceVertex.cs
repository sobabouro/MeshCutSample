using System;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 非単調多角形 (y 単調多角形分割前) の頂点クラス 
    /// </summary>
    public class NonConvexMonotoneCutSurfaceVertex : AbstractCutSurfaceVertex, IEquatable<NonConvexMonotoneCutSurfaceVertex> {

        /*デバッグ用*/
        public String Address;

        /// <summary>
        /// 頂点の種類
        /// </summary>
        public VertexType VertexType;

        /// <summary>
        /// この頂点を含む図形の y 最大地点と最小地点までを結ぶ二つの境界のうち，どちら側に位置するか
        /// </summary>
        public SideType SideType;

        /// <summary>
        /// ヘルパー頂点
        /// this 頂点と，ヘルパー頂点とをリンクするために保持する
        /// </summary>
        public NonConvexMonotoneCutSurfaceVertex Helper;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="localPlane"> 切断平面 </param>
        /// <param name="localPosition"> 頂点の空間座標 </param>
        public NonConvexMonotoneCutSurfaceVertex(Plane localPlane, Vector3 localPosition)
            : base(localPlane, localPosition) {
            Helper = default;
        }

        /// <summary>
        /// Equals メソッドのオーバーライド
        /// </summary>
        /// <param name="other"> AbstractCutSurfaceVertex 型オブジェクト </param>
        /// <returns> 等しければ true, そうでなければ false </returns>
        public override bool Equals(AbstractCutSurfaceVertex other) {
            if (other is NonConvexMonotoneCutSurfaceVertex otherVertex) {
                return LocalPosition.Equals(otherVertex.LocalPosition);
            }
            return false;
        }

        /// <summary>
        /// Equals メソッドのオーバーライド
        /// </summary>
        /// <param name="other"> NonConvexMonotoneCutSurfaceVertex 型オブジェクト </param>
        /// <returns> 等しければ true, そうでなければ false </returns>
        public bool Equals(NonConvexMonotoneCutSurfaceVertex other) {
            if (other == null)
                return false;
            return LocalPosition.Equals(other.LocalPosition);
        }

        /// <summary>
        /// GetHashCode メソッドのオーバーライド
        /// Equala メソッドで利用されるハッシュコードを返す
        /// </summary>
        /// <returns> インスタンスの参照に基づいた数値 </returns>
        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}