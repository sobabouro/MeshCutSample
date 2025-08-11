using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 切断平面上の y 単調な多角形のパスを管理するクラス
    /// </summary>
    public class MonotoneGeometryPath : IEnumerable<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> {

        /// <summary>
        /// (始点, 直前の頂点) ペアのパス
        /// </summary>
        private LinkedList<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> _path = new();

        /// <summary>
        /// パスの要素数を取得するプロパティ
        /// </summary>
        public int Count => _path.Count;

        /// <summary>
        /// パスのイテレータを返すメソッド
        /// </summary>
        /// <returns> シーケンスのリストを列挙するためのイテレータ </returns>
        public IEnumerator<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> GetEnumerator() => _path.GetEnumerator();

        /// <summary>
        /// IEnumerable インターフェースの GetEnumerator メソッドの実装
        /// </summary>
        /// <returns> パスを列挙するためのイテレータ </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// パスの中で最も高い頂点のノード
        /// </summary>
        private LinkedListNode<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> _mostHighestNode = null;

        /// <summary>
        /// パスの中で最も低い頂点のノード
        /// </summary>
        private LinkedListNode<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> _mostLowestNode = null;

        /// <summary>
        /// パスの先頭に頂点を追加するメソッド
        /// </summary>
        /// <param name="pair"> 追加する頂点と辺のペア </param>
        public void AddFirst((NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex) pair) {

            _path.AddFirst(pair);
            UpdateExtremeNodes(pair, _path.First);
        }

        /// <summary>
        /// パスの末尾に頂点を追加するメソッド
        /// </summary>
        /// <param name="pair"> 追加するペア </param>
        public void AddLast((NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex) pair) {

            _path.AddLast(pair);
            UpdateExtremeNodes(pair, _path.Last);
        }

        /// <summary>
        /// 最大最小地点ノードの更新を行うメソッド
        /// </summary>
        /// <param name="pair"> 追加したペア </param>
        /// <param name="newNode"> 追加したペアのノード </param>
        private void UpdateExtremeNodes(
            (NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex) pair,
            LinkedListNode<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> newNode
        ) {
            if (_mostHighestNode == null) {
                _mostHighestNode = newNode;
            }
            else {
                var highestXPosition = _mostHighestNode.Value.Item1.PlanePosition.x;
                var highestYPosition = _mostHighestNode.Value.Item1.PlanePosition.y;
                var newXPosition = pair.Item1.PlanePosition.x;
                var newYPosition = pair.Item1.PlanePosition.y;

                if (newYPosition > highestYPosition || (newYPosition == highestYPosition && newXPosition < highestXPosition)) {
                    _mostHighestNode = newNode;
                }
            }
            if (_mostLowestNode == null) {
                _mostLowestNode = newNode;
            }
            else {
                var lowestXPosition = _mostLowestNode.Value.Item1.PlanePosition.x;
                var lowestYPosition = _mostLowestNode.Value.Item1.PlanePosition.y;
                var newXPosition = pair.Item1.PlanePosition.x;
                var newYPosition = pair.Item1.PlanePosition.y;

                if (newYPosition < lowestYPosition || (newYPosition == lowestYPosition && newXPosition > lowestXPosition)) {
                    _mostLowestNode = newNode;
                }
            }
        }

        /// <summary>
        /// 循環ノードのように各ノードにアクセスするためのメソッド
        /// 次のノードを取得するメソッド
        /// </summary>
        /// <param name="node"> 対象ノード </param>
        /// <returns> 対象ノードの次のノード </returns>
        public LinkedListNode<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> TorusNext(
            LinkedListNode<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> node
        ) {

            if (node == null || _path.Count == 0)
                return null;

            return node.Next ?? _path.First;
        }

        /// <summary>
        /// 循環ノードのように各ノードにアクセスするためのメソッド
        /// 前のノードを取得するメソッド
        /// </summary>
        /// <param name="node"> 対象ノード </param>
        /// <returns> 対象ノードの前のノード </returns>
        public LinkedListNode<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> TorusPrevious(
            LinkedListNode<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> node
        ) {

            if (node == null || _path.Count == 0)
                return null;

            return node.Previous ?? _path.Last;
        }

        /// <summary>
        /// パスに指定された頂点が含まれているかを判定する
        /// </summary>
        /// <param name="vertex"> 判定する頂点 </param>
        /// <returns> 含まれていれば true, そうでなければ false を返す </returns>
        public bool Contains(NonConvexMonotoneCutSurfaceVertex vertex) {
            return _path.Any(pair => pair.Item1.Equals(vertex));
        }

        /// <summary>
        /// パスをもとに三角形ポリゴンを生成し，メッシュコンテナに追加するメソッド
        /// </summary>
        /// <param name="publicBoundingBox"> 切断平面上のすべての図形を包括するバウンディングボックス </param>
        /// <param name="localPlane"> 切断平面 </param>
        /// <param name="hasNewCutSurfaceMaterial"> 断面のマテリアルを所持するかどうか </param>
        /// <param name="frontsideMesh"> 法線側の切断後メッシュコンテナ </param>
        /// <param name="backsideMesh"> 反法線側の切断後メッシュコンテナ </param>
        public void MakePolygon(
            BoundingBox publicBoundingBox,
            Plane localPlane,
            bool hasNewCutSurfaceMaterial,
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh
        ) {

			if (_path.Count <= 4) {
                SinpleMakePolygon(publicBoundingBox, localPlane, hasNewCutSurfaceMaterial, frontsideMesh, backsideMesh);
            }
            else {
                MultipleMakePolygon(publicBoundingBox, localPlane, hasNewCutSurfaceMaterial, frontsideMesh, backsideMesh);
            }
        }

        /// <summary>
        /// パスに含まれる頂点数が多いときに単純な三角形分割をしてポリゴンを作成し，メッシュコンテナに追加するメソッド
        /// このメソッドが呼び出される前に，パスの要素数が 5 個以上であることが保証されている必要がある
        /// </summary>
        /// <param name="publicBoundingBox"> 切断平面上のすべての図形を包括するバウンディングボックス </param>
        /// <param name="localPlane"> 切断平面 </param>
        /// <param name="hasNewCutSurfaceMaterial"> 断面のマテリアルを所持するかどうか </param>
        /// <param name="frontsideMesh"> 法線側の切断後メッシュコンテナ </param>
        /// <param name="backsideMesh"> 反法線側の切断後メッシュコンテナ </param>
        private void MultipleMakePolygon(
            BoundingBox publicBoundingBox,
            Plane localPlane,
            bool hasNewCutSurfaceMaterial,
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh
        ) {
            /**
             * # 図形の中で最も y 座標が高い頂点から最も低い頂点まで辿る際の，右側境界の辺群と左側境界の辺群にそれぞれどちらの群 (チェイン) に属するかを設定する 
             * # その後，すべての頂点を y 座標の降順にソートし，スタック (S) を用意し，左側の辺群と右側の辺群を統一した順序 (u[1], u[2], ..., u[n]) で以下の処理を行う
             * 
             * u[1], u[2] をスタックにプッシュする
             * for i <- 3 to n-1
             * - do if u[i] と S の一番上の頂点が異なるチェイン上にある
             * - - then S からすべての頂点をポップする
             * - - - u[i] とポップされたそれぞれの頂点を結ぶ対角線を D に挿入する．ただし，最後の頂点だけは除く
             * - - - u[i-1] と u[i] を S にプッシュする
             * - - else S から一つの頂点をポップする
             * - - - u[i] からの対角線が P の内部にある限り，S から他の頂点をポップする
             * - - - これらの対角線を D に挿入する
             * - - - ポップされた最後の頂点をスタックに戻す
             * - - - u[i] を S にプッシュする
             * 最初と最後の頂点を覗いて，u[n] からスタック上のすべての頂点への対角線を加える
             * 
             * 
             * ※ 以下実装では，対角線をリストに追加するのではなく，ポリゴンを直接生成する
             */

            ClusteringSideType();

            (NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)[] sortedArray = SortVertexYPosition();
            Stack<(NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)> stack = new();

            stack.Push(sortedArray[0]);
            stack.Push(sortedArray[1]);

            for (int i = 2; i < sortedArray.Length; i++) {

                SideType topSideType = stack.Peek().Item1.SideType;

                // スタックの一番上の頂点と現在の頂点が異なる境界に属している場合
                if (topSideType != sortedArray[i].Item1.SideType) {

                    while (stack.Count >= 2) {

                        var pair1 = stack.Pop();
                        var pair2 = stack.Count >= 2 ? stack.Peek() : stack.Pop();

                        CreateTriangle(
                            (pair1.Item1, pair2.Item1, sortedArray[i].Item1),
                            publicBoundingBox,
                            localPlane,
                            frontsideMesh,
                            backsideMesh,
                            hasNewCutSurfaceMaterial
                        );
                    }
                    stack.Push(sortedArray[i - 1]);

                    stack.Push(sortedArray[i]);
                }
                // スタックの一番上の頂点と現在の頂点が同じ境界に属している場合
                else {

                    bool isProcessPermission = false;
                    (NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex) prevLastPair = default;

                    while (stack.Count >= 2) {

                        var pair1 = prevLastPair = stack.Pop();
                        var pair2 = stack.Pop();

                        isProcessPermission = topSideType == SideType.Left
                            ? Calculation.IsClockwise((pair1.Item1.PlanePosition, pair2.Item1.PlanePosition, sortedArray[i].Item1.PlanePosition))
                            : !Calculation.IsClockwise((pair1.Item1.PlanePosition, pair2.Item1.PlanePosition, sortedArray[i].Item1.PlanePosition));

                        if (!isProcessPermission) {

                            stack.Push(pair2);

                            break;
                        }

                        CreateTriangle(
                            (pair1.Item1, pair2.Item1, sortedArray[i].Item1),
                            publicBoundingBox,
                            localPlane,
                            frontsideMesh,
                            backsideMesh,
                            hasNewCutSurfaceMaterial
                        );
                        stack.Push(pair2);
                    }

                    if (isProcessPermission) {
                        stack.Push(sortedArray[i]);
                    }
                    else {
                        stack.Push(prevLastPair);
                        stack.Push(sortedArray[i]);
                    }
                }
            }
        }

        /// <summary>
        /// パスに含まれる頂点数が少ないときに単純な三角形分割をしてポリゴンを作成し，メッシュコンテナに追加するメソッド
        /// このメソッドが呼び出される前に，パスの要素数が 4 個以下であることが保証されている必要がある
        /// </summary>
        /// <param name="publicBoundingBox"> 切断平面上のすべての図形を包括するバウンディングボックス </param>
        /// <param name="localPlane"> 切断平面 </param>
        /// <param name="hasNewCutSurfaceMaterial"> 断面のマテリアルを所持するかどうか </param>
        /// <param name="frontsideMesh"> 法線側の切断後メッシュコンテナ </param>
        /// <param name="backsideMesh"> 反法線側の切断後メッシュコンテナ </param>
        /// <remarks>
        /// このメソッドはパスに含まれる要素数が [3, 4] 個のときに呼び出される </br>
        /// 水平線を二本所持し，かつ 4 頂点しか持たないパスでは，パス内のソート時に例外が必要となるので，その特別処理も兼ねる
        /// </remarks>
        private void SinpleMakePolygon(
            BoundingBox publicBoundingBox,
            Plane localPlane,
            bool hasNewCutSurfaceMaterial,
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh
        ) {

            if (_path.Count < 3) {

                Debug.LogWarning("MonotoneGeometryPath: SinpleMakePolygon() - path element count is less than 3, cannot create polygon.");
                return;
            }
            if (_path.Count == 3) {

                var firstVertex = _path.First.Value.Item1;
                var secondVertex = _path.First.Next.Value.Item1;
                var thirdVertex = _path.Last.Value.Item1;

                CreateTriangle(
                    (firstVertex, secondVertex, thirdVertex),
                    publicBoundingBox,
                    localPlane,
                    frontsideMesh,
                    backsideMesh,
                    hasNewCutSurfaceMaterial
                );
                return;
            }
            if (_path.Count == 4) {

				var firstVertex = _path.First.Value.Item1;
                var secondVertex = _path.First.Next.Value.Item1;
                var thirdVertex = _path.Last.Previous.Value.Item1;
                var fourthVertex = _path.Last.Value.Item1;

                CreateTriangle(
                    (firstVertex, secondVertex, thirdVertex),
                    publicBoundingBox,
                    localPlane,
                    frontsideMesh,
                    backsideMesh,
                    hasNewCutSurfaceMaterial
                );
                CreateTriangle(
                    (firstVertex, thirdVertex, fourthVertex),
                    publicBoundingBox,
                    localPlane,
                    frontsideMesh,
                    backsideMesh,
                    hasNewCutSurfaceMaterial
                );
                return;
            }
        }

        /// <summary>
        /// 連結図形辺シーケンスの各頂点に対して，その頂点が属する図形の y 最大地点と最小地点までを結ぶ二つの境界のうち，どちら側に位置するかを設定するメソッド
        /// 辺が図形を反時計回りで進む向きで格納されていることが前提である
        /// </summary>
        private void ClusteringSideType() {

            var currNode = _mostHighestNode;

            while (currNode != _mostLowestNode) {
                currNode = TorusNext(currNode);
                currNode.Value.Item1.SideType = SideType.Left;
            }

            currNode = _mostLowestNode;

            while (currNode != _mostHighestNode) {
                currNode.Value.Item1.SideType = SideType.Right;
                currNode = TorusNext(currNode);
            }

            _mostHighestNode.Value.Item1.SideType = SideType.Top;
            _mostLowestNode.Value.Item1.SideType = SideType.Bottom;
        }

        /// <summary>
        /// 連結図形辺シーケンスの頂点を Y 座標の降順にソートするメソッド
        /// Y 座標が等しい場合は，直前の要素の頂点と接続する方を優先してソートする
        /// </summary>
        /// <returns> ソートされた頂点配列 </returns>
        /// <remarks>
        /// Y 座標がすべて異なれば，スイープラインアルゴリズムにおいては単純に降順でソートすればよいが、 </br>
        /// Y 座標が等しいものが混在する場合，一意にソートするためには、接続性に基づいて並び替える必要がある． </br>
        /// </remarks>
        private (NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex)[] SortVertexYPosition() {

            var sortedList = _path
            .OrderByDescending(pair => pair.Item1.PlanePosition.y)
            .ThenBy(pair => pair.Item1.PlanePosition.x)
            .ToList();

            // 2. y座標が同じ頂点群を特定し、接続性に基づいて並び替える
            for (int i = 1; i < sortedList.Count; i++) {

                // 現在の要素のy座標が、前の要素のy座標と等しい場合
                if (Mathf.Abs(sortedList[i].Item1.PlanePosition.y - sortedList[i - 1].Item1.PlanePosition.y) < float.Epsilon) {

                    // y座標が同じ頂点群（サブリスト）の開始インデックスを見つける
                    int startIndex = i - 1;
                    while (startIndex > 0 && Mathf.Abs(sortedList[startIndex - 1].Item1.PlanePosition.y - sortedList[i].Item1.PlanePosition.y) < float.Epsilon) {
                        startIndex--;
                    }

                    // サブリストを切り出す
                    int count = i - startIndex + 1;
                    var sublist = sortedList.GetRange(startIndex, count);

                    var prevElement = startIndex > 0 ? sortedList[startIndex - 1] : default;

                    // 直前頂点と辺の右端が接続する場合，逆順にする
                    if (prevElement.Item2 == sublist.Last().Item1 || prevElement.Item1 == sublist.Last().Item2) {
                        sortedList.Reverse(startIndex, count);
                    }

                    // カウンタを調整して、次の y 座標の異なる要素から再開する
                    i = startIndex + count - 1;
                }
            }

            return sortedList.ToArray();
        }

        /// <summary>
        /// 三角形を生成するメソッド
        /// </summary>
        /// <param name="triangle"> 三角形を構成する三頂点 </param>
        /// <param name="boundingBox"> UV 座標決定のための外枠 </param>
        /// <param name="localPlane"> 切断平面 </param>
        /// <param name="frontsideMesh"> 法線側メッシュ </param>
        /// <param name="backsideMesh"> 反法線側メッシュ </param>
        /// <param name="hasNewCutSurfaceMaterial"></param>
        private void CreateTriangle(
            (NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex, NonConvexMonotoneCutSurfaceVertex) triangle,
            BoundingBox boundingBox,
            Plane localPlane,
            MeshContainer frontsideMesh,
            MeshContainer backsideMesh,
            bool hasNewCutSurfaceMaterial = false
        ) {
            Vector3 triangleNormal = Vector3.Cross(
                triangle.Item2.LocalPosition - triangle.Item1.LocalPosition,
                triangle.Item3.LocalPosition - triangle.Item1.LocalPosition
            ).normalized;

            Vector3 vertex1 = triangle.Item1.LocalPosition;
            Vector3 vertex2 = triangle.Item2.LocalPosition;
            Vector3 vertex3 = triangle.Item3.LocalPosition;

            Vector2 uv1 = new(
                (triangle.Item1.PlanePosition.x - boundingBox.MinX) / boundingBox.Width,
                (triangle.Item1.PlanePosition.y - boundingBox.MinY) / boundingBox.Height
            );
            Vector2 uv2 = new(
                (triangle.Item2.PlanePosition.x - boundingBox.MinX) / boundingBox.Width,
                (triangle.Item2.PlanePosition.y - boundingBox.MinY) / boundingBox.Height
            );
            Vector2 uv3 = new(
                (triangle.Item3.PlanePosition.x - boundingBox.MinX) / boundingBox.Width,
                (triangle.Item3.PlanePosition.y - boundingBox.MinY) / boundingBox.Height
            );

            int materialIndex = frontsideMesh.SubmeshCount - 1;

			frontsideMesh.AddMesh(
                materialIndex,
                localPlane.normal * -1,
                new Vector3[] { vertex1, vertex2, vertex3 },
                new Vector3[] { localPlane.normal, localPlane.normal, localPlane.normal },
                new Vector2[] { uv1, uv2, uv3 }
            );
            backsideMesh.AddMesh(
                materialIndex,
                localPlane.normal,
                new Vector3[] { vertex1, vertex3, vertex2 },
                new Vector3[] { localPlane.normal, localPlane.normal, localPlane.normal },
                new Vector2[] { uv1, uv3, uv2 }
            );
        }
    }
}