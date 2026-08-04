using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialText : MonoBehaviour
{
    public CanvasGroup bubbleGroup;
    public TextMeshProUGUI tutorialText; // 追加(川谷)画像の代わりにテキストボックスを登録する
    public float interval = 0.1f;       // 画像を切り替える間隔

    // 修正(川谷)外部から「この文章を表示してね」と文字列(string)を受け取る
    public void StartTypewriter(string fullText)
    {
        StopAllCoroutines();
        StartCoroutine(TypeRoutine(fullText));
    }

    // 修正(川谷)
    private IEnumerator TypeRoutine(string fullText)
    {
        bubbleGroup.alpha = 1;
        tutorialText.text = ""; // 最初にテキストを空っぽにする

        // 受け取った文章を1文字ずつ(char c)取り出して追加していく
        foreach (char c in fullText)
        {
            tutorialText.text += c; // 文字を後ろにくっつける
            yield return new WaitForSeconds(interval);
        }
    }
}
