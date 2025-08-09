using System.Collections.Generic;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 重複を削除せず、全ての要素をマージする戦略
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AllMergeStrategy<T> : INodeSequenceMergeStrategy<T>
        where T : class {

        /// <summary>
        /// 連結要素シーケンスの後ろに、他の連結要素をマージする
        /// </summary>
        /// <param name="currentLinkedList"> マージされる側の現在のシーケンスの連結要素 (LinkedList) </param>
        /// <param name="otherItemsEnumerable"> マージする側の他のシーケンスの要素（IEnumerable）</param>
        /// <param name="otherFirstNode"> マージする側の他のシーケンスの先頭ノード </param>
        /// <param name="otherLastNode"> マージする側の他のシーケンスの末尾ノード </param>
        public void MergeAfterStrategy(LinkedList<T> currentLinkedList, IEnumerable<T> otherItemsEnumerable, LinkedListNode<T>? otherFirstNode, LinkedListNode<T>? otherLastNode) {
            // 連結要素の後ろに、他の連結要素をマージする
            foreach (var otherItem in otherItemsEnumerable) {
                currentLinkedList.AddLast(otherItem);
            }
        }

        /// <summary>
        /// 連結要素シーケンスの後ろに、他の連結要素をマージする
        /// </summary>
        /// <param name="currentLinkedList"> マージされる側の現在のシーケンスの連結要素 (LinkedList) </param>
        /// <param name="otherItemsEnumerable"> マージする側の他のシーケンスの要素（IEnumerable）</param>
        /// <param name="otherFirstNode"> マージする側の他のシーケンスの先頭ノード </param>
        /// <param name="otherLastNode"> マージする側の他のシーケンスの末尾ノード </param>
        public void MergeBeforeStrategy(LinkedList<T> currentLinkedList, IEnumerable<T> otherItemsEnumerable, LinkedListNode<T>? otherFirstNode, LinkedListNode<T>? otherLastNode) {
            // 連結要素の前に、他の連結要素をマージする
            var node = otherLastNode;
            while (node != null) {
                currentLinkedList.AddFirst(node.Value);
                node = node.Previous;
            }
        }
    }
}