using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// フェード演出を行い、シーン遷移（リトライやステージ選択）を管理するクラス
/// </summary>
public class GenericFaderTrue : MonoBehaviour
{
    [Header("フェード設定")]
    [SerializeField, Tooltip("フェード演出に使用する画面全体のUIイメージ")]
    private Image fadeImage;

    [SerializeField, Tooltip("フェードイン・フェードアウトにかかる時間（秒）")]
    private float _duration = 1.0f;

    /// <summary>
    /// 画面を徐々に表示するフェードイン処理
    /// </summary>
    /// <param name="duration">フェードにかける時間</param>
    public IEnumerator FadeIn(float duration)
    {
        float time = 0.0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            Color color = fadeImage.color;
            // 経過時間に応じて透明度を1(真っ暗)から0(透明)へ近づける
            color.a = 1.0f - (time / duration);
            fadeImage.color = color;

            yield return null;
        }

        Color endColor = fadeImage.color;
        endColor.a = 0.0f;
        fadeImage.color = endColor;
    }

    /// <summary>
    /// 画面を徐々に暗くするフェードアウト処理
    /// </summary>
    /// <param name="duration">フェードにかける時間</param>
    public IEnumerator FadeOut(float duration)
    {
        float time = 0.0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            Color color = fadeImage.color;
            // 経過時間に応じて透明度を0(透明)から1(真っ暗)へ近づける
            color.a = time / duration;
            fadeImage.color = color;

            yield return null;
        }

        Color endColor = fadeImage.color;
        endColor.a = 1.0f;
        fadeImage.color = endColor;
    }

    /// <summary>
    /// 【リスタートボタン用】
    /// フェードアウトを開始し、RetryManagerに保存されたシーン（死亡したシーン）を読み込む
    /// </summary>
    public void StartFadeOutAndLoad()
    {
        Debug.Log($"<color=yellow>[Fader] RetryManagerに保存されているシーン名: '{RetryManager.LastSceneName}'</color>");

        string targetScene = string.IsNullOrEmpty(RetryManager.LastSceneName)
            ? SceneManager.GetActiveScene().name
            : RetryManager.LastSceneName;

        Debug.Log($"<color=yellow>[Fader] 最終的に遷移しようとしているシーン名: '{targetScene}'</color>");

        StartCoroutine(FadeOutAndLoadRoutine(_duration, targetScene));
    }

    /// <summary>
    /// 【ステージセレクトボタン等用】
    /// UIボタンから呼び出し、引数で指定した任意のシーン名へフェードアウトして遷移する
    /// </summary>
    /// <param name="sceneName">遷移先のシーン名（例："SelectScene"）</param>
    public void LoadSpecificScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadRoutine(_duration, sceneName));
    }

    /// <summary>
    /// フェードアウトが完了した後にシーン遷移を行う実際のコルーチン
    /// </summary>
    private IEnumerator FadeOutAndLoadRoutine(float duration, string sceneName)
    {
        yield return FadeOut(duration);
        SceneManager.LoadScene(sceneName);
    }
}