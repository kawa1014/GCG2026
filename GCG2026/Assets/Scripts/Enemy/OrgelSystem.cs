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

    /// <summary>
    /// 現在音が鳴っているかどうかの状態
    /// 外部(これから作る敵管理スクリプトなど)から読み取れるようにpublicにしています
    /// </summary>
    [Tooltip("現在音が鳴っているか(ON/OFF)")]
    public bool isPlaying = false;

    // 自動で音が鳴りだすまでの時間の幅を設定します
    [Header("自動ON設定")]
    [Tooltip("自動でONになるまでの最短時間(秒)")]
    public float minTime = 5.0f;
    [Tooltip("自動でONになるまでの最長時間(秒)")]
    public float maxTime = 15.0f;

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

        // 初期の状態に合わせて色を設定する
        UpdateColor();

        ResetTimer(); // 最初のカウントダウンを開始
    }

    private void Update()
    {
        // 音が止まっている時だけ、タイマーを減らしていく
        if(!isPlaying)
        {
            timer -= Time.deltaTime;

            // タイマーが0以下になったら勝手に鳴りだす
            if(timer <= 0f)
            {
                TurnOn();
            }
        }
    }

    /// <summary>
    /// オルゴールが起動する際の処理
    /// タイマーなどから呼び出され、異常状態をONにして音を鳴らします
    /// </summary>
    private void TurnOn()
    {
        isPlaying = true;

        // 3Dサウンドの再生開始
        if(orgelAudioSource != null)
        {
            orgelAudioSource.Play();
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

            UpdateColor();
            ResetTimer(); // 消したら、また次になるまでのタイマーをセットする
            Debug.Log("<color=green>【Orgel】オルゴールを止めました。</color>");
        }
    }

    /// <summary>
    /// ランダム時間を計算してタイマーにセットする処理
    /// </summary>
    private void ResetTimer()
    {
        timer = Random.Range(minTime, maxTime);
        Debug.Log($"<color=gray>【Orgel】次に鳴るまであと {timer:F1} 秒...</color>");
    }

    /// <summary>
    /// isPlayingの状態に応じてオブジェクトの色を変更する自作のメソッド
    /// </summary>
    private void UpdateColor()
    {
        // 取得したRendererの中にあるMaterialの色を変更します
        if (isPlaying)
        {
            // 音が鳴っている状態(ON)は赤色
            objRenderer.material.color = Color.red;
        }
        else
        {
            // 音が止まっている状態(OFF)は白色
            objRenderer.material.color = Color.white;
        }
    }
}