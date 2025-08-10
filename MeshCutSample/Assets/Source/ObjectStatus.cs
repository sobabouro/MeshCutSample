using UnityEngine;

/// <summary>
/// オブジェクトの状態を管理するクラス
/// </summary>
public class ObjectStatus : MonoBehaviour {

	[SerializeField, Tooltip("ゲームアクション対象オブジェクト")]
	private bool _isGamingObject = true;

	[SerializeField, Tooltip("切断可能オブジェクト")]
	private bool _isCuttable = true;

	/// <summary>
	/// 切断可能回数の制限
	/// </summary>
	private int _statusOfCutableLimit = 5;

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
