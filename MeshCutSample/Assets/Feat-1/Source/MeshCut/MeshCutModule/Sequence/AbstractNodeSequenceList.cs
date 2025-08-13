using System;
using System.Collections;
using System.Collections.Generic;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 連結した要素を管理するシーケンスを、リストで管理する抽象基底クラス
    /// </summary>
    /// <typeparam name="TNodeSequence"> シーケンスの型 </typeparam>
    /// <typeparam name="TItem"> シーケンスが管理するオブジェクトの型 </typeparam>
    public abstract class AbstractNodeSequenceList<TNodeSequence, TItem> : IEnumerable<TNodeSequence>
        where TNodeSequence : AbstractNodeSequence<TItem>, new()
        where TItem : class {

        /// <summary>
        /// インデクサー
        /// </summary>
        /// <param name="index"> インデックス番号 </param>
        /// <returns> 対応する要素 </returns>
        public AbstractNodeSequence<TItem> this[int index] {
            get {
                if (index < 0 || index >= _nodeSequenceList.Count) {
                    throw new IndexOutOfRangeException("Index is out of range.");
                }
                return _nodeSequenceList[index];
            }
        }

        /// <summary>
        /// 連結した要素のシーケンスを保持するリスト
        /// </summary>
        protected readonly List<TNodeSequence> _nodeSequenceList = new();

        /// <summary>
        /// シーケンスのリストの要素数を取得するプロパティ
        /// </summary>
        public int Count => _nodeSequenceList.Count;

        /// <summary>
        /// シーケンスのリストを列挙するためのイテレータを返すメソッド
        /// </summary>
        /// <returns> シーケンスのリストを列挙するためのイテレータ </returns>
        public IEnumerator<TNodeSequence> GetEnumerator() => _nodeSequenceList.GetEnumerator();

        /// <summary>
        /// IEnumerable インターフェースの GetEnumerator メソッドの実装
        /// </summary>
        /// <returns> シーケンスのリストを列挙するためのイテレータ </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// リストの各シーケンスに対して、新しい連結要素の追加判定を行い、適切なシーケンスに追加を行うメソッド
        /// </summary>
        /// <param name="args"> 追加する要素 </param>
        public abstract void Add(params object[] args);

        /// <summary>
        /// 連結要素シーケンスのリスト内のすべてのシーケンスに対して，最後の要素を削除するメソッド
        /// </summary>
        public void DeleteLastElement() {

            foreach (var linkedList in _nodeSequenceList) {
                linkedList.DeleteLastElement();
            }
        }

		/// <summary>
		/// シーケンスの値と比較キーを使って、マージ可能かどうかを判定する抽象メソッド
		/// </summary>
		/// <param name="target"> あるシーケンスの先頭または末尾の連結要素 </param>
		/// <param name="key"> 比較キー (Add() によって追加された新要素) </param>
		/// <param name="isAfter"> 前後のどちらに対してマージを試みるかを示すフラグ </param>
		/// <returns> true ならマージ可能、false ならマージ不可 </returns>
		protected abstract bool CheckMerge(TItem target, TItem key, bool isAfter);

		/// <summary>
		/// リスト中の指定されたインデックスのシーケンスに対して、後ろにマージできるシーケンスを探してマージを試みるメソッド
		/// </summary>
		/// <param name="index"> リストのインデックス </param>
		/// <param name="key"> 比較キー </param>
		protected void TryMergeAfter(int index, TItem key) {
            for (int j = _nodeSequenceList.Count - 1; j > index; j--) {
                if (CheckMerge(_nodeSequenceList[j].First?.Value, key, true)) {
                    _nodeSequenceList[index].MergeAfter(_nodeSequenceList[j]);
                    _nodeSequenceList.RemoveAt(j);
                    return;
                }
            }
        }

        /// <summary>
        /// リスト中の指定されたインデックスのシーケンスに対して、前にマージできるシーケンスを探してマージを試みるメソッド
        /// </summary>
        /// <param name="index"> リストのインデックス </param>
        /// <param name="key"> 比較キー </param>
        protected void TryMergeBefore(int index, TItem key) {
            for (int j = _nodeSequenceList.Count - 1; j > index; j--) {
                if (CheckMerge(_nodeSequenceList[j].Last?.Value, key, false)) {
                    _nodeSequenceList[index].MergeBefore(_nodeSequenceList[j]);
                    _nodeSequenceList.RemoveAt(j);
                    return;
                }
            }
        }
    }
}