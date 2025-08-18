using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;       // 左右移動速度
    public float jumpForce = 10f;      // ジャンプ力

    [Header("ワイヤー設定")]
    public float wireSpeed = 10f;      // ワイヤー巻き取り速度
    public LineRenderer wireLine;      // ワイヤー表示用

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isAttached;
    private Vector2 wireTarget;
    private bool isWireActive;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // --- 左右移動 ---
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // --- ジャンプ ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isWireActive)
            {
                // ワイヤー方向を加味したジャンプ
                Vector2 wireDir = (wireTarget - (Vector2)transform.position).normalized;
                Vector2 jumpDir = (Vector2.up + wireDir).normalized;

                rb.linearVelocity = jumpDir * jumpForce; // ワイヤー方向を反映したジャンプ
                isWireActive = false;
                isAttached = false;

                if (wireLine != null)
                    wireLine.positionCount = 0;
            }
            else if (isGrounded)
            {
                // 地上ジャンプ
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        // --- ワイヤー射出 ---
        if (Input.GetMouseButtonDown(0) && !isWireActive)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wireTarget = mousePos;
            isWireActive = true;
            isAttached = false;

            if (wireLine != null)
            {
                wireLine.positionCount = 2;
                wireLine.SetPosition(0, transform.position);
                wireLine.SetPosition(1, transform.position);
            }
        }
    }

    void FixedUpdate()
    {
        // ワイヤー自動巻取り
        if (isWireActive && !isAttached)
        {
            Vector2 direction = (wireTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * wireSpeed;

            // 簡易衝突判定：目標に近づいたら張り付き
            if (Vector2.Distance(transform.position, wireTarget) < 0.1f)
            {
                isAttached = true;
                rb.linearVelocity = Vector2.zero;
            }

            // ワイヤー表示更新
            if (wireLine != null)
            {
                wireLine.SetPosition(0, transform.position);
                wireLine.SetPosition(1, wireTarget);
            }
        }
    }

    // --- 接地判定 ---
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
