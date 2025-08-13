using System;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 非単調な多角形 (y に単調な多角形への分割操作前) の辺クラス 
    /// </summary>
    public class EdgeTwoEarsTheorem : Edge<VertexTwoEarsTheorem> {
        /*デバッグ用*/
        public String Address;

		/// <summary>
		/// 辺の x 座標の最小値
		/// </summary>
		public float MinX => Mathf.Min(Start.PlanePosition.x, End.PlanePosition.x);

		/// <summary>
		/// 辺の x 座標の最大値
		/// </summary>
		public float MaxX => Mathf.Max(Start.PlanePosition.x, End.PlanePosition.x);

		/// <summary>
		/// 辺の y 座標の最小値
		/// </summary>
		public float MinY => Mathf.Min(Start.PlanePosition.y, End.PlanePosition.y);

        /// <summary>
        /// 辺の y 座標の最大値
        /// </summary>
        public float MaxY => Mathf.Max(Start.PlanePosition.y, End.PlanePosition.y);

        /// <summary>
        /// ヘルパー頂点
        /// </summary>
        public VertexTwoEarsTheorem Helper {
            get; set;
        }

        public EdgeTwoEarsTheorem(
            VertexTwoEarsTheorem start,
            VertexTwoEarsTheorem end
        ) : base(start, end) {
            Helper = default;
        }

        /// <summary>
        /// 辺の y 座標に対する x 座標を取得するメソッド
        /// 水平線が辺の y 範囲内に存在することが前提である
        /// </summary>
        /// <param name="y"> 水平線の y 座標 </param>
        /// <returns> 辺上の水平線との交点の x 座標 </returns>
        public float GetXPositionIntersectionWithHorizon(float y) {

            if (Mathf.Abs(Start.PlanePosition.y - End.PlanePosition.y) < Epsilon) {
                return (Start.PlanePosition.x + End.PlanePosition.x) / 2;
            }
            return Start.PlanePosition.x +
                   (y - Start.PlanePosition.y) *
                   (End.PlanePosition.x - Start.PlanePosition.x) /
                   (End.PlanePosition.y - Start.PlanePosition.y);
        }

        /// <summary>
        /// this 辺の逆辺を取得するメソッド
        /// </summary>
        /// <returns> 対象の辺と逆向きの辺 </returns>
        public EdgeTwoEarsTheorem GetReverseEdge() {

            /*デバッグ用*/
            var edge = new EdgeTwoEarsTheorem(End, Start);
            edge.Address = Address + "_reverse";
            return edge;

            //return new EdgeTwoEarsTheorem(End, Start);
        }
    }
}