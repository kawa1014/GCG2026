using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
    /// <summary>
    /// SEを再生するAudioSource
    /// </summary>
    [SerializeField]
    [Tooltip("タイトル画面のSEを再生するAudioSource")]
    private AudioSource audioSource;

    /// <summary>
    /// 決定したときのSE
    /// </summary>
    [SerializeField]
    [Tooltip("メニューを決定したときに鳴らすSE")]
    private AudioClip decisionSE;

    [SerializeField]
    [Tooltip("ゲームを終了するときに鳴らすSE")]
    private AudioClip exitSE;

    /// <summary>
    /// タイトルシーンへ移動
    /// </summary>
    public void Title()
    {
        SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// ステージセレクトへ移動
    /// </summary>
    public void Select()
    {
        StartCoroutine(PlaySEAndLoadScene("SelectScene"));
    }

    /// <summary>
    /// ゲームシーンへ移動
    /// </summary>
    public void Game()
    {
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// リザルトシーンへ移動
    /// </summary>
    public void Result()
    {
        SceneManager.LoadScene("ResultScene");
    }

    /// <summary>
    /// ゲームオーバーシーンへ移動
    /// </summary>
    public void GameOver()
    {
        SceneManager.LoadScene("GameOverScene");
    }

    /// <summary>
    /// オプション画面へ移動
    /// </summary>
    public void Option()
    {
        StartCoroutine(PlaySEAndLoadScene("OptionScene"));
    }

    /// <summary>
    /// 決定SEを鳴らしてからシーンを移動する
    /// </summary>
    /// <param name="sceneName">移動先のシーン名</param>
    private IEnumerator PlaySEAndLoadScene(string sceneName)
    {
        // 決定SEを再生
        audioSource.PlayOneShot(decisionSE);

        // SEが少し聞こえるまで待つ
        yield return new WaitForSecondsRealtime(0.25f);

        // シーン移動
        SceneManager.LoadScene(sceneName);
    }
}