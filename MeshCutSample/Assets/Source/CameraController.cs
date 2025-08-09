using UnityEngine;

/// <summary>
/// カメラコントローラー
/// </summary>
public class CameraController : MonoBehaviour {

	[SerializeField, Range(0.0f, 50.0f), Tooltip("カメラ移動速度（単位: m/s）")]
	private float _moveSpeed = 10.0f;
	[SerializeField, Range(0.0f, 10.0f), Tooltip("カメラ感度（単位: 度/秒）")]
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
	/// カメラの移動処理
	/// </summary>
	private void HandleMovement() {
		// 軸入力の取得 A, D
		float horizontalInput = Input.GetAxis("Horizontal");
		// 軸入力の取得 W, S
		float verticalInput = Input.GetAxis("Vertical");
		float upDownInput = 0.0f;

		// Qキーで上昇、Eキーで下降
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
	/// カメラの回転処理
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