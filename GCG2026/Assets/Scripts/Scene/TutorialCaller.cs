using UnityEngine;

// 修正(川谷)
public class TutorialCaller : MonoBehaviour
{
    public TutorialText tutorial;

    [Tooltip("ここに各ページの文章を入力します")]
    [TextArea(3, 5)] // インスペクターで文章を改行して入力しやすくする便利機能
    public string[] pages;

    // 追加(川谷)現在何ページ目を表示しているかを記憶する変数
    private int currentPageIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ページが1つ以上設定されていれば、最初のページを表示
        if (pages.Length > 0)
        {
            tutorial.StartTypewriter(pages[0]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 左クリックで次のページへ
        if (Input.GetMouseButtonDown(0))
        {
            currentPageIndex++;

            if (currentPageIndex < pages.Length)
            {
                // 次のページの文章を渡す
                tutorial.StartTypewriter(pages[currentPageIndex]);
            }
            else
            {
                // 全ページ終わった時の処理(吹き出しを見えなくする)
                tutorial.bubbleGroup.alpha = 0;
            }

        }

    }
}
