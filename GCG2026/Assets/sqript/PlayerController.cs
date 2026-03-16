using UnityEditor;
using UnityEngine;

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
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * playerSettings.moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// マウス入力でプレイヤーの左右回転とカメラの上下回転を行う
    /// </summary>
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * playerSettings.mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * playerSettings.mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80.0f, 80.0f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}