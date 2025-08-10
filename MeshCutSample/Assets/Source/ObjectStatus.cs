using UnityEngine;

/// <summary>
/// オブジェクトの状態を管理するクラス
/// </summary>
public class ObjectStatus {

    [SerializeField, Tooltip("切断可能オブジェクト")]
	private bool _isCuttable = false;
}
