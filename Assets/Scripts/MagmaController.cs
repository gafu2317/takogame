using UnityEngine;

public class MagmaController : MonoBehaviour
{
    [Header("スクロール設定")]
    public float initialSpeed = 2f;        // 初期スクロール速度
    public float acceleration = 0.02f;     // 基本加速度（毎秒どれだけ速くなるか）
    public float maxSpeed = 15f;           // 最大スクロール速度
    
    [Header("段階的加速設定")]
    public float speedIncreaseInterval = 8f;   // 速度上昇の間隔（秒）
    public float speedIncreaseAmount = 0.8f;   // 間隔ごとの速度上昇量
    public float maxAcceleration = 5f;         // 最大加速度
    
    [Header("プレイヤー設定")]
    public Transform player;               // プレイヤーのTransform
    public float killOffset = 1f;          // プレイヤーより何Unit上まで上がったら死亡判定
    public float waveOffset = 0.5f;        // 波を考慮した当たり判定の調整値
    
    private float currentSpeed;            // 現在のスクロール速度
    private float gameTime;                // ゲーム開始からの経過時間
    private float currentAcceleration;     // 現在の加速度
    private bool gameOver = false;
    
    // ゲームオーバー時のイベント
    public System.Action OnPlayerDeath;
    
    void Start()
    {
        currentSpeed = initialSpeed;
        currentAcceleration = acceleration;
        gameTime = 0f;
    }
    
    void Update()
    {
        if (gameOver) return;
        
        // ゲーム時間を更新
        gameTime += Time.deltaTime;
        
        // 段階的に加速度を上昇
        UpdateAcceleration();
        
        // 速度を徐々に上げる
        currentSpeed += currentAcceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed); // 最大速度を超えないように
        
        // マグマを上に移動
        transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
        
        // プレイヤーとの距離をチェック
        CheckPlayerCollision();
    }
    
    // 段階的に加速度を更新
    private void UpdateAcceleration()
    {
        // 指定間隔ごとに加速度を上昇
        int currentStage = Mathf.FloorToInt(gameTime / speedIncreaseInterval);
        float targetAcceleration = acceleration + (currentStage * speedIncreaseAmount * 0.01f);
        currentAcceleration = Mathf.Min(targetAcceleration, maxAcceleration);
        
        // デバッグログ（段階が変わったときのみ表示）
        if (currentStage > 0 && gameTime % speedIncreaseInterval < Time.deltaTime)
        {
            Debug.Log($"マグマ段階 {currentStage}: 加速度 {currentAcceleration:F3}, 現在速度 {currentSpeed:F2}");
        }
    }
    
    void CheckPlayerCollision()
    {
        if (player == null) return;
        
        // マグマの実際の上端とプレイヤーの下端を比較（波を考慮して少し下げる）
        SpriteRenderer magmaSprite = GetComponent<SpriteRenderer>();
        float magmaHeight = magmaSprite != null ? magmaSprite.bounds.size.y : transform.localScale.y;
        float magmaTop = transform.position.y + (magmaHeight / 2) - waveOffset;
        float playerBottom = player.position.y - (player.localScale.y / 2);
        
        // マグマがプレイヤーの実際の位置に触れたら
        if (magmaTop >= playerBottom)
        {
            PlayerDeath();
        }
    }
    

    void PlayerDeath()
    {
        if (gameOver) return;
        
        gameOver = true;
        Debug.Log("プレイヤーがマグマに飲み込まれました！");
        
        // ゲームオーバー処理
        OnPlayerDeath?.Invoke();
        
        // 時間を停止（オプション）
        // Time.timeScale = 0f;
    }
    
    // 外部からゲームオーバー状態をリセットする用
    public void ResetGame()
    {
        gameOver = false;
        currentSpeed = initialSpeed;
        currentAcceleration = acceleration;
        gameTime = 0f;
        Time.timeScale = 1f;
    }
    
    // 現在の速度を取得（UI表示等で使用）
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    // 現在のゲーム段階を取得
    public int GetCurrentStage()
    {
        return Mathf.FloorToInt(gameTime / speedIncreaseInterval);
    }
    
    // 現在の加速度を取得
    public float GetCurrentAcceleration()
    {
        return currentAcceleration;
    }
    
    // デバッグ用：ギズモで範囲を表示
    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            float magmaTop = transform.position.y + (transform.localScale.y / 2);
            Gizmos.DrawLine(new Vector3(-10, magmaTop, 0), new Vector3(10, magmaTop, 0));
            
            Gizmos.color = Color.yellow;
            float playerDeathLine = player.position.y - killOffset;
            Gizmos.DrawLine(new Vector3(-10, playerDeathLine, 0), new Vector3(10, playerDeathLine, 0));
        }
    }
}