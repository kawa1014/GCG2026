using UnityEngine;
using UnityEngine.SceneManagement;
using static SelectStage;

public class StageSelect : MonoBehaviour
{
    public GameObject checkMark;
    public string sceneToLoad;
    public NewMonoBehaviourScript fadeController;

    public bool isTitleButton = false;
    private bool isSelected = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseEnter()
    {
        if (!isSelected)
            checkMark.SetActive(true);
    }

    void OnMouseExit()
    {
        if (!isSelected)
            checkMark.SetActive(false);
    }

    void OnMouseDown()
    {
        if (isTitleButton)
        {
            fadeController.FadeAndLoadScene(sceneToLoad, 1.0f);
            return;
        }

        // トグル処理
        isSelected = !isSelected;

        if (isSelected)
        {
            // 選択された
            checkMark.SetActive(true);
            SelectedStage.stageName = sceneToLoad;
        }
        else
        {
            // 選択解除
            checkMark.SetActive(false);
            SelectedStage.stageName = "";
        }
    }
}
