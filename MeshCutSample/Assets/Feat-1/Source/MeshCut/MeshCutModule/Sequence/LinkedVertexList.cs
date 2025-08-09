using System;
using System.Linq;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 連結辺シーケンス (LinkedVertex) のリストを管理するクラス
    /// </summary>
    public class LinkedVertexList : AbstractNodeSequenceList<LinkedVertex, NonConvexMonotoneCutSurfaceVertex> {

        /// <summary>
        /// 連結辺シーケンスに頂点を追加するメソッド
        /// </summary>
        /// <param name="args"> (NewVertex 始点, NewVertex 終点) </param>
        public override void Add(params object[] args) {

            if (args.Length != 2 || !(args[0] is NonConvexMonotoneCutSurfaceVertex toward) || !(args[1] is NonConvexMonotoneCutSurfaceVertex away)) {
                Debug.LogError("Add for LinkedVertexList requires two NewVertex arguments.");
                return;
            }

            for (int i = 0; i < _nodeSequenceList.Count; i++) {
                var current = _nodeSequenceList[i];

                if (current.TryAppend(toward, away)) {
                    TryMergeAfter(i, away);
                    return;
                }
                if (current.TryPrepend(toward, away)) {
                    TryMergeBefore(i, toward);
                    return;
                }
            }
            // どの連結辺にも追加できなかった場合は新しい連結辺を作成する
            LinkedVertex newLinked = new();
            newLinked.TryAppend(toward, away);
            _nodeSequenceList.Add(newLinked);
        }

        /// <summary>
        /// シーケンス同士のマージ可能性を判定するメソッド
        /// </summary>
        /// <param name="target"> あるシーケンスの頂点 </param>
        /// <param name="key"> Add() によって追加した新有向辺の対応する頂点 </param>
        /// <param name="isAfter"> 前後のどちらに対してマージを試みるかを示すフラグ </param>
        /// <returns></returns>
        protected override bool CheckMerge(NonConvexMonotoneCutSurfaceVertex target, NonConvexMonotoneCutSurfaceVertex key, bool isAfter) {

            if (target == null)
                return false;
            return target.Equals(key);
        }

        /// <summary>
        /// 連結辺シーケンスのリスト内のすべての新頂点たちに頂点種類のラベル付与を行う
        /// </summary>
        public void ClusteringVertexType() {

            foreach (var linkedVertex in _nodeSequenceList) {
                linkedVertex.ClusteringVertexType();
            }
        }

        /// <summary>
        /// this リスト内のすべての連結辺シーケンスに対して，総じた以下の処理を行う
        /// 辺を構成する頂点すべてを planePosition.y の降順でソートする
        /// planePositon.y が等しい場合には， planePosition.x の昇順でソートする
        /// </summary>
        /// <returns> ソート結果配列 </returns>
        public NonConvexMonotoneCutSurfaceVertex[] GetAllVertexSortedPlanePositionY() {

            NonConvexMonotoneCutSurfaceVertex[] sortedPlanePositionY = _nodeSequenceList
                .SelectMany(linkedVertex => linkedVertex.Vertices)
                .OrderByDescending(vertex => vertex.PlanePosition.y)
                .ThenBy(vertex => vertex.PlanePosition.x)
                .ToArray();

            return sortedPlanePositionY;
        }

        /// <summary>
        /// this リスト内のすべての連結辺シーケンスに対して，総じた以下の処理を行う
        /// 辺を構成する頂点すべてを planePosition.y の降順でソートする
        /// planePositon.y が等しい場合には， planePosition.x の昇順でソートする
        /// ソート後に、頂点リストと元のインデックスを保持したリストを返す
        /// </summary>
        /// <returns> (ソート順インデックス二次元配列) </returns>
        public (int row, int col)[] GetAllIndexSortedPlanePositionY() {

            // すべての頂点に対して (行, 列, Vertex) を付加する
            var indexedList = _nodeSequenceList
                .SelectMany((linkedVertex, rowIndex) =>
                    linkedVertex.Vertices.Select((vertex, colIndex) =>
                        new {
                            Vertex = vertex,
                            Row = rowIndex,
                            Col = colIndex
                        }
                    )
                )
                .ToList();

            // y 降順でソート (等しい場合は x 昇順)
            var sorted = indexedList
                .OrderByDescending(item => item.Vertex.PlanePosition.y)
                .ThenBy(item => item.Vertex.VertexType)
                .ThenBy(item =>
                    item.Vertex.VertexType == VertexType.Merge
                        ? -item.Vertex.PlanePosition.x
                        : item.Vertex.PlanePosition.x
                )
                .ToList();

            return sorted
                .Select(item => (item.Row, item.Col))
                .ToArray();
        }
    }
}