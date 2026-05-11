using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// オルゴールの状態管理と3Dサウンド制御を行うクラス
/// ランダムなタイミングで起動してAudioSourceを再生し、プレイヤーのアクションによって止められます
/// </summary>
public class OrgelSystem : MonoBehaviour
{
    [Header("サウンド設定")]
    /// <summary>
    /// @brief オルゴールの音を鳴らすためのコンポーネント
    /// </summary>
    [Tooltip("3Dサウンド設定を行ったAudioSourceをアタッチしてください")]
    public AudioSource orgelAudioSource;

    [Header("時間指定")]
    [Tooltip("このオルゴールが抽選されてからなりだすまでの待機時間(秒)")]
    public float waitTime = 10.0f;

    /// <summary>
    /// 現在音が鳴っているかどうかの状態
    /// 外部(これから作る敵管理スクリプトなど)から読み取れるようにpublicにしています
    /// </summary>
    [Tooltip("現在音が鳴っているか(ON/OFF)")]
    [HideInInspector] public bool isPlaying = false;

    /// <summary>
    /// 現在抽選されて出番待ちかどうか
    /// </summary>
    [HideInInspector] public bool isWaiting = false;

    /// <summary>
    /// オブジェクトの色を変更するための描画コンポーネントを保持しておく変数
    /// </summary>
    private Renderer objRenderer;
    private float timer; // 次に鳴るまでのカウントダウンタイマー

    /// <summary>
    /// ゲーム開始時に1回だけ呼ばれる初期化処理
    /// </summary>
    private void Start()
    {
        // 自分がくっついているオブジェクトのRendererを取得して保存
        objRenderer = GetComponent<Renderer>();

        if (orgelAudioSource != null) orgelAudioSource.Stop();

        isWaiting = false;
        isPlaying = false;

        // 初期の状態に合わせて色を設定する
        UpdateColor();
    }

    /// <summary>
    ///  GameManagerから「次はお前だ」と抽選されたときに呼ばれる
    /// </summary>
    public void StartCountdown()
    {
        isWaiting = true;
        StartCoroutine(CountdownCoroutine());
    }

    // コルーチン本体(IEnumeratorを返すメソッド)
    private System.Collections.IEnumerator CountdownCoroutine()
    {
        // 指定した時間(waitTime)だけここで待つ
        yield return new WaitForSeconds(waitTime);

        // 待機が終わったらTurnOnを実行
        TurnOn();
    }

    /// <summary>
    /// オルゴールが起動する際の処理
    /// タイマーなどから呼び出され、異常状態をONにして音を鳴らします
    /// </summary>
    private void TurnOn()
    {
        isWaiting = false; // 待機状態を終了
        isPlaying = true;

        // 3Dサウンドの再生開始
        if(orgelAudioSource != null)
        {
            orgelAudioSource.Play();
        }
        
        // GameManagerに「鳴った」と報告する(ここで次の1個が連鎖的に抽選される)
        if(GameManager.instance != null)
        {
            GameManager.instance.AddPlayingOrgel();
        }

        UpdateColor();
        Debug.Log("<color=red>【Orgel】オルゴールが勝手に鳴り出しました！</color>");
    }

    /// <summary>
    /// 外部から呼ばれてOFFにするメソッド
    /// </summary>
    public void TurnOff()
    {
        // 鳴っている時だけ消せる
        if(isPlaying)
        {
            isPlaying = false;

            // 3Dサウンドの再生を停止
            if(orgelAudioSource != null)
            {
                orgelAudioSource.Stop();
            }

            if(GameManager.instance != null)
            {
                GameManager.instance.RemovePlayingOrgel();
            }

            UpdateColor();
            Debug.Log("<color=green>【Orgel】オルゴールを止めました。</color>");
        }
    }

    /// <summary>
    /// isPlayingの状態に応じてオブジェクトの色とレイヤーを変更する自作のメソッド
    /// </summary>
    private void UpdateColor()
    {
        if (objRenderer == null) return;
        objRenderer.material.color = isPlaying ? Color.red : Color.white;
    }
}