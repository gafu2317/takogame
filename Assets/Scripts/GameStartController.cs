using System.Collections;
using UnityEngine;
using TMPro;

public class GameStartController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI gameStartText;
    public GameObject gameUI; // 通常のゲームUI
    
    [Header("Start Settings")]
    public float displayTime = 2f; // "ゲームスタート"表示時間
    public float fadeTime = 0.5f;  // フェード時間
    
    private bool gameStarted = false;
    
    void Start()
    {
        Debug.Log("GameStartController: Start called");
        
        // 選択されたコースを取得
        string selectedCourse = PlayerPrefs.GetString("SelectedCourse", "A");
        Debug.Log($"Selected Course: {selectedCourse}");
        
        // GameStartText の確認
        if (gameStartText == null)
        {
            Debug.LogError("GameStartText is not assigned!");
            return;
        }
        
        // ゲーム開始演出を開始
        StartCoroutine(GameStartSequence(selectedCourse));
        
        // 最初はゲームUIを非表示
        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }
        
        // プレイヤーの動きを停止
        StopPlayerMovement();
    }
    
    IEnumerator GameStartSequence(string course)
    {
        Debug.Log("GameStartSequence: Starting");
        
        // "ゲームスタート"テキストを表示
        if (gameStartText != null)
        {
            gameStartText.text = $"コース{course}\nゲームスタート！";
            gameStartText.gameObject.SetActive(true);
            Debug.Log("GameStartText activated");
            
            // フェードイン効果
            Color textColor = gameStartText.color;
            textColor.a = 0f;
            gameStartText.color = textColor;
            
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                textColor.a = Mathf.Lerp(0f, 1f, elapsed / fadeTime);
                gameStartText.color = textColor;
                yield return null;
            }
        }
        else
        {
            Debug.LogError("gameStartText is null!");
        }
        
        // 指定時間待機
        yield return new WaitForSeconds(displayTime);
        
        // フェードアウト効果
        if (gameStartText != null)
        {
            Color textColor = gameStartText.color;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                textColor.a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                gameStartText.color = textColor;
                yield return null;
            }
            
            gameStartText.gameObject.SetActive(false);
        }
        
        // ゲーム開始
        StartGame();
    }
    
    void StartGame()
    {
        gameStarted = true;
        
        // ゲームUIを表示
        if (gameUI != null)
        {
            gameUI.SetActive(true);
        }
        
        // プレイヤーの動きを再開
        ResumePlayerMovement();
        
        Debug.Log("ゲーム開始！");
    }
    
    void StopPlayerMovement()
    {
        // プレイヤーの移動を停止
        TestPlayerController playerController = FindFirstObjectByType<TestPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // マグマの動きも停止
        MagmaController magmaController = FindFirstObjectByType<MagmaController>();
        if (magmaController != null)
        {
            magmaController.enabled = false;
        }
    }
    
    void ResumePlayerMovement()
    {
        // プレイヤーの移動を再開
        TestPlayerController playerController = FindFirstObjectByType<TestPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // マグマの動きも再開
        MagmaController magmaController = FindFirstObjectByType<MagmaController>();
        if (magmaController != null)
        {
            magmaController.enabled = true;
        }
    }
    
    public bool IsGameStarted()
    {
        return gameStarted;
    }
}