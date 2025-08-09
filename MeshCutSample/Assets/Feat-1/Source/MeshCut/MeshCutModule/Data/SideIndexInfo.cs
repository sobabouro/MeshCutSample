using System.Collections.Generic;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 新頂点情報・新ポリゴン情報の生成用の一時データ
    /// </summary>
    public readonly struct SideIndexInfo {

        /// <summary>
        /// 法線側に辺を所持しているかどうか
        /// </summary>
        public readonly bool IsFrontSideEdge;

        /// <summary>
        /// 法線側の (辺 | 頂点) 始点頂点 (切断前メッシュのインデックス)
        /// </summary>
        public readonly int FrontTowardIndex;

        /// <summary>
        /// 法線側の (辺 | 頂点) 終点頂点 (切断前メッシュのインデックス)
        /// </summary>
        public readonly int FrontAwayIndex;

        /// <summary>
        /// 反法線側の (辺 | 頂点) 始点頂点 (切断前メッシュのインデックス)
        /// </summary>
        public readonly int BackTowardIndex;

        /// <summary>
        /// 反法線側の (辺 | 頂点) 終点頂点 (切断前メッシュのインデックス)
        /// </summary>
        public readonly int BackAwayIndex;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="frontIndices"> 法線側の頂点集合 (ポリゴンを形成する頂点が右回りになる向き) </param>
        /// <param name="backIndices"> 反法線側の頂点集合 (ポリゴンを形成する頂点が右回りになる向き) </param>
        public SideIndexInfo(
            IList<int> frontIndices,
            IList<int> backIndices
        ) {
            List<int> frontList = new(frontIndices);
            List<int> backList = new(backIndices);
            if (frontList.Count == 1) {
                FrontTowardIndex = FrontAwayIndex = frontList[0];
                BackTowardIndex = backList[0];
                BackAwayIndex = backList[1];
                IsFrontSideEdge = false;
            }
            else {
                FrontTowardIndex = frontList[0];
                FrontAwayIndex = frontList[1];
                BackTowardIndex = BackAwayIndex = backList[0];
                IsFrontSideEdge = true;
            }
        }
    }
}