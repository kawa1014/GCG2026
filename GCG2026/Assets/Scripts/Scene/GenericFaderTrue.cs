using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GenericFaderTrue : MonoBehaviour
{
    // フェードするのに使うイメージ
    [SerializeField] private Image fadeImage;

    [SerializeField] private float _duration;
    [SerializeField] private string _sceneName;

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
    public void StartFadeOutAndLoad()
    {
        StartCoroutine(FadeOutAndLoad(_duration , _sceneName));
    }

    // ルーチン
    private IEnumerator FadeOutAndLoad(float duration, string sceneName)
    {
        yield return FadeOut(duration);
        SceneManager.LoadScene(sceneName);
    }
}
