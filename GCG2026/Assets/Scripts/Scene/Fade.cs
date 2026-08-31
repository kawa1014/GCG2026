using System.Collections;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField]
    [Tooltip("SEを再生するAudioSource")]
    private AudioSource audioSource;

    [SerializeField]
    [Tooltip("決定したときに鳴らすSE")]
    private AudioClip decisionSE;

    [SerializeField]
    [Tooltip("ゲームを終了するときに鳴らすSE")]
    private AudioClip exitSE;

    public IEnumerator FadeIn(float duration)
    {
        float time = 0.0f;

        while(time < duration) 
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

        while(time < duration) 
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

        StartFadeIn(1.0f);
    }

    // タイトルからセレクトへのフェード
    public void OnClickStartButton1()
    {
        // 決定SEを再生
        audioSource.PlayOneShot(decisionSE);

        // フェードしてシーン移動
        StartCoroutine(FadeAndLoad1());
    }

    private IEnumerator FadeAndLoad1()
    {
        yield return FadeOut(1.0f);
        SceneManager.LoadScene("SelectScene");
    }

    // セレクトからゲームへのフェード
    public void OnClickStartButton2()
    {
        StartCoroutine(FadeAndLoad2());
    }
    private IEnumerator FadeAndLoad2()
    {
        yield return FadeOut(1.0f);
        SceneManager.LoadScene("GameScene");
    }

    // ゲームからリザルトへのフェード
    public void OnClickStartButton3()
    {
        StartCoroutine(FadeAndLoad3());
    }
    private IEnumerator FadeAndLoad3()
    {
        yield return FadeOut(1.0f);
        SceneManager.LoadScene("ResultScene");
    }

    // リザルトからセレクトへのフェード
    public void OnClickStartButton4()
    {
        StartCoroutine(FadeAndLoad4());
    }
    private IEnumerator FadeAndLoad4()
    {
        yield return FadeOut(1.0f);
        SceneManager.LoadScene("SelectScene");
    }

    // リザルトからタイトルへのフェード
    public void OnClickStartButton5()
    {
        StartCoroutine(FadeAndLoad5());
    }
    private IEnumerator FadeAndLoad5()
    {
        yield return FadeOut(1.0f);
        SceneManager.LoadScene("TitleScene");
    }

    // タイトルからオプションへのフェード
    public void OnClickStartButton6()
    {
        // 決定SEを再生
        audioSource.PlayOneShot(decisionSE);

        // フェードしてOptionSceneへ移動
        StartCoroutine(FadeAndLoad6());
    }
    private IEnumerator FadeAndLoad6()
    {
        yield return FadeOut(1.0f);
        SceneManager.LoadScene("OptionScene");
    }

    // オプションからタイトルへのフェード
    public void OnClickStartButton7()
    {
        StartCoroutine(FadeAndLoad7());
    }
    private IEnumerator FadeAndLoad7()
    {
        yield return FadeOut(1.0f);
        SceneManager.LoadScene("TitleScene");
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


    /// <summary>
    /// Exitボタンを押したとき
    /// </summary>
    public void OnClickExitButton()
    {
        StartCoroutine(FadeAndQuit());
    }

    /// <summary>
    /// Exit用SEを鳴らし、フェード後にゲームを終了する
    /// </summary>
    private IEnumerator FadeAndQuit()
    {
        // Exit専用SEを再生
        audioSource.PlayOneShot(exitSE);

        // フェードアウト
        yield return FadeOut(1.0f);

        // Unity Editor上での終了処理
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // ビルドしたゲームを終了
    Application.Quit();
#endif
    }
}
