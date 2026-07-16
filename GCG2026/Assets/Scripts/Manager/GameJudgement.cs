using UnityEngine;

public class GameJudgement : MonoBehaviour
{
    // 汎用フェードコンポーネント
    [SerializeField]
    private GenericFader _genericFader;

    // ゲームクリアシーン名
    [SerializeField]
    private string _gameClearSceneName;
    // シーン遷移までの余韻
    [SerializeField]
    private float _gameClearTransitionDelay = 1.0f;
    // フェードアウトにかかる時間
    [SerializeField]
    private float _gameClearFadeOutDuration = 1.0f;

    // ゲームオーバーシーン名
    [SerializeField]
    private string _gameOverSceneName;
    // シーン遷移までの余韻
    [SerializeField]
    private float _gameOverTransitionDelay = 3.0f;
    // フェードアウトにかかる時間
    [SerializeField]
    private float _gameOverFadeOutDuration = 1.0f;

    // 停止対象のプレイヤー
    [SerializeField]
    private GameObject _stopTargetPlayer;
    // ゲームクリア時にプレイヤーの操作を停止するかどうか
    [SerializeField]
    private bool _isGameClearPlayerStop = false;
    // ゲームオーバー時にプレイヤーの操作を停止するかどうか
    [SerializeField]
    private bool _isGameOverPlayerStop = true;

    // ゲーム終了済みかどうか
    private bool _gameFinished = false;

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
        if (_gameFinished) return;

        if (_gameManager.IsGameClear)
        {
            // ゲームクリアになったら

            // Delay分待ってから遷移開始
            Invoke(nameof(StartGameClear), _gameClearTransitionDelay);

            // プレイヤーの操作を効かなくする
            if (_isGameClearPlayerStop)
                _stopTargetPlayer.GetComponent<PlayerController>()._isStop = true;

            _gameFinished = true;
        }
        else if (_gameManager.IsGameOver)
        {
            // ゲームオーバーになったら

            // Delay分待ってから遷移開始
            Invoke(nameof(StartGameOver), _gameOverTransitionDelay);

            // プレイヤーの操作を効かなくする
            if (_isGameOverPlayerStop)
                _stopTargetPlayer.GetComponent<PlayerController>()._isStop = true;

            _gameFinished = true;
        }
    }

    // ゲームクリアにフェードアウトしながら遷移
    private void StartGameClear()
    {
        _genericFader.StartFadeOutAndLoad(_gameClearFadeOutDuration, _gameClearSceneName);
    }

    // ゲームオーバーにフェードアウトしながら遷移
    private void StartGameOver()
    {
        _genericFader.StartFadeOutAndLoad(_gameOverFadeOutDuration, _gameOverSceneName);
    }
}
