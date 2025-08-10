using UnityEngine;

/// <summary>
/// �I�u�W�F�N�g�̏�Ԃ��Ǘ�����N���X
/// </summary>
public class ObjectStatus : MonoBehaviour {

	[SerializeField, Tooltip("�Q�[���A�N�V�����ΏۃI�u�W�F�N�g")]
	private bool _isGamingObject = true;

	[SerializeField, Tooltip("�ؒf�\�I�u�W�F�N�g")]
	private bool _isCuttable = true;

	/// <summary>
	/// �ؒf�\�񐔂̐���
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
