using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour, IInteractable
{
    [Header("ドア設定")]
    public float OpenAngle = 90f;
    public float Smooth = 2f;

    // 外部から勝手に書き換えられないようにカプセル化
    public bool IsOpen { get; private set; } = false;

    // ---IInteractableの実装---
    public bool IsInteractable => true; // ドアはいつでもインタラクト可能

    private Quaternion _defaultRotation;

    // 実際に開く方向（角度）を保持する変数
    private float _currentOpenAngle;

    void Start()
    {
        _defaultRotation = transform.localRotation;
    }

    // 視線を合わせて長押し(クリック)されたら呼ばれる
    public void ExecuteInteraction()
    {
        if (!IsOpen)
        {
            // 開くときの処理：プレイヤー（カメラ）の位置を取得
            Transform playerTransform = Camera.main.transform;

            // 扉からプレイヤーへの方向ベクトルを計算
            Vector3 directionToPlayer = playerTransform.position - transform.position;

            // 扉の正面方向とプレイヤーへの方向の内積を計算
            float dot = Vector3.Dot(transform.forward, directionToPlayer);

            // プレイヤーが前にいるか後ろにいるかで開く方向を反転させる
            if (dot > 0)
            {
                _currentOpenAngle = OpenAngle;
            }
            else
            {
                _currentOpenAngle = -OpenAngle;
            }

            IsOpen = true;
        }
        else
        {
            // 閉じるときの処理
            IsOpen = false;
        }
    }

    void Update()
    {
        // _currentOpenAngle を使って回転させる
        Quaternion target = IsOpen ? _defaultRotation * Quaternion.Euler(0, _currentOpenAngle, 0) : _defaultRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * Smooth);
    }
}