using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("ワイヤー設定")]
    public float wireSpeed = 10f;
    public float maxWireDistance = 15f; // ワイヤー射程
    public LineRenderer wireLine;
    public Material solidMaterial;  // 射出可能時の実線
    public Material dashedMaterial; // 射出不可能時の点線
    public float wireJumpFactor = 0.5f; // ワイヤー方向慣性

    private Rigidbody2D rb;
    private bool isGrounded;

    private bool isWireActive;
    private bool isAttached;
    private Vector2 wireTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // LineRendererの初期設定
        if (!wireLine)
        {
            // LineRendererが未設定の場合は自動で作成
            GameObject wireObj = new("WireLine");
            wireObj.transform.SetParent(transform);
            wireLine = wireObj.AddComponent<LineRenderer>();
        }
        
        if (wireLine)
        {
            wireLine.positionCount = 2;
            wireLine.startWidth = 0.1f;
            wireLine.endWidth = 0.1f;
            wireLine.material = solidMaterial;
            wireLine.sortingOrder = 10; // 前面に表示
        }
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        UpdateWirePrediction();
        HandleWireShoot();
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isWireActive)
            {
                Vector2 wireDir = (wireTarget - (Vector2)transform.position).normalized;
                Vector2 jumpDir = (Vector2.up + wireDir * wireJumpFactor).normalized;

                rb.linearVelocity = jumpDir * jumpForce;

                // ワイヤーキャンセル
                isWireActive = false;
                isAttached = false;
            }
            else if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    private void UpdateWirePrediction()
    {
        // マウス座標をワールド座標に変換
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f; // 2D 平面に固定

        Vector2 direction = mouseWorld - transform.position;

        // Player自身を無視してclickable タグに当たるかチェック
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, maxWireDistance);
        bool canShoot = false;
        RaycastHit2D validHit = new();
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != gameObject && hit.collider.CompareTag("clickable"))
            {
                canShoot = true;
                validHit = hit;
                break;
            }
        }
        Vector2 targetPoint = canShoot ? validHit.point : (Vector2)mouseWorld;

        // LineRenderer 更新
        if (wireLine)
        {
            wireLine.SetPosition(0, transform.position);
            wireLine.SetPosition(1, isWireActive ? wireTarget : targetPoint);
            wireLine.material = canShoot ? solidMaterial : dashedMaterial;
        }
    }

    private void HandleWireShoot()
    {
        if (Input.GetMouseButtonDown(0) && !isWireActive)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 direction = mouseWorld - transform.position;

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, maxWireDistance);
            
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject && hit.collider.CompareTag("clickable"))
                {
                    wireTarget = hit.point;
                    isWireActive = true;
                    isAttached = false;
                    break;
                }
            }
        }

        // 自動巻取り
        if (isWireActive && !isAttached)
        {
            Vector2 dir = (wireTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * wireSpeed;

            if (Vector2.Distance(transform.position, wireTarget) < 0.2f)
            {
                isAttached = true;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
