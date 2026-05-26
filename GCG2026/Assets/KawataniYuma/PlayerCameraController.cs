using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// カメラの基本設定（FOV・壁透け防止）と、画面の揺れ演出（ヘッドボブ・カメラシェイク）を統合管理するクラス。
/// 独自の入力や他クラスからの直接参照を持たず、イベント駆動（Action）によって完全に疎結合な設計となっています。
/// </summary>
public class PlayerCameraController : MonoBehaviour
{
    /// <summary>
    /// 外部のスクリプト（敵の攻撃、爆発、イベントなど）からカメラを揺らすためのグローバルイベントです。
    /// 引数は (揺らす秒数: float, 揺れの強さ: float) となります。
    /// </summary>
    public static event Action<float, float> OnCameraShakeRequested;

    [Header("カメラ基本設定")]

    /// <summary>
    /// 初期視野角(FOV)。Unityのインスペクター上でスライダー調整が可能です。
    /// </summary>
    [Tooltip("初期視野角(FOV) - スライダーで調整可能")]
    [SerializeField, Range(30f, 120f)] private float InitialFov = 60f;

    /// <summary>
    /// 壁の透け防止用(Near Clip Plane)。極小値にすることで、狭い通路で壁に近づいた際の裏世界透けを防ぎます。
    /// </summary>
    [Tooltip("壁の透け防止用(Near Clip Plane) - 極小値で裏世界を防止")]
    [SerializeField] private float NearClipPlane = 0.01f;

    [Header("ヘッドボブ（歩行揺れ）設定")]

    /// <summary>
    /// ヘッドボブ（歩行時のカメラの揺れ）を有効にするかどうかのフラグです。
    /// </summary>
    [Tooltip("ヘッドボブを有効にするか")]
    [SerializeField] private bool EnableHeadBob = true;

    /// <summary>
    /// 揺れの速さ（波の周波数）です。数値を大きくすると素早く揺れます。
    /// </summary>
    [Tooltip("揺れの速さ")]
    [SerializeField] private float BobFrequency = 1.5f;

    /// <summary>
    /// 上下の揺れの幅（高さ・振幅）です。初期値を抑えめにしています。
    /// </summary>
    [Tooltip("上下の揺れ幅（縦）")]
    [SerializeField] private float BobVerticalAmplitude = 0.02f;

    /// <summary>
    /// 左右の揺れの幅（横・振幅）です。
    /// </summary>
    [Tooltip("左右の揺れ幅（横）")]
    [SerializeField] private float BobHorizontalAmplitude = 0.02f;

    [Header("参照設定")]

    /// <summary>
    /// プレイヤーの移動速度を取得するための CharacterController コンポーネントの参照です。
    /// </summary>
    [Tooltip("プレイヤーの移動速度を取得するためのコンポーネント")]
    [SerializeField] private CharacterController PlayerCharacterController;

    /// <summary>
    /// 制御対象となる、自身にアタッチされたカメラコンポーネントです。
    /// </summary>
    private Camera TargetCamera;

    /// <summary>
    /// カメラの初期のローカルX座標（左右）を保持し、横揺れの基準点とします。
    /// </summary>
    private float DefaultXPos;

    /// <summary>
    /// カメラの初期のローカルY座標（高さ）を保持し、縦揺れの基準点とします。
    /// </summary>
    private float DefaultYPos;

    /// <summary>
    /// サイン波・コサイン波の計算に使用する時間経過用の累積タイマーです。
    /// </summary>
    private float Timer;

    /// <summary>
    /// 現在実行中のカメラシェイク（震え）のコルーチンを保持します。新しいシェイクが発生した際の上書き中断に使用します。
    /// </summary>
    private Coroutine ActiveShakeCoroutine;

    private void OnEnable()
    {
        OnCameraShakeRequested += TriggerShake;
    }

    private void OnDisable()
    {
        OnCameraShakeRequested -= TriggerShake;
    }

    private void Start()
    {
        TargetCamera = GetComponent<Camera>();

        if (TargetCamera != null)
        {
            TargetCamera.fieldOfView = InitialFov;
            TargetCamera.nearClipPlane = NearClipPlane;
        }

        // 初期位置を記憶（XとY両方）
        DefaultXPos = transform.localPosition.x;
        DefaultYPos = transform.localPosition.y;
    }

    private void Update()
    {
        if (EnableHeadBob && PlayerCharacterController != null)
        {
            HandleHeadBob();
        }
    }

    /// <summary>
    /// プレイヤーの水平移動速度に基づいて、カメラのローカル座標を滑らかに揺らします。
    /// 人間の歩行メカニズムに合わせて、横揺れは縦揺れの半分のテンポ（Cos波）で計算されます。
    /// </summary>
    private void HandleHeadBob()
    {
        Vector3 horizontalVelocity = new Vector3(PlayerCharacterController.velocity.x, 0, PlayerCharacterController.velocity.z);
        float speed = horizontalVelocity.magnitude;

        if (speed > 0.1f && PlayerCharacterController.isGrounded)
        {
            Timer += Time.deltaTime * BobFrequency * (speed * 0.5f);

            // 縦揺れ：通常のサイン波
            float newY = DefaultYPos + Mathf.Sin(Timer) * BobVerticalAmplitude;
            // 横揺れ：テンポを半分にしたコサイン波（右足と左足の体重移動を表現）
            float newX = DefaultXPos + Mathf.Cos(Timer * 0.5f) * BobHorizontalAmplitude;

            transform.localPosition = new Vector3(newX, newY, transform.localPosition.z);
        }
        else
        {
            Timer = 0f;
            // 停止時は縦横ともに滑らかに初期位置へ復帰
            float newX = Mathf.Lerp(transform.localPosition.x, DefaultXPos, Time.deltaTime * 5f);
            float newY = Mathf.Lerp(transform.localPosition.y, DefaultYPos, Time.deltaTime * 5f);
            transform.localPosition = new Vector3(newX, newY, transform.localPosition.z);
        }
    }

    private void TriggerShake(float duration, float magnitude)
    {
        if (ActiveShakeCoroutine != null)
        {
            StopCoroutine(ActiveShakeCoroutine);
        }
        ActiveShakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = originalPos.x + UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y + UnityEngine.Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}