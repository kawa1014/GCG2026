using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInImage : MonoBehaviour
{
    public CanvasGroup targetGroup;   // ← 画像の CanvasGroup
    public float delay = 1f;          // ← 表示までの待ち時間
    public float fadeTime = 0.5f;     // ← フェード時間
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetGroup.alpha = 0;        // 最初は透明
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        // 1秒待つ
        yield return new WaitForSeconds(delay);

        // フェードイン
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            targetGroup.alpha = Mathf.Lerp(0, 1, t / fadeTime);
            yield return null;
        }

        targetGroup.alpha = 1; // 最終的に完全表示
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
