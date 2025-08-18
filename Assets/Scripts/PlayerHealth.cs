using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("死亡設定")]
    public bool isDead = false;
    
    // 死亡時のイベント
    public System.Action OnPlayerDeath;
    
    void Start()
    {
        // MagmaControllerからの死亡イベントを受信
        MagmaController magma = FindFirstObjectByType<MagmaController>();
        if (magma != null)
        {
            magma.OnPlayerDeath += HandleDeath;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 棘に触れた場合
        if (other.CompareTag("Spike"))
        {
            HandleDeath();
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // マグマに触れた場合（念のため）
        if (collision.gameObject.name.Contains("Magma"))
        {
            HandleDeath();
        }
    }
    
    public void HandleDeath()
    {
        if (isDead) return; // 既に死んでいる場合は何もしない
        
        isDead = true;
        Debug.Log("プレイヤーが死亡しました！");
        
        // プレイヤーの動きを停止
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // プレイヤーの操作を無効化
        TestPlayerController controller = GetComponent<TestPlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // 死亡イベントを発火
        OnPlayerDeath?.Invoke();
        
        // 視覚的フィードバック（色を変える）
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
        }
    }
    
    // ゲームリセット用
    public void Respawn(Vector3 spawnPosition)
    {
        isDead = false;
        transform.position = spawnPosition;
        
        // 物理を再有効化
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector2.zero;
        }
        
        // 操作を再有効化
        TestPlayerController controller = GetComponent<TestPlayerController>();
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        // 色を元に戻す
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.blue;
        }
        
        Debug.Log("プレイヤーがリスポーンしました");
    }
}