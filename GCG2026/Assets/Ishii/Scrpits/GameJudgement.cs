using UnityEngine;

public class GameJudgement : MonoBehaviour
{
    // 汎用フェードコンポーネント
    [SerializeField]
    private GenericFader _genericFader;

    // ゲームクリアシーン名
    [SerializeField]
    private string _gameClearSceneName;
    // ゲームオーバーシーン名
    [SerializeField]
    private string _gameOverSceneName;


    // ゲームマネージャー
    private GameManager _gameManager;

    void Start()
    {
        // ゲームマネージャー取得
        _gameManager = GameManager.Instance;
    }

    // ゲーム
    void Update()
    {
        if (_gameManager.IsGameClear)
        {
            // ゲームクリアになったら
            _genericFader.StartFadeOutAndLoad(1.0f, _gameClearSceneName);

        }
        else if (_gameManager.IsGameOver)
        {
            // ゲームオーバーになったら
            _genericFader.StartFadeOutAndLoad(1.0f, _gameOverSceneName);
        }
    }
}
