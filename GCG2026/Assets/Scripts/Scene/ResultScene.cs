using UnityEngine;

public class ResulrScene : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject gameClearText;

    // ゲームオーバー表示
    public void ShowGameOver()
        {
        resultPanel.SetActive(true);
        gameOverText.SetActive(true);
        gameClearText.SetActive(false);
        }

    // ゲームクリア表示
    public void ShowGameClear()
    {
        resultPanel.SetActive(true);
        gameOverText.SetActive(false);
        gameClearText.SetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        resultPanel.SetActive(false);
        gameOverText.SetActive(false);
        gameClearText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
