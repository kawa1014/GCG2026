using System.Collections;
using UnityEngine;

/// <summary>
/// カメラの設定(FOV等)と、画面の揺れ演出(ヘッドボブ・シェイク)を統合管理するクラス
/// </summary>
public class PlayerCameraController : MonoBehaviour
{
    [Header("カメラの基本設定")]
    [Tooltip("初期視野角(FOV) - スライダーで調整可能")]
    [SerializeField, Range(30.0f, 120.0f)] private float _initialFov = 60.0f;

    [Tooltip("壁の透け防止用(Near Clip Plane) - 極小値で裏世界を防止")]
    [SerializeField] private float _nearClipPlane = 0.01f;

    [Header("ヘッドボブ(歩行揺れ)設定")]
    [Tooltip("ヘッドボブを有効にするか")]
    [SerializeField] private bool _enableHeadBob = true;
    [Tooltip("揺れの速さ")]
    [SerializeField] private float _bobFrequency = 1.5f;
    [Tooltip("揺れの幅(高さ)")]
    [SerializeField] private float _bobAmplitube = 0.05f;

    [Header("参照設定")]
    [Tooltip("プレイヤーの移動速度を取得するためのコンポーネント")]
    [SerializeField] private CharacterController _characterController;

    private Camera _targetCamera;
    private float _defaultYpos;
    private float _timer;
    private Coroutine _shakeCoroutine;

    private void Start()
    {
        _targetCamera = GetComponent<Camera>();

        // 1 & 4. FOVとNear Clip Planeの初期化(スクリプトから強制適用して事故を防ぐ)
        if(_targetCamera != null)
        {
            _targetCamera.fieldOfView = _initialFov;
            _targetCamera.nearClipPlane = _nearClipPlane;
        }

        // カメラの初期の高さを記憶しておく
        _defaultYpos = transform.localPosition.y;
    }

    private void Update()
    {
        if(_enableHeadBob && _characterController != null)
        {
            HandleHeadBob();
        }
    }

    /// <summary>
    /// 2. ヘッドボブ(歩行時の揺れ)の数学的な計算と適用
    /// </summary>
    private void HandleHeadBob()
    {
        // プレイヤーの水平方向の移動速度を取得
    }
}
