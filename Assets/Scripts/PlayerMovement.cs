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
    private bool canShoot;            // ワイヤー射出可能かどうか

    private bool isWireActive;        // ワイヤー射出中かどうか
    private bool isAttached;          // ワイヤーが目標に接触して固定されているか
    private Vector2 wireTarget;       // ワイヤー射出先の座標
    private Transform attachedObject = null;
    private Vector2 attachNormal;

    private float detachDelay = 0.05f; // 解除判定を遅らせる時間
    private float detachTimer = 0f;

    private int groundContacts = 0;   // 接地カウント

    private SpriteRenderer sr; //Player色情報

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 高速移動時の衝突検知
                                                                         // SpriteRenderer を取得
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.white; // 初期色
        }

        // LineRenderer 初期化
        if (!wireLine)
        {
            GameObject wireObj = new GameObject("WireLine");
            wireObj.transform.SetParent(transform);
            wireLine = wireObj.AddComponent<LineRenderer>();
        }

        if (wireLine)
        {
            wireLine.positionCount = 2;
            wireLine.startWidth = 0.1f;
            wireLine.endWidth = 0.1f;
            wireLine.material = dashedMaterial;
            wireLine.sortingOrder = 10;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        UpdateWirePrediction();
        HandleWireShoot();
        Debug.Log("isAttached : " + isAttached);
        // 張り付き処理を一箇所で管理
        UpdateAttachment();
        // --- isAttached に応じて色を変える ---
        if (sr != null)
        {
            sr.color = isAttached ? Color.red : Color.white;
        }
    }

    // --- 左右移動 ---
    private void HandleMovement()
    {
        if (isAttached) return; // 張り付き中は別制御

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // --- ジャンプ ---
    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        if (isWireActive)
        {
            Vector2 wireDir = (wireTarget - (Vector2)transform.position).normalized;
            Vector2 jumpDir = (Vector2.up + wireDir * wireJumpFactor).normalized;
            rb.linearVelocity = jumpDir * jumpForce;

            // ワイヤーキャンセル
            Detach();
        }
        else if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // --- ワイヤー予測線 ---
    private void UpdateWirePrediction()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
        canShoot = false;
        Vector2 targetPoint = mouseWorld;

        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            float dist = Vector2.Distance(transform.position, hit.point);
            if (dist <= maxWireDistance)
            {
                canShoot = true;
                targetPoint = hit.point;
            }
        }

        if (wireLine)
        {
            wireLine.SetPosition(0, transform.position);
            wireLine.SetPosition(1, isWireActive ? wireTarget : targetPoint);
            wireLine.material = canShoot ? solidMaterial : dashedMaterial;
            wireLine.startColor = canShoot ? Color.green : Color.white;
            wireLine.endColor = canShoot ? Color.green : Color.white;
        }
    }

    // --- ワイヤー射出＆巻取り ---
    private void HandleWireShoot()
    {
        if (Input.GetMouseButtonDown(0) && !isWireActive)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                float dist = Vector2.Distance(transform.position, hit.point);
                if (dist <= maxWireDistance)
                {
                    wireTarget = hit.point;
                    isWireActive = true;
                    isAttached = false;
                }
            }
        }

        //修正前
        //if (isWireActive && !isAttached)
        //{
        //    Vector2 dir = (wireTarget - (Vector2)transform.position).normalized;
        //    rb.linearVelocity = dir * wireSpeed;

        //    if (Vector2.Distance(transform.position, wireTarget) < 0.2f)
        //    {
        //        // 張り付き状態へ
        //        isAttached = true;
        //        rb.linearVelocity = Vector2.zero;
        //        rb.gravityScale = 0f;
        //    }
        //}

        // 修正
        if (isWireActive && !isAttached)
        {
            Vector2 dir = (wireTarget - (Vector2)transform.position);
            float distance = dir.magnitude;
            dir.Normalize();

            // Raycast で衝突判定
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                isAttached = true;
                attachedObject = hit.transform;
                attachNormal = hit.normal;
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                isWireActive = false;
            }
            else
            {
                rb.linearVelocity = dir * wireSpeed; // 移動
            }
        }
    }

    // --- 張り付き処理をまとめる ---
    private void UpdateAttachment()
    {
        if (!isAttached) return;

        bool stillAttached = false;
        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
        if (playerCollider == null)
        {
            Detach();
            return;
        }

        // 元オブジェクトがまだ接触しているか
        if (attachedObject != null)
        {
            Collider2D attachedCollider = attachedObject.GetComponent<Collider2D>();
            if (attachedCollider != null && playerCollider.IsTouching(attachedCollider))
            {
                stillAttached = true;
            }
        }

        // 元オブジェクトに接触していない場合も周囲のGroundに接触していれば張り付き継続
        if (!stillAttached)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.55f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Ground"))
                {
                    attachedObject = hit.transform;
                    attachNormal = ((Vector2)transform.position - hit.ClosestPoint(transform.position)).normalized;
                    stillAttached = true;
                    break;
                }
            }
        }

        // 接触するオブジェクトがなければ解除判定を遅延
        if (!stillAttached)
        {
            detachTimer += Time.deltaTime;
            if (detachTimer >= detachDelay)
            {
                Detach();
                detachTimer = 0f;
                return;
            }
        }
        else
        {
            detachTimer = 0f; // 接触していればタイマーリセット
        }

        // ジャンプや手動解除
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 jumpDir = Vector2.up;
            if (isWireActive)
            {
                Vector2 wireDir = (wireTarget - (Vector2)transform.position).normalized;
                jumpDir = (Vector2.up + wireDir * wireJumpFactor).normalized;
            }
            rb.linearVelocity = jumpDir * jumpForce;
            Detach();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Detach();
            return;
        }

        HandleAttachMovement();
    }

    // --- 張り付き移動 ---
    private void HandleAttachMovement()
    {
        if (!isAttached) return;

        Vector2 move = Vector2.zero;

        if (Mathf.Abs(attachNormal.y) > 0.9f)
        {
            // 天井・床：横移動のみ
            move.x = Input.GetAxisRaw("Horizontal") * moveSpeed;
        }
        else if (Mathf.Abs(attachNormal.x) > 0.9f)
        {
            // 壁：縦移動のみ
            move.y = Input.GetAxisRaw("Vertical") * moveSpeed;
        }

        rb.linearVelocity = move;
    }

    // --- 張り付き解除 ---
    private void Detach()
    {
        isAttached = false;
        attachedObject = null;
        rb.gravityScale = 1f;
        isWireActive = false;
    }

    // --- 接地判定 ---
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContacts++;
            isGrounded = true;
        }

        // --- 修正前 ---
        // ワイヤー巻取り中に衝突 & Player以外
        // if (isWireActive && !isAttached && collision.gameObject != gameObject)
        // {
        //     isAttached = true;
        //     attachedObject = collision.transform;
        //     attachNormal = collision.contacts[0].normal;
        //     rb.linearVelocity = Vector2.zero;
        //     rb.gravityScale = 0f;
        // }

        // --- 修正後 ---
        // ワイヤーが目標に達したとき or 衝突したときに張り付き
        if (isWireActive && !isAttached && collision.gameObject != gameObject)
        {
            isAttached = true;
            attachedObject = collision.transform;
            attachNormal = collision.contacts[0].normal; // 衝突面の法線
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            isWireActive = false; // ワイヤー巻取り終了
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("Collision Exit: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContacts--;
            isGrounded = groundContacts > 0;
        }
    }
}
