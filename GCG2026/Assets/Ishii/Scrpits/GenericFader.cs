using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GenericFader : MonoBehaviour
{
    // フェードするのに使うイメージ
    [SerializeField] private Image fadeImage;

    // フェードイン
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

    // フェードアウト
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

    // フェードアウト開始
    public void StartFadeInAndLoad(float duration, string sceneName)
    {
        StartCoroutine(FadeInAndLoad(duration , sceneName));
    }
    // フェードイン開始
    public void StartFadeOutAndLoad(float duration, string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(duration, sceneName));
    }


    // ルーチン
    private IEnumerator FadeInAndLoad(float duration, string sceneName)
    {
        yield return FadeIn(duration);

        if (sceneName != "")
            SceneManager.LoadScene(sceneName);
    }
    // ルーチン
    private IEnumerator FadeOutAndLoad(float duration, string sceneName)
    {
        yield return FadeOut(duration);

        if (sceneName != "")
            SceneManager.LoadScene(sceneName);
    }
}
