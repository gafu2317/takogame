using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;        // プレイ中のスコア（左上）
    public TextMeshProUGUI resultScoreText;  // リザルトのスコア（中央上）
    public GameObject gameOverText;
    public GameObject restartText;
    
    [Header("Game Objects")]
    public Transform player;
    public Transform magma;
    
    [Header("Score Settings")]
    public float scoreMultiplier = 1f;
    
    private float currentScore = 0f;
    private bool gameOver = false;
    private PlayerHealth playerHealth;
    private Vector3 initialPlayerPosition;
    private Vector3 initialMagmaPosition;
    
    void Start()
    {
        
        // 初期位置を記録
        if (player != null)
        {
            initialPlayerPosition = player.position;
        }
        if (magma != null)
        {
            initialMagmaPosition = magma.position;
        }
        
        // PlayerHealthコンポーネントを取得
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDeath += HandlePlayerDeath;
            }
        }
        
        // 初期UI設定
        UpdateScoreUI();
        if (gameOverText != null) gameOverText.SetActive(false);
        if (restartText != null) restartText.SetActive(false);
        if (resultScoreText != null) resultScoreText.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (!gameOver)
        {
            // スコア更新（プレイヤーの高度ベース）
            if (player != null)
            {
                float height = player.position.y - initialPlayerPosition.y;
                currentScore = Mathf.Max(0, height * scoreMultiplier);
                UpdateScoreUI();
                // Debug.Log($"Player height: {height}, Score: {currentScore}"); // ログを削除
            }
            else
            {
                Debug.LogError("Player is null!");
            }
        }
        
        // リスタート処理
        if (gameOver && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
        
        // タイトルに戻る処理
        if (gameOver && Keyboard.current.tKey.wasPressedThisFrame)
        {
            ReturnToTitle();
        }
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"高度: {currentScore:F0}m";
            // Debug.Log($"Score updated: {currentScore:F0}m"); // ログを削除
        }
        else
        {
            Debug.LogError("scoreText is null!");
        }
    }
    
    public void HandlePlayerDeath()
    {
        if (gameOver) return;
        
        gameOver = true;
        
        // 死亡UI表示
        if (gameOverText != null) gameOverText.SetActive(true);
        if (restartText != null) restartText.SetActive(true);
        
        // リザルトスコア表示
        if (resultScoreText != null)
        {
            resultScoreText.text = $"最終高度: {currentScore:F0}m";
            resultScoreText.gameObject.SetActive(true);
        }
        
        Debug.Log($"ゲームオーバー！最終高度: {currentScore:F0}m");
    }
    
    public void RestartGame()
    {
        gameOver = false;
        currentScore = 0f;
        
        // UI非表示
        if (gameOverText != null) gameOverText.SetActive(false);
        if (restartText != null) restartText.SetActive(false);
        if (resultScoreText != null) resultScoreText.gameObject.SetActive(false);
        
        // プレイヤーをリスポーン
        if (playerHealth != null)
        {
            playerHealth.Respawn(initialPlayerPosition);
        }
        
        // マグマを初期位置に戻す
        if (magma != null)
        {
            magma.position = initialMagmaPosition;
            
            // MagmaControllerをリセット
            MagmaController magmaController = magma.GetComponent<MagmaController>();
            if (magmaController != null)
            {
                magmaController.ResetGame();
            }
        }
        
        
        // スコアUI更新
        UpdateScoreUI();
        
        Debug.Log("ゲームリスタート");
    }
    
    public void ReturnToTitle()
    {
        // タイトルシーンに遷移
        SceneManager.LoadScene("Title");
    }
}