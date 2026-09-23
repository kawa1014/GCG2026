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

    [Header("視線誘導")]
    [SerializeField]
    private TutorialManager tutorialManager;
    private int currentPageIndex;

    private bool tutorialFinished;
    private Coroutine delayedUICoroutine;
    private const int Phase4PageIndex = 3;
    private bool canAdvancePage = false;
    private bool phase1Hidden = false;

    [Header("フェーズ3 SAN値UI")]
    [SerializeField]
    private GameObject sanTutorialUI;

    [SerializeField]private SanTutorialPreview sanTutorialPreview;
    private const int Phase3PageIndex = 2;

    [Header("通常のSAN値UI")]
    [SerializeField]
    private GameObject sanUI;
    private bool phase7Shown = false;
    private bool waitingForPhase7 = false;
    private const int Phase8PageIndex = 7;
    private bool waitingForOrgelStop = false;
    private const int Phase9PageIndex = 8;
    private bool waitingForPhase9Sound = false;
    private bool phase9Shown = false;
    public static bool CanUpdateSan
    {
        get;
        private set;
    }

    private void Start()
    {
        IsTutorialActive = true;
        CanUpdateSan = false;
        currentPageIndex = 0;
        tutorialFinished = false;

        HideAllPageUI();
        if (sanUI != null)
        {
            sanUI.SetActive(false);
        }
        if (pages != null && pages.Length > 0)
        {
            ShowPage(0);
        }
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

    private void OnEnable()
    {
        OrgelSystem.OnOrgelStarted += HandleOrgelStarted;
        OrgelSystem.OnOrgelStopped += HandleOrgelStopped;
    }

    private void OnDisable()
    {
        OrgelSystem.OnOrgelStarted -= HandleOrgelStarted;
        OrgelSystem.OnOrgelStopped -= HandleOrgelStopped;
    }

    private void OnClick()
    {
        // フェーズ1の処理
        if (currentPageIndex == 0 && !canAdvancePage)
        {
            // 文字送り中なら、最初のクリックで全文表示
            if (tutorial.IsTyping)
            {
                tutorial.CompleteTypewriter();
                return;
            }
            // 全文表示後のクリックで吹き出しを消す
            if (!phase1Hidden)
            {
                phase1Hidden = true;

                tutorial.Hide();
                HideAllPageUI();
            }
            return;
        }

        // フェーズ2以降の文字送り中
        if (tutorial.IsTyping)
        {
            tutorial.CompleteTypewriter();
            return;
        }

        if (currentPageIndex == Phase8PageIndex && waitingForOrgelStop)
        {
            Debug.Log("オルゴールを止めるまで次へ進めません");
            return;
        }

        if (waitingForPhase9Sound)
        {
            return;
        }

        // 全文表示後なら次のページへ
        int nextPageIndex = currentPageIndex + 1;
        const int phase7PageIndex = 6;

        // 次がフェーズ7なら、近づくまで吹き出しを非表示にして待つ
        if (nextPageIndex == phase7PageIndex)
        {
            waitingForPhase7 = true;

            tutorial.Hide();
            HideAllPageUI();

            return;
        }

        if (nextPageIndex < pages.Length)
        {
            if (currentPageIndex == 1 && tutorialManager != null)
            {
                tutorialManager.ReturnLookFromTutorialClick();
            }
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
        if (pageIndex == Phase8PageIndex)
        {
            waitingForOrgelStop = true;
            ShowPageUI(pageIndex);

            Debug.Log("【Tutorial】フェーズ8を表示しました");
            return;
        }
        if (pageIndex == Phase3PageIndex)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartSanSystemFromPhase3();
            }
            else
            {
                Debug.LogError("GameManager.Instanceがありません" );
            }
            return;
        }

        if (pageIndex == Phase4PageIndex)
        {
            // フェーズ4だけ、全文表示後に画像を表示
            delayedUICoroutine = StartCoroutine(ShowUIAfterTyping(pageIndex));
            return;
        }
           // ほかのページは文章と同時にUIを表示
           ShowPageUI(pageIndex);
    }

    private void HideAllPageUI()
    {
        if (pageUIs != null)
        {
            foreach (GameObject pageUI in pageUIs)
            {
                if (pageUI != null)
                {
                    pageUI.SetActive(false);
                }
            }
        }

        if (sanTutorialUI != null)
        {
            sanTutorialUI.SetActive(false);
        }
    }

    private void FinishTutorial()
    {
        tutorialFinished = true;

        if (delayedUICoroutine != null)
        {
            StopCoroutine(delayedUICoroutine);
            delayedUICoroutine = null;
        }
        HideAllPageUI();
        tutorial.Hide();
        // 最後に通常ゲームへ切り替える
        IsTutorialActive = false;
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
        SanTutorialPreview sanPreview = currentUI.GetComponentInChildren<SanTutorialPreview>(true);

        if (sanPreview != null)
        {
            sanPreview.PlayPreview();
        }
    }

    public void ShowPhase2()
    {
        // PagesのElement 1がフェーズ2
        const int phase2PageIndex = 1;

        if (pages == null || pages.Length <= phase2PageIndex)
        {
            return;
        }
        phase1Hidden = false;
        canAdvancePage = true;
        ShowPage(phase2PageIndex);
    }
    public void ShowPhase7()
    {
        const int phase7PageIndex = 6;
        if (phase7Shown)
        {
            return;
        }
        if (!waitingForPhase7)
        {
            return;
        }
        if (pages == null || pages.Length <= phase7PageIndex)
        {
            return;
        }

        phase7Shown = true;
        waitingForPhase7 = false;
        ShowPage(phase7PageIndex);
    }
    private void ShowSanTutorial()
    {
        if (sanTutorialUI == null)
        {
            return;
        }

        if (sanTutorialPreview == null)
        {
            return;
        }

        // SAN値UIを表示
        sanTutorialUI.SetActive(true);

        sanTutorialPreview.PlayPreview();
    }

    private void HandleOrgelStopped(OrgelSystem stoppedOrgel)
    {
        // フェーズ8以外で止まった場合は無視
        if (currentPageIndex != Phase8PageIndex)
        {
            return;
        }

        if (!waitingForOrgelStop)
        {
            return;
        }

        if (stoppedOrgel == null)
        {
            return;
        }

        waitingForOrgelStop = false;
        // フェーズ8の吹き出しとUIを消す
        tutorial.Hide();
        HideAllPageUI();

        // 次のオルゴールが鳴るまで待つ
        waitingForPhase9Sound = true;
    }

    private void HandleOrgelStarted(OrgelSystem startedOrgel)
    {
        if (!waitingForPhase9Sound)
        {
            return;
        }

        if (phase9Shown)
        {
            return;
        }

        if (startedOrgel == null)
        {
            return;
        }

        if (pages == null || pages.Length <= Phase9PageIndex)
        {
            return;
        }

        waitingForPhase9Sound = false;
        phase9Shown = true;

        ShowPage(Phase9PageIndex);
    }
    private void Awake()
    {
        IsTutorialActive = true;
    }
    public static bool IsTutorialActive
    {
        get;
        private set;
    }
}
