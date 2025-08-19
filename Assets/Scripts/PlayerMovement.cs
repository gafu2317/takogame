using UnityEngine;

// Rigidbody2D を必ず持たせることで物理挙動を簡単に扱えるようにする
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;      // 左右移動速度
    public float jumpForce = 10f;     // ジャンプ力

    [Header("ワイヤー設定")]
    public float wireSpeed = 10f;     // ワイヤー巻き取り速度
    public float maxWireDistance = 15f; // ワイヤー射程
    public LineRenderer wireLine;     // ワイヤー描画用
    public Material solidMaterial;    // 射出可能時の実線マテリアル
    public Material dashedMaterial;   // 射出不可時の点線マテリアル
    public float wireJumpFactor = 0.5f; // ワイヤー方向の慣性をジャンプに反映する割合

    private Rigidbody2D rb;           // Rigidbody2D 参照
    private bool isGrounded;          // 接地判定
    private bool canShoot; // ワイヤー射出可能かどうか

    private bool isWireActive;        // ワイヤー射出中かどうか
    private bool isAttached;          // ワイヤーが目標に接触して固定されているか
    private Vector2 wireTarget;       // ワイヤー射出先の座標

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // LineRenderer の初期設定
        if (!wireLine)
        {
            // wireLine が未設定の場合、自動生成して Player の子にする
            GameObject wireObj = new GameObject("WireLine");
            wireObj.transform.SetParent(transform);
            wireLine = wireObj.AddComponent<LineRenderer>();
        }

        if (wireLine)
        {
            wireLine.positionCount = 2;         // 線は常に2点
            wireLine.startWidth = 0.1f;         // 線の始点幅
            wireLine.endWidth = 0.1f;           // 線の終点幅
            wireLine.material = dashedMaterial;  // 初期マテリアル
            wireLine.sortingOrder = 10;         // 描画優先度（前面）
        }
    }

    void Update()
    {
        HandleMovement();       // 左右移動処理
        HandleJump();           // ジャンプ処理
        UpdateWirePrediction(); // ワイヤー予測線更新
        HandleWireShoot();      // ワイヤー射出＆巻取り
    }

    // --- 左右移動処理 ---
    private void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal"); // A/D または ←→
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // --- ジャンプ処理 ---
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isWireActive)
            {
                // ワイヤー射出中はジャンプ方向にワイヤー方向の慣性を加える
                Vector2 wireDir = (wireTarget - (Vector2)transform.position).normalized;
                Vector2 jumpDir = (Vector2.up + wireDir * wireJumpFactor).normalized;

                rb.linearVelocity = jumpDir * jumpForce;

                // ワイヤーキャンセル
                isWireActive = false;
                isAttached = false;
            }
            else if (isGrounded)
            {
                // 地上ジャンプ
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    // --- ワイヤー予測線更新 ---
    // --- ワイヤー予測線更新 ---
    private void UpdateWirePrediction()
    {
        // マウス座標をワールド座標に変換
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f; // 2D 平面に固定

        // --- 変更前 ---
        // Vector2 direction = mouseWorld - transform.position;

        // // Raycast で clickable タグを持つオブジェクトに当たるかチェック
        // RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, maxWireDistance);
        // canShoot = false;
        // RaycastHit2D validHit = new();

        // foreach (RaycastHit2D hit in hits)
        // {
        //     if (hit.collider != null && hit.collider.gameObject != gameObject)
        //     {
        //         canShoot = true;
        //         validHit = hit;
        //         break;
        //     }
        // }

        // Vector2 targetPoint = canShoot ? validHit.point : (Vector2)mouseWorld;

        // --- 変更後 ---
        // クリック位置にあるオブジェクトを取得
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);

        canShoot = false;
        Vector2 targetPoint = mouseWorld; // デフォルトはマウス位置

        // Player以外のオブジェクトが射程内にある場合のみ有効
        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            float dist = Vector2.Distance(transform.position, hit.point);
            if (dist <= maxWireDistance)
            {
                canShoot = true;
                targetPoint = hit.point;
            }
        }

        // LineRenderer に反映（常に表示）
        if (wireLine)
        {
            wireLine.SetPosition(0, transform.position);
            wireLine.SetPosition(1, isWireActive ? wireTarget : targetPoint);

            // 射出可能時は実線＋緑、不可時は点線＋白
            wireLine.material = canShoot ? solidMaterial : dashedMaterial;
            wireLine.startColor = canShoot ? Color.green : Color.white;
            wireLine.endColor = canShoot ? Color.green : Color.white;
        }
    }

    // --- ワイヤー射出＆巻取り処理 ---
    private void HandleWireShoot()
    {
        // 左クリック && ワイヤー未使用時
        if (Input.GetMouseButtonDown(0) && !isWireActive)
        {
            // マウス座標をワールドに変換
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;


            // --- 変更後 ---
            // クリック位置にあるコライダーを取得
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);

            // ヒットした && Player自身ではない
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                float dist = Vector2.Distance(transform.position, hit.point);

                // 射程内チェック
                if (dist <= maxWireDistance)
                {
                    wireTarget = hit.point;
                    isWireActive = true;
                    isAttached = false;
                }
            }
        }

        // 自動巻取り処理
        if (isWireActive && !isAttached)
        {
            Vector2 dir = (wireTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * wireSpeed;

            // 目標に近づいたら固定
            if (Vector2.Distance(transform.position, wireTarget) < 0.2f)
            {
                isAttached = true;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }


    // --- 接地判定 ---(自動呼出し)
    private int groundContacts = 0; // 接触している地面の数

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContacts++;   // 地面に触れたらカウント増
            isGrounded = true;  // 接地フラグON
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContacts--;               // 地面から離れたらカウント減
            isGrounded = groundContacts > 0; // 接地判定を更新
        }
    }

}