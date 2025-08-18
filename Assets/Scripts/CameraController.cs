using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("追従設定")]
    public Transform target;        // 追従するターゲット（プレイヤー）
    public float smoothSpeed = 0.125f; // 追従の滑らかさ
    public Vector3 offset = new Vector3(0, 0, -10); // カメラのオフセット
    
    [Header("追従範囲制限")]
    public bool limitX = true;      // X軸の追従を制限するか
    public bool limitY = false;     // Y軸の追従を制限するか
    public float minX = -10f;       // X軸の最小値
    public float maxX = 10f;        // X軸の最大値
    public float minY = -5f;        // Y軸の最小値
    public float maxY = 100f;       // Y軸の最大値
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // 目標位置を計算
        Vector3 desiredPosition = target.position + offset;
        
        // 追従制限を適用
        if (limitX)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }
        if (limitY)
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // 滑らかに移動
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}