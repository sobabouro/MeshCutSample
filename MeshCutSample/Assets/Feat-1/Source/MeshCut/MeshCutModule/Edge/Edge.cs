using System;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// 辺に関する情報を保持するクラス
    /// </summary>
    /// <typeparam name="T"> AbstractCutSurfaceVertex の継承型 </typeparam>
    public class Edge<T> : IEquatable<Edge<T>>
        where T : AbstractCutSurfaceVertex {

        /// <summary>
        /// 浮動小数点数の誤差吸収用
        /// </summary>
        protected const float Epsilon = 0.0001f;

        /// <summary>
        /// 辺の始点
        /// </summary>
        public readonly T Start;

        /// <summary>
        /// 辺の終点
        /// </summary>
        public readonly T End;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="start"> 辺の始点 </param>
        /// <param name="end"> 辺の終点 </param>
        public Edge(T start, T end) {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Equals メソッドのオーバーライド
        /// インスタンスが同じ Edge<T> 型であり、始点と終点が同じかどうかを比較する
        /// </summary>
        /// <param name="obj"> object 型 </param>
        /// <returns> 等しければ true, そうでなければ false </returns>
        public override bool Equals(object? obj) => obj is Edge<T> other && Equals(other);

        /// <summary>
        /// Equals メソッドのオーバーライド
        /// 辺の始点と終点が同じかどうかを比較する
        /// </summary>
        /// <param name="other"> Edge<T> 型 </param>
        /// <returns> 等しければ true, そうでなければ false </returns>
        public bool Equals(Edge<T> other) => Start.Equals(other.Start) && End.Equals(other.End);

        /// <summary>
        /// GetHashCode メソッドのオーバーライド
        /// start, end のハッシュコードを組み合わせて一意のハッシュコードを生成する
        /// </summary>
        /// <returns> ハッシュコード </returns>
        public override int GetHashCode() => HashCode.Combine(Start, End);
    }
}