
using UnityEngine;

/// <summary>
/// プレイヤーによって描画された線から平面を生成するクラス
/// </summary>
public class PlaneVisualizer : MonoBehaviour
{
    [Tooltip("ギズモとして描画する平面のサイズ")]
    public float gizmoSize = 5.0f;

    private Plane plane;
    private Vector3 planeCenter;
    private bool shouldDraw = false;

    /// <summary>
    /// 描画する平面と、その中心点を設定するメソッド
    /// </summary>
    public void DrawPlane(Plane plane, Vector3 center)
    {
        this.plane = plane;
        this.planeCenter = center;
        this.shouldDraw = true;
    }

    /// <summary>
    /// 平面の描画を削除するメソッド
    /// </summary>
    public void ClearPlane()
    {
        this.shouldDraw = false;
    }

    // Sceneビューでギズモを描画するために呼び出されます
    private void OnDrawGizmos()
    {
        if (!shouldDraw) return;

        // 平面の法線から、平面上の基底ベクトル(right, up)を計算
        Quaternion rotation = Quaternion.LookRotation(plane.normal);
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;

        // 4つの角の点を計算
        Vector3 p1 = planeCenter + right * gizmoSize + up * gizmoSize;
        Vector3 p2 = planeCenter + right * gizmoSize - up * gizmoSize;
        Vector3 p3 = planeCenter - right * gizmoSize - up * gizmoSize;
        Vector3 p4 = planeCenter - right * gizmoSize + up * gizmoSize;

        // ギズモの色を設定
        Gizmos.color = Color.cyan;

        // 4つの線を描画して四角形を表現
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);

        // 法線ベクトルを可視化
        Gizmos.color = Color.red;
        Gizmos.DrawRay(planeCenter, plane.normal * (gizmoSize / 2));
    }
}
