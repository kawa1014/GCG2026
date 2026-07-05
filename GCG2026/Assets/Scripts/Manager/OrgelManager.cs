using System.Collections.Generic; // Listを使うために必要な機能が入っています
using System.Linq;                // リストの中身を条件で絞り込んだ(Where)するための便利な機能が入っています
using UnityEngine;

/// <summary>
/// Managerのインスペクター上で「どのオルゴールが」「何秒後に鳴るか」をセットで設定するためのクラスです
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
/// 鳴る順番ごとに、複数のオルゴール候補をまとめるためのクラスです
/// </summary>
[System.Serializable]
public class OrgelPhase
{
    /// <summary>
    /// この順番に鳴った時に、抽選されるオルゴールの候補リスト
    /// </summary>
    [Tooltip("この順番の時に抽選されるオルゴールの候補リスト")]
    public List<OrgelSetup> Candidates = new List<OrgelSetup>();
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

    // マスターボリュームのスライダー
    [Header("全体音量設定")]
    /// <summary>
    /// すべてのオルゴールの基準となる音量(0.0～1.0)
    /// </summary>
    [Tooltip("すべてのオルゴールの基準となる音量です(0.0で無音、1.0で最大)")]
    [Range(0.0f, 1.0f)]
    public float MasterVolume = 1.0f;

    [Header("順番ごとのオルゴール設定")]
    /// <summary>
    /// 順番のリストです。上から順に進みます
    /// 各順番の中に複数のオルゴールを登録すると、その中からランダムで1つ抽選されます
    /// </summary>
    [Tooltip("上から順に順番が進みます。各順番の中で複数のオルゴールを登録すると、その中からランダムで1つが選ばれます")]
    public List<OrgelPhase> PhaseList = new List<OrgelPhase>();

    /// <summary>
    /// 現在鳴っているオルゴールの数。
    /// GameManagerが恐怖度を計算するためにここを読み取ります。
    /// </summary>
    public int CurrentOrgelPlayingCount { get; private set; } = 0;

    /// <summary>
    /// 現在のリストの何番目の順番を十個すいているかを記憶する番号
    /// </summary>
    private int _currentPhaseIndex = 0;

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
        if (PhaseList.Count == 0)
        {
            OrgelSystem[] orgels = FindObjectsByType<OrgelSystem>(FindObjectsSortMode.None);
            foreach (var o in orgels)
            {
                OrgelPhase newPhase = new OrgelPhase();
                newPhase.Candidates.Add(new OrgelSetup { Orgel = o, OrgelSoundWaitTime = 10.0f });
                PhaseList.Add(newPhase);
            }
        }

        // リストの0番目(一番上)のオルゴールを鳴らす準備を始める
        _currentPhaseIndex = 0;
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
        if (PhaseList == null || PhaseList.Count == 0) return;

        // もしリストの最後まで鳴り終わった場合、とりあえず最初(一番上)に戻ってループするようにしています。
        if (_currentPhaseIndex >= PhaseList.Count)
        {
            Debug.Log("【OrgelManager】リストのオルゴールをすべて鳴らしました。最初（一番上）からループさせます。");
            _currentPhaseIndex = 0;
        }

        // 現在の順番のデータを取得
        OrgelPhase currentPhase = PhaseList[_currentPhaseIndex];

        // 候補の中から、まだ鳴っていない・待機していない安全なものを絞り込む
        List<OrgelSetup> validCandidates = currentPhase.Candidates.Where(setup => setup.Orgel != null && !setup.Orgel.IsPlaying && !setup.Orgel.IsWaiting).ToList();

        // もし今の順番の候補がすべて使用不可だった場合、スキップして次の順番へ進む
        if (validCandidates.Count == 0)
        {
            Debug.LogWarning($"【OrgelManager】フェーズ {_currentPhaseIndex} の候補に鳴らせるオルゴールがありません。次のフェーズにスキップします。");
            _currentPhaseIndex++;
            ChooseNextOrgel(); // 再帰的に次を呼ぶ
            return;
        }

        // 有効な候補の中から1つをランダムで抽選
        OrgelSetup nextSetup = validCandidates[Random.Range(0, validCandidates.Count)];

        // 選ばれたオルゴールに、設定された待機時間を渡してカウントダウンをスタートさせる
        nextSetup.Orgel.StartCountdown(nextSetup.OrgelSoundWaitTime);

        Debug.Log($"<color=green>【OrgelManager】次弾装填：フェーズ {_currentPhaseIndex} から {nextSetup.Orgel.gameObject.name} が抽選されました（{nextSetup.OrgelSoundWaitTime}秒後に鳴ります）。</color>");

        // 次回呼ばれたときに次のフェーズに進むため、順番を1つ進めておく
        _currentPhaseIndex++;
    }
}
