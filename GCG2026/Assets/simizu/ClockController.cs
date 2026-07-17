using UnityEngine;

public class ClockController : MonoBehaviour
{
    public Transform hourHand;   // ’Zj (smolenidole)
    public Transform minuteHand; // ’·j (largenidole)

    private float duration = 180f; // 3•ª = 180•b
    private float targetHourAngle =150f; // 5‚ÌˆÊ’ui150“xj

    void Update()
    {
        // Œo‰ßŠÔ
        float time = Time.time;

        // --- ’Zj‚Ìˆ—: 3•ª‚©‚¯‚Ä5‚ÌˆÊ’u‚ÖˆÚ“®‚µA‚»‚±‚Å~‚Ü‚é ---
        float progress = Mathf.Clamp01(time / duration);
        float currentHourAngle = progress * targetHourAngle;
        hourHand.localRotation = Quaternion.Euler(0, 0, currentHourAngle);

        // --- ’·j‚Ìˆ—: 3•ª‚Å1ü‚µ‘±‚¯‚é ---
        float minuteAngle = ((time % duration) / duration * 360f);
        minuteHand.localRotation = Quaternion.Euler(0, 0, minuteAngle);
    }
}