using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// フェード演出を行い、シーン遷移（特にリトライ時の元のシーンへの遷移）を管理するクラス
/// </summary>
public class GenericFaderTrue : MonoBehaviour
{
    [Header("フェード設定")]
    [SerializeField, Tooltip("フェード演出に使用する画面全体のUIイメージ")]
    private Image fadeImage;

    [SerializeField, Tooltip("フェードイン・フェードアウトにかかる時間（秒）")]
    private float _duration = 1.0f;

    // ※ _sceneName はインスペクターから設定せず、RetryManagerから動的に取得するため削除しました

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

            yield return null; // 1フレーム待機
        }

        // 最終的な色を完全に透明にして補正
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

            yield return null; // 1フレーム待機
        }

        // 最終的な色を完全に不透明(黒など)にして補正
        Color endColor = fadeImage.color;
        endColor.a = 1.0f;
        fadeImage.color = endColor;
    }

    /// <summary>
    /// フェードアウトを開始し、RetryManagerに保存されたシーン（死亡したシーン）を読み込む
    /// </summary>
    public void StartFadeOutAndLoad()
    {
        // RetryManagerに保存されているシーン名を取得
        // ※直接起動した場合などでシーン名が空の場合は、安全策として現在のシーンを読み込む
        string targetScene = string.IsNullOrEmpty(RetryManager.LastSceneName)
            ? SceneManager.GetActiveScene().name
            : RetryManager.LastSceneName;

        // フェードアウトとシーン読み込みのコルーチンを開始
        StartCoroutine(FadeOutAndLoadRoutine(_duration, targetScene));
    }

    /// <summary>
    /// フェードアウトが完了した後にシーン遷移を行う実際のコルーチン
    /// </summary>
    private IEnumerator FadeOutAndLoadRoutine(float duration, string sceneName)
    {
        // フェードアウトが完全に終わるまで待機
        yield return FadeOut(duration);

        // ターゲットのシーンを読み込む
        SceneManager.LoadScene(sceneName);
    }
}