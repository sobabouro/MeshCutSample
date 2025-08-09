using System.Collections.Generic;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 非凸単調切断面の辺を比較するためのコンパレータクラス
    /// </summary>
    public class EdgeComparer : IComparer<NonConvexMonotoneCutSurfaceEdge> {

        /// <summary>
        /// 浮動小数点数の誤差吸収用
        /// </summary>
        private const float Epsilon = 0.0001f;

        /// <summary>
        /// 比較のための水平線の y 座標
        /// </summary>
        public static float HorizonY {
            get;
            set;
        }

        /// <summary>
        /// 非凸単調切断面の辺を比較するためのコンパレータ
        /// </summary>
        /// <param name="edge1"> 辺 1 </param>
        /// <param name="edge2"> 辺 2 </param>
        /// <returns> edge1 が edge2 より小さい場合は -1、等しい場合は 0、edge1 が edge2 より大きい場合は 1 を返す </returns>
        public int Compare(NonConvexMonotoneCutSurfaceEdge? edge1, NonConvexMonotoneCutSurfaceEdge? edge2) {

            // 同じオブジェクトを比較する場合は等しいとみなす
            if (ReferenceEquals(edge1, edge2))
                return 0;
            // edge1 が null の場合は edge1 < edge2 とみなす
            if (ReferenceEquals(null, edge1))
                return -1;
            // edge2 が null の場合は edge1 > edge2 とみなす
            if (ReferenceEquals(null, edge2))
                return 1;

            float x1 = edge1.GetXPositionIntersectionWithHorizon(HorizonY);
            float x2 = edge2.GetXPositionIntersectionWithHorizon(HorizonY);

            if (Mathf.Abs(x1 - x2) < Epsilon) {
                return edge1.GetHashCode().CompareTo(edge2.GetHashCode());
            }
            return x1.CompareTo(x2);
        }
    }
}