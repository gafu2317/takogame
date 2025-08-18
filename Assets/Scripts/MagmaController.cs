using UnityEngine;

public class MagmaController : MonoBehaviour
{
    [Header("スクロール設定")]
    public float initialSpeed = 1f;        // 初期スクロール速度
    public float acceleration = 0.01f;     // 加速度（毎秒どれだけ速くなるか）
    public float maxSpeed = 5f;            // 最大スクロール速度
    
    [Header("プレイヤー設定")]
    public Transform player;               // プレイヤーのTransform
    public float killOffset = 2f;          // プレイヤーより何Unit上まで上がったら死亡判定
    
    private float currentSpeed;            // 現在のスクロール速度
    private bool gameOver = false;
    
    // ゲームオーバー時のイベント
    public System.Action OnPlayerDeath;
    
    void Start()
    {
        currentSpeed = initialSpeed;
    }
    
    void Update()
    {
        if (gameOver) return;
        
        // 速度を徐々に上げる
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed); // 最大速度を超えないように
        
        // マグマを上に移動
        transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
        
        // プレイヤーとの距離をチェック
        CheckPlayerCollision();
    }
    
    void CheckPlayerCollision()
    {
        if (player == null) return;
        
        // マグマの実際の上端とプレイヤーの下端を比較
        float magmaTop = transform.position.y + (transform.localScale.y / 2);
        float playerBottom = player.position.y - (player.localScale.y / 2);
        
        // マグマがプレイヤーの実際の位置に触れたら
        if (magmaTop >= playerBottom)
        {
            PlayerDeath();
        }
        
        // デバッグ用
        Debug.Log($"Magma Top: {magmaTop}, Player Bottom: {playerBottom}");
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
        Time.timeScale = 1f;
    }
    
    // 現在の速度を取得（UI表示等で使用）
    public float GetCurrentSpeed()
    {
        return currentSpeed;
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