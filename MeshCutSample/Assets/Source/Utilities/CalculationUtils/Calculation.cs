using System;
using UnityEngine;

/// <summary>
/// 計算に関するユーティリティクラス
/// </summary>
public class Calculation {


    /// <summary>
    /// 対象の頂点 2 (current) を中心とした，二辺の成す角が鋭角か鈍角かを判定するメソッド
    /// </summary>
    /// <param name="prevVector"> 連続する頂点 1 </param>
    /// <param name="currVector"> 連続する頂点 2 </param>
    /// <param name="nextVector"> 連続する頂点 3 </param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"> 連続する三頂点が平行の時 </exception>
    public static bool IsAcuteOrObtuseAngle(Vector2 prevVector, Vector2 currVector, Vector2 nextVector) {
        Vector2 v1 = (currVector - prevVector).normalized;
        Vector2 v2 = (nextVector - currVector).normalized;

        float dot = Vector2.Dot(v1, v2);

        // 鋭角
        if (dot > 0)
            return true;
        // 鈍角
        if (dot < 0)
            return false;
        // 平行の場合は例外を投げる
        throw new InvalidOperationException("The angle is exactly 90 degrees.");
    }

    /// <summary>
    /// 頂点1, 頂点2, 頂点3 の順に頂点が時計回りに並んでいるかを判定するメソッド
    /// 辺21 と 辺23 の外積 < 0 の場合は時計周り
    /// </summary>
    /// <param name="vectors"> 連続する三頂点 </param>
    /// <returns> 時計回りであれば true, そうでなければ false </returns>
    public static bool IsClockwise(
        (Vector2, Vector2, Vector2) vectors
    ) {
        Vector2 v1 = vectors.Item1 - vectors.Item2;
        Vector2 v2 = vectors.Item3 - vectors.Item2;

        return (v1.x * v2.y - v1.y * v2.x) > 0;
    }

    /// <summary>
    /// 法線ベクトルを int 型に圧縮するメソッド
    /// 0.0 ~ 1.0 の範囲のベクトルをスケーリングして、上位 22bit を制限し、(x, y, z)10bit ずつの情報に圧縮する
    /// およそ各小数点以下第二位までの精度となる (第三位まで許容したければ、(long) にして 14bit(16384) シフトして対応する上位ビットを制限する)
    /// </summary>
    /// <param name="vector"> 法線ベクトル </param>
    /// <returns> int 型へ圧縮した法線 </returns>
    public static int NormalizedVector3ToInt(Vector3 vector) {
        // 0 ~ 1023 の範囲に制限する (下位 10bit のみが対象)
        int filter = 0x000003FF;
        int amp = 1 << 10;

        int x = ((int)(vector.x * amp) & filter) << 20;
        int y = ((int)(vector.y * amp) & filter) << 10;
        int z = ((int)(vector.z * amp) & filter);

        return x | y | z;
    }
}
