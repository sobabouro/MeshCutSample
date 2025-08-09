using System;
using System.Collections;
using System.Collections.Generic;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 連結した要素の管理を行う抽象基底クラス
    /// </summary>
    /// <typeparam name="T"> ノードとして管理するオブジェクト </typeparam>
    public abstract class AbstractNodeSequence<T> : IEnumerable<T>
        where T : class {

        private INodeSequenceMergeStrategy<T> _mergeStrategy;

        /// <summary>
        /// 連結リストを保持する双方向リスト
        /// 派生クラスからはアクセス可
        /// </summary>
        protected readonly LinkedList<T> _nodeSequence = new();

        /// <summary>
        /// 連結シーケンスの先頭の要素を取得するプロパティ
        /// </summary>
        public LinkedListNode<T>? First => _nodeSequence.First;

        /// <summary>
        /// 連結シーケンスの末尾の要素を取得するプロパティ
        /// </summary>
        public LinkedListNode<T>? Last => _nodeSequence.Last;

        /// <summary>
        /// 連結要素の数を取得するプロパティ
        /// </summary>
        public int Count => _nodeSequence.Count;

        /// <summary>
        /// シーケンスのリストを列挙するためのイテレータを返すメソッド
        /// </summary>
        /// <returns> シーケンスのリストを列挙するためのイテレータ </returns>
        public IEnumerator<T> GetEnumerator() => _nodeSequence.GetEnumerator();

        /// <summary>
        /// IEnumerable インターフェースの GetEnumerator メソッドの実装
        /// </summary>
        /// <returns> シーケンスのリストを列挙するためのイテレータ </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// 連結要素のシーケンスのコレクションを取得するプロパティ
        /// </summary>
        protected IEnumerable<T> GetItemsEnumerable() => _nodeSequence;

        /// <summary>
        /// 連結要素のマージ戦略プロパティ
        /// </summary>
        public INodeSequenceMergeStrategy<T> MergeStrategy {
            get => _mergeStrategy;
            set => _mergeStrategy = value
                ?? throw new ArgumentNullException(nameof(value), "Merge strategy cannot be null.");
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mergeStrategy"> 連結要素のマージ戦略 </param>
        protected AbstractNodeSequence(INodeSequenceMergeStrategy<T> mergeStrategy) {
            _mergeStrategy = mergeStrategy;
        }

        /// <summary>
        /// 循環ノードのように各ノードにアクセスするためのメソッド
        /// 次のノードを取得するメソッド
        /// </summary>
        /// <param name="node"> 対象ノード </param>
        /// <returns> 対象ノードの次のノード </returns>
        public LinkedListNode<T> TorusNext(LinkedListNode<T> node) {

            if (node == null || _nodeSequence.Count == 0)
                return null;

            return node.Next ?? _nodeSequence.First;
        }

        /// <summary>
        /// 循環ノードのように各ノードにアクセスするためのメソッド
        /// 前のノードを取得するメソッド
        /// </summary>
        /// <param name="node"> 対象ノード </param>
        /// <returns> 対象ノードの前のノード </returns>
        public LinkedListNode<T> TorusPrevious(LinkedListNode<T> node) {

            if (node == null || _nodeSequence.Count == 0)
                return null;

            return node.Previous ?? _nodeSequence.Last;
        }

        /// <summary>
        /// 連結要素の後ろに要素を追加できるか試みる (抽象メソッド)
        /// </summary>
        public abstract bool TryAppend(params object[] args);

        /// <summary>
        /// 連結要素の前に要素を追加できるか試みる (抽象メソッド)
        /// </summary>
        public abstract bool TryPrepend(params object[] args);

        /// <summary>
        /// この連結要素の後ろに他の連結要素をマージする
        /// </summary>
        /// <param name="other">マージする他の要素</param>
        public void MergeAfter(AbstractNodeSequence<T> other) {
            if (_mergeStrategy == null) {
                throw new InvalidOperationException("Merge strategy is not set.");
            }
            _mergeStrategy.MergeAfterStrategy(_nodeSequence, other.GetItemsEnumerable(), other.First, other.Last);
        }

        /// <summary>
        /// この連結要素の前に他の連結要素をマージする
        /// </summary>
        /// <param name="other">マージする他の要素</param>
        public void MergeBefore(AbstractNodeSequence<T> other) {
            if (_mergeStrategy == null) {
                throw new InvalidOperationException("Merge strategy is not set.");
            }
            _mergeStrategy.MergeBeforeStrategy(_nodeSequence, other.GetItemsEnumerable(), other.First, other.Last);
        }

        /// <summary>
        /// 連結要素のシーケンスから指定されたノードを削除するメソッド
        /// </summary>
        /// <param name="nodeToDelete"> 削除するノード </param>
        public void RemoveNode(LinkedListNode<T> nodeToDelete) {
            _nodeSequence.Remove(nodeToDelete);
        }

        /// <summary>
        /// シーケンスの最後の要素を削除するメソッド
        /// </summary>
        public void DeleteLastElement() {
            _nodeSequence.Remove(_nodeSequence.Last);
        }
    }
}