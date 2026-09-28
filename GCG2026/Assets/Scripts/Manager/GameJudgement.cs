using UnityEngine;

public class GameJudgement : MonoBehaviour
{
    // 汎用フェードコンポーネント
    [SerializeField]
    private GenericFader _genericFader;

    // ＝＝＝ クリア関連の変数はインスペクターのエラーを防ぐため残すか、不要なら削除してOKです ＝＝＝
    [SerializeField]
    private string _gameClearSceneName;
    [SerializeField]
    private float _gameClearTransitionDelay = 1.0f;
    [SerializeField]
    private float _gameClearFadeOutDuration = 1.0f;
    [SerializeField]
    private bool _isGameClearPlayerStop = false;
    // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

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

    void Update()
    {
        if (_gameFinished) return;

        // ▼ クリア遷移は ClearMovieController が行うため、ここにあった IsGameClear の処理を削除しました。

        // ゲームオーバーになった場合のみ、こちらの処理を行う
        if (_gameManager.IsGameOver)
        {
            // Delay分待ってから遷移開始
            Invoke(nameof(StartGameOver), _gameOverTransitionDelay);

            // プレイヤーの操作を効かなくする
            if (_isGameOverPlayerStop && _stopTargetPlayer != null)
                _stopTargetPlayer.GetComponent<PlayerController>()._isStop = true;

            _gameFinished = true;
        }
    }

    // ゲームオーバーにフェードアウトしながら遷移
    private void StartGameOver()
    {
        _genericFader.StartFadeOutAndLoad(_gameOverFadeOutDuration, _gameOverSceneName);
    }
}