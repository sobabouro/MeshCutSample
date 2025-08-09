using UnityEngine;

/// <summary>
/// �J�����R���g���[���[
/// </summary>
public class CameraController : MonoBehaviour {

	[SerializeField, Range(0.0f, 50.0f), Tooltip("�J�����ړ����x�i�P��: m/s�j")]
	private float _moveSpeed = 10.0f;
	[SerializeField, Range(0.0f, 10.0f), Tooltip("�J�������x�i�P��: �x/�b�j")]
	private float _rotationSpeed = 2.0f;

	private float _yaw = 0.0f;
	private float _pitch = 0.0f;

	void Start() {
		Vector3 rotation = transform.eulerAngles;
		_yaw = rotation.y;
		_pitch = rotation.x;
	}

	void Update() {
		HandleMovement();
		HandleRotation();
	}

	/// <summary>
	/// �J�����̈ړ�����
	/// </summary>
	private void HandleMovement() {
		// �����͂̎擾 A, D
		float horizontalInput = Input.GetAxis("Horizontal");
		// �����͂̎擾 W, S
		float verticalInput = Input.GetAxis("Vertical");
		float upDownInput = 0.0f;

		// Q�L�[�ŏ㏸�AE�L�[�ŉ��~
		if (Input.GetKey(KeyCode.Q)) {
			upDownInput = 1.0f;
		}
		else if (Input.GetKey(KeyCode.E)) {
			upDownInput = -1.0f;
		}

		Vector3 movement = new Vector3(horizontalInput, upDownInput, verticalInput);
		movement = transform.TransformDirection(movement);

		transform.position += movement * _moveSpeed * Time.deltaTime;
	}

	/// <summary>
	/// �J�����̉�]����
	/// </summary>
	private void HandleRotation() {

		if (Input.GetMouseButton(1)) {
			_yaw += _rotationSpeed * Input.GetAxis("Mouse X");
			_pitch -= _rotationSpeed * Input.GetAxis("Mouse Y");
			_pitch = Mathf.Clamp(_pitch, -90f, 90f);

			transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);
		}
	}
}