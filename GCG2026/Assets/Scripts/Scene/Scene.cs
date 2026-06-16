using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
    public void Title()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void Select()
    {
        SceneManager.LoadScene("SelectScene");
    }

    public void Game()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void Result()
    {
        SceneManager.LoadScene("ResultScene");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
