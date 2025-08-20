using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("追従設定")]
    public Transform target;        // 追従するターゲット（プレイヤー）
    public float smoothSpeed = 0.125f; // 追従の滑らかさ
    public Vector3 offset = new Vector3(0, 0, -10); // カメラのオフセット
    
    [Header("追従設定")]
    public bool followX = false;    // X軸を追従するか（左右）
    public bool followY = true;     // Y軸を追従するか（上下）
    public float fixedX = 0f;       // X軸固定位置
    
    private Vector3 initialCameraPosition;
    
    void Start()
    {
        // 初期カメラ位置を記録
        initialCameraPosition = transform.position;
        if (fixedX == 0f) fixedX = initialCameraPosition.x;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // 目標位置を計算
        Vector3 desiredPosition = transform.position;
        
        // X軸（左右）の追従設定
        if (followX)
        {
            desiredPosition.x = target.position.x + offset.x;
        }
        else
        {
            desiredPosition.x = fixedX;
        }
        
        // Y軸（上下）の追従設定
        if (followY)
        {
            desiredPosition.y = target.position.y + offset.y;
        }
        
        // Z軸は常にオフセット値
        desiredPosition.z = offset.z;
        
        // 滑らかに移動
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}