using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;
    private bool isPaused = false;

    void Start()
    {
        // 開始時は確実にポーズ画面を消し、時間を動かしておく
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        Time.timeScale = 1.0f;
    }

    void Update()
    {
        // Escキーでポーズと再開を切り替える
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    /// <summary>
    /// ゲームを一時停止する
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        Time.timeScale = 0.0f;   // ゲーム停止

        // ポーズ中はUIのボタンを押せるようにカーソルを表示する
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// ゲームを再開する
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        Time.timeScale = 1.0f;   // ゲーム再開

        // ゲーム中は視点移動のためにカーソルを隠す
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// ポーズ画面からオプション画面を呼び出す
    /// </summary>
    public void OpenOptionScene()
    {
        // 普通にロードするとプレイ中のゲーム画面が破棄されてしまうため、
        // 「LoadSceneMode.Additive」を使ってゲーム画面の上にオプションシーンを重ねて表示します。
        SceneManager.LoadScene("OptionScene", LoadSceneMode.Additive);
    }
}