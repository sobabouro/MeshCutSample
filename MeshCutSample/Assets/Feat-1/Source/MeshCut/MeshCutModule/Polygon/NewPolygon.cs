namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 新ポリゴン情報を管理するためのクラス
    /// 切断平面の法線方向を "A", ポリゴンの法線方向を "B", 切断辺 (NewVertex, NewVertex) の方向を "C" とする
    /// このとき、A, B, C のベクトルが、フレミングの左手の法則に従う向きになるように仮置きし、
    /// C は front, back によって向きが可変のベクトルなので、とくに深い意味はなく、A,B,C の 3 ベクトル空間上での始点を "toward", 終点を "away" と仮置きしている
    /// </summary>
    public class NewPolygon {

        /// <summary>
        /// サブメッシュのインデックス番号
        /// </summary>
        public readonly int SubmeshIndex;

        /// <summary>
        /// 切断によってできた辺の始点 (ポリゴンを形成する頂点が右回りになる向き)
        /// </summary>
        public readonly NewVertex TowardVertex;

        /// <summary>
        /// 切断によってできた辺の終点 (ポリゴンを形成する頂点が右回りになる向き)
        /// </summary>
        public readonly NewVertex AwayVertex;

        /// <summary>
        /// 新ポリゴン情報を管理するためのコンストラクタ
        /// </summary>
        /// <param name="submeshIndex"> サブメッシュのグループ番号 (マテリアル番号) </param>
        /// <param name="newVertexTowardPosition"> 切断辺を形成する新頂点の + 点 </param>
        /// <param name="newVertexAwayPosition"> 切断辺を形成する新頂点の - 点 </param>
        public NewPolygon(
            int submeshIndex,
            NewVertex newVertexTowardPosition,
            NewVertex newVertexAwayPosition
        ) {
            SubmeshIndex = submeshIndex;
            TowardVertex = newVertexTowardPosition;
            AwayVertex = newVertexAwayPosition;
        }
    }
}