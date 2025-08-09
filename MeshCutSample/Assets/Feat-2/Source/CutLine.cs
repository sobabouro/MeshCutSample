using UnityEngine;

[RequireComponent(typeof(PlaneVisualizer))]
public class CutLine : MonoBehaviour
{
    [Tooltip("カメラから平面を生成する位置までの距離")]
    public float planeDistance = 10f;

    [Header("Line Settings")]
    [Tooltip("描画する線の太さ")]
    public float lineWidth = 0.05f;
    [Tooltip("描画する線の色")]
    public Color lineColor = Color.black;

    private Camera mainCamera;
    private Vector2 mouseStartPos;
    private PlaneVisualizer planeVisualizer;
    private LineRenderer lineRenderer;

    void Start()
    {
        mainCamera = Camera.main;
        planeVisualizer = GetComponent<PlaneVisualizer>();
        SetupLineRenderer();
    }

    /// <summary>
    /// Line Rendererコンポーネントを初期化し、設定します。
    /// </summary>
    void SetupLineRenderer()
    {
        GameObject lineObj = new GameObject("CuttingLine");
        lineObj.transform.SetParent(transform, true);
        lineRenderer = lineObj.AddComponent<LineRenderer>();

        // シェーダーが見つからない場合のエラーを防ぐため、Unity標準のシェーダーを使う
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        
        // 色と太さを設定
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // 頂点数は2 (始点と終点)
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        // 初期状態では非表示
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // マウスの左ボタンが押された瞬間
        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPos = Input.mousePosition;

            // Line Rendererを有効化し、始点を設定
            lineRenderer.enabled = true;
            Vector3 startWorldPos = mainCamera.ScreenPointToRay(mouseStartPos).GetPoint(planeDistance);
            lineRenderer.SetPosition(0, startWorldPos);
            lineRenderer.SetPosition(1, startWorldPos);
        }
        // マウスの左ボタンが押されている間
        else if (Input.GetMouseButton(0))
        {
            // Line Rendererの終点を現在のマウス位置に更新
            Vector3 currentWorldPos = mainCamera.ScreenPointToRay(Input.mousePosition).GetPoint(planeDistance);
            lineRenderer.SetPosition(1, currentWorldPos);
        }
        // マウスの左ボタンが離された瞬間
        else if (Input.GetMouseButtonUp(0))
        {
            // Line Rendererを無効化して非表示に
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