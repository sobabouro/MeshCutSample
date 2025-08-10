using UnityEngine;

/// <summary>
/// オブジェクトの状態を管理するクラス
/// </summary>
public class ObjectStatus : MonoBehaviour {

	[SerializeField, Tooltip("ゲームアクション対象オブジェクト")]
	private bool _isGamingObject = true;

	[SerializeField, Tooltip("切断可能オブジェクト")]
	private bool _isCuttable = true;

	[SerializeField, Tooltip("切断面に割り当てるマテリアル (任意)")]
	public Material CutSarfaceMaterial;

	[SerializeField, Range(0.00f, 0.50f), Tooltip("切断後にずらす距離 (m)")]
	public float CutOffset = 0.1f;

	/// <summary>
	/// 切断可能回数の制限
	/// </summary>
	private int _statusOfCutableLimit = 5;

	/// <summary>
	/// 切断可能かどうかを確認するメソッド
	/// </summary>
	/// <returns> 切断可能であれば ture, そうでなければ false </returns>
	public bool IsCuttable() {
		if (_isGamingObject && _isCuttable) {
			return true;
		}
		else {
			Debug.Log("This object is not cuttable or not a gaming object.");
			return false;
		}
	}

	/// <summary>
	/// 切断可能状態を更新するメソッド
	/// </summary>
	public void UpdateCutStatus() {
		if (!_isCuttable) {
			return;
		}
		else {
			_statusOfCutableLimit--;
			if (_statusOfCutableLimit <= 0) {
				_isCuttable = false;
				Debug.Log("This object is no longer cuttable.");
			}
			else {
				Debug.Log($"Remaining cuts: {_statusOfCutableLimit}");
			}
		}
	}
}
