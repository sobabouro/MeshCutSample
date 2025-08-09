using System.Collections.Generic;
using UnityEngine;
using CalculationUtils;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 法線方向ごとにポリゴンの情報を保持するクラス
    /// </summary>
    public class EquivalentNormalPolygonBuffer {

        /// <summary>
        /// ローカル座標系の切断平面
        /// </summary>
        private Plane _localPlane;

        /// <summary>
        /// 法線の向きが同じ連結ポリゴン情報を管理するための辞書
        /// </summary>
        private Dictionary<int, LinkedPolygonList> _polygonDictionary = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="localPlane"> ローカル座標系の切断平面 </param>
        public EquivalentNormalPolygonBuffer(Plane localPlane) {
            _localPlane = localPlane;
        }

        /// <summary>
        /// 同じ法線のポリゴンを、マージ判定リストに追加するメソッド
        /// </summary>
        /// <param name="normal"> ポリゴンの法線ベクトル (正規化) </param>
        /// <param name="polygon"> ポリゴン情報 </param>
        public void Add(Vector3 normal, NewPolygon polygon) {
            int castedNormal = Calculation.NormalizedVector3ToInt(normal);

            // 登録されている法線の場合は、同じ法線の連結ポリゴンリストに追加する
            if (_polygonDictionary.TryGetValue(castedNormal, out LinkedPolygonList linkedPolygonList)) {
                linkedPolygonList.Add(polygon);
            }
            // 登録されていない法線の場合は、新規登録する
            else {
                _polygonDictionary[castedNormal] = new LinkedPolygonList();
                _polygonDictionary[castedNormal].Add(polygon);
            }
        }

        /// <summary>
        /// リスト内の連結ポリゴンに対して、ポリゴン生成を行うメソッド
        /// </summary>
        /// <param name="trackerArray"> 切断後の配属頂点インデックスを格納する配列 </param>
        /// <param name="originMesh"> 切断前メッシュ (MeshContainer) </param>
        /// <param name="frontsideMesh"> 切断後法線側メッシュ (MeshContainer) </param>
        /// <param name="backsideMesh"> 切断後反法線側メッシュ (MeshContainer) </param>
        public void MakeBaseSurfacePolygon(
            int[] trackerArray,
            MeshContainer originMesh,
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh,
            CutSurfacePolygonBuffer cutSurfacePolygonBuffer
        ) {
            foreach (var pair in _polygonDictionary) {
                int normal = pair.Key;

                LinkedPolygonList linkedPolygonList = pair.Value;
                linkedPolygonList.MakePolygon(
                    _localPlane,
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