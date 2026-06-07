using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic; // Listを使うために必要
using System.Linq; // シャッフルに便利

/// <summary>
/// @brief GameManager.cs
/// @brief ゲーム全体のルールを管理するクラス
/// @detalis
/// </summary>
public class GameManager : MonoBehaviour
{
    //---シングルトン---
    /// <summary>
    /// 他のスクリプトからGameManager.Instanceでアクセスできるようにする変数
    /// </summary>
    public static GameManager Instance { get; private set; }

    [Header("ゲームルール設定")]
    [Tooltip("ゲームクリアとなる制限時間(秒)")]
    public float TimeLimit = 180.0f;

    [Tooltip("ゲームオーバーになる最大恐怖度")]
    public float MaxFear = 100.0f;

    [Header("恐怖度設定")]
    [Tooltip("オルゴール1つにつき、1秒間に増加する恐怖度の量")]
    public float FearIncreaseRate = 2.0f;
    [Tooltip("すべてのオルゴールが止まっている時の1秒間の回復量")]
    public float FearRecoveryRate = 1.0f;

    [Header("抽選除外・階層判定設定")]
    [Tooltip("プレイヤーのTransform。距離判定に使用します")]
    public Transform PlayerTransform;
    [Tooltip("抽選から除外する半径(メートル")]
    public float ExclusionRadius = 5.0f;
    [Tooltip("同じ階層とみなすY座標の差(例：2m未満なら同じ階層)")]
    public float FloorHeightDifference = 2.0f;

    [Header("UI参照")]
    [Tooltip("残り時間を表示するTextMeshPro")]
    public TextMeshProUGUI TimeText;
    [Tooltip("恐怖度に応じて透明度が変わる画面の縁の赤いUIグループ")]
    public CanvasGroup FearVignetteGroup;

    //---内部状態を管理する変数---
    private float _currentFear = 0.0f; ///< 現在の恐怖度
    private int _currentPlayingOrgels = 0; ///< 現在なっているオルゴールの数
    private bool _isGameOver = false; ///< ゲームが終了したかどうかのフラグ

    /// <summary>
    /// @brief 初期化処理
    /// </summary>
    private void Awake()
    {
        // GameManagerがシーン内に1つだけになるようにする
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// イベントの購読(スクリプトが有効になった時)
    /// </summary>
    private void OnEnable()
    {
        OrgelSystem.OnOrgelStarted += HandleOrgelStarted;
        OrgelSystem.OnOrgelStopped += HandleOrgelStopped;
    }

    /// <summary>
    /// @brief ゲーム開始時の処理。UIの初期表示を行います
    /// </summary>
    private void Start()
    {
        UpdateTimerUI();
        UpdateFearUI();

        // プレイヤーがセットされていなければ自動取得
        if (PlayerTransform == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null) PlayerTransform = player.transform;
        }

        // 最初の一個目を抽選する
        ChooseNextOrgel();
    }

    /// <summary>
    /// @brief 毎フレーム呼ばれる処理
    /// </summary>
    private void Update()
    {
        // 終了済みの場合は何もしない
        if (_isGameOver) return;

        // 制限時間の処理
        TimeLimit -= Time.deltaTime;
        UpdateTimerUI();

        if (TimeLimit <= 0.0f)
        {
            GameClear();
            return;
        }

        // 恐怖度の処理(鳴っている数に応じて加算)
        if (_currentPlayingOrgels > 0)
        {
            // 1個でも鳴っていれば、一定速度で上昇
            _currentFear += FearIncreaseRate * Time.deltaTime;
        }
        else
        {
            // 全て止まっていれば徐々に回復
            _currentFear -= FearRecoveryRate * Time.deltaTime;
        }

        _currentFear = Mathf.Clamp(_currentFear, 0.0f, MaxFear);
        // 恐怖度のUIを更新
        UpdateFearUI();

        if (_currentFear >= MaxFear)
        {
            GameOver("恐怖度が限界に達した");
        }
    }

    /// <summary>
    /// イベントから呼ばれるメソッド等
    /// </summary>
    private void HandleOrgelStarted(OrgelSystem orgel)
    {
        if (_isGameOver) return;
        _currentPlayingOrgels++;
    }

    private void HandleOrgelStopped(OrgelSystem orgel)
    {
        _currentPlayingOrgels--;
        _currentPlayingOrgels = Mathf.Max(0, _currentPlayingOrgels);
        if (!_isGameOver) ChooseNextOrgel();
    }


    /// <summary>
    /// @brief オルゴールが鳴り始めた時にOrgelSystemから呼ばれるメソッド
    /// </summary>
    public void AddPlayingOrgel()
    {
        if (_isGameOver) return;
        _currentPlayingOrgels++;
    }

    /// <summary>
    /// @brief オルゴールが止められた時にOrgelSystemから呼ばれるメソッド
    /// </summary>
    public void RemovePlayingOrgel()
    {
        _currentPlayingOrgels--;
        // マイナスにならないように安全対策
        _currentPlayingOrgels = Mathf.Max(0, _currentPlayingOrgels);

        // オルゴールを解除した瞬間に、次の1つを抽選します
        if (!_isGameOver)
        {
            ChooseNextOrgel();
        }
    }

    /// <summary>
    /// 条件に合うオルゴールから1つをランダムに抽選する
    /// </summary>
    private void ChooseNextOrgel()
    {
        List<OrgelSystem> allOrgels = FindObjectsByType<OrgelSystem>(FindObjectsSortMode.None).ToList();

        // 候補1：まだ鳴っていなくて。かつ次の待機状態でないもの
        List<OrgelSystem> candidates = allOrgels.Where(o => !o.IsPlaying && !o.IsWaiting).ToList();

        if(PlayerTransform != null)
        {
            // 候補2：プレイヤーと同じ階層で、かつ5m以内のものを排除する
            candidates = candidates.Where(o =>
            {
                // Y座標の差を計算して階層が同じか判定
                bool isSameFloor = Mathf.Abs(o.transform.position.y - PlayerTransform.position.y) < FloorHeightDifference;

                // 高さを無視した平面(x, z)での距離を計算
                Vector2 playerPos2D = new Vector2(PlayerTransform.position.x, PlayerTransform.position.z);
                Vector2 orgelPos2D = new Vector2(o.transform.position.x, o.transform.position.z);
                float distance = Vector2.Distance(playerPos2D, orgelPos2D);

                // 同じ階層かつ5m以内なら除外(falseを返す)
                if (isSameFloor && distance <= ExclusionRadius) return false;

                return true; // それ以外は候補に残す
            }).ToList();
        }

        // 候補が0になってしまった場合の安全対策(全部なっている、全部近くにある等)
        if(candidates.Count == 0)
        {
            Debug.LogWarning("【System】抽選条件に合うオルゴールがありません。応急措置として距離制限を無視して再抽選します。");
            candidates = allOrgels.Where(o => !o.IsPlaying && !o.IsWaiting).ToList();
            if (candidates.Count == 0) return; // それでもなければ何もしない
        }

        // の子xg蔦候補からランダムに1つ選んでカウントダウン開始
        OrgelSystem nextOrgel = candidates[Random.Range(0, candidates.Count)];
        nextOrgel.StartCountdown();

        Debug.Log($"<color=green>【System】次弾装填：{nextOrgel.gameObject.name} が {nextOrgel.WaitTime}秒後に鳴ります。</color>");
    }

    /// <summary>
    /// @brief ゲームーバーの処理
    /// @brief reason ゲームオーバーの理由(コンソール表示用)
    /// </summary>
    public void GameOver(string reason)
    {
        _isGameOver = true;
        Debug.Log($"<color=red>【Game Over】{reason}</color>");

        if (TimeText != null)
        {
            TimeText.text = "GAME OVER";
        }

        // 今後ここでリトライ画面を表示する処理を作る
    }

    /// <summary>
    /// @brief ゲーム画面クリア
    /// </summary>
    private void GameClear()
    {
        _isGameOver = true;
        Debug.Log("<color=cyan>【Game Clear】朝まで生き延びた！</color>");

        if (TimeText != null)
        {
            TimeText.text = "SURVIVED";
        }

        // 今後ここでクリア画面を表示する処理を作る
    }

    /// <summary>
    /// @brief 残り時間をMM:SS形式でUIを表示する
    /// </summary>
    private void UpdateTimerUI()
    {
        if (TimeText == null) return;

        // 0秒未満にならないようにする
        float displayTime = Mathf.Max(0, TimeLimit);
        int minutes = Mathf.FloorToInt(displayTime / 60.0f);
        int seconds = Mathf.FloorToInt(displayTime % 60.0f);

        TimeText.text = $"{minutes:D2}:{seconds:D2}";
    }

    /// <summary>
    /// @brief 恐怖度に応じて、CanvasGroupの透明度を更新する
    /// </summary>
    private void UpdateFearUI()
    {
        if (FearVignetteGroup == null) return;

        // 恐怖度の割合(0.0～1.0)を計算し、CanvasGroupのAlphaに直接セットする
        // 恐怖度0で完全に透明、恐怖度100で真っ赤になります
        float fearRatio = _currentFear / MaxFear;
        FearVignetteGroup.alpha = fearRatio;
    }

    /// <summary>
    /// エディタ上で除外範囲を可視化する
    /// </summary>
    private void OnDrawGizmos()
    {
        if (PlayerTransform == null) return;

        // プレイヤーの周囲に除外範囲を表示(緑色の線)
        Gizmos.color = Color.green;

        // Unityには円を描く標準機能がないため、短い線をつなぎ合わせて円を描画します
        float radius = ExclusionRadius;
        int segments = 36; // 円の滑らかさ(36角形)
        float angleStep = 360.0f / segments;

        Vector3 prevPoint = PlayerTransform.position + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = PlayerTransform.position + new Vector3(
                Mathf.Cos(currentAngle) * radius,
                0,
                Mathf.Sin(currentAngle) * radius
                );

            // 線を繋ぐ
            Gizmos.DrawLine( prevPoint, nextPoint );
            prevPoint = nextPoint;
        }
    }
}
