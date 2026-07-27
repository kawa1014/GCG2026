using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialText : MonoBehaviour
{
    public CanvasGroup bubbleGroup;
    public Transform imageParent;      // ← 画像を並べる親オブジェクト
    public Image imagePrefab;          // ← 1文字分の画像プレハブ
    public Sprite[] sprites;            // 表示したい画像を全部入れる
    public float interval = 0.1f;       // 画像を切り替える間隔


    public void StartTypewriter()
    {
        StopAllCoroutines();
        StartCoroutine(TypeRoutine());
    }

    private IEnumerator TypeRoutine()
    {
        bubbleGroup.alpha = 1;

        foreach (Sprite s in sprites)
        {
            // ★ 画像を複製して横に並べる
            Image img = Instantiate(imagePrefab, imageParent);
            img.sprite = s;

            yield return new WaitForSeconds(interval);
        }
    }
}
