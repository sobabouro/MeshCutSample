namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 分割されたポリゴン情報を管理するクラス (三角形)
    /// </summary>
    public class FragmentTrianglePolygon : NewPolygon {

        /// <summary>
        /// 切断辺の向かい側の頂点へのプロパティ
        /// </summary>
        public readonly int Point;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="submeshIndex"> サブメッシュのグループ番号 (マテリアル番号) </param>
        /// <param name="newEdgeStart"> 切断辺を形成する新頂点の始点 </param>
        /// <param name="newEdgeEnd"> 切断辺を形成する新頂点の始点 </param>
        /// <param name="point"> 切断辺の向かい側の頂点のインデックス (切断前メッシュ頂点リストでのインデックス) </param>
        public FragmentTrianglePolygon(
            int submeshIndex,
            NewVertex newEdgeStart,
            NewVertex newEdgeEnd,
            int point
        ) : base(
            submeshIndex,
            newEdgeStart,
            newEdgeEnd
        ) {
            Point = point;
        }
    }
}