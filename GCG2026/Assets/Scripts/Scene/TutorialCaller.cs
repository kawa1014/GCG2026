using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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
    private Coroutine delayedUICoroutine;
    private const int Phase4PageIndex = 3;

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

        if (delayedUICoroutine != null)
        {
            StopCoroutine(delayedUICoroutine);
            delayedUICoroutine = null;
        }


        HideAllPageUI();

        tutorial.StartTypewriter(pages[pageIndex]);

        if (pageIndex == Phase4PageIndex)
        {
            // フェーズ4だけ、全文表示後に画像を表示
            delayedUICoroutine =
                StartCoroutine(ShowUIAfterTyping(pageIndex));
        }
        else
        {
            // ほかのページは文章と同時にUIを表示
            ShowPageUI(pageIndex);
        }
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

    private IEnumerator ShowUIAfterTyping(int pageIndex)
    {
        // 文字送りが終了するまで待つ
        while (tutorial.IsTyping)
        {
            yield return null;
        }

        // 待っている間に別ページへ移動していたら表示しない
        if (currentPageIndex != pageIndex)
        {
            yield break;
        }

        ShowPageUI(pageIndex);
        delayedUICoroutine = null;
    }

    private void ShowPageUI(int pageIndex)
    {
        if (pageUIs == null || pageIndex < 0 || pageIndex >= pageUIs.Length ||
            pageUIs[pageIndex] == null)
        {
            return;
        }

        GameObject currentUI = pageUIs[pageIndex];
        currentUI.SetActive(true);

        // SAN値UIの場合はプレビューを開始
        SanTutorialPreview sanPreview = currentUI.GetComponent<SanTutorialPreview>();

        if (sanPreview != null)
        {
            sanPreview.PlayPreview();
        }
    }
}
