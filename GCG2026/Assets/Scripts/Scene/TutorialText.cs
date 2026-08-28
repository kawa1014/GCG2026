using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private CanvasGroup bubbleGroup;

    [SerializeField]
    private TextMeshProUGUI tutorialText;

    [Header("文字送り")]
    [SerializeField]
    private float interval = 0.05f;

    private Coroutine typewriterCoroutine;

    public bool IsTyping { get; private set; }

    /// <summary>
    /// 指定した文章を1文字ずつ表示する
    /// </summary>
    public void StartTypewriter(string fullText)
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        typewriterCoroutine = StartCoroutine(TypeRoutine(fullText));
    }

    private IEnumerator TypeRoutine(string fullText)
    {
        IsTyping = true;

        bubbleGroup.alpha = 1f;
        bubbleGroup.interactable = false;
        bubbleGroup.blocksRaycasts = false;

        tutorialText.text = fullText;
        tutorialText.maxVisibleCharacters = 0;

        // TextMeshProに文章の文字数を計算させる
        tutorialText.ForceMeshUpdate();

        int characterCount = tutorialText.textInfo.characterCount;

        for (int i = 0; i <= characterCount; i++)
        {
            tutorialText.maxVisibleCharacters = i;

            if (interval > 0f)
            {
                yield return new WaitForSecondsRealtime(interval);
            }
        }

        IsTyping = false;
        typewriterCoroutine = null;
    }

    /// <summary>
    /// 文字送り中の文章をすべて表示する
    /// </summary>
    public void CompleteTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        tutorialText.maxVisibleCharacters = int.MaxValue;
        IsTyping = false;
    }

    /// <summary>
    /// 吹き出しを非表示にする
    /// </summary>
    public void Hide()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        IsTyping = false;
        bubbleGroup.alpha = 0f;
    }
}
