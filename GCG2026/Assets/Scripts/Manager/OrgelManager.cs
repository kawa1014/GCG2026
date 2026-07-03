using System.Collections.Generic; // Listを使うために必要な機能が入っています
using System.Linq;                // リストの中身を条件で絞り込んだ(Where)するための便利な機能が入っています
using UnityEngine;

/// <summary>
/// Managerのインスペクター上で「度のオルゴールが」「何秒後に鳴るか」をセットで設定するためのクラスです
/// [System.Serializable]を付けることで、Unityのインスペクター画面にリストとして表示・編集できるようになります
/// </summary>
[System.Serializable]
public class OrgelSetup
{
    /// <summary>
    /// 設定対象となるオルゴール本体(シーン上のオブジェクト)
    /// </summary>
    public OrgelSystem Orgel;

    /// <summary>
    /// このオルゴールが抽選されてから鳴りだすまでの待機時間(秒)
    /// </summary>
    [Tooltip("このオルゴールが鳴りだすまでの待機時間")]
    public float OrgelSoundWaitTime = 10.0f;
}

/// <summary>
/// マップ上のすべてのオルゴールを一元管理し、
/// 抽選や時間設定の指示を出す「オルゴール専用の司令塔」です
/// </summary>
public class OrgelManager : MonoBehaviour
{
    /// <summary>
    /// 他のスクリプトからOrgelManager.Instanceで簡単にアクセスできるようにする変数(シングルトン)
    /// </summary>
    public static OrgelManager Instance {  get; private set; }

    [Header("オルゴール一覧と個別時間設定")]
    /// <summary>
    /// インスペクター上でオルゴールとその待機時間をまとめて登録・管理するためのリスト
    /// このリストの「上(0番目)から順番に」鳴っていきます
    /// </summary>
    [Tooltip("ここにシーン内のオルゴールを登録し、個別の待機時間を設定できます")]
    public List<OrgelSetup> OrgelList = new List<OrgelSetup>();

    /// <summary>
    /// 現在鳴っているオルゴールの数。
    /// GameManagerが恐怖度を計算するためにここを読み取ります。
    /// </summary>
    public int CurrentOrgelPlayingCount { get; private set; } = 0;

    /// <summary>
    /// 現在リストの何番目のオルゴールを狙っているかを記憶する番号(インデックス)
    /// </summary>
    private int _currentIndex = 0;

    /// <summary>
    /// ゲーム開始時に1度だけ呼ばれ、OrgelManagerがシーンに1つだけ存在するように設定(シングルトン)
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// スクリプトが有効になった時に呼ばれ、オルゴールが鳴った/止まった時のイベント(合図)を受け取る準備をします
    /// </summary>
    private void OnEnable()
    {
        OrgelSystem.OnOrgelStarted += HandleOrgelStarted;
        OrgelSystem.OnOrgelStopped += HandleOrgelStopped;
    }

    /// <summary>
    /// スクリプトが無効になった時に呼ばれ、イベントの受け取りを解除します(エラー防止のため)
    /// </summary>
    private void OnDisable()
    {
        OrgelSystem.OnOrgelStarted -= HandleOrgelStarted;
        OrgelSystem.OnOrgelStopped -= HandleOrgelStopped;
    }

    /// <summary>
    /// ゲーム開始時の初期化処理。リストが空なら自動取得し、最初のオルゴールのカウントダウンを始めます
    /// </summary>
    void Start()
    {
        // もしインスペクター上でリストが空っぽのママ開始されたら、シーン内のオルゴールを自動でかき集めて登録する
        if (OrgelList.Count == 0)
        {
            OrgelSystem[] orgels = FindObjectsByType<OrgelSystem>(FindObjectsSortMode.None);
            foreach (var o in orgels)
            {
                OrgelList.Add(new OrgelSetup { Orgel = o, OrgelSoundWaitTime = 10.0f });
            }
        }

        // リストの0番目(一番上)のオルゴールを鳴らす準備を始める
        _currentIndex = 0;
        ChooseNextOrgel();
    }

    /// <summary>
    /// どこかのオルゴールが鳴り始めたときに呼ばれる処理。鳴っている数を増やします
    /// </summary>
    /// <param name="orgel">鳴り始めたオルゴールの本体</param>
    private void HandleOrgelStarted(OrgelSystem orgel)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        CurrentOrgelPlayingCount++;
    }

    /// <summary>
    /// どこかのオルゴールが止められた時に呼ばれる処理。鳴っている数を減らし、次を呼び出します
    /// </summary>
    /// <param name="orgel">止められたオルゴールの本体</param>
    private void HandleOrgelStopped(OrgelSystem orgel)
    {
        CurrentOrgelPlayingCount--;
        CurrentOrgelPlayingCount = Mathf.Max(0, CurrentOrgelPlayingCount);

        // ゲームオーバーでなければ、リストの「次のオルゴール」のカウントダウンを始める
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            ChooseNextOrgel();
        }
    }

    /// <summary>
    /// リストの上から順番に次のオルゴールを選び、カウントダウンを指示する処理です
    /// </summary>
    private void ChooseNextOrgel()
    {
        // リストが空っぽなら何もしない
        if (OrgelList == null || OrgelList.Count == 0) return;

        // もしリストの最後まで鳴り終わった場合、とりあえず最初(一番上)に戻ってループするようにしています。
        if (_currentIndex >= OrgelList.Count)
        {
            Debug.Log("【OrgelManager】リストのオルゴールをすべて鳴らしました。最初（一番上）からループさせます。");
            _currentIndex = 0;
        }

        // リストの「今の順番」のオルゴール情報を取得
        OrgelSetup nextSetup = OrgelList[_currentIndex];

        // もし登録されているオルゴールが空(None)だったり、なぜか既になっていた場合は、飛ばして次を探す
        if (nextSetup.Orgel == null || nextSetup.Orgel.IsPlaying || nextSetup.Orgel.IsWaiting)
        {
            _currentIndex++;
            ChooseNextOrgel(); // 再帰的に次を呼ぶ
            return;
        }

        // 選ばれたオルゴールに、設定された待機時間を渡してカウントダウンをスタートさせる
       // nextSetup.Orgel.StartCountdown(nextSetup.OrgelSoundWaitTime);

        Debug.Log($"<color=green>【OrgelManager】次弾装填：{nextSetup.Orgel.gameObject.name} が {nextSetup.OrgelSoundWaitTime}秒後に鳴ります。（リストの {_currentIndex + 1} 番目）</color>");

        //次回呼ばれた時にその次のオルゴールを鳴らすため、順番を1つ進めておく
    }
}
