using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

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
    /// <summary>
    /// ゲームクリアとなる制限時間(秒)
    /// </summary>
    [Tooltip("ゲームクリアとなる制限時間(秒)")]
    public float TimeLimit = 180.0f;

    /// <summary>
    /// ゲームオーバーになる最大の恐怖度
    /// </summary>
    [Tooltip("ゲームオーバーになる最大恐怖度")]
    public float MaxFear = 100.0f;

    [Header("恐怖度設定")]
    /// <summary>
    /// 1秒間に増加する恐怖度の量
    /// </summary>
    [Tooltip("オルゴール1つにつき、1秒間に増加する恐怖度の量")]
    public float FearIncreaseRate = 2.0f;

    /// <summary>
    /// 1秒間に減少(回復)する恐怖度の量
    /// </summary>
    [Tooltip("すべてのオルゴールが止まっている時の1秒間の回復量")]
    public float FearRecoveryRate = 1.0f;

    [Header("UI参照")]
    /// <summary>
    /// 残り時間を表示するTextMeshProのUI
    /// </summary>
    [Tooltip("残り時間を表示するTextMeshPro")]
    public TextMeshProUGUI TimeText;

    /// <summary>
    /// 恐怖度に応じて透明度(赤み)が変わる画面の縁のUIグループ
    /// </summary>
    [Tooltip("恐怖度に応じて透明度が変わる画面の縁の赤いUIグループ")]
    public CanvasGroup FearVignetteGroup;

    /// <summary>
    ///  SAN値のアニメーション
    /// </summary>
    [Header("SAN値UI設定")]
    public UnityEngine.UI.Image SanImage;
    public Sprite[] SanSprites;
    public float SanChangeInterval = 1.0f;

    private float _sanTimer = 0.0f;
    private int _sanIndex = 0;

    //---内部状態を管理する変数---
    private float _currentFear = 0.0f; ///< 現在の恐怖度
    private bool _isGameOver = false; ///< ゲームオーバーフラグ
    private bool _isGameClear = false; ///< ゲームクリアフラグ

    /// <summary>
    /// 外部(他のスクリプト)からゲームオーバーかどうかを確認するためのプロパティ
    /// </summary>
    public bool IsGameOver => _isGameOver;

    /// <summary>
    /// 外部(他のスクリプト)からゲームオーバーかどうかを確認するためのプロパティ
    /// </summary>
    public bool IsGameClear => _isGameClear;

    /// <summary>
    /// ゲーム開始時に1度だけ呼ばれ、GameManagerがシーンに1つだけ存在するように設定(シングルトン化)します
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
    /// @brief ゲーム開始時の処理。UIの初期表示を行います
    /// </summary>
    private void Start()
    {
        UpdateTimerUI();
        UpdateFearUI();
    }

    /// <summary>
    /// @brief 毎フレーム呼ばれる処理
    /// </summary>
    private void Update()
    {
        // 終了済みの場合は何もしない
        if (_isGameOver || _isGameClear) return;

        // 制限時間の処理
        TimeLimit -= Time.deltaTime;
        UpdateTimerUI();

        if (TimeLimit <= 0.0f)
        {
            GameClear();
            return;
        }

        // OrgelManagerに「今何個なってる？」と直接聞きに行くようにしました
        if (OrgelManager.Instance != null && OrgelManager.Instance.CurrentOrgelPlayingCount > 0)
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

        _sanTimer += Time.deltaTime;

        //if(_sanTimer >= SanChangeInterval)
        //{
        //    _sanTimer = 0.0f;

        //    // 次の画像へ
        //    _sanIndex++;

        //    // 範囲のチェック
        //    if (_sanIndex >= SanSprites.Length)
        //        _sanIndex = SanSprites.Length - 1;

        //    // Imageに変換
        //    if (SanImage != null)
        //        SanImage.sprite = SanSprites[_sanIndex];
        //}
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

        // 3秒後にQuitGameメソッドを実行してゲームを閉じる
        Invoke(nameof(QuitGame), 3.0f);

        // 今後ここでリトライ画面を表示する処理を作る
    }

    /// <summary>
    /// @brief ゲーム画面クリア
    /// </summary>
    private void GameClear()
    {
        _isGameClear = true;
        Debug.Log("<color=cyan>【Game Clear】朝まで生き延びた！</color>");

        if (TimeText != null)
        {
            TimeText.text = "SURVIVED";
        }

        // 3秒後にQuitGameメソッドを実行してゲームを閉じる
        Invoke(nameof(QuitGame), 3.0f);

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
        //if (FearVignetteGroup == null) return;

        // 恐怖度の割合(0.0～1.0)を計算し、CanvasGroupのAlphaに直接セットする
        // 恐怖度0で完全に透明、恐怖度100で真っ赤になります
       // float fearRatio = _currentFear / MaxFear;
       // FearVignetteGroup.alpha = fearRatio;

        // SANUI更新
        UpdateSanUI();
    }

    /// <summary>
    /// @brief エネミーに接触された際に、恐怖度を最大にして即座にゲームオーバーにするメソッド
    /// @details Enemy.csの接触判定から呼び出されます。
    /// </summary>
    public void MaxOutFearAndGameOver()
    {
        // 既にゲームオーバー状態なら処理を重複させないためにブロック
        if (_isGameOver || _isGameClear) return;

        // 恐怖度を強制的に最大値（MaxFear）に上書きする
        _currentFear = MaxFear;

        // 画面の赤いエフェクト（Vignette）を最大にするためにUIを更新
        UpdateFearUI();

        // 理由を添えてゲームオーバー処理を実行
        GameOver("エネミーに捕獲されたため、恐怖度が限界を突破した");
    }

    private void UpdateSanUI()
    {
        if (SanImage == null || SanSprites == null || SanSprites.Length == 0)
            return;

        // 恐怖度の割合（0.0 ～ 1.0）
        float fearRatio = _currentFear / MaxFear;

        // 画像インデックスを計算（0 ～ SanSprites.Length-1）
        int index = Mathf.FloorToInt(fearRatio * (SanSprites.Length - 1));

        // 範囲チェック
        index = Mathf.Clamp(index, 0, SanSprites.Length - 1);

        // Image に反映
        SanImage.sprite = SanSprites[index];
    }

    /// <summary>
    /// @brief ゲームアプリケーション自体を終了する処理
    /// @details Unityエディター上でのプレイ停止と、ビルド後のアプリ終了の両方に対応します
    /// </summary>
    private void QuitGame()
    {
        Debug.Log("ゲームを終了します");

#if UNITY_EDITOR
        // Unityエディターでプレイ中の場合は、プレイモードを停止する
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 実際にビルドされたゲームの場合は、アプリを終了する
        Application.Quit();
#endif
    }
}
