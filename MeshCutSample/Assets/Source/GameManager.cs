
using UnityEngine;
using Feat1.MeshCut;

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

	[SerializeField, Tooltip("ゲームオブジェクトのプレハブ")]
	private GameObject _objectPrefab;

	// ロジッククラス
	private Cutter cutLine;

    void Start() {
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

        cutLine = new Cutter(Camera.main, visualizer, line, this.planeDistance);

        cutLine.OnCutReady += TryCut;
    }

    void Update() {
        if (cutLine != null) {
            cutLine.Tick();
        }
    }

	/// <summary>
	/// 切断処理のコールメソッドへ繋ぐ前のデリゲートメソッド
	/// </summary>
	/// <param name="cuttingPlane"> 切断平面 </param>
	/// <param name="raycastHit"> RaycastHit情報 </param>
	/// <remarks>
	/// 切断平面とRaycastHit情報を受け取り、切断可能なオブジェクトに対して切断処理を行う
	/// </remarks>
	private void TryCut(Plane cuttingPlane, RaycastHit raycastHit) {

        GameObject target = raycastHit.collider.gameObject;
        ObjectStatus targetStatus = target.GetComponent<ObjectStatus>();
        Mesh targetMesh = target.GetComponent<MeshFilter>().mesh;

        if (targetMesh != null && targetStatus.IsCuttable()) {

            try {
				ExecuteCutProcess(
                    cuttingPlane, 
					target,
					targetStatus,
					targetMesh
                );
            }
            catch (System.Exception e) {
                Debug.Log("catch: " + e.Message);
            }
        }
        else {
            return;
        }
    }

	/// <summary>
	/// 切断機能を呼ぶコールメソッド
	/// </summary>
	/// <param name="cuttingPlane"> 切断平面 </param>
	/// <param name="targetObject"> 切断対象のオブジェクト </param>
	/// <param name="targetStatus"> 切断対象のステータス (ObjectStatus) </param>
	/// <param name="targetMesh"> 切断対象のメッシュ </param>
	private void ExecuteCutProcess(
        Plane cuttingPlane,
		GameObject targetObject,
		ObjectStatus targetStatus,
		Mesh targetMesh
	) {

		Transform targetTransform = targetObject.transform;
		Material[] newMaterial;
		Material[] baseMaterials = targetObject.GetComponent<MeshRenderer>().sharedMaterials;

		newMaterial = GanerateMaterial(
			baseMaterials,
			targetStatus.CutSarfaceMaterial,
			out bool hasCutSurfaceMaterial
		);

		(Mesh rightMesh, Mesh leftMesh) = MeshCut.Cut(
			targetMesh,
			targetTransform,
			cuttingPlane,
			hasCutSurfaceMaterial
		);

		if (rightMesh == null || leftMesh == null) {
			Debug.Log("メッシュの計算ができませんでした。");
			return;
		}

		Destroy(targetObject);

		// 切断された後のオブジェクトを生成する
		if (rightMesh != null) {
			CreateCutObject(
				true,
				cuttingPlane,
				targetStatus,
				targetTransform,
				rightMesh,
				newMaterial
			);
		}
		if (leftMesh != null) {
			CreateCutObject(
				false,
				cuttingPlane,
				targetStatus,
				targetTransform,
				leftMesh,
				newMaterial
			);
		}
	}

	/// <summary>
	/// 新しいマテリアル配列を生成するメソッド
	/// このメソッドは切断面のマテリアルを追加するために使用される
	/// </summary>
	/// <param name="baseMaterials"> 切断前のオブジェクトのマテリアル配列 </param>
	/// <param name="cutSurfaceMaterial"> 切断面に割り当てるマテリアル </param>
	/// <returns> 新マテリアル配列 </returns>
	private Material[] GanerateMaterial(Material[] baseMaterials, Material cutSurfaceMaterial, out bool hasCutSurfaceMaterial) {

		hasCutSurfaceMaterial = false;
		Material[] newMaterials;

        // 一度 CallCut() で切断し、新規マテリアル割り当て済みのオブジェクトにも対応できるようにする
        if (cutSurfaceMaterial != null)
            if (baseMaterials[baseMaterials.Length - 1].name != cutSurfaceMaterial.name)
                hasCutSurfaceMaterial = true;

        if (hasCutSurfaceMaterial) {

            newMaterials = new Material[baseMaterials.Length + 1];
			baseMaterials.CopyTo(newMaterials, 0);
			newMaterials[newMaterials.Length - 1] = cutSurfaceMaterial;
		}
		else {
			newMaterials = baseMaterials;
		}

		return newMaterials;
	}

	/// <summary>
	/// 切断された後のオブジェクトを生成する
	/// </summary>
	/// <param name="sideOfCuttingPlaneNormal"> 法線側: 1, 反法線側: 0</param>
	/// <param name="prevStatus"> 切断前のオブジェクトのステータス (ObjectStatus) </param>
	/// <param name="prevTransform"> 元オブジェクトの Transform </param>
	/// <param name="newMesh"> 作成したメッシュ </param>
	/// <param name="newMaterials"> 割り当てるマテリアル </param>
	private void CreateCutObject(
		bool sideOfCuttingPlaneNormal,
		Plane cuttingPlane,
		ObjectStatus prevStatus,
		Transform prevTransform,
		Mesh newMesh, 
		Material[] newMaterials
	) {

		GameObject newObject = _objectPrefab;
		Vector3 newPosition = prevTransform.position;
		Vector3 offset = cuttingPlane.normal * prevStatus.CutOffset;

		if (sideOfCuttingPlaneNormal)
			newPosition += offset;
		else
			newPosition -= offset;

		newObject.transform.localScale = prevTransform.localScale;
		newObject.GetComponent<MeshFilter>().mesh = newMesh;
		newObject.GetComponent<MeshRenderer>().sharedMaterials = newMaterials;

		MeshCollider meshCollider = newObject.GetComponent<MeshCollider>();

		if (meshCollider) {
			meshCollider.sharedMesh = newMesh;
		}

		ObjectStatus newObjectStatus = newObject.GetComponent<ObjectStatus>();

		if (newObjectStatus != null) {
			newObjectStatus.TakeOverStatus(prevStatus);
		}

		newObject.GetComponent<ObjectStatus>().DecrementCutableLimit();

		Instantiate(newObject, newPosition, prevTransform.rotation, null);
	}
}
