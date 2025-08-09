namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 分割されたポリゴン情報を管理するクラス (四角形)
    /// </summary>
    public class FragmentQuadranglePolygon : NewPolygon {

        /// <summary>
        /// 切断辺の向かい側の辺へのプロパティ
        /// </summary>
        public readonly (int, int) Edge;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="submeshIndex"> サブメッシュのグループ番号 (マテリアル番号) </param>
        /// <param name="newEdgeStart"> 切断辺を形成する新頂点の始点 </param>
        /// <param name="newEdgeEnd"> 切断辺を形成する新頂点の始点 </param>
        /// <param name="edge"> 切断辺の向かい側の辺の始点と終点のタプル (切断前メッシュ頂点リストでのインデックス) </param>
        public FragmentQuadranglePolygon(
            int submeshIndex,
            NewVertex newEdgeStart,
            NewVertex newEdgeEnd,
            (int, int) edge
        ) : base(
            submeshIndex,
            newEdgeStart,
            newEdgeEnd
        ) {
            Edge = edge;
        }
    }
}