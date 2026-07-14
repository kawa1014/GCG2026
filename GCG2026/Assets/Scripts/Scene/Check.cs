using UnityEngine;
using UnityEngine.SceneManagement;

public class Select : MonoBehaviour
{
    public GameObject checkMark;
    public string sceneToLoad;
    public NewMonoBehaviourScript fadeController;
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
        checkMark.SetActive(false);
    }

    void OnMouseDown()
    {
        fadeController.FadeAndLoadScene(sceneToLoad, 1.0f);
    }
}
