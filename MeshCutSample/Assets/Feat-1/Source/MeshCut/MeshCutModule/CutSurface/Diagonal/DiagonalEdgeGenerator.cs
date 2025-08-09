using System;
using System.Collections.Generic;
using UnityEngine;
using DebugUtils;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 対角線を生成するクラス
    /// </summary>
    public class DiagonalEdgeGenerator {

        /// <summary>
        /// 浮動小数点数の誤差吸収用
        /// </summary>
        private const float Epsilon = 0.0001f;

        /// <summary>
        /// 図形すべての頂点を y 座標でソートした順番のインデックスリスト
        /// 図形ごと (LinkedVertexList ごと) の連結辺の番地に対応する
        /// もとの頂点リストでの頂点を "v" とする
        /// </summary>
        private (int, int)[] _indexBeforeSortY = new (int, int)[] { };

        /// <summary>
        /// 図形の辺のリスト (以下 "e" とする)
        /// v[i] を始点とする辺を e[i] とする (頂点は以下 "v" とする)
        /// </summary>
        private List<List<NonConvexMonotoneCutSurfaceEdge>> _edgeList = new();

        /// <summary>
        /// 辺の走査を行うための木 (以下 "T" とする)
        /// </summary>
        private EdgeIntervalTree _edgeIntervalTree = new();

        /// <summary>
        /// T の中の辺を x 座標でソートした順序を保持するための辞書
        /// </summary>
        private SortedDictionary<NonConvexMonotoneCutSurfaceEdge, NonConvexMonotoneCutSurfaceEdge> _sortedXPositionEdgeInTree = new(new EdgeComparer());

        /// <summary>
        /// 対角線の集合 (以下 "D" とする)
        /// 一つの対角線を追加する際，両方向に分けて二つ追加していく
        /// </summary>
        private HashSet<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> _diagonalSet = new();

        /// <summary>
        /// コンストラクタ
        /// ここで対角線の生成を行う
        /// </summary>
        /// <param name="linkedVertexList"> 連結辺シーケンスリスト </param>
        public DiagonalEdgeGenerator(LinkedVertexList linkedVertexList) {

            FormattingData(linkedVertexList);
            ProcessSweepLineForMakeDiagonalEdge(linkedVertexList);
        }

        /// <summary>
        /// 対角線のリストを取得するメソッド
        /// 対角線は一つの辺につき二本登録される (始点 -> 終点, 終点 -> 始点 の向き)
        /// </summary>
        /// <returns> 対角線のリスト </returns>
        public HashSet<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> GetDiagonalSet() {
            return _diagonalSet;
        }

        /// <summary>
        /// 整形された辺のリストを取得するメソッド
        /// </summary>
        /// <returns> 辺のリスト </returns>
        public List<List<NonConvexMonotoneCutSurfaceEdge>> GetEdgeList() {
            return _edgeList;
        }

        /// <summary>
        /// 連結辺シーケンスリストを整形するメソッド
        /// </summary>
        /// <param name="linkedVertexList"> 連結辺シーケンスのリスト </param>
        private void FormattingData(LinkedVertexList linkedVertexList) {

            linkedVertexList.DeleteLastElement();
            linkedVertexList.ClusteringVertexType();

            for (int i = 0; i < linkedVertexList.Count; i++) {

                List<NonConvexMonotoneCutSurfaceEdge> edges = new();
                var list = linkedVertexList[i];
                var currentNode = list.First;

                for (int j = 0; j < list.Count; j++) {

                    var startVertex = currentNode.Value;
                    var endVertex = currentNode.Next != null ? currentNode.Next.Value : list.First.Value;

                    if (startVertex != null && endVertex != null && !startVertex.Equals(endVertex)) {
                        NonConvexMonotoneCutSurfaceEdge edge = new NonConvexMonotoneCutSurfaceEdge(startVertex, endVertex);
                        edges.Add(edge);
                        _edgeIntervalTree.AddEdge(edge);
                    }
                    currentNode = currentNode.Next;
                }
                _edgeList.Add(edges);
            }
            _indexBeforeSortY = linkedVertexList.GetAllIndexSortedPlanePositionY();

            for (int i = 0; i < _edgeList.Count; i++) {
                for (int j = 0; j < _edgeList[i].Count; j++) {
                    Debug.Log($"Edge[{i}][{j}]: {_edgeList[i][j].Start.VertexType} - {_edgeList[i][j].Start.PlanePosition} ,  {_edgeList[i][j].End.VertexType} - {_edgeList[i][j].End.PlanePosition}");
                }
            }
        }

        /// <summary>
        /// 対角線を生成するための処理
        /// イベントポイント (頂点) の頂点種類によって処理を分岐する
        /// インベントポイントは，図形の頂点リストを y 座標でソートした順で処理される
        /// </summary>
        /// <param name="linkedVertexList"> 連結辺シーケンスのリスト (リストの数だけ図形がある ※連結した辺 -> 図形を構成している) </param>
        private void ProcessSweepLineForMakeDiagonalEdge(LinkedVertexList linkedVertexList) {

            for (int i = 0; i < _indexBeforeSortY.Length; i++) {

                var currVertex = _edgeList[_indexBeforeSortY[i].Item1][_indexBeforeSortY[i].Item2].Start;

                /*デバッグ用*/
                currVertex.Address = $"v[{_indexBeforeSortY[i].Item1}, {_indexBeforeSortY[i].Item2}]";

                var currEdge = _edgeList[_indexBeforeSortY[i].Item1][_indexBeforeSortY[i].Item2];

                /*デバッグ用*/
                currEdge.Address = $"e[{_indexBeforeSortY[i].Item1}, {_indexBeforeSortY[i].Item2}]";

                var prevEdge = _indexBeforeSortY[i].Item2 > 0
                    ? _edgeList[_indexBeforeSortY[i].Item1][_indexBeforeSortY[i].Item2 - 1]
                    : _edgeList[_indexBeforeSortY[i].Item1][_edgeList[_indexBeforeSortY[i].Item1].Count - 1];

                // 走査線の y 座標を設定する
                EdgeComparer.HorizonY = currVertex.PlanePosition.y;
                // 走査対象の頂点に接続する辺を取得する
                var activeEdges = _edgeIntervalTree.GetEdgesPassThroughHorizon(currVertex.PlanePosition.y);

                foreach (var edge in activeEdges) {
                    _sortedXPositionEdgeInTree.Add(edge, edge);
                }

                switch (currVertex.VertexType) {
                    case VertexType.Start:
                        HandleStartVertex(currVertex, currEdge);
                        break;
                    case VertexType.Split:
                        HandleSplitVertex(currVertex, currEdge);
                        break;
                    case VertexType.Regular:
                        HandleRegularVertex(currVertex, currEdge, prevEdge);
                        break;
                    case VertexType.Merge:
                        HandleMergeVertex(currVertex, prevEdge);
                        break;
                    case VertexType.End:
                        HandleEndVertex(currVertex, prevEdge);
                        break;
                }
                _sortedXPositionEdgeInTree.Clear();
            }
        }

        /// <summary>
        /// イベントポイントが開始点 (Start) の場合の処理メソッド
        /// </summary>
        /// <param name="currVertex"> v[i] </param>
        /// <param name="currEdge"> e[i] </param>
        private void HandleStartVertex(
            NonConvexMonotoneCutSurfaceVertex currVertex,
            NonConvexMonotoneCutSurfaceEdge currEdge
        ) {
            /**
             * e[i] を T に挿入し，helper(e[i]) を v[i] とする
             */
            _edgeIntervalTree.AddEdge(currEdge);
            currEdge.Helper = currVertex;
        }

        /// <summary>
        /// イベントポイントが分離点 (Split) の場合の処理メソッド
        /// </summary>
        /// <param name="currVertex"> v[i] </param>
        /// <param name="currEdge"> e[i] </param>
        private void HandleSplitVertex(
            NonConvexMonotoneCutSurfaceVertex currVertex,
            NonConvexMonotoneCutSurfaceEdge currEdge
        ) {
            /**
             * T の中を探索して，v[i] のすぐ左にある辺 e[j] を求める
             * v[i] と helper(e[j]) を結ぶ対角線を D に挿入する
             * helper(e[j]) を v[i] にする
             * e[i] を T に挿入し，helper(e[i]) を v[i] とする
             */
            var mostLeftNeighboringEdge = GetEdgeMostLeftNeighboringFromVertex(currVertex);

            AddDiagonalEdge(currVertex, mostLeftNeighboringEdge.Helper);
            mostLeftNeighboringEdge.Helper = currVertex;

            _edgeIntervalTree.AddEdge(currEdge);
            currEdge.Helper = currVertex;
        }

        /// <summary>
        /// イベントポイントが通常点 (Regular) の場合の処理メソッド
        /// </summary>
        /// <param name="currVertex"> v[i] </param>
        /// <param name="currEdge"> e[i] </param>
        /// <param name="prevEdge"> e[i-1] </param>
        private void HandleRegularVertex(
            NonConvexMonotoneCutSurfaceVertex currVertex,
            NonConvexMonotoneCutSurfaceEdge currEdge,
            NonConvexMonotoneCutSurfaceEdge prevEdge
        ) {
            /** 
             * if P の内部が v[i] の右にある
             * - then if helper(e[i-1]) が統合点である
             * - - then v[i] と helper(e[i-1]) を結ぶ対角線を D に挿入する
             * - - e[i-1] を T から削除する
             * - - e[i] を T に挿入し，helper(e[i]) を v[i] にする
             * - else T の中を探索して，v[i] のすぐ左にある辺 e[j] を求める
             * - - if helper(e[j]) が統合点である
             * - - - then v[i] と helper(e[j]) を結ぶ対角線を D に挿入する
             * - - helper(e[j]) を v[i] にする
             */

            if (!hasSolidInRightSide(currEdge))
                return;

            if (prevEdge.Helper?.VertexType == VertexType.Merge) {

                AddDiagonalEdge(currVertex, prevEdge.Helper);
                _edgeIntervalTree.RemoveEdge(prevEdge);
                _edgeIntervalTree.AddEdge(currEdge);
                currEdge.Helper = currVertex;
            }
            else {

                var mostLeftNeighboringEdge = GetEdgeMostLeftNeighboringFromVertex(currVertex);

                if (mostLeftNeighboringEdge == null)
                    return;

                if (mostLeftNeighboringEdge.Helper?.VertexType == VertexType.Merge) {
                    AddDiagonalEdge(currVertex, mostLeftNeighboringEdge.Helper);
                }
                mostLeftNeighboringEdge.Helper = currVertex;
            }
        }

        /// <summary>
        /// イベントポイントが統合点 (Merge) の場合の処理メソッド
        /// </summary>
        /// <param name="currVertex"> v[i] </param>
        /// <param name="prevEdge"> e[i-1] </param>
        private void HandleMergeVertex(
            NonConvexMonotoneCutSurfaceVertex currVertex,
            NonConvexMonotoneCutSurfaceEdge prevEdge
        ) {
            /**
             * if helper(e[i-1]) が統合点である
             * - then v[i] と helper(e[i-1]) を結ぶ対角線を D に挿入する
             * e[i-1] を T から削除する
             * T の中を探索して，v[i] のすぐ左にある辺 e[j] を求める
             * if helper(e[j]) が統合点である
             * - then v[i] と helper(e[j]) を結ぶ対角線を D に挿入する
             * helper(e[j]) を v[i] にする
             */
            if (prevEdge.Helper?.VertexType == VertexType.Merge)
                AddDiagonalEdge(currVertex, prevEdge.Helper);

            _edgeIntervalTree.RemoveEdge(prevEdge);
            var mostLeftNeighboringEdge = GetEdgeMostLeftNeighboringFromVertex(currVertex);

            if (mostLeftNeighboringEdge.Helper?.VertexType == VertexType.Merge)
                AddDiagonalEdge(currVertex, mostLeftNeighboringEdge.Helper);

            mostLeftNeighboringEdge.Helper = currVertex;
        }

        /// <summary>
        /// イベントポイントが終了点 (End) の場合の処理メソッド
        /// </summary>
        /// <param name="currVertex"> v[i] </param>
        /// <param name="prevEdge"> e[i-1] </param>
        private void HandleEndVertex(
            NonConvexMonotoneCutSurfaceVertex currVertex,
            NonConvexMonotoneCutSurfaceEdge prevEdge
        ) {
            /**
             * if helper(e[i-1]) が統合点である
             * - then v[i] と helper(e[i-1]) を結ぶ対角線を D に挿入する
             * e[i-1] を T から削除する
             */
            if (prevEdge.Helper?.VertexType == VertexType.Merge)
                AddDiagonalEdge(currVertex, prevEdge.Helper);

            _edgeIntervalTree.RemoveEdge(prevEdge);
        }

        /// <summary>
        /// 指定された頂点の最も左側にある辺の始点を取得するメソッド
        /// comparer の HorizonY が既定されていることが前提
        /// </summary>
        /// <param name="vertex"> 頂点 </param>
        /// <returns> 頂点の最も左側にある辺 </returns>
        /// <exception cref="InvalidOperationException"> 隣接頂点がない場合 </exception>
        private NonConvexMonotoneCutSurfaceEdge GetEdgeMostLeftNeighboringFromVertex(NonConvexMonotoneCutSurfaceVertex vertex) {

            NonConvexMonotoneCutSurfaceEdge tmpSearchKey = new(vertex, vertex);
            NonConvexMonotoneCutSurfaceEdge? mostLeftNeighboringEdge = null;
            bool isRegularType = vertex.VertexType == VertexType.Regular;

            foreach (var edge in _sortedXPositionEdgeInTree.Keys) {
                int comparisonResult = _sortedXPositionEdgeInTree.Comparer.Compare(tmpSearchKey, edge);

                // 辺が対象頂点よりも左側にある場合
                if (comparisonResult > 0) {

                    // 条件1. 対象頂点を含む辺ではない
                    bool isNotContainsVertex = !edge.Start.Equals(vertex) && !edge.End.Equals(vertex);
                    // 条件2. 対象頂点に暫定解より近い
                    bool isMoreCloser = mostLeftNeighboringEdge == null
                        ? true
                        : mostLeftNeighboringEdge.GetXPositionIntersectionWithHorizon(EdgeComparer.HorizonY) <
                          edge.GetXPositionIntersectionWithHorizon(EdgeComparer.HorizonY);
                    // 条件3. 通常点は MinY <= y < MaxY を満たすもの，通常点以外であれば MinY < y < MaxY を満たしているもののみが対象である
                    if (isRegularType)
                        isMoreCloser = isMoreCloser && edge.MinY <= EdgeComparer.HorizonY && EdgeComparer.HorizonY < edge.MaxY;
                    else
                        isMoreCloser = isMoreCloser && edge.MinY < EdgeComparer.HorizonY && EdgeComparer.HorizonY < edge.MaxY;

                    // 条件を満たす場合、最も左側の辺を更新する
                    if (isNotContainsVertex && isMoreCloser)
                        mostLeftNeighboringEdge = edge;
                }
                else {
                    break;
                }
            }
            //if (mostLeftNeighboringEdge == null) 
            //    throw new InvalidOperationException("no neighboring edge found for the vertex.");
            return mostLeftNeighboringEdge;
        }

        /// <summary>
        /// 右側に図形の内部があるかどうかを判定するメソッド
        /// </summary>
        /// <param name="edge"> 判定対象の辺 </param>
        /// <returns> 辺の右側に図形の内部があれば true, 無ければ false を返す </returns>
        private bool hasSolidInRightSide(NonConvexMonotoneCutSurfaceEdge edge) {
            bool hasSolid = false;

            if (edge.Start.PlanePosition.y < edge.End.PlanePosition.y)
                hasSolid = false;
            else if (edge.Start.PlanePosition.y > edge.End.PlanePosition.y)
                hasSolid = true;
            else {
                if (edge.Start.PlanePosition.x < edge.End.PlanePosition.x)
                    hasSolid = true;
                else if (edge.Start.PlanePosition.x > edge.End.PlanePosition.x)
                    hasSolid = false;
                else
                    Debug.LogError($"DiagonalEdgeGenerator: hasSolidInRightSide() - edge position of edge are equal{edge.Start.PlanePosition}, {edge.End.PlanePosition}");
            }
            return hasSolid;
        }

        /// <summary>
        /// 対角線を追加するメソッド
        /// </summary>
        /// <param name="startVertex"> 辺の始点 </param>
        /// <param name="endVertex"> 辺の終点 </param>
        /// <remarks>
        /// 対角線が水平である場合，アクティブな辺に水平なものがあれば，これら二つが重なっている場合がある </br>
        /// この場合は，重複した部分を取り除いて残る部分を対角線として追加する
        /// </remarks>
        private void AddDiagonalEdge(
            NonConvexMonotoneCutSurfaceVertex startVertex,
            NonConvexMonotoneCutSurfaceVertex endVertex
        ) {
            // 対角線が水平でない場合
            if (Mathf.Abs(startVertex.PlanePosition.y - endVertex.PlanePosition.y) > Epsilon) {

                // そのまま対角線を追加する
                _diagonalSet.Add((startVertex, endVertex));
            }
            // 対角線が水平である場合
            else {

                (var diagonalMin, var diagonalMax) = startVertex.PlanePosition.x < endVertex.PlanePosition.x
                        ? (startVertex, endVertex)
                        : (endVertex, startVertex);

                foreach (var edge in _sortedXPositionEdgeInTree.Keys) {

                    // アクティブな辺のうち，水平のものである場合
                    if (Mathf.Abs(edge.Start.PlanePosition.y - edge.End.PlanePosition.y) < Epsilon) {

                        (var edgeMin, var edgeMax) = edge.Start.PlanePosition.x < edge.End.PlanePosition.x
                                ? (edge.Start, edge.End)
                                : (edge.End, edge.Start);

                        // 対角線とアクティブな辺が重なっている場合
                        if (diagonalMin.PlanePosition.x == edgeMin.PlanePosition.x && edgeMax.PlanePosition.x <= diagonalMax.PlanePosition.x) {

                            _diagonalSet.Add((edgeMax, diagonalMax));
                            break;
                        }
                        else if (diagonalMin.PlanePosition.x <= edgeMin.PlanePosition.x && edgeMax.PlanePosition.x == diagonalMax.PlanePosition.x) {

                            _diagonalSet.Add((diagonalMin, edgeMin));
                            break;
                        }
                        // 対角線とアクティブな辺が重なっていないならスルー
                        else {
                            continue;
                        }
                    }
                }
            }
        }
    }
}