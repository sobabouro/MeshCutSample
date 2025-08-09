
using UnityEngine;

/// <summary>
/// マウスのドラッグ操作を検知し、ゲーム内のオブジェクトを切断するための仮想的な平面を生成します。
/// このクラスは、マウスで画面上に描かれた線を3D空間の切断面として扱い、
/// その切断面の情報を他のコンポーネント（例：MeshCutter）に提供する役割を担います。
/// </summary>
public class CutLine {
    // 外部から渡される設定とコンポーネント
    private Camera mainCamera;
    private PlaneVisualizer planeVisualizer;
    private LineRenderer lineRenderer;
    private float planeDistance;

    private Vector2 mouseStartPos;
    private bool isDragging = false;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="camera"> メインカメラのインスタンス </param>
	/// <param name="visualizer"> 切断平面を視覚的に表示するためのコンポーネント </param>
	/// <param name="line"> マウスのドラッグ軌跡を描画するための LineRenderer コンポーネント </param>
	/// <param name="distance"> カメラから平面までの距離 </param>
	public CutLine(Camera camera, PlaneVisualizer visualizer, LineRenderer line, float distance)
    {
        mainCamera = camera;
        planeVisualizer = visualizer;
        lineRenderer = line;
        planeDistance = distance;
    }

	/// <summary>
	/// マウスの左ボタン操作を監視し、ドラッグイベントに応じて処理を実行します。
	/// ドラッグの開始、更新、終了を検知し、切断平面の計算と可視化を行います。
	/// このメソッドは、外部のUpdateループから毎フレーム呼び出されることを想定しています。
	/// </summary>
	public void Tick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            mouseStartPos = Input.mousePosition;

            lineRenderer.enabled = true;
            Vector3 startWorldPos = mainCamera.ScreenPointToRay(mouseStartPos).GetPoint(planeDistance);
            lineRenderer.SetPosition(0, startWorldPos);
            lineRenderer.SetPosition(1, startWorldPos);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 currentWorldPos = mainCamera.ScreenPointToRay(Input.mousePosition).GetPoint(planeDistance);
            lineRenderer.SetPosition(1, currentWorldPos);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            lineRenderer.enabled = false;

            Vector2 mouseEndPos = Input.mousePosition;

            if (Vector2.Distance(mouseStartPos, mouseEndPos) < 10f)
            {
                planeVisualizer.ClearPlane();
                return;
            }

            Ray startRay = mainCamera.ScreenPointToRay(mouseStartPos);
            Ray endRay = mainCamera.ScreenPointToRay(mouseEndPos);
            Vector3 p1 = startRay.GetPoint(planeDistance);
            Vector3 p2 = endRay.GetPoint(planeDistance);
            Plane cuttingPlane = new Plane(p1, p2, p1 + mainCamera.transform.forward);

            Debug.Log($"Plane Created: Normal = {cuttingPlane.normal}, Distance = {cuttingPlane.distance}");

            Vector3 centerPoint = (p1 + p2) / 2f;
            planeVisualizer.DrawPlane(cuttingPlane, centerPoint);
        }
    }
}
