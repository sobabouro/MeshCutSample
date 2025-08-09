using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 各頂点から接続する辺を管理するマップクラス
    /// </summary>
    public class Map {

        private readonly IEdgeSortStrategy _sortStrategy;

        /// <summary>
        /// 各頂点において，その頂点から接続するすべての辺へのマッピング
        /// </summary>
        private readonly Dictionary<NonConvexMonotoneCutSurfaceVertex, List<NonConvexMonotoneCutSurfaceEdge>> _map;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Map() {

            _sortStrategy = new PriorityLeftTurnSortStrategy();
            _map = new Dictionary<NonConvexMonotoneCutSurfaceVertex, List<NonConvexMonotoneCutSurfaceEdge>>();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Map(IEdgeSortStrategy sortStrategy) {

            _sortStrategy = sortStrategy;
            _map = new Dictionary<NonConvexMonotoneCutSurfaceVertex, List<NonConvexMonotoneCutSurfaceEdge>>();
        }

        /// <summary>
        /// グラフに無向辺を追加する
        /// </summary>
        /// <param name="edge"> 追加する辺 </param>"
        public void AddEdge(NonConvexMonotoneCutSurfaceEdge edge) {

            AddDirectedEdgeToMap(edge);
            //AddDirectedEdgeToMap(edge.GetReverseEdge());
        }

        /// <summary>
        /// 指定された始点から終点への有向辺をマップに追加する
        /// </summary>
        /// <param name="edge"> 追加する有向辺 </param>
        private void AddDirectedEdgeToMap(NonConvexMonotoneCutSurfaceEdge edge) {

            // 始点のリストが存在しない場合は新規作成する
            if (!_map.ContainsKey(edge.Start))
                _map[edge.Start] = new List<NonConvexMonotoneCutSurfaceEdge>();

            // 既に存在する辺でない場合のみ追加する
            if (!_map[edge.Start].Contains(edge)) {
                _map[edge.Start].Add(edge);
            }
        }

        /// <summary>
        /// 指定された頂点に接続する辺のリストを取得する
        /// </summary>
        /// <remarks>
        /// リストは最も左折する順，もしくは最も右折する順でソートされる <br/>
        /// 右折と左折の決定は，前の頂点からの入射ベクトルに対して，次の辺がどのように曲折するかで行う
        /// </remarks>
        /// <param name="currVertex"> 現在の頂点 </param>
        /// <param name="prevVertex"> 前の頂点 </param>
        /// <returns> ソート済みの辺のリスト </returns>
        public List<NonConvexMonotoneCutSurfaceEdge> GetSortedEdgesFromIncomingVector(
            NonConvexMonotoneCutSurfaceVertex currVertex,
            NonConvexMonotoneCutSurfaceVertex prevVertex
        ) {
            if (!_map.TryGetValue(currVertex, out var edges)) {
                return new List<NonConvexMonotoneCutSurfaceEdge>();
            }

            Vector2 incomingVector = prevVertex == null || prevVertex.Equals(currVertex)
                    ? Vector2.right
                    : currVertex.PlanePosition - prevVertex.PlanePosition;

            var edgesWithAngles = edges.Select(edge => {
                Vector2 nextVector = (edge.End.PlanePosition - edge.Start.PlanePosition).normalized;
                float angle = Vector2.SignedAngle(incomingVector, nextVector);
                return (edge, angle);
            }).ToList();

            return _sortStrategy.Sort(edgesWithAngles);
        }

        /// <summary>
        /// グラフ内のすべての頂点を取得する
        /// </summary>
        public IEnumerable<NonConvexMonotoneCutSurfaceVertex> GetAllKeys() {
            return _map.Keys;
        }
    }
}