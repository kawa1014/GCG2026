using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 聞き耳の分かりやすさを上げるための追加UI制御クラス。
/// 既存のListeningEarUIを直接書き換えず、
/// 正面・距離・壁越しによってUIの強さを変える。
/// </summary>
public class ListeningHintUI : MonoBehaviour
{
    /// <summary>
    /// プレイヤー、またはMain CameraのTransform。
    /// </summary>
    [Header("参照")]
    [SerializeField] private Transform listenerTransform;

    /// <summary>
    /// 方向を示す赤い点。
    /// ListeningEarUIで使っているDirection Dotと同じものを入れる。
    /// </summary>
    [SerializeField] private RectTransform directionDot;

    /// <summary>
    /// 聞き耳の円背景。
    /// ListeningEarUIで使っているListen Circle Imageと同じものを入れる。
    /// </summary>
    [SerializeField] private Image listenCircleImage;

    /// <summary>
    /// 聞き耳UI全体。
    /// ListeningEarUIで使っているListen Ear UIと同じものを入れる。
    /// </summary>
    [SerializeField] private GameObject listenEarUI;

    /// <summary>
    /// オルゴールのTag名。
    /// </summary>
    [Header("検索")]
    [SerializeField] private string orgelTagName = "Orgel";

    /// <summary>
    /// 聞き耳キー。
    /// 既存のListeningEarUIと同じEキー。
    /// </summary>
    [SerializeField] private KeyCode listenKey = KeyCode.E;

    /// <summary>
    /// 聞こえる最大距離。
    /// </summary>
    [Header("距離")]
    [SerializeField] private float maxListenDistance = 35.0f;

    /// <summary>
    /// 一番近い扱いにする距離。
    /// </summary>
    [SerializeField] private float nearDistance = 2.0f;

    /// <summary>
    /// 鮮明に反応する正面角度。
    /// 小さいほど正面限定になる。
    /// </summary>
    [Header("指向性")]
    [SerializeField] private float clearAngle = 35.0f;

    /// <summary>
    /// この角度以上はかなり弱くする。
    /// </summary>
    [SerializeField] private float weakAngle = 140.0f;

    /// <summary>
    /// 後ろや横を向いた時の最低反応。
    /// </summary>
    [SerializeField] private float backSideRate = 0.08f;

    /// <summary>
    /// 壁として扱うLayer。
    /// Wall Layerを指定する。
    /// </summary>
    [Header("壁越し")]
    [SerializeField] private LayerMask wallLayer;

    /// <summary>
    /// 壁越しの時のUI反応倍率。
    /// </summary>
    [SerializeField] private float wallRate = 0.35f;

    /// <summary>
    /// Dotの最小サイズ。
    /// </summary>
    [Header("見た目")]
    [SerializeField] private float minDotScale = 0.25f;

    /// <summary>
    /// Dotの最大サイズ。
    /// </summary>
    [SerializeField] private float maxDotScale = 2.2f;

    /// <summary>
    /// 弱い時の円の色。
    /// </summary>
    [SerializeField] private Color weakColor = new Color(0.35f, 0.35f, 0.35f, 0.25f);

    /// <summary>
    /// 強い時の円の色。
    /// </summary>
    [SerializeField] private Color clearColor = new Color(1.0f, 0.0f, 0.0f, 0.75f);

    /// <summary>
    /// 壁越し時の色。
    /// </summary>
    [SerializeField] private Color wallColor = new Color(0.45f, 0.25f, 0.65f, 0.45f);

    /// <summary>
    /// 聞き耳モード中かどうか。
    /// </summary>
    private bool isListening = false;

    /// <summary>
    /// 毎フレーム、聞き耳の追加UI反応を更新する。
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(listenKey))
        {
            isListening = !isListening;
        }

        if (!isListening)
        {
            return;
        }

        if (listenerTransform == null || directionDot == null)
        {
            return;
        }

        Transform playingOrgel = FindPlayingOrgel();

        if (playingOrgel == null)
        {
            return;
        }

        bool blockedByWall;
        float clarity = CalculateClarity(playingOrgel, out blockedByWall);

        ApplyUI(clarity, blockedByWall);
    }

    /// <summary>
    /// 現在鳴っているオルゴールを探す。
    /// </summary>
    /// <returns>鳴っているオルゴール。なければnull。</returns>
    private Transform FindPlayingOrgel()
    {
        GameObject[] orgels = GameObject.FindGameObjectsWithTag(orgelTagName);

        foreach (GameObject orgel in orgels)
        {
            AudioSource audioSource = orgel.GetComponent<AudioSource>();

            if (audioSource != null && audioSource.isPlaying)
            {
                return orgel.transform;
            }
        }

        return null;
    }

    /// <summary>
    /// 距離・正面角度・壁越しから聞き耳の鮮明度を計算する。
    /// </summary>
    /// <param name="target">鳴っているオルゴール。</param>
    /// <param name="blockedByWall">壁越しかどうか。</param>
    /// <returns>0.0～1.0の鮮明度。</returns>
    private float CalculateClarity(Transform target, out bool blockedByWall)
    {
        blockedByWall = false;

        Vector3 toTarget = target.position - listenerTransform.position;
        float distance = toTarget.magnitude;

        if (distance >= maxListenDistance)
        {
            return 0.0f;
        }

        Vector3 directionToTarget = toTarget.normalized;

        float distanceRate = Mathf.InverseLerp(maxListenDistance, nearDistance, distance);

        Vector3 forward = listenerTransform.forward;
        forward.y = 0.0f;

        Vector3 flatDirection = directionToTarget;
        flatDirection.y = 0.0f;

        if (forward.sqrMagnitude <= 0.0001f || flatDirection.sqrMagnitude <= 0.0001f)
        {
            return distanceRate;
        }

        forward.Normalize();
        flatDirection.Normalize();

        float angle = Vector3.Angle(forward, flatDirection);

        float directionRate;

        if (angle <= clearAngle)
        {
            directionRate = 1.0f;
        }
        else if (angle >= weakAngle)
        {
            directionRate = backSideRate;
        }
        else
        {
            directionRate = Mathf.Lerp(
                backSideRate,
                1.0f,
                Mathf.InverseLerp(weakAngle, clearAngle, angle)
            );
        }

        blockedByWall = Physics.Raycast(
            listenerTransform.position,
            directionToTarget,
            distance,
            wallLayer
        );

        float obstacleRate = blockedByWall ? wallRate : 1.0f;

        return Mathf.Clamp01(distanceRate * directionRate * obstacleRate);
    }

    /// <summary>
    /// 鮮明度に応じてUIを変化させる。
    /// </summary>
    /// <param name="clarity">聞き耳の鮮明度。</param>
    /// <param name="blockedByWall">壁越しかどうか。</param>
    private void ApplyUI(float clarity, bool blockedByWall)
    {
        float dotScale = Mathf.Lerp(minDotScale, maxDotScale, clarity);
        directionDot.localScale = new Vector3(dotScale, dotScale, 1.0f);

        if (listenCircleImage != null)
        {
            Color targetColor = blockedByWall
                ? Color.Lerp(weakColor, wallColor, clarity)
                : Color.Lerp(weakColor, clearColor, clarity);

            listenCircleImage.color = targetColor;
        }
    }
}