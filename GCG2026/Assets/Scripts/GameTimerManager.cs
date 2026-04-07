using UnityEngine;

/// <summary>
/// ゲーム全体の制限時間を管理するクラス
/// 内部でカウントダウンを行い、ゼロになるとプレイヤーを消滅させます
/// </summary>
public class GameTimerManager : MonoBehaviour
{
    [Header("時間指定")]
    [Tooltip("制限時間(秒)　5分なら300")]
    public float timeLimit = 300.0f;

    // 現在の残り時間
    private float remainingTime;

    // ゲームオーバーになったかどうかのフラグ
    private bool isGameOver = false;

    private void Start()
    {
        // 残り時間を初期化
        remainingTime = timeLimit;
        Debug.Log($"<color=yellow>【Timer】ゲーム開始！制限時間は {timeLimit / 60:F0} 分です。</color>");
    }

    private void Update()
    {
        if (isGameOver) return;

        // 時間を減らす
        remainingTime -= Time.deltaTime;

        // --- デバッグ用ログ出力 ---
        // 1秒ごとにログを出すとコンソールが埋まるため、5秒おきに出力します
        if (Mathf.FloorToInt(remainingTime) % 5 == 0 && Time.frameCount % 60 == 0)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            Debug.Log($"<color=yellow>【Timer】残り時間: {minutes:D2}:{seconds:D2}</color>");
        }

        // タイムアップ判定
        if(remainingTime <= 0)
        {
            TimeUp();
        }
    }

    /// <summary>
    /// 時間切れの処理
    /// </summary>
    private void TimeUp()
    {
        isGameOver = true;
        remainingTime = 0;

        Debug.Log("<color=red>【Game Over】タイムアップ！制限時間に達しました。</color>");

        // シーン内のプレイヤーを探して削除する
        GameObject player = GameObject.FindWithTag("Player");

        // ※以前のステップでPlayerに"Player"タグを設定していない場合は、
        // FindAnyObjectByType<PlayerController>() で探します
        if (player == null)
        {
            PlayerController pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }

        if (player != null)
        {
            Destroy(player);
        }
    }
}
