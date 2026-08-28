using UnityEngine;
using UnityEngine.Events;

// 修正(川谷)
public class TutorialCaller : MonoBehaviour
{
    [Header("吹き出し")]
    [SerializeField]
    private TutorialText tutorial;

    [Header("各ページの文章")]
    [Tooltip("ここに各ページの文章を入力します")]
    [TextArea(3, 5)]
    [SerializeField]
    private string[] pages;

    [Header("各ページで表示するUI")]
    [Tooltip("文章と同じ番号のUIが表示されます")]
    [SerializeField]
    private GameObject[] pageUIs;

    private int currentPageIndex;

    private bool tutorialFinished;

    private void Start()
    {
        currentPageIndex = 0;
        tutorialFinished = false;

        HideAllPageUI();

        if (tutorial == null)
        {
            Debug.LogError("TutorialTextが登録されていません", this);

            return;
        }

        if (pages == null || pages.Length == 0)
        {
            tutorial.Hide();
            return;
        }

        ShowPage(0);
    }

    private void Update()
    {
        if (tutorialFinished)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            OnClick();
        }
    }

    private void OnClick()
    {
        // 文字送り中なら、次ページには進まず全文表示する
        if (tutorial.IsTyping)
        {
            tutorial.CompleteTypewriter();
            return;
        }

        // 全文表示済みなら次ページへ進む
        int nextPageIndex = currentPageIndex + 1;

        if (nextPageIndex < pages.Length)
        {
            ShowPage(nextPageIndex);
        }
        else
        {
            FinishTutorial();
        }
    }

    private void ShowPage(int pageIndex)
    {
        currentPageIndex = pageIndex;

        HideAllPageUI();

        if (pageUIs != null && pageIndex < pageUIs.Length && pageUIs[pageIndex] != null)
        {
            GameObject currentUI = pageUIs[pageIndex];

            // 最初にUIを表示
            currentUI.SetActive(true);

            // SAN値UIならアニメーションを開始
            SanTutorialPreview sanPreview = currentUI.GetComponent<SanTutorialPreview>();

            if (sanPreview != null)
            {
                sanPreview.PlayPreview();
            }
        }

        tutorial.StartTypewriter(pages[pageIndex]);
    }

    private void HideAllPageUI()
    {
        if (pageUIs == null)
        {
            return;
        }

        foreach (GameObject pageUI in pageUIs)
        {
            if (pageUI != null)
            {
                pageUI.SetActive(false);
            }
        }
    }

    private void FinishTutorial()
    {
        tutorialFinished = true;

        HideAllPageUI();
        tutorial.Hide();
    }
}
