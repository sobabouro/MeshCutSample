using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 連結ポリゴンシーケンス (LinkedPolygon) のリストを管理するクラス
    /// </summary>
    public class LinkedPolygonList : AbstractNodeSequenceList<LinkedPolygon, NewPolygon> {

        /// <summary>
        /// 連結ポリゴンシーケンスのリストに対して、所属すべきシーケンスに新ポリゴンを追加するメソッド
        /// </summary>
        /// <param name="args"> 追加するポリゴン情報を含む引数 </param>
        public override void Add(params object[] args) {
            if (args.Length != 1 || !(args[0] is NewPolygon polygon)) {
                return;
            }
            for (int i = 0; i < _nodeSequenceList.Count; i++) {
                var current = _nodeSequenceList[i];

                if (current.TryAppend(polygon)) {
                    TryMergeAfter(i, polygon);
                    return;
                }
                if (current.TryPrepend(polygon)) {
                    TryMergeBefore(i, polygon);
                    return;
                }
            }
            // どの連結ポリゴンにも追加できなかった場合は新しい連結ポリゴンを作成する
            LinkedPolygon newLinked = new();
            newLinked.TryAppend(polygon);
            _nodeSequenceList.Add(newLinked);
        }

        /// <summary>
        /// シーケンス同士のマージ可能性を判定するメソッド
        /// </summary>
        /// <param name="target"> あるシーケンスの先頭または末尾の連結ポリゴンの要素 </param>
        /// <param name="key"> Add() によって追加されたポリゴン </param>
        /// <param name="isAfter"> 前後のどちらに対してマージを試みるかを示すフラグ </param>
        /// <returns></returns>
        protected override bool CheckMerge(NewPolygon target, NewPolygon key, bool isAfter) {
            if (target == null)
                return false;
            return isAfter ? target.NewEdgeToward.Domein == key.NewEdgeAway.Domein : target.NewEdgeAway.Domein == key.NewEdgeToward.Domein;
        }

        /// <summary>
        /// 連結ポリゴンに対して、ポリゴン生成を行うメソッド
        /// </summary>
        /// <param name="localPlane"> 切断平面 </param>
        /// <param name="trackerArray"> 切断後の配属頂点インデックスを格納する配列 </param>
        /// <param name="originMesh"> 切断前メッシュ (MeshContainer) </param>
        /// <param name="frontsideMesh"> 切断後法線側メッシュ (MeshContainer) </param>
        /// <param name="backsideMesh"> 切断後反法線側メッシュ (MeshContainer) </param>
        public void MakePolygon(
            Plane localPlane,
            int[] trackerArray,
            MeshContainer originMesh,
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh,
            CutSurfacePolygonBuffer cutSurfacePolygonBuffer
        ) {
            foreach (var linkedPolygon in _nodeSequenceList) {
                if (linkedPolygon.First == null || linkedPolygon.Last == null)
                    continue;
                linkedPolygon.MakePolygon(
                    localPlane,
                    trackerArray,
                    originMesh,
                    frontsideMesh,
                    backsideMesh,
                    cutSurfacePolygonBuffer
                );
            }
        }
    }
}