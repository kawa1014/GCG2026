using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// カメラの設定(FOV等)と、画面の揺れ演出(ヘッドボブ・シェイク)を統合管理するクラス
/// </summary>
public class PlayerCameraController : MonoBehaviour
{
    /// <summary>
    /// 外部のスクリプトなどからカメラを揺らすためのグローバルイベント
    /// 引数は(揺らす秒数: float, 揺れの強さ: float)となります
    /// </summary>
    public static event Action<float, float> OnCameraShakeRequested;

    /// <summary>
    /// 初期視野角(FOV)。スライダーで調整可能です
    /// </summary>
    [Header("カメラの基本設定")]
    [Tooltip("初期視野角(FOV) - スライダーで調整可能")]
    [SerializeField, Range(30.0f, 120.0f)] private float InitialFov = 60.0f;

    /// <summary>
    /// 壁の透け防止用。極小値にすることで、狭い通路で壁に近づいた際の裏世界透けを防ぎます
    /// </summary>
    [Tooltip("壁の透け防止用(Near Clip Plane) - 極小値で裏世界を防止")]
    [SerializeField] private float NearClipPlane = 0.01f;

    [Header("ヘッドボブ(歩行揺れ)設定")]
    /// <summary>
    /// ヘッドボブ(歩行時のカメラの上下揺れ)を有効にするかどうかのフラグです
    /// </summary>
    [Tooltip("ヘッドボブを有効にするか")]
    [SerializeField] private bool EnableHeadBob = true;
    /// <summary>
    /// 揺れの速さです。数値を大きくすると素早く揺れます
    /// </summary>
    [Tooltip("揺れの速さ")]
    [SerializeField] private float BobFrequency = 1.5f;
    /// <summary>
    /// 揺れの幅です。数値を大きくすると上下に大きく揺れます
    /// </summary>
    [Tooltip("揺れの幅(高さ)")]
    [SerializeField] private float BobAmplitube = 0.05f;

    [Header("参照設定")]
    /// <summary>
    /// プレイヤーの移動速度を取得するためのCharacterControllerコンポーネントの参照です
    /// </summary>
    [Tooltip("プレイヤーの移動速度を取得するためのコンポーネント")]
    [SerializeField] private CharacterController PlayerCharacterController;


    /// <summary>
    /// 制御対象となる、自身にアタッチされたカメラコンポーネントです・
    /// </summary>
    private Camera TargetCamera;

    /// <summary>
    /// カメラの初期のローカルY座標(高さ)を保持し、揺れの基準点とします
    /// </summary>
    private float DefaultYpos;

    /// <summary>
    /// サイン波の計算に使用する時間経過用の累積タイマー
    /// </summary>
    private float Timer;

    /// <summary>
    /// 現在実行中のカメラシェイクのコルーチンを保持します。新しいシェイクが発生した際の上書き中断に使用します
    /// </summary>
    private Coroutine ActiveShakeCoroutine;

    /// <summary>
    /// スクリプトが有効になったタイミングで、カメラシェイクイベントへの聞き耳を開始します
    /// </summary>
    private void OnEnable()
    {
        OnCameraShakeRequested += TriggerShake;
    }

    /// <summary>
    /// スクリプトが無効になったタイミングで聞き耳を解除し、メモリリークを防止します
    /// </summary>
    private void OnDisable()
    {
        OnCameraShakeRequested -= TriggerShake;
    }

    /// <summary>
    /// ゲーム開始時にカメラコンポーネントの初期化を行い、基準の高さを記憶します
    /// </summary>
    private void Start()
    {
        TargetCamera = GetComponent<Camera>();

        // 1 & 4. FOVとNear Clip Planeの初期化(スクリプトから強制適用して事故を防ぐ)
        if(TargetCamera != null)
        {
            // インスペクターで指定した値を強制適用して設定事故を防ぐ
            TargetCamera.fieldOfView = InitialFov;
            TargetCamera.nearClipPlane = NearClipPlane;
        }

        // カメラの初期の高さを記憶しておく
        DefaultYpos = transform.localPosition.y;
    }

    /// <summary>
    /// 毎フレーム、プレイヤーの移動状態を監視し、接地して移動している場合にヘッドボブを実行します
    /// </summary>
    private void Update()
    {
        if(EnableHeadBob && PlayerCharacterController != null)
        {
            HandleHeadBob();
        }
    }

    /// <summary>
    /// プレイヤーの水平移動速度に基づいて、カメラのローカルY座標をサイン波で滑らかに上下させます
    /// 停止時、または空中時は元の位置へと滑らかに戻ります
    /// </summary>
    private void HandleHeadBob()
    {
        // プレイヤーの水平方向の移動速度を取得
        Vector3 horizontalVelocity = new Vector3(PlayerCharacterController.velocity.x, 0, PlayerCharacterController.velocity.z);
        float speed = horizontalVelocity.magnitude;

        // 接地しており、かつある程度の速度で移動している場合
        if (speed > 0.1f && PlayerCharacterController.isGrounded)
        {
            // 移動速度に比例させてタイマーの進行度を変化させ、歩行と走行の店舗に合わせます
            Timer += Time.deltaTime * BobFrequency * (speed * 0.5f);

            // サイン波を用いて上下の滑らかな座標を算出
            float newY = DefaultYpos + Mathf.Sin(Timer) * BobAmplitube;
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
        else
        {
            // 停止時はタイマーをクリアし、カメラの高さを滑らかに初期位置へ復帰させます
            Timer = 0.0f;
            float newY = Mathf.Lerp(transform.localPosition.y, DefaultYpos, Time.deltaTime * 5.0f);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
    }

    /// <summary>
    /// イベント通知をトリガーとして内部コルーチンを起動し、カメラシェイクを開始します
    /// 既に実行中のシェイクがある場合は即座に中断して 上書きします
    /// </summary>
    /// <param name="duration">画面を揺らす秒数</param>
    /// <param name="magnitude">画面を揺らす強さの幅</param>
    public void TriggerShake(float duration, float magnitude)
    {
        if (ActiveShakeCoroutine != null)
        {
            StopCoroutine(ActiveShakeCoroutine);
        }
        ActiveShakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    
    /// <summary>
    /// 指定された時間の間、カメラのローカル座標にランダムな微小値を加算し、画面の震えを表現するコルーチン
    /// </summary>
    /// <param name="duration">画面を揺らす秒数</param>
    /// <param name="magnitude">画面を揺らす強さの幅</param>
    /// <returns></returns>
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // 現在の基準位置に対して、ランダムなノイズ値を加算
            float x = originalPos.x + UnityEngine.Random.Range(-1.0f, 1.0f) * magnitude;
            float y = originalPos.y + UnityEngine.Random.Range(-1.0f, 1.0f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; // 1フレーム待機
        }

        // 揺れ時間が終了したら必ず正確な初期位置へ戻す
        transform.localPosition = originalPos;
    }
}
