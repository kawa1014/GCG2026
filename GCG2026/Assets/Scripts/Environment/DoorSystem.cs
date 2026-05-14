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

    // 視線を合わせて長押し(クリック)されたら呼ばれる
    public void ExecuteInteraction()
    {
        IsOpen = !IsOpen; // 開いてれば閉じ、閉じていれば開く
    }

    private Quaternion _targetRotation;
    private Quaternion _defaultRotation;

    void Start()
    {
        _defaultRotation = transform.localRotation;
    }

    void Update()
    {
        // 状態に合わせて回転させるだけの処理
        Quaternion target = IsOpen ? _defaultRotation * Quaternion.Euler(0, OpenAngle, 0) : _defaultRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * Smooth);
    }
}