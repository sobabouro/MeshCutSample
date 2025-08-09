
using UnityEngine;

/// <summary>
/// ゲームシーンを管理するマネージャー
/// </summary>
public class GameManager : MonoBehaviour {
    [Header("Cut System Settings")]
    [Tooltip("カメラから平面を生成する位置までの距離")]
    public float planeDistance = 10f;
    [Tooltip("描画する線の太さ")]
    public float lineWidth = 0.05f;
    [Tooltip("描画する線の色")]
    public Color lineColor = Color.black;

    // ロジッククラス
    private Cutter cutLine;

    void Start()
    {
        // --- 依存コンポーネントの準備 ---
        GameObject cutSystemObject = new GameObject("CutSystem");
        cutSystemObject.transform.SetParent(this.transform, true);

        // 1. PlaneVisualizer (MonoBehaviour) をGameObjectに追加
        PlaneVisualizer visualizer = cutSystemObject.AddComponent<PlaneVisualizer>();

        // 2. LineRenderer (MonoBehaviour) をGameObjectに追加して設定
        LineRenderer line = cutSystemObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        line.startColor = this.lineColor;
        line.endColor = this.lineColor;
        line.startWidth = this.lineWidth;
        line.endWidth = this.lineWidth;
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.enabled = false;

        // --- ロジッククラスのインスタンス化 ---
        // 3. Cutter (POCO) を new で生成し、依存性を渡す
        cutLine = new Cutter(Camera.main, visualizer, line, this.planeDistance);
        cutLine.OnCutReady += CallCut;
	}

    void Update()
    {
        if (cutLine != null)
        {
            cutLine.Tick();
        }
    }

    private void CallCut(Plane cuttingPlane, RaycastHit raycastHit) {
		
        Transform targetTransform = raycastHit.transform;
        Mesh targetMesh = raycastHit.collider.GetComponent<MeshFilter>().mesh;
        if (targetMesh != null) {
            try {
				// 切断処理を実行
			}
			catch (System.Exception e) {
				Debug.Log("catch: " + e.Message);
			}
		}
        else {
            return;
        }
    }
}
