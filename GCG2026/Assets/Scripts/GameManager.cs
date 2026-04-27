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
    /// 他のスクリプトからGameManager.instanceでアクセスできるようにする変数
    /// </summary>
    public static GameManager instance;

    [Header("ゲームルール設定")]
    [Tooltip("ゲームクリアとなる制限時間(秒)")]
    public float timeLimit = 180.0f;

    [Tooltip("ゲームオーバーになる最大恐怖度")]
    public float maxFear = 100.0f;

    [Tooltip("同時に鳴ると即ゲームオーバーになるオルゴールの数")]
    public int maxSimultaneousOrgels = 5;

    [Header("選抜システムで設定")]
    [Tooltip("今回プレイで実際に動かすオルゴールの数")]
    public int sessionActiveOrgelCount = 5;

    [Header("恐怖度設定")]
    [Tooltip("オルゴール1つにつき、1秒間に増加する恐怖度の量")]
    public float fearIncreaseRate = 2.0f;
    [Tooltip("すべてのオルゴールが止まっている時の1秒間の回復量")]
    public float fearRecoveryRate = 1.0f;

    [Header("オルゴールの出現設定")]
    [Tooltip("オルゴールが次に鳴りだすまでの最短時間(秒)")]
    public float minOrgelWaitTime = 5.0f;
    [Tooltip("オルゴールが次に鳴りだすまでの最長時間")]
    public float maxOrgelWaitTime = 15.0f;

    [Header("UI参照")]
    [Tooltip("残り時間を表示するTextMeshPro")]
    public TextMeshProUGUI timeText;

    [Tooltip("恐怖度に応じて透明度が変わる画面の縁の赤いUIグループ")]
    public CanvasGroup fearVignetteGroup;

    //---内部状態を管理する変数---
    private float currentFear = 0.0f; ///< 現在の恐怖度
    private int currentPlayingOrgels = 0; ///< 現在なっているオルゴールの数
    private bool isGameOver = false; ///< ゲームが終了したかどうかのフラグ

    /// <summary>
    /// @brief 初期化処理
    /// </summary>
    private void Awake()
    {
        // GameManagerがシーン内に1つだけになるようにする
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// @brief ゲーム開始時の処理。UIの初期表示を行います
    /// </summary>
    private void Start()
    {
        UpdateTimerUI();
        UpdateFearUI();
        SetupOrgelSession(); // オルゴールの抽選処理
    }

    /// <summary>
    /// @brief 毎フレーム呼ばれる処理
    /// </summary>
    private void Update()
    {
        // 終了済みの場合は何もしない
        if (isGameOver) return;

        // 制限時間の処理
        timeLimit -= Time.deltaTime;
        UpdateTimerUI();

        if(timeLimit <= 0.0f)
        {
            GameClear();
            return;
        }

        // 恐怖度の処理(鳴っている数に応じて加算)
        if (currentPlayingOrgels > 0)
        {
            // 1個でも鳴っていれば、一定速度で上昇
            currentFear += fearIncreaseRate * Time.deltaTime;
        }
        else
        {
            // 全て止まっていれば徐々に回復
            currentFear -= fearIncreaseRate * Time.deltaTime;
        }

        currentFear = Mathf.Clamp(currentFear, 0.0f, maxFear);
        // 恐怖度のUIを更新
        UpdateFearUI();

        if(currentFear >= maxFear)
        {
            GameOver("恐怖度が限界に達した");
        }
    }

    /// <summary>
    /// シーン内の全オルゴールかｒ規定数だけを選んで起動する
    /// </summary>
    private void SetupOrgelSession()
    {
        // シーン内の全てのOrgelSystemを探してリストに入れる
        List<OrgelSystem> allOrgels = FindObjectsByType<OrgelSystem>(FindObjectsSortMode.None).ToList();

        Debug.Log($"<color=white>【System】合計 {allOrgels.Count} 個のオルゴールを検知。</color>");

        // リストをランダムに並び替える
        List<OrgelSystem> shuffledOrgels = allOrgels.OrderBy(x => System.Guid.NewGuid()).ToList();

        // 順番に有効・無効を指示していく
        for(int i = 0; i < shuffledOrgels.Count; i++)
        {
            // 設定した数以内なら有効、それ以外は無効
            bool shouldBeActive = (i < sessionActiveOrgelCount);
            shuffledOrgels[i].SetSessionActivity(shouldBeActive);

            if (shouldBeActive)
            {
                Debug.Log($"<color=green>【System】オルゴール {shuffledOrgels[i].name} を有効化しました。</color>");
            }
        }
    }

    /// <summary>
    /// @brief オルゴールが鳴り始めた時にOrgelSystemから呼ばれるメソッド
    /// </summary>
    public void AddPlayingOrgel()
    {
        currentPlayingOrgels++;
        Debug.Log($"<color=yellow>【GameManager】鳴っているオルゴール: {currentPlayingOrgels} / {maxSimultaneousOrgels}</color>");

        // 同時発火数の判定(規定数に達したらゲームオーバー)
        if(currentPlayingOrgels >= maxSimultaneousOrgels)
        {
            GameOver("オルゴールの音が許容量を超え、意識を刈り取られた...");
        }
    }

    /// <summary>
    /// @brief オルゴールが止められた時にOrgelSystemから呼ばれるメソッド
    /// </summary>
    public void RemovePlayingOrgel()
    {
        currentPlayingOrgels--;
        // マイナスにならないように安全対策
        currentPlayingOrgels = Mathf.Max(0, currentPlayingOrgels);
    }

    /// <summary>
    /// @brief ゲームーバーの処理
    /// @brief reason ゲームオーバーの理由(コンソール表示用)
    /// </summary>
    public void GameOver(string reason)
    {
        isGameOver = true;
        Debug.Log($"<color=red>【Game Over】{reason}</color>");

        if (timeText != null)
        {
            timeText.text = "GAME OVER";
        }

        // 今後ここでリトライ画面を表示する処理を作る
    }

    /// <summary>
    /// @brief ゲーム画面クリア
    /// </summary>
    private void GameClear()
    {
        isGameOver = true;
        Debug.Log("<color=cyan>【Game Clear】朝まで生き延びた！</color>");

        if (timeText != null)
        {
            timeText.text = "SURVIVED";
        }

        // 今後ここでクリア画面を表示する処理を作る
    }

    /// <summary>
    /// @brief 残り時間をMM:SS形式でUIを表示する
    /// </summary>
    private void UpdateTimerUI()
    {
        if (timeText == null) return;

        // 0秒未満にならないようにする
        float displayTime = Mathf.Max(0, timeLimit);
        int minutes = Mathf.FloorToInt(displayTime / 60.0f);
        int seconds = Mathf.FloorToInt(displayTime % 60.0f);

        timeText.text = $"{minutes:D2}:{seconds:D2}";
    }

    /// <summary>
    /// @brief 恐怖度に応じて、CanvasGroupの透明度を更新する
    /// </summary>
    private void UpdateFearUI()
    {
        if (fearVignetteGroup == null) return;

        // 恐怖度の割合(0.0～1.0)を計算し、CanvasGroupのAlphaに直接セットする
        // 恐怖度0で完全に透明、恐怖度100で真っ赤になります
        float fearRatio = currentFear / maxFear;
        fearVignetteGroup.alpha = fearRatio;
    }
}
