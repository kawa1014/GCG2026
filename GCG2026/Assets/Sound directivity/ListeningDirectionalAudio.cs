using UnityEngine;

/// <summary>
/// 聞き耳中のオルゴール音を、距離・左右方向・上下方向・壁越しで制御するクラス。
/// 右にある音は右から、左にある音は左から分かりやすく聞こえるようにする。
/// さらに、1階/2階のような高さ違いでは、違う階を探している時の音を全体的に小さくする。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ListeningDirectionalAudio : MonoBehaviour
{
    /// <summary>
    /// プレイヤー、またはMain CameraのTransform。
    /// 正面判定・左右判定・上下判定に使う。
    /// </summary>
    [Header("参照")]
    [SerializeField] private Transform listenerTransform;

    /// <summary>
    /// 制御対象のAudioSource。
    /// 空なら自動で取得する。
    /// </summary>
    [SerializeField] private AudioSource targetAudioSource;

    /// <summary>
    /// 音を濁らせるためのLowPassFilter。
    /// 空なら自動で追加する。
    /// </summary>
    [SerializeField] private AudioLowPassFilter lowPassFilter;

    /// <summary>
    /// 聞き耳キー。
    /// </summary>
    [Header("聞き耳")]
    [SerializeField] private KeyCode listenKey = KeyCode.E;

    /// <summary>
    /// 聞き耳をしていない時の音量倍率。
    /// </summary>
    [SerializeField] private float normalVolumeRate = 0.25f;

    /// <summary>
    /// 聞き耳中の最大音量。
    /// </summary>
    [Header("音量")]
    [SerializeField] private float maxVolume = 1.0f;

    /// <summary>
    /// 急に音が大きくなりすぎるのを防ぐ最終音量の上限。
    /// 1.0に近いほど大きく、0.7〜0.85だと自然に聞こえやすい。
    /// </summary>
    [Header("急な爆音防止")]
    [SerializeField] private float naturalVolumeLimit = 0.82f;

    /// <summary>
    /// プレイヤーが音源に近い時でも、音量が一気に最大まで跳ねないようにする距離音量の上限。
    /// </summary>
    [SerializeField] private float closeDistanceVolumeLimit = 0.78f;

    /// <summary>
    /// 正面・上下・横方向ボーナスで増やしてよい最大量。
    /// ボーナスの足し算が原因で突然大きくなるのを防ぐ。
    /// </summary>
    [SerializeField] private float directionBonusLimit = 0.22f;

    /// <summary>
    /// 音量が大きくなる時の最大変化量。
    /// 小さいほど、急に爆音にならず自然に大きくなる。
    /// </summary>
    [SerializeField] private float maxVolumeRisePerSecond = 0.65f;

    /// <summary>
    /// 音量が小さくなる時の最大変化量。
    /// 上昇より少し速くすると、遠ざかった時や向きが外れた時に自然に弱くなる。
    /// </summary>
    [SerializeField] private float maxVolumeFallPerSecond = 1.8f;

    /// <summary>
    /// この距離までは距離による音量減衰を計算する。
    /// これより遠くても完全には0にしない。
    /// </summary>
    [SerializeField] private float distanceFalloffRange = 35.0f;

    /// <summary>
    /// 遠距離でも残す最低音量。
    /// 「鳴っているのに気づけない」を防ぐ。
    /// </summary>
    [SerializeField] private float farMinimumVolume = 0.18f;
    /// <summary>
    /// 正面に近い時に追加する音量。
    /// </summary>
    [SerializeField] private float frontBonusVolume = 0.18f;

    /// <summary>
    /// 真後ろ方向を向いた時の最低音量倍率。
    /// </summary>
    [SerializeField] private float backSideVolumeRate = 0.15f;

    /// <summary>
    /// 右左の横方向にある音を聞こえやすくする追加音量。
    /// 視点を横へ振った時に「右から鳴っている」「左から鳴っている」を分かりやすくする。
    /// </summary>
    [SerializeField] private float sideDirectionBonusVolume = 0.22f;

    /// <summary>
    /// 右左に音を振る補助を使うかどうか。
    /// 右に音源があるならpanStereoを右へ、左に音源があるなら左へ振る。
    /// </summary>
    [Header("左右の指向性")]
    [SerializeField] private bool useStereoPanAssist = true;

    /// <summary>
    /// AudioSourceの3D音量比率。
    /// 1.0だと完全3D。panStereo補助を強く使いたい場合は0.75〜0.90が分かりやすい。
    /// </summary>
    [Range(0.0f, 1.0f)]
    [SerializeField] private float spatialBlend = 0.85f;

    /// <summary>
    /// 左右パンの強さ。
    /// 1.0で通常、1.5〜2.5で方向がかなり分かりやすくなる。
    /// </summary>
    [SerializeField] private float stereoPanStrength = 2.0f;

    /// <summary>
    /// パンが急に動かないようにする補間速度。
    /// </summary>
    [SerializeField] private float panLerpSpeed = 14.0f;

    /// <summary>
    /// クリアに聞こえる左右の正面角度。
    /// 小さいほど「正面を向いた時だけクリア」になる。
    /// </summary>
    [SerializeField] private float clearAngle = 30.0f;

    /// <summary>
    /// この左右角度以上はかなり聞きにくくする。
    /// </summary>
    [SerializeField] private float weakAngle = 130.0f;

    /// <summary>
    /// クリアに聞こえる上下方向の角度。
    /// 例：2階のオルゴールなら、見上げた時に1に近くなる。
    /// </summary>
    [Header("上下の指向性")]
    [SerializeField] private float verticalClearAngle = 18.0f;

    /// <summary>
    /// この上下角度以上ズレていたら、上下方向の音量をかなり弱くする。
    /// </summary>
    [SerializeField] private float verticalWeakAngle = 60.0f;

    /// <summary>
    /// 上下方向がズレている時の最低音量倍率。
    /// 小さいほど、1階/2階の方向違いが分かりやすくなる。
    /// </summary>
    [SerializeField] private float verticalMismatchRate = 0.12f;

    /// <summary>
    /// 上下方向がズレている時のLowPassFilter用の鮮明度倍率。
    /// 小さいほど「目の前にあるような鮮明さ」を消せる。
    /// </summary>
    [SerializeField] private float verticalMismatchClarityRate = 0.06f;

    /// <summary>
    /// 上下方向が合っている時に追加する音量。
    /// 見上げた時に2階の音が分かりやすくなる。
    /// </summary>
    [SerializeField] private float verticalBonusVolume = 0.18f;

    /// <summary>
    /// プレイヤーと音源の高さ差がこれ以上ある場合、階層違いとして扱う。
    /// </summary>
    [Header("階層差")]
    [SerializeField] private float floorHeightThreshold = 1.5f;

    /// <summary>
    /// 階層差がある時に、常に全体音量へ掛ける倍率。
    /// 例：1階で鳴っているのに2階を探している時は、音を全体的に小さくする。
    /// </summary>
    [SerializeField] private float differentFloorBaseVolumeRate = 0.65f;

    /// <summary>
    /// 階層差がある時に、常に鮮明度へ掛ける倍率。
    /// 違う階にいる時の「目の前で鳴っている感じ」を弱める。
    /// </summary>
    [SerializeField] private float differentFloorBaseClarityRate = 0.75f;

    /// <summary>
    /// 階層差があるのに上下方向を見ていない時の追加音量倍率。
    /// 小さいほど、違う階を探している時に音がさらに小さくなる。
    /// </summary>
    [SerializeField] private float differentFloorWrongLookVolumeRate = 0.35f;
    /// <summary>
    /// 階層差があるのに上下方向を見ていない時の追加鮮明度倍率。
    /// 小さいほど、違う階を見ている時にさらにこもる。
    /// </summary>
    [SerializeField] private float differentFloorWrongLookClarityRate = 0.18f;

    /// <summary>
    /// 壁として扱うLayer。
    /// Wall Layerに指定したレイヤーだけを壁越し判定に使う。
    /// </summary>
    [Header("壁越し")]
    [SerializeField] private LayerMask wallLayer;

    /// <summary>
    /// 壁1枚越しの音量倍率。
    /// 少しだけ小さくするため、0.70〜0.80くらいが使いやすい。
    /// </summary>
    [SerializeField] private float singleWallVolumeRate = 0.75f;

    /// <summary>
    /// 壁2枚以上越しの音量倍率。
    /// 何枚あってもこの倍率より極端には下げない。
    /// </summary>
    [SerializeField] private float multiWallVolumeRate = 0.60f;

    /// <summary>
    /// 壁越し音量の最低保証。
    /// 壁が何重にもあっても、音が消えすぎないようにする。
    /// </summary>
    [SerializeField] private float minimumWallVolumeRate = 0.45f;

    /// <summary>
    /// 壁1枚越しの鮮明度倍率。
    /// 1に近いほどクリア、0に近いほどこもる。
    /// </summary>
    [SerializeField] private float singleWallClarityRate = 0.65f;

    /// <summary>
    /// 壁2枚以上越しの鮮明度倍率。
    /// 2枚以上はまとめてこの濁りにする。
    /// </summary>
    [SerializeField] private float multiWallClarityRate = 0.35f;

    /// <summary>
    /// 数える壁の最大数。
    /// 0枚 / 1枚 / 2枚以上の3段階で使うため、基本は2でOK。
    /// </summary>
    [SerializeField] private int maxWallHitCount = 2;

    /// <summary>
    /// クリアな音のLowPassFilter周波数。
    /// </summary>
    [Header("音の濁り")]
    [SerializeField] private float clearCutoffFrequency = 22000.0f;

    /// <summary>
    /// 濁った音のLowPassFilter周波数。
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
    /// 音量・鮮明度・左右パンの計算結果。
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

        /// <summary>
        /// 左右パン。
        /// -1が左、0が中央、1が右。
        /// </summary>
        public float pan;
    }

    /// <summary>
    /// 壁越し判定の計算結果。
    /// 壁の枚数は、0枚 / 1枚 / 2枚以上の3段階で扱う。
    /// </summary>
    private struct WallOcclusionState
    {
        /// <summary>
        /// 壁のヒット数。
        /// 0、1、2以上のどれかとして使う。
        /// </summary>
        public int wallCount;

        /// <summary>
        /// 壁越しによる音量倍率。
        /// </summary>
        public float volumeRate;

        /// <summary>
        /// 壁越しによる鮮明度倍率。
        /// </summary>
        public float clarityRate;
    }

    /// <summary>
    /// 初期化処理。
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

        ApplyAudioSourceSettings();
    }

    /// <summary>
    /// Inspectorの値を変更した時にもAudioSource設定を反映する。
    /// </summary>
    private void OnValidate()
    {
        distanceFalloffRange = Mathf.Max(0.01f, distanceFalloffRange);
        farMinimumVolume = Mathf.Clamp01(farMinimumVolume);
        normalVolumeRate = Mathf.Clamp01(normalVolumeRate);
        backSideVolumeRate = Mathf.Clamp01(backSideVolumeRate);
        verticalMismatchRate = Mathf.Clamp01(verticalMismatchRate);
        verticalMismatchClarityRate = Mathf.Clamp01(verticalMismatchClarityRate);
        differentFloorBaseVolumeRate = Mathf.Clamp01(differentFloorBaseVolumeRate);
        differentFloorBaseClarityRate = Mathf.Clamp01(differentFloorBaseClarityRate);
        differentFloorWrongLookVolumeRate = Mathf.Clamp01(differentFloorWrongLookVolumeRate);
        differentFloorWrongLookClarityRate = Mathf.Clamp01(differentFloorWrongLookClarityRate);
        singleWallVolumeRate = Mathf.Clamp01(singleWallVolumeRate);
        multiWallVolumeRate = Mathf.Clamp01(multiWallVolumeRate);
        minimumWallVolumeRate = Mathf.Clamp01(minimumWallVolumeRate);
        singleWallClarityRate = Mathf.Clamp01(singleWallClarityRate);
        multiWallClarityRate = Mathf.Clamp01(multiWallClarityRate);
        maxWallHitCount = Mathf.Max(2, maxWallHitCount);

        // 壁が何枚あっても最低45%前後は残すため、
        // 1枚/2枚以上の倍率が最低保証を下回らないようにする。
        singleWallVolumeRate = Mathf.Max(singleWallVolumeRate, minimumWallVolumeRate);
        multiWallVolumeRate = Mathf.Max(multiWallVolumeRate, minimumWallVolumeRate);
        stereoPanStrength = Mathf.Max(0.0f, stereoPanStrength);
        naturalVolumeLimit = Mathf.Clamp01(naturalVolumeLimit);
        closeDistanceVolumeLimit = Mathf.Clamp01(closeDistanceVolumeLimit);
        directionBonusLimit = Mathf.Clamp01(directionBonusLimit);
        maxVolumeRisePerSecond = Mathf.Max(0.01f, maxVolumeRisePerSecond);
        maxVolumeFallPerSecond = Mathf.Max(0.01f, maxVolumeFallPerSecond);

        ApplyAudioSourceSettings();
    }

    /// <summary>
    /// AudioSourceの基本設定を反映する。
    /// </summary>
    private void ApplyAudioSourceSettings()
    {
        if (targetAudioSource == null)
        {
            return;
        }

        targetAudioSource.spatialBlend = spatialBlend;
        targetAudioSource.dopplerLevel = 0.0f;

        // ここはUnity標準の3D広がりを抑える。
        // 左右の分かりやすさは、このスクリプトのpanStereo補助で作る。
        targetAudioSource.spread = 0.0f;
    }

    /// <summary>
    /// 毎フレーム、音量・鮮明度・左右パンを更新する。
    /// </summary>
    private void Update()
    {
        if (listenerTransform == null || targetAudioSource == null)
        {
            return;
        }

        ApplyAudioSourceSettings();

        if (!targetAudioSource.isPlaying)
        {
            return;
        }

        AudioState audioState = CalculateAudioState();

        bool isListening = Input.GetKey(listenKey);
        float listenRate = isListening ? 1.0f : normalVolumeRate;

        float targetVolume = audioState.volume * listenRate * maxVolume;

        // Lerpだけだと、角度や階層判定が切り替わった瞬間に音量が跳ねることがある。
        // MoveTowardsで1秒あたりの増減量を制限して、急な爆音を防ぐ。
        float volumeChangeSpeed = targetVolume > targetAudioSource.volume
            ? maxVolumeRisePerSecond
            : maxVolumeFallPerSecond;

        targetAudioSource.volume = Mathf.MoveTowards(
            targetAudioSource.volume,
            targetVolume,
            volumeChangeSpeed * Time.deltaTime
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

        float targetPan = useStereoPanAssist ? audioState.pan : 0.0f;

        targetAudioSource.panStereo = Mathf.Lerp(
            targetAudioSource.panStereo,
            targetPan,
            Time.deltaTime * panLerpSpeed
        );
    }

    /// <summary>
    /// 距離・左右方向・上下方向・壁越しから音量、鮮明度、左右パンを計算する。
    /// </summary>
    /// <returns>音量・鮮明度・左右パン。</returns>
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
                clarity = 1.0f,
                pan = 0.0f
            };
        }

        Vector3 directionToSound = toSound.normalized;

        float horizontalRate = CalculateHorizontalRate(directionToSound);
        float verticalRate = CalculateVerticalRate(directionToSound);
        float verticalClarityRate = CalculateVerticalClarityRate(directionToSound);
        float sideRate = CalculateSideRate(directionToSound);
        float pan = CalculateStereoPan(directionToSound);

        WallOcclusionState wallOcclusionState = CalculateWallOcclusionState(
            listenerPosition,
            directionToSound,
            distance
        );

        float wallVolume = wallOcclusionState.volumeRate;
        float wallClarity = wallOcclusionState.clarityRate;

        float distanceVolume = Mathf.Lerp(
            farMinimumVolume,
            1.0f,
            1.0f - Mathf.Clamp01(distance / distanceFalloffRange)
        );

        // 近距離で距離音量が1.0まで上がると、方向ボーナスと重なって急に大きく聞こえる。
        // ここで距離音量に上限をかけて、音源の近くでも自然な大きさに抑える。
        distanceVolume = Mathf.Min(distanceVolume, closeDistanceVolumeLimit);

        float heightDifference = Mathf.Abs(toSound.y);
        bool differentFloor = heightDifference >= floorHeightThreshold;
        bool verticalDirectionIsWrong = verticalRate <= 0.55f;

        float floorBaseVolumeRate = differentFloor ? differentFloorBaseVolumeRate : 1.0f;
        float floorBaseClarityRate = differentFloor ? differentFloorBaseClarityRate : 1.0f;

        float floorLookVolumeRate = 1.0f;
        float floorLookClarityRate = 1.0f;

        if (differentFloor && verticalDirectionIsWrong)
        {
            floorLookVolumeRate = differentFloorWrongLookVolumeRate;
            floorLookClarityRate = differentFloorWrongLookClarityRate;
        }

        // 正面と上下が合っているほど、クリアで少し大きく聞こえる。
        float frontAndVerticalRate = horizontalRate * verticalRate;
        float directionBonus = (frontBonusVolume + verticalBonusVolume) * frontAndVerticalRate;

        // 右・左に音源がある時も最低限分かるように、横方向の存在感を少し足す。
        // ただし足しすぎると特定地点で急に爆音になるため、合計ボーナスに上限をかける。
        float sideDirectionBonus = sideDirectionBonusVolume * sideRate * verticalRate;
        float limitedDirectionBonus = Mathf.Min(
            directionBonus + sideDirectionBonus,
            directionBonusLimit
        );

        float finalVolume = Mathf.Clamp01(
            (distanceVolume + limitedDirectionBonus)
            * horizontalRate
            * verticalRate
            * floorBaseVolumeRate
            * floorLookVolumeRate
            * wallVolume
        );

        // 最終段でも上限をかける。
        // これで「ある場所だけ急にデカい」を抑える。
        finalVolume = Mathf.Min(finalVolume, naturalVolumeLimit);

        // 鮮明度は、真正面・上下一致・壁なしの時に最も高くなる。
        float finalClarity = Mathf.Clamp01(
            horizontalRate
            * verticalClarityRate
            * floorBaseClarityRate
            * floorLookClarityRate
            * wallClarity
        );

        return new AudioState
        {
            volume = finalVolume,
            clarity = finalClarity,
            pan = pan
        };
    }

    /// <summary>
    /// プレイヤーとオルゴールの間にある壁の枚数を調べて、音量と鮮明度の倍率を返す。
    /// 壁の枚数は、0枚 / 1枚 / 2枚以上の3段階だけで扱う。
    /// </summary>
    /// <param name="listenerPosition">プレイヤー、またはMain Cameraの位置。</param>
    /// <param name="directionToSound">プレイヤーから音源への方向。</param>
    /// <param name="distance">プレイヤーから音源までの距離。</param>
    /// <returns>壁越し判定の計算結果。</returns>
    private WallOcclusionState CalculateWallOcclusionState(
        Vector3 listenerPosition,
        Vector3 directionToSound,
        float distance
    )
    {
        // Wall Layerが未設定なら、壁越し補正は行わない。
        if (wallLayer.value == 0)
        {
            return new WallOcclusionState
            {
                wallCount = 0,
                volumeRate = 1.0f,
                clarityRate = 1.0f
            };
        }

        RaycastHit[] hits = Physics.RaycastAll(
            listenerPosition,
            directionToSound,
            distance,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );

        int wallCount = Mathf.Min(hits.Length, maxWallHitCount);

        if (wallCount <= 0)
        {
            return new WallOcclusionState
            {
                wallCount = 0,
                volumeRate = 1.0f,
                clarityRate = 1.0f
            };
        }

        if (wallCount == 1)
        {
            return new WallOcclusionState
            {
                wallCount = 1,
                volumeRate = Mathf.Max(singleWallVolumeRate, minimumWallVolumeRate),
                clarityRate = singleWallClarityRate
            };
        }

        return new WallOcclusionState
        {
            wallCount = wallCount,
            volumeRate = Mathf.Max(multiWallVolumeRate, minimumWallVolumeRate),
            clarityRate = multiWallClarityRate
        };
    }

    /// <summary>
    /// 左右方向の向きが、音源に合っているかを計算する。
    /// 1に近いほど正面、0に近いほどズレている。
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
    /// 音源が右・左のどちらに寄っているかの強さを計算する。
    /// 0が正面/真後ろ、1が真横。
    /// </summary>
    /// <param name="directionToSound">プレイヤーから音源への方向。</param>
    /// <returns>横方向の強さ。</returns>
    private float CalculateSideRate(Vector3 directionToSound)
    {
        Vector3 listenerRight = listenerTransform.right;
        listenerRight.y = 0.0f;

        Vector3 flatDirectionToSound = directionToSound;
        flatDirectionToSound.y = 0.0f;

        if (listenerRight.sqrMagnitude <= 0.0001f || flatDirectionToSound.sqrMagnitude <= 0.0001f)
        {
            return 0.0f;
        }

        listenerRight.Normalize();
        flatDirectionToSound.Normalize();

        return Mathf.Abs(Vector3.Dot(listenerRight, flatDirectionToSound));
    }

    /// <summary>
    /// 左右パンを計算する。
    /// -1なら左、1なら右。
    /// </summary>
    /// <param name="directionToSound">プレイヤーから音源への方向。</param>
    /// <returns>左右パン。</returns>
    private float CalculateStereoPan(Vector3 directionToSound)
    {
        Vector3 listenerRight = listenerTransform.right;
        listenerRight.y = 0.0f;

        Vector3 flatDirectionToSound = directionToSound;
        flatDirectionToSound.y = 0.0f;

        if (listenerRight.sqrMagnitude <= 0.0001f || flatDirectionToSound.sqrMagnitude <= 0.0001f)
        {
            return 0.0f;
        }

        listenerRight.Normalize();
        flatDirectionToSound.Normalize();

        float rawPan = Vector3.Dot(listenerRight, flatDirectionToSound);

        // 小さい左右差も分かりやすくするために強調する。
        return Mathf.Clamp(rawPan * stereoPanStrength, -1.0f, 1.0f);
    }

    /// <summary>
    /// 上下方向の向きが、音源に合っているかを計算する。
    /// 音量用なので、ズレていても完全には0にしない。
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
    /// 音量を残したとしても、「目の前で鳴っているような鮮明さ」は消す。
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
    /// 左右方向は無視するため、左右を向いていても上下を見ていなければズレとして扱える。
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
