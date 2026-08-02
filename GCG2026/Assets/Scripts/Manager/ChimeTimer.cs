using UnityEngine;

public class ChimeTimer : MonoBehaviour
{
    [Header("オーディオ設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chimeSound;

    [Header("タイマー設定 (秒)")]
    [SerializeField] private float interval = 60f; // 鳴らす間隔

    private float _initialTime;
    private float _nextChimeTarget;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // GameManagerが存在すれば初期設定を行う
        if (GameManager.Instance != null)
        {
            _initialTime = GameManager.Instance.TimeLimit;
            // 最初に鳴る残り時間のターゲット（例: 180秒スタートで60秒間隔なら、次は120秒の時）
            _nextChimeTarget = _initialTime - interval;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // ゲームオーバーまたはクリア時は処理を止める
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsGameClear) return;

        // GameManagerから現在の残り時間を取得
        float currentTime = GameManager.Instance.TimeLimit;

        // 残り時間がターゲット時間を下回ったら（=一定時間経過したら）
        if (currentTime <= _nextChimeTarget && _nextChimeTarget > 0)
        {
            PlayChime();
            // 次に鳴らす時間を設定
            _nextChimeTarget -= interval;
        }
    }

    private void PlayChime()
    {
        if (audioSource != null && chimeSound != null)
        {
            audioSource.PlayOneShot(chimeSound);
        }
    }
}