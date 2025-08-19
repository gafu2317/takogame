using System.Collections.Generic;
using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    [Header("ステージパーツ")]
    public GameObject initialStage; // 最初のステージ（床あり）
    public GameObject[] stageParts; // 通常のステージ（床に穴あり）
    public GameObject backgroundPrefab; // 背景パーツ
    
    [Header("生成設定")]
    public int initialStageCount = 5; // 最初に生成するステージ数
    public float stageHeight = 10f; // 1つのステージの高さ
    public Transform player; // プレイヤーのTransform
    
    [Header("生成管理")]
    public float generateDistance = 30f; // プレイヤーから何Unity単位先まで生成するか
    
    private List<GameObject> generatedStages = new List<GameObject>();
    private float nextStageY = 0f; // 次のステージのY座標
    private bool isFirstStage = true; // 最初のステージかどうか
    
    void Start()
    {
        // 初期ステージを生成
        for (int i = 0; i < initialStageCount; i++)
        {
            GenerateNextStage();
        }
    }
    
    void Update()
    {
        // プレイヤーが上に進んだら新しいステージを生成
        if (player != null)
        {
            float playerY = player.position.y;
            
            // プレイヤーが一定距離まで来たら次のステージを生成
            if (nextStageY - playerY < generateDistance)
            {
                GenerateNextStage();
            }
            
            // 下の古いステージを削除（最適化）
            RemoveOldStages(playerY);
        }
    }
    
    void GenerateNextStage()
    {
        GameObject selectedPart;
        
        // 最初のステージかどうかで使用するPrefabを決定
        if (isFirstStage)
        {
            if (initialStage == null) return;
            selectedPart = initialStage;
            isFirstStage = false; // 次回からは通常ステージ
        }
        else
        {
            if (stageParts.Length == 0) return;
            // ランダムに通常ステージパーツを選択
            int randomIndex = Random.Range(0, stageParts.Length);
            selectedPart = stageParts[randomIndex];
        }
        
        // ステージを生成
        Vector3 spawnPosition = new Vector3(0, nextStageY, 0);
        GameObject newStage = Instantiate(selectedPart, spawnPosition, Quaternion.identity);
        newStage.transform.parent = this.transform; // StageGeneratorの子オブジェクトにする
        
        // 背景も生成
        if (backgroundPrefab != null)
        {
            GameObject newBackground = Instantiate(backgroundPrefab, spawnPosition, Quaternion.identity);
            newBackground.transform.parent = this.transform;
        }
        
        // リストに追加
        generatedStages.Add(newStage);
        
        // 次のステージのY座標を更新
        nextStageY += stageHeight;
        
        Debug.Log($"ステージ生成: {newStage.name} at Y={spawnPosition.y}");
    }
    
    void RemoveOldStages(float playerY)
    {
        // プレイヤーより下に一定距離離れたステージを削除
        float removeDistance = 50f;
        
        for (int i = generatedStages.Count - 1; i >= 0; i--)
        {
            if (generatedStages[i] != null)
            {
                float stageY = generatedStages[i].transform.position.y;
                
                if (playerY - stageY > removeDistance)
                {
                    Destroy(generatedStages[i]);
                    generatedStages.RemoveAt(i);
                }
            }
        }
    }
}