using UnityEngine;
using UnityEngine.InputSystem;

public class DoorSystem : MonoBehaviour, IInteractable
{
    [Header("ドア設定")]
    public float OpenAngle = 90f;
    public float Smooth = 2f;

    [Header("自動で閉まる設定")]
    public bool AutoClose = true; // チェックを入れると自動で閉まる
    public float CloseTime = 3f;  // 開いてから何秒後に閉まるか

    public bool IsOpen { get; private set; } = false;
    public bool IsInteractable => true;

    private Quaternion _defaultRotation;
    private float _targetAngle = 0f;

    // 時間を測るためのタイマー変数
    private float _timer = 0f;

    // 追加(川谷)ドアが現在動いているかどうかを判定するフラグ
    private bool _isMoving = false;

    void Start()
    {
        _defaultRotation = transform.localRotation;
    }

    public void ExecuteInteraction()
    {
        // 念のため、動いている最中は処理をはじく(連打対策)
        if (_isMoving) return;

        IsOpen = !IsOpen;

        if (IsOpen)
        {
            Vector3 dirToPlayer = Camera.main.transform.position - transform.position;
            float dot = Vector3.Dot(transform.right, dirToPlayer);
            _targetAngle = (dot > 0) ? OpenAngle : -OpenAngle;

            // ドアを開けた瞬間にタイマーを0にリセットする
            _timer = 0f;
        }
        else
        {
            _targetAngle = 0f;
        }
    }

    void Update()
    {
        // 目標の回転値を計算
        Quaternion targetRot = _defaultRotation * Quaternion.Euler(0, _targetAngle, 0);

        // 現在の角度と目標の角度の差を計算し、0.1度以上離れていたら「動いている」と判定
        _isMoving = Quaternion.Angle(transform.localRotation, targetRot) > 0.1f;

        // 1. ドアを滑らかに回転させる処理
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * Smooth);

        // 2. 自動で閉まるタイマー処理
        if (IsOpen && AutoClose)
        {
            _timer += Time.deltaTime; // 経過時間をどんどん足していく

            if (_timer >= CloseTime) // 設定した秒数（CloseTime）を超えたら
            {
                IsOpen = false;
                _targetAngle = 0f; // 角度を0に戻す＝閉める
                _timer = 0f;       // タイマーをリセット
            }
        }
    }
}