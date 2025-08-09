namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 三角形ポリゴン情報を管理するためのクラス
    /// </summary>
    public class TrianglePolygon : NewPolygon {

        /// <summary>
        /// 法線側に辺を持っているかどうか
        /// </summary>
        public readonly bool IsFrontsideEdge;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="submeshIndex"> サブメッシュのグループ番号 (マテリアル番号) </param>
        /// <param name="newEdgePositive"> 切断辺を形成する新頂点の + 点 </param>
        /// <param name="newEdgeNegative"> 切断辺を形成する新頂点の - 点 </param>
        /// <param name="isFrontsideEdge"> 法線側に辺を持っているかどうか </param>
        public TrianglePolygon(
            int submeshIndex,
            NewVertex newEdgePositive,
            NewVertex newEdgeNegative,
            bool isFrontsideEdge
        ) : base(
            submeshIndex,
            newEdgePositive,
            newEdgeNegative
        ) {
            IsFrontsideEdge = isFrontsideEdge;
        }
    }
}