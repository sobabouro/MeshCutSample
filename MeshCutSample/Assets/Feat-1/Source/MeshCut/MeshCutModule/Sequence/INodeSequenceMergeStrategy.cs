using System.Collections.Generic;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 連結された要素の管理クラスの、マージメソッドのストラテジーインターフェース
    /// </summary>
    /// <typeparam name="T"> NodeSequence が管理する要素の型 (制約: class) </typeparam>
    public interface INodeSequenceMergeStrategy<T>
        where T : class {

        /// <summary>
        /// 連結要素シーケンスの後ろに、他の連結要素をマージする
        /// </summary>
        /// <param name="currentLinkedList"> マージされる側の現在のシーケンスの連結要素 (LinkedList) </param>
        /// <param name="otherItemsEnumerable"> マージする側の他のシーケンスの要素（IEnumerable）</param>
        /// <param name="otherFirstNode"> マージする側の他のシーケンスの先頭ノード </param>
        /// <param name="otherLastNode"> マージする側の他のシーケンスの末尾ノード </param>
        void MergeAfterStrategy(LinkedList<T> currentLinkedList, IEnumerable<T> otherItemsEnumerable, LinkedListNode<T>? otherFirstNode, LinkedListNode<T>? otherLastNode);

        /// <summary>
        /// 連結要素シーケンスの前に、他の連結要素をマージする
        /// </summary>
        /// <param name="currentLinkedList"> マージされる側の現在のシーケンスの連結要素 (LinkedList) </param>
        /// <param name="otherItemsEnumerable"> マージする側の他のシーケンスの要素（IEnumerable）</param>
        /// <param name="otherFirstNode"> マージする側の他のシーケンスの先頭ノード </param>
        /// <param name="otherLastNode"> マージする側の他のシーケンスの末尾ノード </param>
        void MergeBeforeStrategy(LinkedList<T> currentLinkedList, IEnumerable<T> otherItemsEnumerable, LinkedListNode<T>? otherFirstNode, LinkedListNode<T>? otherLastNode);
    }
}