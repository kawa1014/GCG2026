using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの移動とマウス視点操作を行うクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// プレイヤー設定データ
    /// </summary>
    [SerializeField] private PlayerSettings playerSettings;

    /// <summary>
    /// プレイヤーに付いているCharacterController
    /// </summary>
    private CharacterController controller;

    /// <summary>
    /// 子にあるカメラのTransform
    /// </summary>
    [SerializeField] private Transform cameraTransform;

    /// <summary>
    /// カメラの上下回転角度
    /// </summary>
    private float xRotation = 0.0f;

    /// <summary>
    /// 開始時に必要なコンポーネント取得とマウスカーソル設定を行う
    /// </summary>
    private void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 毎フレーム、移動処理と視点操作を行う
    /// </summary>
    private void Update()
    {
        Move();
        Look();
    }

    /// <summary>
    /// WASD入力でプレイヤーを移動させる
    /// </summary>
    private void Move()
    {
        // キーボードが接続されていない場合は処理をスキップ
        if (Keyboard.current == null) return;

        float x = 0.0f;
        float z = 0.0f;

        // W, A, S, Dキーが押されているかを直接判定
        if (Keyboard.current.dKey.isPressed) x += 1.0f;
        if (Keyboard.current.aKey.isPressed) x -= 1.0f;
        if (Keyboard.current.wKey.isPressed) z += 1.0f;
        if (Keyboard.current.sKey.isPressed) z -= 1.0f;

        // 斜め移動時に速度が2倍(約1.4倍)にならないよう、長さを正規化する
        Vector3 inputDir = new Vector3(x, 0, z).normalized;

        // プレイヤーの向いている方向を基準に移動方向を決定
        Vector3 move = transform.right * inputDir.x + transform.forward * inputDir.z;

        // CharacterControllerを使って移動を実行
        controller.Move(move * playerSettings.moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// マウス入力でプレイヤーの左右回転とカメラの上下回転を行う
    /// </summary>
    private void Look()
    {
        // マウスが接続されていない場合はスキップ
        if (Mouse.current == null) return;

        // マウスの移動量を取得
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float sensitivityMultiplier = 0.01f;
        float mouseX = mouseDelta.x * playerSettings.mouseSensitivity * sensitivityMultiplier;
        float mouseY = mouseDelta.y * playerSettings.mouseSensitivity * sensitivityMultiplier;

        // 上下視点の計算(Y軸の動きでX軸を回転させる)
        xRotation -= mouseY;
        // 真上や真下を向きすぎなように角度を制限
        xRotation = Mathf.Clamp(xRotation, -80.0f, 80.0f);

        // カメラは上下回転のみ
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        // プレイヤー本体を左右回転
        transform.Rotate(Vector3.up * mouseX);
    }
}