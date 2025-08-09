namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// バウンディングボックスを表すクラス
    /// </summary>
    public class BoundingBox {

        /// <summary>
        /// バウンディングボックスの最小X座標
        /// </summary>
        public float MinX;

        /// <summary>
        /// バウンディングボックスの最小Y座標
        /// </summary>
        public float MinY;

        /// <summary>
        /// バウンディングボックスの最大X座標
        /// </summary>
        public float MaxX;

        /// <summary>
        ///　バウンディングボックスの最大Y座標
        /// </summary>
        public float MaxY;

        /// <summary>
        /// バウンディングボックスの幅
        /// </summary>
        public float Width => MaxX - MinX;

        /// <summary>
        /// バウンディングボックスの高さ
        /// </summary>
        public float Height => MaxY - MinY;

        /// <summary>
        /// バウンディングボックスのコンストラクタ
        /// </summary>
        public BoundingBox() {
            Initialize();
        }

        /// <summary>
        /// バウンディングボックスの初期化を行うメソッド
        /// </summary>
        public void Initialize() {
            MinX = float.MaxValue;
            MinY = float.MaxValue;
            MaxX = float.MinValue;
            MaxY = float.MinValue;
        }

        /// <summary>
        /// バウンディングボックスの更新を行うメソッド
        /// </summary>
        /// <param name="x"> 更新する x 座標 </param>
        /// <param name="y"> 更新する y 座標 </param>
        /// <returns> 更新されれば true, そうでなければ false </returns>
        public bool TryUpdate(float x, float y) {
            if (x < MinX) {
                MinX = x;
                return true;
            }
            else if (x > MaxX) {
                MaxX = x;
                return true;
            }
            else if (y < MinY) {
                MinY = y;
                return true;
            }
            else if (y > MaxY) {
                MaxY = y;
                return true;
            }
            return false;
        }
    }
}