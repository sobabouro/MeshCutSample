namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 頂点の種類を表す列挙型
    /// 定義順で，優先順位を表す
    /// </summary>
    public enum VertexType {

        /// <summary>
        /// 出発点
        /// 頂点 v が出発点となるのは、その隣接頂点がいずれも下にあり、 v の内角が 180 度より小さい場合
        /// </summary>
        Start,

        /// <summary>
        /// 分離点
        /// 頂点 v が分離点となるのは、その隣接頂点がいずれも下にあり、 v の内角が 180 度より大きい場合
        /// </summary>
        Split,

        /// <summary>
        /// 通常点
        /// 以下の頂点に分類されない、何の変哲もない点
        /// </summary>
        Regular,

        /// <summary>
        /// 統合点
        /// 頂点 v が統合点となるのは、その隣接頂点がいずれも上にあり、 v の内角が 180 度より大きい場合
        /// </summary>
        Merge,

        /// <summary>
        /// 最終点
        /// 頂点 v が最終点となるのは、その隣接頂点がいずれも上にあり、 v の内角が 180 度より小さい場合
        /// </summary>
        End
    }
}