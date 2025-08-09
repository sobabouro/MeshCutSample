using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 入射角からの現在の辺の角度差情報を所持した辺リストのソート戦略クラス
    /// </summary>
    public class PriorityAcuteAngleSortStrategy : IEdgeSortStrategy {
        /// <summary>
        /// 切断辺のソートを行うメソッド
        /// </summary>
        /// <param name="edgesWithAngles"> ソート対象の入射角度差タグを所持した切断辺リスト </param>
        /// <returns> ソートされた切断辺リスト </returns>
        public List<NonConvexMonotoneCutSurfaceEdge> Sort(
            IEnumerable<(NonConvexMonotoneCutSurfaceEdge edge, float angle)> edgesWithAngles
        ) {
            bool hasLeftTurnEdge = edgesWithAngles.Any(item => item.angle > 0);

            IOrderedEnumerable<(NonConvexMonotoneCutSurfaceEdge edge, float angle)> sortedEdgesWithAngles;

            if (hasLeftTurnEdge) {
                sortedEdgesWithAngles = edgesWithAngles
                    .OrderByDescending(item => Mathf.Abs(item.angle) > 179.9f ? -181f : item.angle);
            }
            else {
                sortedEdgesWithAngles = edgesWithAngles
                    .OrderBy(item => Mathf.Abs(item.angle) > 179.9f ? 181f : item.angle);
            }

            return sortedEdgesWithAngles.Select(item => item.edge).ToList();
        }
    }
}