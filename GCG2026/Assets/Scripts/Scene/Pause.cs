using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;
    private bool isPaused = false;

    [Header("Quit時の遷移先シーン名")]
    [SerializeField] private string _stageSelectSceneName = "SelectScene";

    void Start()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        Time.timeScale = 1.0f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // もしオプション画面が開いてる状態なら、ここで処理を中断する
            if (SceneManager.GetSceneByName("OptionScene").isLoaded) return;

            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseCanvas != null) pauseCanvas.SetActive(true);

        // 【問題2の解決】前回のテストでコメントアウトしていたら、必ず戻して時間を止める
        Time.timeScale = 0.0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        Time.timeScale = 1.0f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenOptionScene()
    {
        // 【問題1の解決】オプションを開く瞬間にポーズ画面のUIを一旦隠す
        if (pauseCanvas != null) pauseCanvas.SetActive(false);

        PlayerPrefs.SetInt("FromGameScene", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("OptionScene", LoadSceneMode.Additive);
    }

    // 【問題3の解決用】オプション画面を閉じたときに呼び出されて、ポーズ画面を復活させる
    public void ReturnFromOption()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
    }

    // Quitボタンに割り当てるメソッド
    public void QuitGame()
    {
        // 1. 止めていた時間を元に戻す（超重要）
        Time.timeScale = 1.0f;

        // 2. 遷移先のシーン（ステージセレクト画面）を読み込む
        SceneManager.LoadScene(_stageSelectSceneName);
    }
}