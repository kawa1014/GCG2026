using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;
    private bool isPaused = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                PauseGame();
            else
                ResumeGame();
        }
    }

    public void ResumeGame()
    {
        pauseCanvas.SetActive(true);
        Time.timeScale = 1.0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PauseGame()
    {
        pauseCanvas.SetActive(false);
        Time.timeScale = 0.0f;   // ÉQÅ[ÉÄí‚é~
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
