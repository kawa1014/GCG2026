using UnityEngine;

/// <summary>
/// 聞き耳中のオルゴール音を、距離・左右方向・上下方向・壁越しで制御するクラス。
/// 左右だけでなく、上下の視線ズレも音量と鮮明度に強く反映する。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ListeningDirectionalAudio : MonoBehaviour
{
    /// <summary>
    /// プレイヤー、またはMain CameraのTransform。
    /// 正面判定・上下判定に使う。
    /// </summary>
    [Header("参照")]
    [SerializeField] private Transform listenerTransform;

    /// <summary>
    /// 制御対象のAudioSource。
    /// 空なら自動で取得する。
    /// </summary>
    [SerializeField] private AudioSource targetAudioSource;

    /// <summary>
    /// 音をこもらせるためのLowPassFilter。
    /// 空なら自動で追加する。
    /// </summary>
    [SerializeField] private AudioLowPassFilter lowPassFilter;

    /// <summary>
    /// 聞き耳キー。
    /// </summary>
    [Header("聞き耳")]
    [SerializeField] private KeyCode listenKey = KeyCode.E;

    /// <summary>
    /// 聞き耳していない時の音量倍率。
    /// </summary>
    [SerializeField] private float normalVolumeRate = 0.25f;

    /// <summary>
    /// 聞き耳中の最大音量。
    /// </summary>
    [Header("音量")]
    [SerializeField] private float maxVolume = 1.0f;

    /// <summary>
    /// この距離までは距離による音量減衰を強く計算する。
    /// これより遠くても完全には0にしない。
    /// </summary>
    [SerializeField] private float distanceFalloffRange = 35.0f;

    /// <summary>
    /// 遠距離でも残す最低音量。
    /// 「遠すぎて気づかない」を防ぐ。
    /// </summary>
    [SerializeField] private float farMinimumVolume = 0.14f;

    /// <summary>
    /// 左右と上下の両方が合っている時に足す遠距離用の気づき音量。
    /// </summary>
    [SerializeField] private float frontBonusVolume = 0.18f;

    /// <summary>
    /// 横や後ろを向いた時の最低音量倍率。
    /// </summary>
    [SerializeField] private float backSideVolumeRate = 0.15f;

    /// <summary>
    /// 鮮明に聞こえる左右方向の正面角度。
    /// 小さいほど「左右方向が合っている時だけクリア」になる。
    /// </summary>
    [Header("左右の指向性")]
    [SerializeField] private float clearAngle = 30.0f;

    /// <summary>
    /// この左右角度以上はかなり弱くする。
    /// </summary>
    [SerializeField] private float weakAngle = 130.0f;

    /// <summary>
    /// 鮮明に聞こえる上下方向の角度。
    /// 例：2階のオルゴールなら、見上げた時に1に近くなる。
    /// </summary>
    [Header("上下の指向性")]
    [SerializeField] private float verticalClearAngle = 12.0f;

    /// <summary>
    /// この上下角度以上ズレていたら、上下方向の音をかなり弱くする。
    /// </summary>
    [SerializeField] private float verticalWeakAngle = 45.0f;

    /// <summary>
    /// 上下方向がズレている時の最低音量倍率。
    /// 小さいほど、1階/2階の聞き分けが強くなる。
    /// </summary>
    [SerializeField] private float verticalMismatchRate = 0.10f;

    /// <summary>
    /// 上下方向がズレている時のLowPassFilter用の鮮明度倍率。
    /// 小さいほど「目の前にあるような鮮明さ」が消える。
    /// </summary>
    [SerializeField] private float verticalMismatchClarityRate = 0.08f;

    /// <summary>
    /// 上下方向が合っている時だけ追加する音量。
    /// 遠くても2階方向を向いた時に気づきやすくする。
    /// </summary>
    [SerializeField] private float verticalBonusVolume = 0.12f;

    /// <summary>
    /// プレイヤーと音源の高さ差がこれ以上ある場合、上下の向きが合っていない音をさらに弱くする。
    /// </summary>
    [Header("階層差")]
    [SerializeField] private float floorHeightThreshold = 1.5f;

    /// <summary>
    /// 階層差があるのに上下方向が合っていない時の追加音量倍率。
    /// </summary>
    [SerializeField] private float differentFloorWrongLookVolumeRate = 0.45f;

    /// <summary>
    /// 階層差があるのに上下方向が合っていない時の追加鮮明度倍率。
    /// </summary>
    [SerializeField] private float differentFloorWrongLookClarityRate = 0.30f;

    /// <summary>
    /// 壁として扱うLayer。
    /// </summary>
    [Header("壁越し")]
    [SerializeField] private LayerMask wallLayer;

    /// <summary>
    /// 壁越しの音量倍率。
    /// </summary>
    [SerializeField] private float wallVolumeRate = 0.55f;

    /// <summary>
    /// 壁越しの鮮明度倍率。
    /// 小さいほどこもる。
    /// </summary>
    [SerializeField] private float wallClarityRate = 0.25f;

    /// <summary>
    /// クリアな音のLowPassFilter周波数。
    /// </summary>
    [Header("音の濁り")]
    [SerializeField] private float clearCutoffFrequency = 22000.0f;

    /// <summary>
    /// こもった音のLowPassFilter周波数。
    /// </summary>
    [SerializeField] private float muffledCutoffFrequency = 650.0f;

    /// <summary>
    /// 音量変化の滑らかさ。
    /// </summary>
    [Header("補間")]
    [SerializeField] private float volumeLerpSpeed = 10.0f;

    /// <summary>
    /// フィルター変化の滑らかさ。
    /// </summary>
    [SerializeField] private float filterLerpSpeed = 10.0f;

    /// <summary>
    /// 音量と鮮明度の計算結果。
    /// </summary>
    private struct AudioState
    {
        /// <summary>
        /// 最終音量。
        /// </summary>
        public float volume;

        /// <summary>
        /// 最終鮮明度。
        /// 1に近いほどクリア、0に近いほどこもる。
        /// </summary>
        public float clarity;
    }

    /// <summary>
    /// 起動時の初期化。
    /// </summary>
    private void Awake()
    {
        if (targetAudioSource == null)
        {
            targetAudioSource = GetComponent<AudioSource>();
        }

        if (lowPassFilter == null)
        {
            lowPassFilter = GetComponent<AudioLowPassFilter>();
        }

        if (lowPassFilter == null)
        {
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        }

        if (targetAudioSource != null)
        {
            targetAudioSource.spatialBlend = 1.0f;
        }
    }

    /// <summary>
    /// 毎フレーム、音量と鮮明度を更新する。
    /// </summary>
    private void Update()
    {
        if (listenerTransform == null || targetAudioSource == null)
        {
            return;
        }

        if (!targetAudioSource.isPlaying)
        {
            return;
        }

        AudioState audioState = CalculateAudioState();

        bool isListening = Input.GetKey(listenKey);
        float listenRate = isListening ? 1.0f : normalVolumeRate;

        float targetVolume = audioState.volume * listenRate * maxVolume;

        targetAudioSource.volume = Mathf.Lerp(
            targetAudioSource.volume,
            targetVolume,
            Time.deltaTime * volumeLerpSpeed
        );

        float targetCutoff = Mathf.Lerp(
            muffledCutoffFrequency,
            clearCutoffFrequency,
            audioState.clarity
        );

        lowPassFilter.cutoffFrequency = Mathf.Lerp(
            lowPassFilter.cutoffFrequency,
            targetCutoff,
            Time.deltaTime * filterLerpSpeed
        );
    }

    /// <summary>
    /// 距離・左右方向・上下方向・壁越しから音量と鮮明度を計算する。
    /// </summary>
    /// <returns>音量と鮮明度。</returns>
    private AudioState CalculateAudioState()
    {
        Vector3 listenerPosition = listenerTransform.position;
        Vector3 soundPosition = transform.position;

        Vector3 toSound = soundPosition - listenerPosition;
        float distance = toSound.magnitude;

        if (distance <= 0.001f)
        {
            return new AudioState
            {
                volume = 1.0f,
                clarity = 1.0f
            };
        }

        Vector3 directionToSound = toSound.normalized;

        float horizontalRate = CalculateHorizontalRate(directionToSound);
        float verticalRate = CalculateVerticalRate(directionToSound);
        float verticalClarityRate = CalculateVerticalClarityRate(directionToSound);

        bool blockedByWall = Physics.Raycast(
            listenerPosition,
            directionToSound,
            distance,
            wallLayer
        );

        float wallVolume = blockedByWall ? wallVolumeRate : 1.0f;
        float wallClarity = blockedByWall ? wallClarityRate : 1.0f;

        float distanceVolume = Mathf.Lerp(
            farMinimumVolume,
            1.0f,
            1.0f - Mathf.Clamp01(distance / distanceFalloffRange)
        );

        float heightDifference = Mathf.Abs(toSound.y);
        bool differentFloor = heightDifference >= floorHeightThreshold;
        bool verticalDirectionIsWrong = verticalRate <= 0.55f;

        float floorVolumeRate = 1.0f;
        float floorClarityRate = 1.0f;

        if (differentFloor && verticalDirectionIsWrong)
        {
            floorVolumeRate = differentFloorWrongLookVolumeRate;
            floorClarityRate = differentFloorWrongLookClarityRate;
        }

        // 左右と上下の両方が合った時だけ、遠距離でも分かる音量を足す。
        // これにより「横は合っているが、上を見ていない」状態では鮮明になりすぎない。
        float frontAndVerticalRate = horizontalRate * verticalRate;
        float directionBonus = (frontBonusVolume + verticalBonusVolume) * frontAndVerticalRate;

        float finalVolume = Mathf.Clamp01(
            (distanceVolume + directionBonus)
            * horizontalRate
            * verticalRate
            * floorVolumeRate
            * wallVolume
        );

        // 鮮明度は特に上下ズレを強く見る。
        // 下にいるのに2階の音源を見上げていない場合、LowPassFilterでこもらせる。
        float finalClarity = Mathf.Clamp01(
            horizontalRate
            * verticalClarityRate
            * floorClarityRate
            * wallClarity
        );

        return new AudioState
        {
            volume = finalVolume,
            clarity = finalClarity
        };
    }

    /// <summary>
    /// 左右方向の向きが、音源に合っているかを計算する。
    /// 1に近いほど左右方向が合っていて、0に近いほどズレている。
    /// </summary>
    /// <param name="directionToSound">プレイヤーから音源への方向。</param>
    /// <returns>左右方向の一致率。</returns>
    private float CalculateHorizontalRate(Vector3 directionToSound)
    {
        Vector3 listenerForward = listenerTransform.forward;
        listenerForward.y = 0.0f;

        Vector3 flatDirectionToSound = directionToSound;
        flatDirectionToSound.y = 0.0f;

        if (listenerForward.sqrMagnitude <= 0.0001f || flatDirectionToSound.sqrMagnitude <= 0.0001f)
        {
            return 1.0f;
        }

        listenerForward.Normalize();
        flatDirectionToSound.Normalize();

        float horizontalAngle = Vector3.Angle(listenerForward, flatDirectionToSound);

        if (horizontalAngle <= clearAngle)
        {
            return 1.0f;
        }

        if (horizontalAngle >= weakAngle)
        {
            return backSideVolumeRate;
        }

        return Mathf.Lerp(
            backSideVolumeRate,
            1.0f,
            Mathf.InverseLerp(weakAngle, clearAngle, horizontalAngle)
        );
    }

    /// <summary>
    /// 上下方向の向きが、音源に合っているかを計算する。
    /// 音量用なので、ズレても完全には0にしない。
    /// </summary>
    /// <param name="directionToSound">プレイヤーから音源への方向。</param>
    /// <returns>上下方向の一致率。</returns>
    private float CalculateVerticalRate(Vector3 directionToSound)
    {
        float verticalAngle = CalculateVerticalAngle(directionToSound);

        if (verticalAngle <= verticalClearAngle)
        {
            return 1.0f;
        }

        if (verticalAngle >= verticalWeakAngle)
        {
            return verticalMismatchRate;
        }

        return Mathf.Lerp(
            verticalMismatchRate,
            1.0f,
            Mathf.InverseLerp(verticalWeakAngle, verticalClearAngle, verticalAngle)
        );
    }

    /// <summary>
    /// 上下方向の鮮明度を計算する。
    /// 音量よりも強く落として、「目の前で鳴っているような鮮明さ」を消す。
    /// </summary>
    /// <param name="directionToSound">プレイヤーから音源への方向。</param>
    /// <returns>上下方向の鮮明度。</returns>
    private float CalculateVerticalClarityRate(Vector3 directionToSound)
    {
        float verticalAngle = CalculateVerticalAngle(directionToSound);

        if (verticalAngle <= verticalClearAngle)
        {
            return 1.0f;
        }

        if (verticalAngle >= verticalWeakAngle)
        {
            return verticalMismatchClarityRate;
        }

        return Mathf.Lerp(
            verticalMismatchClarityRate,
            1.0f,
            Mathf.InverseLerp(verticalWeakAngle, verticalClearAngle, verticalAngle)
        );
    }

    /// <summary>
    /// プレイヤーの視線の上下角度と、音源方向の上下角度の差を求める。
    /// 左右方向は無視するため、横が合っていても上を見ていなければズレとして扱える。
    /// </summary>
    /// <param name="directionToSound">プレイヤーから音源への方向。</param>
    /// <returns>上下方向の角度差。</returns>
    private float CalculateVerticalAngle(Vector3 directionToSound)
    {
        float listenerPitch = Mathf.Asin(Mathf.Clamp(listenerTransform.forward.normalized.y, -1.0f, 1.0f)) * Mathf.Rad2Deg;
        float soundPitch = Mathf.Asin(Mathf.Clamp(directionToSound.normalized.y, -1.0f, 1.0f)) * Mathf.Rad2Deg;

        return Mathf.Abs(Mathf.DeltaAngle(listenerPitch, soundPitch));
    }
}
