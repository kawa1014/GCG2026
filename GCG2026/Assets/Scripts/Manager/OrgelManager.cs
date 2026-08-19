using System.Collections.Generic; // Listを使うために必要な機能が入っています
using System.Linq;                // リストの中身を条件で絞り込んだ(Where)するための便利な機能が入っています
using UnityEngine;

/// <summary>
/// マップ上のすべてのオルゴールを一元管理し、
/// 完全なランダム抽選による再生指示を出す「オルゴール専用の司令塔」です。
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

    [Header("抽選除外設定")]
    /// <summary>
    /// プレイヤーの位置を特定するためのTransform
    /// </summary>
    [Tooltip("プレイヤーのTransformを設定します。設定されていない場合はタグ「Player」から自動検索します")]
    public Transform PlayerTransform;

    /// <summary>
    /// 抽選から除外するプレイヤー周辺の半径(メートル)
    /// </summary>
    [Tooltip("この半径(m)以内にいるオルゴールは抽選から除外され、遠くのものが鳴るようになります。")]
    public float ExclusionRadius = 10.0f;

    [Header("待機時間設定")]
    /// <summary>
    /// オルゴールが抽選されてから鳴るまでの最小待機時間(秒)
    /// </summary>
    [Tooltip("オルゴールが抽選されてから鳴るまでの最小待機時間(秒)")]
    public float MinWaitTime = 5.0f;

    /// <summary>
    /// オルゴールが抽選されてから鳴るまでの最大待機時間(秒)
    /// </summary>
    [Tooltip("オルゴールが抽選されてから鳴るまでの最大待機時間(秒)")]
    public float MaxWaitTime = 15.0f;

    /// <summary>
    /// 現在鳴っているオルゴールの数。
    /// GameManagerが恐怖度を計算するためにここを読み取ります。
    /// </summary>
    public int CurrentOrgelPlayingCount { get; private set; } = 0;

    /// <summary>
    /// 現在抽選されて待機中、もしくは鳴っている最新のオルゴールを取得するためのプロパティ
    /// </summary>
    public OrgelSystem CurrentTargetOrgel { get; private set; }

    /// <summary>
    /// 現在のリストの何番目の順番を十個すいているかを記憶する番号
    /// </summary>
    private int _currentPhaseIndex = 0;

    /// <summary>
    /// シーン内に存在するすべてのオルゴールのリスト
    /// </summary>
    private List<OrgelSystem> _allOrgels = new List<OrgelSystem>();

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
        // プレイヤーがインスペクターで設定されていない場合、タグで自動検索する
        if (PlayerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                PlayerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("【OrgelManager】Playerタグを持つオブジェクトが見つかりません。距離による除外処理が機能しません。");
            }
        }

        // シーン内に存在するすべてのオルゴールを自動取得してリストに登録
        _allOrgels = FindObjectsByType<OrgelSystem>(FindObjectsSortMode.None).ToList();

        // 最初のオルゴールを抽選
        if (_allOrgels.Count > 0)
        {
            ChooseNextOrgel();
        }
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
        if (_allOrgels == null || _allOrgels.Count == 0) return;

        // 1. 全オルゴールの中から、まだ鳴っていない・待機していない安全なものを絞り込む
        List<OrgelSystem> validCandidates = _allOrgels.Where(o => !o.IsPlaying && !o.IsWaiting).ToList();

        if (validCandidates.Count == 0)
        {
            Debug.LogWarning("【OrgelManager】現在鳴らせるオルゴールがありません。");
            return;
        }

        // 2. プレイヤーとの距離を計算し、近すぎるもの(ExclusionRadius以内)を除外する
        List<OrgelSystem> filteredCandidates = new List<OrgelSystem>();

        if (PlayerTransform != null)
        {
            foreach (var orgel in validCandidates)
            {
                // プレイヤーとオルゴールの距離を計算
                float distanceToPlayer = Vector3.Distance(PlayerTransform.position, orgel.transform.position);

                // 設定した除外半径より遠ければ、抽選候補に入れる
                if (distanceToPlayer > ExclusionRadius)
                {
                    filteredCandidates.Add(orgel);
                }
            }
        }
        else
        {
            filteredCandidates = validCandidates;
        }

        // 【セーフティ機能】もし全てのオルゴールが近すぎて候補が0になってしまった場合は距離制限を無視する
        if (filteredCandidates.Count == 0 && validCandidates.Count > 0)
        {
            Debug.LogWarning("【OrgelManager】除外範囲外(遠く)に鳴らせるオルゴールがありません。範囲制限を一時的に無視して抽選します。");
            filteredCandidates = validCandidates;
        }

        // 3. 最終的な候補の中から1つを完全にランダムで抽選
        OrgelSystem nextOrgel = filteredCandidates[Random.Range(0, filteredCandidates.Count)];

        // 待機時間をMinとMaxの間からランダムに決定する
        float waitTime = Random.Range(MinWaitTime, MaxWaitTime);

        // 選ばれたオルゴールにカウントダウンをスタートさせる
        nextOrgel.StartCountdown(waitTime);
 
        // 選ばれたオルゴールを記録する
        CurrentTargetOrgel = nextOrgel;

        Debug.Log($"<color=green>【OrgelManager】次弾装填：{nextOrgel.gameObject.name} が抽選されました（{waitTime:F1}秒後に鳴ります）。</color>");
    }

    /// <summary>
    /// Unityエディタ上で選択した際に、除外範囲を視覚的に確認するためのギズモを描画します
    /// </summary>
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // プレイヤーのTransformがインスペクターでセットされている場合のみ描画
        if (PlayerTransform != null)
        {
            // ギズモの色を半透明の赤色に設定
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);

            // プレイヤーの現在位置を中心に、除外半径(ExclusionRadius)の大きさの球体を描画
            Gizmos.DrawWireSphere(PlayerTransform.position, ExclusionRadius);
        }
    }
#endif
}