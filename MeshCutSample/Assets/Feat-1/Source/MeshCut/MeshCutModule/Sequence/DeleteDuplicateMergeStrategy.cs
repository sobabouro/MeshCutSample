using System;
using System.Collections.Generic;

namespace Feat1.MeshCut.MeshCutModule {

	/// <summary>
	/// 重複は削除 (無視) し、他の連結要素をマージする戦略
	/// </summary>
	/// <typeparam name="T"> ジェネリック型 (制約: class, IEquatable) </typeparam>
	public class DeleteDuplicateMergeStrategy<T> : INodeSequenceMergeStrategy<T>
        where T : class, IEquatable<T> {

        /// <summary>
        /// 連結要素シーケンスの後ろに、他の連結要素をマージする
        /// </summary>
        /// <param name="currentLinkedList"> マージされる側の現在のシーケンスの連結要素 (LinkedList) </param>
        /// <param name="otherItemsEnumerable"> マージする側の他のシーケンスの要素（IEnumerable）</param>
        /// <param name="otherFirstNode"> マージする側の他のシーケンスの先頭ノード </param>
        /// <param name="otherLastNode"> マージする側の他のシーケンスの末尾ノード </param>
        public void MergeAfterStrategy(LinkedList<T> currentLinkedList, IEnumerable<T> otherItemsEnumerable, LinkedListNode<T>? otherFirstNode, LinkedListNode<T>? otherLastNode) {

            if (otherItemsEnumerable == null) {
                throw new ArgumentNullException(nameof(otherItemsEnumerable), "Other items enumerable cannot be null.");
            }

            var node = otherFirstNode;
            if (currentLinkedList.Last != null && node != null && currentLinkedList.Last.Value.Equals(node.Value)) {
                node = node.Next;
            }

            while (node != null) {
                currentLinkedList.AddLast(node.Value);
                node = node.Next;
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

            if (otherItemsEnumerable == null) {
                throw new ArgumentNullException(nameof(otherItemsEnumerable), "Other items enumerable cannot be null.");
            }

            var node = otherLastNode;
            if (currentLinkedList.First != null && node != null && currentLinkedList.First.Value.Equals(node.Value)) {
                node = node.Previous;
            }

            while (node != null) {
                currentLinkedList.AddFirst(node.Value);
                node = node.Previous;
            }
        }

    }
}