using UnityEngine;

public class WireController : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;          // 射出位置（Player の子など）
    public LineRenderer previewLine;     // 予測線表示用

    [Header("Materials")]
    public Material solidMaterial;       // 実線
    public Material dashedMaterial;      // 点線（破線テクスチャ必要）

    [Header("Settings")]
    public float maxDistance = 15f;      // 射程
    public LayerMask rayMask = ~0;       // Raycast 対象レイヤー（Everything でOK）
    public string clickableTag = "clickable";

    // 実行時情報
    public bool CanShoot { get; private set; }
    public Vector2 HitPoint { get; private set; }

    void Reset()
    {
        // 初期設定（アタッチ時の保険）
        if (!previewLine) previewLine = GetComponent<LineRenderer>();
        if (previewLine)
        {
            previewLine.positionCount = 2;
            previewLine.alignment = LineAlignment.View;
            previewLine.textureMode = LineTextureMode.Tile; // 点線用に Tile を使う
        }
    }

    void Update()
    {
        if (!firePoint || !previewLine || Camera.main == null)
            return;

        // マウス → ワールド座標
        Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 origin = firePoint.position;
        Vector2 dir = ((Vector2)m - origin);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up; // 安全策
        dir.Normalize();

        // クリック方向に Raycast
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxDistance, rayMask);

        Vector2 endPoint;
        if (hit.collider != null)
        {
            endPoint = hit.point;
            CanShoot = hit.collider.CompareTag(clickableTag);
        }
        else
        {
            endPoint = origin + dir * maxDistance;
            CanShoot = false;
        }

        // 予測線の見た目（実線/点線）切り替え
        if (solidMaterial && dashedMaterial)
        {
            previewLine.material = CanShoot ? solidMaterial : dashedMaterial;
        }

        // 予測線を更新
        previewLine.positionCount = 2;
        previewLine.SetPosition(0, origin);
        previewLine.SetPosition(1, endPoint);

        // クリックで射出（clickable に当たっている時のみ）
        if (Input.GetMouseButtonDown(0) && CanShoot)
        {
            HitPoint = endPoint;
            ShootWire(HitPoint);
        }
    }

    void ShootWire(Vector2 target)
    {
        // 射出確定時の処理
        Debug.Log($"[WireController] Shoot to {target}");
        // TODO: 実ワイヤー生成・Joint 接続 等の本実装をここに。
        // 例）あなたの既存ワイヤーシステムに target を渡して開始する。
    }
}
