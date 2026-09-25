using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("フェード・オーディオ設定")]
    [SerializeField] private Image fadeImage;
    [SerializeField, Tooltip("SEを再生するAudioSource")] private AudioSource audioSource;
    [SerializeField, Tooltip("決定したときに鳴らすSE")] private AudioClip decisionSE;
    [SerializeField, Tooltip("ゲームを終了するときに鳴らすSE")] private AudioClip exitSE;

    [Header("タイトル演出用")]
    [SerializeField, Tooltip("変化後の女の子の画像 (Image TypeをFilled, Leftに変更しておく)")]
    private Image girlAfterImage;

    public IEnumerator FadeIn(float duration)
    {
        float time = 0.0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            Color color = fadeImage.color;
            color.a = 1.0f - (time / duration);
            fadeImage.color = color;
            yield return null;
        }
        Color endColor = fadeImage.color;
        endColor.a = 0.0f;
        fadeImage.color = endColor;
    }

    public IEnumerator FadeOut(float duration)
    {
        float time = 0.0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            Color color = fadeImage.color;
            color.a = time / duration;
            fadeImage.color = color;
            yield return null;
        }
        Color endColor = fadeImage.color;
        endColor.a = 1.0f;
        fadeImage.color = endColor;
    }

    public void StartFadeIn(float duration)
    {
        StartCoroutine(FadeIn(duration));
    }

    public void StartFadeOut(float duration)
    {
        StartCoroutine(FadeOut(duration));
    }

    void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError("fadeImage が Inspector にセットされていません");
        }

        // ▼ここを変更：初期状態で変化後の画像を透明（アルファ値0）にしておく
        if (girlAfterImage != null)
        {
            Color startColor = girlAfterImage.color;
            startColor.a = 0f;
            girlAfterImage.color = startColor;
        }

        StartFadeIn(1.0f);
    }

    // ==========================================
    // タイトルからセレクトへの遷移（仕様書通りの演出を統合）
    // ==========================================
    public void OnClickStartButton1()
    {
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        // 1. 決定SEを再生
        if (audioSource != null && decisionSE != null)
        {
            audioSource.PlayOneShot(decisionSE);
        }

        // 2. 女の子の見た目変更（全体がじわっと浮かび上がるフェードイン）
        if (girlAfterImage != null)
        {
            // ★追加：念のためオブジェクトを強制的にアクティブにする
            girlAfterImage.gameObject.SetActive(true);

            // ★追加：コンソールにログを出して処理が走っているか確認
            Debug.Log("女の子の画像変化スタート！");

            float time = 0f;
            float changeDuration = 0.4f;
            Color fadeColor = girlAfterImage.color;

            while (time < changeDuration)
            {
                time += Time.deltaTime;
                fadeColor.a = Mathf.Clamp01(time / changeDuration);
                girlAfterImage.color = fadeColor;
                yield return null;
            }
            fadeColor.a = 1f;
            girlAfterImage.color = fadeColor;

            Debug.Log("女の子の画像変化完了！暗転開始");
        }
        else
        {
            // ★追加：もし枠に画像がセットされていなければエラーを出す
            Debug.LogError("インスペクターの Girl After Image に画像がセットされていません！");
        }

        // 3. 仕様書通り「0.5秒で暗転」
        yield return FadeOut(0.5f);

        // 4. シーン移動
        SceneManager.LoadScene("SelectScene");
    }
    // ==========================================
    // 既存の他のボタン遷移処理
    // ==========================================

    // セレクトからゲームへのフェード
    public void OnClickStartButton2()
    {
        StartCoroutine(FadeAndLoadRoutine("GameScene", 1.0f));
    }

    // ゲームからリザルトへのフェード
    public void OnClickStartButton3()
    {
        StartCoroutine(FadeAndLoadRoutine("ResultScene", 1.0f));
    }

    // リザルトからセレクトへのフェード
    public void OnClickStartButton4()
    {
        StartCoroutine(FadeAndLoadRoutine("SelectScene", 1.0f));
    }

    // リザルトからタイトルへのフェード
    public void OnClickStartButton5()
    {
        StartCoroutine(FadeAndLoadRoutine("TitleScene", 1.0f));
    }

    // タイトルからオプションへのフェード
    public void OnClickStartButton6()
    {
        if (audioSource != null && decisionSE != null) audioSource.PlayOneShot(decisionSE);
        StartCoroutine(FadeAndLoadRoutine("OptionScene", 1.0f));
    }

    // オプションからタイトルへのフェード
    public void OnClickStartButton7()
    {
        StartCoroutine(FadeAndLoadRoutine("TitleScene", 1.0f));
    }

    public void FadeAndLoadScene(string sceneName, float duration = 1.0f)
    {
        StartCoroutine(FadeAndLoadRoutine(sceneName, duration));
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName, float duration)
    {
        yield return FadeOut(duration);
        SceneManager.LoadScene(sceneName);
    }

    public void OnClickExitButton()
    {
        StartCoroutine(FadeAndQuit());
    }

    private IEnumerator FadeAndQuit()
    {
        if (audioSource != null && exitSE != null) audioSource.PlayOneShot(exitSE);
        yield return FadeOut(1.0f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}