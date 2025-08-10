using UnityEngine;

/// <summary>
/// �I�u�W�F�N�g�̏�Ԃ��Ǘ�����N���X
/// </summary>
public class ObjectStatus : MonoBehaviour {

	[SerializeField, Tooltip("�Q�[���A�N�V�����ΏۃI�u�W�F�N�g")]
	private bool _isGamingObject = true;

	[SerializeField, Tooltip("�ؒf�\�I�u�W�F�N�g")]
	private bool _isCuttable = true;

	[SerializeField, Tooltip("�ؒf�ʂɊ��蓖�Ă�}�e���A�� (�C��)")]
	public Material CutSarfaceMaterial;

	[SerializeField, Range(0.00f, 0.50f), Tooltip("�ؒf��ɂ��炷���� (m)")]
	public float CutOffset = 0.1f;

	/// <summary>
	/// �ؒf�\�񐔂̐���
	/// </summary>
	private int _statusOfCutableLimit = 5;


	/// <summary>
	/// �X�e�[�^�X�������p�����\�b�h
	/// </summary>
	/// <param name="status"> ���p�������X�e�[�^�X </param>
	public void TakeOverStatus(ObjectStatus status) {
		_isGamingObject = status._isGamingObject;
		_isCuttable = status._isCuttable;
		CutSarfaceMaterial = status.CutSarfaceMaterial;
		CutOffset = status.CutOffset;
		_statusOfCutableLimit = status._statusOfCutableLimit;
	}

	/// <summary>
	/// �ؒf�\���ǂ������m�F���郁�\�b�h
	/// </summary>
	/// <returns> �ؒf�\�ł���� ture, �����łȂ���� false </returns>
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
	/// �ؒf�\��Ԃ��X�V���郁�\�b�h
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
