using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// オルゴールの状態管理と3Dサウンド制御を行うクラス
/// </summary>
public class OrgelSystem : MonoBehaviour, IInteractable
{
    // イベント駆動：自分が鳴った/止まったことを外部に知らせるAction
    public static event Action<OrgelSystem> OnOrgelStarted;
    public static event Action<OrgelSystem> OnOrgelStopped;

    [Header("サウンド設定")]
    /// <summary>
    /// @brief オルゴールの音を鳴らすためのコンポーネント
    /// </summary>
    [Tooltip("3Dサウンド設定を行ったAudioSourceをアタッチしてください")]
    public AudioSource OrgelAudioSource;

    [Header("時間指定")]
    [Tooltip("このオルゴールが抽選されてからなりだすまでの待機時間(秒)")]
    public float WaitTime = 10.0f;

    [Header("レイヤー設定")]
    public string HighlightLayerName = "Highlight";

    // プロパティによるカプセル化(外部からは読み取り専用)
    public bool IsPlaying { get; private set; } = false;
    /// <summary>
    /// 現在抽選されて出番待ちかどうか
    /// </summary>
    public bool IsWaiting { get; private set; } = false;

    //--- IInteractableの実装---
    /// <summary>
    /// 現在音が鳴っているかどうかの状態
    /// </summary>
    public bool IsInteractable => IsPlaying; // 鳴っている時だけインタラクト可能
    public void ExecuteInteraction() => TurnOff(); // インタラクトされたらTurnOffを実行

    /// <summary>
    /// オブジェクトの色を変更するための描画コンポーネントを保持しておく変数
    /// </summary>
    private Renderer _objRenderer;
    private float timer; // 次に鳴るまでのカウントダウンタイマー

    /// <summary>
    /// ゲーム開始時に1回だけ呼ばれる初期化処理
    /// </summary>
    private void Start()
    {
        // 自分がくっついているオブジェクトのRendererを取得して保存
        _objRenderer = GetComponent<Renderer>();
        if (OrgelAudioSource != null) OrgelAudioSource.Stop();
        IsWaiting = false;
        IsPlaying = false;
        UpdateColorAndLayer();
    }

    /// <summary>
    ///  GameManagerから「次はお前だ」と抽選されたときに呼ばれる
    /// </summary>
    public void StartCountdown()
    {
        IsWaiting = true;
        StartCoroutine(CountdownCoroutine());
    }

    // コルーチン本体(IEnumeratorを返すメソッド)
    private System.Collections.IEnumerator CountdownCoroutine()
    {
        // 指定した時間(WaitTime)だけここで待つ
        yield return new WaitForSeconds(WaitTime);

        // 待機が終わったらTurnOnを実行
        TurnOn();
    }

    /// <summary>
    /// オルゴールが起動する際の処理
    /// タイマーなどから呼び出され、異常状態をONにして音を鳴らします
    /// </summary>
    private void TurnOn()
    {
        IsWaiting = false; // 待機状態を終了
        IsPlaying = true;

        // 3Dサウンドの再生開始
        if(OrgelAudioSource != null)
        {
            OrgelAudioSource.Play();
        }

        // GameManagerを直接呼ばず、イベントを発火するだけ
        OnOrgelStarted?.Invoke(this);

        UpdateColorAndLayer();
        Debug.Log("<color=red>【Orgel】オルゴールが勝手に鳴り出しました！</color>");
    }

    /// <summary>
    /// 外部から呼ばれてOFFにするメソッド
    /// </summary>
    public void TurnOff()
    {
        // 鳴っている時だけ消せる
        if(IsPlaying)
        {
            IsPlaying = false;

            // 3Dサウンドの再生を停止
            if(OrgelAudioSource != null)
            {
                OrgelAudioSource.Stop();
            }

            // イベントを発火するだけ
            OnOrgelStarted?.Invoke(this);

            UpdateColorAndLayer();
            Debug.Log("<color=green>【Orgel】オルゴールを止めました。</color>");
        }
    }

    /// <summary>
    /// IsPlayingの状態に応じてオブジェクトの色とレイヤーを変更する自作のメソッド
    /// </summary>
    private void UpdateColorAndLayer()
    {
        if (_objRenderer != null) _objRenderer.material.color = IsPlaying ? Color.red : Color.white;

        if(IsPlaying)
        {
            int targetLayer = LayerMask.NameToLayer(HighlightLayerName);
            if (targetLayer != -1) gameObject.layer = targetLayer;
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}