using UnityEngine;

/// <summary>
/// セレクト画面の項目選択を管理するクラス
/// </summary>
public class Select : MonoBehaviour
{
    [Header("Select Setting")]

    [Tooltip("選択中に表示するチェックマーク")]
    public GameObject checkMark;

    [Tooltip("クリックしたときに移動するシーン名")]
    public string sceneToLoad;

    [Tooltip("フェードを管理するスクリプト")]
    public NewMonoBehaviourScript fadeController;


    [Header("SE Setting")]

    [SerializeField]
    [Tooltip("SEを再生するAudioSource")]
    private AudioSource audioSource;

    [SerializeField]
    [Tooltip("カーソルを項目に合わせたときのSE")]
    private AudioClip cursorMoveSE;

    [SerializeField]
    [Tooltip("項目を決定したときのSE")]
    private AudioClip decisionSE;

    [SerializeField]
    [Tooltip("タイトルへ戻るときのSE")]
    private AudioClip backSE;

    [SerializeField]
    [Tooltip("この項目がタイトルへ戻るボタンならON")]
    private bool isBackButton;


    /// <summary>
    /// マウスカーソルが項目に入ったとき
    /// </summary>
    private void OnMouseEnter()
    {
        // チェックマークを表示
        checkMark.SetActive(true);

        // カーソル移動SE
        if (audioSource != null && cursorMoveSE != null)
        {
            audioSource.PlayOneShot(cursorMoveSE);
        }
    }

    /// <summary>
    /// マウスカーソルが項目から出たとき
    /// </summary>
    private void OnMouseExit()
    {
        // チェックマークを非表示
        checkMark.SetActive(false);
    }

    /// <summary>
    /// 項目をクリックしたとき
    /// </summary>
    private void OnMouseDown()
    {
        // Titleへ戻る項目なら戻るSE
        if (isBackButton)
        {
            if (audioSource != null && backSE != null)
            {
                audioSource.PlayOneShot(backSE);
            }
        }
        else
        {
            // 通常の決定SE
            if (audioSource != null && decisionSE != null)
            {
                audioSource.PlayOneShot(decisionSE);
            }
        }

        // フェードしてシーン移動
        fadeController.FadeAndLoadScene(sceneToLoad, 1.0f);
    }
}