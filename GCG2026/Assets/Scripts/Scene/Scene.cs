using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
    // タイトルシーン
    public void Title()
    {
        SceneManager.LoadScene("TitleScene");
    }

    // ステージセレクトシーン
    public void Select()
    {
        SceneManager.LoadScene("SelectScene");
    }

    // ゲームシーン
    public void Game()
    {
        SceneManager.LoadScene("GameScene");
    }

    // リザルトシーン
    public void Result()
    {
        SceneManager.LoadScene("ResultScene");
    }

    // ゲームオーバーシーン
    public void GameOver()
    {
        SceneManager.LoadScene("GameOverScene");
    }

    // オプションシーン
    public void Option()
    {
        SceneManager.LoadScene("OptionScene");
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
