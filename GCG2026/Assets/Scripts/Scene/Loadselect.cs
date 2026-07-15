using UnityEngine;
using UnityEngine.SceneManagement;
using static SelectStage;

public class LoadSelect : MonoBehaviour
{
    public GameObject checkMark;
    public NewMonoBehaviourScript fadeController;
    public bool isLoadGameButton = false;
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
        checkMark.SetActive(true);
    }

    void OnMouseExit()
    {
        //checkMark.SetActive(false);
    }

    void OnMouseDown()
    {
        if (string.IsNullOrEmpty(SelectedStage.stageName))
        {
            Debug.Log("ステージが選択されていません");
            return;
        }
        checkMark.SetActive(true);
        fadeController.FadeAndLoadScene(SelectedStage.stageName, 1.0f);
    }
}
