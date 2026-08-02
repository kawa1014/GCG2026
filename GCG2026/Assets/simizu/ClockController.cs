using UnityEngine;

public class ClockController : MonoBehaviour
{
    public Transform hourHand;   // 短針
    public Transform minuteHand; // 長針

    [Header("時計の設定")]
    [Tooltip("ゲーム開始時の短針の角度（例: 9時の位置から始めるなら270を指定）")]
    [SerializeField] private float startHourAngle = 270f;

    [Tooltip("ゲーム終了までに長針が何周するか")]
    [SerializeField] private int minuteRotations = 3;

    private float _initialTime;

    private void Start()
    {
        // GameManagerから初期の制限時間を取得しておく
        if (GameManager.Instance != null)
        {
            _initialTime = GameManager.Instance.TimeLimit;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || _initialTime <= 0) return;

        // GameManagerから現在の残り時間を取得
        float currentTime = GameManager.Instance.TimeLimit;

        // 進行度を計算（0.0: ゲーム開始時 -> 1.0: ゲーム終了時）
        float progress = 1.0f - Mathf.Clamp01(currentTime / _initialTime);

        // --- 短針の処理: スタート時の角度から始まり、最終的に12時(360度)へ移動 ---
        // 12時の位置を360度（0度と同じ）として補間します
        float targetHourAngle = 360f;
        float currentHourAngle = Mathf.Lerp(startHourAngle, targetHourAngle, progress);
        hourHand.localRotation = Quaternion.Euler(0, 0, currentHourAngle);

        // --- 長針の処理: 指定した回数回転し、最終的に12時(0度/360度)で止まる ---
        // 進行度 × 360度 × 周回数 で計算します
        float minuteAngle = progress * 360f * minuteRotations;
        minuteHand.localRotation = Quaternion.Euler(0, 0, minuteAngle);
    }
}