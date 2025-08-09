namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// スイープラインアルゴリズムによる境界探索時の、最大地点から最小地点までを結ぶ二つの境界のうち、どちら側に位置するかを表す列挙型
    /// y 単調な図形に分割後に、境界探索を行う際に使用される
    /// </summary>
    public enum SideType {

        /// <summary>
        /// 図形の y 最大地点
        /// </summary>
        Top,

        /// <summary>
        /// 図形の y 最小地点
        /// </summary>
        Bottom,

        /// <summary>
        /// 図形の y 最大地点と最小地点までを結ぶ二つの境界のうち右側に位置する場合
        /// </summary>
        Right,

        /// <summary>
        /// 図形の y 最大地点と最小地点までを結ぶ二つの境界のうち左側に位置する場合
        /// </summary>
        Left
    }
}