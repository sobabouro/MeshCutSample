using System.Collections.Generic;
using IntervalTree;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// interval tree を使用して，辺用に機能改良したクラス
    /// interval (区間) ごとの走査を高速に行える自己平衡二分探索木のデータ構造
    /// </summary>
    public class EdgeIntervalTree {

        /// <summary>
        /// 許容誤差
        /// </summary>
        private const float Epsilon = 0.0001f;

        /// <summary>
        /// y 座標を基準にした区間木
        /// </summary>
        /// <remarks>
        /// 自己平衡な区間木を使用する
        /// オリジナルクラスでは struct であることを Interval が要求するため，以下の辞書 (edgeMap) を使用する
        /// </remarks>
        private readonly IntervalTree<float> _tree = new IntervalTree<float>();

        /// <summary>
        /// 辺と y 座標の区間をマッピングするための辞書
        /// </summary>
        /// <remarks>
        /// vvondra.IntervalTree では Value を持つ区間をサポートしていない (区間のみでノード管理している)
        /// Interval<T> を毎回生成せず，参照型の edge をキーとして管理することで，RemoveEdge() の際の不要な探索と比較を回避する
        /// </remarks>
        private readonly Dictionary<NonConvexMonotoneCutSurfaceEdge, Interval<float>> _edgeMap = new Dictionary<NonConvexMonotoneCutSurfaceEdge, Interval<float>>();

        /// <summary>
        /// EdgeIntervalTree に辺を追加するメソッド
        /// </summary>
        /// <param name="edge"> 追加する辺 </param>
        public void AddEdge(NonConvexMonotoneCutSurfaceEdge edge) {

            //Debug.Log($"EdgeIntervalTree: AddEdge - {edge.Start.VertexType} -> {edge.End.VertexType}, MinY: {edge.MinY}, MaxY: {edge.MaxY}");

            var interval = new Interval<float>(edge.MinY, edge.MaxY);
            _tree.Add(interval);
            _edgeMap[edge] = interval;
        }

        /// <summary>
        /// EdgeIntervalTree から辺を削除するメソッド
        /// </summary>
        /// <param name="edge"> 削除する辺 </param>
        public void RemoveEdge(NonConvexMonotoneCutSurfaceEdge edge) {
            if (_edgeMap.TryGetValue(edge, out var interval)) {

                //Debug.Log($"EdgeIntervalTree: RemoveEdge - {edge.Start.VertexType} -> {edge.End.VertexType}, MinY: {edge.MinY}, MaxY: {edge.MaxY}");

                _tree.Remove(interval);
                _edgeMap.Remove(edge);
            }
        }

        /// <summary>
        /// 入力された y 座標の水平線が通過する辺のリストを取得するメソッド
        /// </summary>
        /// <param name="y"> 水平線の y 座標 </param>
        /// <returns> y 座標の水平線が通過する辺のリスト </returns>
        public List<NonConvexMonotoneCutSurfaceEdge> GetEdgesPassThroughHorizon(float y) {

            var result = new List<NonConvexMonotoneCutSurfaceEdge>();
            var candidates = _tree.Search(y);

            foreach (var pair in _edgeMap) {
                var edge = pair.Key;
                if (y > edge.MinY - Epsilon && y < edge.MaxY + Epsilon) {
                    result.Add(edge);
                }
                //if (y >= edge.MinY && y <= edge.MaxY) {
                //    result.Add(edge);
                //}
            }

            return result;
        }
    }
}