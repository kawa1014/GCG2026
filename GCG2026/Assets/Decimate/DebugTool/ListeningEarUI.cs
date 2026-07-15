using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 聞き耳UIを制御するクラス。
/// 鳴っているオルゴールを自動で探し、その方向を360度で表示する。
/// </summary>
public class ListeningEarUI : MonoBehaviour
{
    /// <summary>
    /// 聞き耳UI全体。
    /// </summary>
    [Header("UI")]
    [SerializeField] private GameObject listenEarUI;

    /// <summary>
    /// 方向を示す赤い点。
    /// </summary>
    [SerializeField] private RectTransform directionDot;

    /// <summary>
    /// 聞き耳の円背景。
    /// 距離に応じて色を変える。
    /// </summary>
    [SerializeField] private Image listenCircleImage;

    /// <summary>
    /// プレイヤー、またはメインカメラのTransform。
    /// </summary>
    [Header("Player")]
    [SerializeField] private Transform playerTransform;

    /// <summary>
    /// Dotを円の中心からどれだけ離すか。
    /// </summary>
    [Header("Settings")]
    [SerializeField] private float radius = 70.0f;

    /// <summary>
    /// プレイヤー操作を止めるための参照。
    /// </summary>
    [SerializeField] private PlayerController playerController;

    /// <summary>
    /// この距離より近いと最大反応になる。
    /// </summary>
    [SerializeField] private float minDistance = 2.0f;

    /// <summary>
    /// この距離より遠いと最小反応になる。
    /// </summary>
    [SerializeField] private float maxDistance = 20.0f;

    /// <summary>
    /// Dotの最小サイズ。
    /// </summary>
    [SerializeField] private float minDotScale = 0.7f;

    /// <summary>
    /// Dotの最大サイズ。
    /// </summary>
    [SerializeField] private float maxDotScale = 1.8f;

    /// <summary>
    /// 高さの差をどれだけ距離として扱うか。
    /// 1階と2階の差を少し遠く感じさせるための倍率。
    /// </summary>
    [SerializeField] private float heightDistanceMultiplier = 1.5f;

    /// <summary>
    /// 階が違うと判定する高さの差。
    /// 例：2.5なら、Y座標が2.5以上違うと別階扱い。
    /// </summary>
    [SerializeField] private float floorHeightThreshold = 2.5f;

    /// <summary>
    /// 階が違う場合に追加する距離。
    /// 真上・真下にいても少し遠く感じさせる。
    /// </summary>
    [SerializeField] private float differentFloorPenalty = 5.0f;

    /// <summary>
    /// 遠い時の円の色。
    /// </summary>
    [SerializeField] private Color farCircleColor = new Color(1.0f, 1.0f, 1.0f, 0.35f);

    /// <summary>
    /// 近い時の円の色。
    /// </summary>
    [SerializeField] private Color nearCircleColor = new Color(1.0f, 0.0f, 0.0f, 0.65f);
    /// <summary>
    /// 聞き耳を使うキー。
    /// </summary>
    [SerializeField] private KeyCode listenKey = KeyCode.E;

    /// <summary>
    /// オルゴールに付けるタグ名。
    /// </summary>
    [SerializeField] private string orgelTagName = "Orgel";



    /// <summary>
    /// 現在鳴っているオルゴール。
    /// </summary>
    private Transform currentPlayingOrgel;

    /// <summary>
    /// 聞き耳モード中かどうか。
    /// trueなら聞き耳中。
    /// </summary>
    private bool isListeningMode = false;

    /// <summary>
    /// 毎フレーム、聞き耳UIを更新する。
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(listenKey))
        {
            isListeningMode = !isListeningMode;

            if (playerController != null)
            {
                playerController.SetCanMove(!isListeningMode);
            }

            if (!isListeningMode)
            {
                ResetListenUI();
            }
        }

        if (listenEarUI != null)
        {
            listenEarUI.SetActive(isListeningMode);
        }

        if (!isListeningMode)
        {
            return;
        }

        currentPlayingOrgel = FindPlayingOrgel();

        if (currentPlayingOrgel == null)
        {
            ResetListenUI();
            return;
        }

        if (directionDot != null)
        {
            directionDot.gameObject.SetActive(true);
        }

        UpdateDirectionDot(currentPlayingOrgel);
    }

    /// <summary>
    /// 鳴っているAudioSourceを持つオルゴールを探す。
    /// </summary>
    /// <returns>鳴っているオルゴールのTransform。なければnull。</returns>
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
    /// プレイヤーから見たオルゴールの方向を計算し、DirectionDotを円周上に移動させる。
    /// </summary>
    /// <param name="target">方向を表示したい対象。</param>
    private void UpdateDirectionDot(Transform target)
    {
        if (directionDot == null || playerTransform == null || target == null)
        {
            return;
        }

        Vector3 toTarget = target.position - playerTransform.position;

        // 高さの差は無視する。
        toTarget.y = 0.0f;

        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        toTarget.Normalize();

        Vector3 forward = playerTransform.forward;
        forward.y = 0.0f;
        forward.Normalize();

        Vector3 right = playerTransform.right;
        right.y = 0.0f;
        right.Normalize();

        float x = Vector3.Dot(right, toTarget);
        float y = Vector3.Dot(forward, toTarget);

        Vector2 dotPosition = new Vector2(x, y) * radius;

        directionDot.anchoredPosition = dotPosition;

        float distance = CalculateListeningDistance(target);

        float distanceRate = Mathf.InverseLerp(maxDistance, minDistance, distance);

        float dotScale = Mathf.Lerp(minDotScale, maxDotScale, distanceRate);

        directionDot.localScale = new Vector3(dotScale, dotScale, 1.0f);

        if (listenCircleImage != null)
        {
            listenCircleImage.color = Color.Lerp(farCircleColor, nearCircleColor, distanceRate);
        }
    }

    /// <summary>
    /// 聞き耳用の距離を計算する。
    /// 横方向の距離に加えて、高さの差と階層ペナルティを反映する。
    /// </summary>
    /// <param name="target">鳴っているオルゴール。</param>
    /// <returns>聞き耳用に補正された距離。</returns>
    private float CalculateListeningDistance(Transform target)
    {
        Vector3 playerPosition = playerTransform.position;
        Vector3 targetPosition = target.position;

        Vector2 playerXZ = new Vector2(playerPosition.x, playerPosition.z);
        Vector2 targetXZ = new Vector2(targetPosition.x, targetPosition.z);

        float horizontalDistance = Vector2.Distance(playerXZ, targetXZ);
        float heightDifference = Mathf.Abs(playerPosition.y - targetPosition.y);

        float adjustedHeightDistance = heightDifference * heightDistanceMultiplier;

        float listeningDistance = Mathf.Sqrt(
            horizontalDistance * horizontalDistance +
            adjustedHeightDistance * adjustedHeightDistance
        );

        if (heightDifference >= floorHeightThreshold)
        {
            listeningDistance += differentFloorPenalty;
        }

        return listeningDistance;
    }

    /// <summary>
    /// 聞き耳UIの表示状態を初期状態に戻す。
    /// </summary>
    private void ResetListenUI()
    {
        if (directionDot != null)
        {
            directionDot.gameObject.SetActive(false);
            directionDot.anchoredPosition = Vector2.zero;
            directionDot.localScale = Vector3.one;
        }

        if (listenCircleImage != null)
        {
            listenCircleImage.color = farCircleColor;
        }
    }

}