using UnityEngine;

/// <summary>
/// 聞き耳中のオルゴール音を、距離・左右方向・上下方向・壁越しで制御するクラス。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ListeningDirectionalAudio : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform listenerTransform;
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField] private AudioLowPassFilter lowPassFilter;

    [Header("聞き耳 (ListenSkill連動)")]
    [Tooltip("聞き耳中、オルゴールの方を向いている時の音量倍率")]
    [SerializeField] private float listenFacingVolumeRate = 1.2f;
    [Tooltip("聞き耳中、オルゴールの方を向いていない時の音量倍率（通常時より小さくする）")]
    [SerializeField] private float listenNotFacingVolumeRate = 0.1f;
    [Tooltip("この角度以内なら「向いている」と判定して音を大きくする")]
    [SerializeField] private float listenFacingAngle = 30.0f;
    [Tooltip("この角度以上ズレていたら「向いていない」と判定して音を小さくする")]
    [SerializeField] private float listenNotFacingAngle = 90.0f;

    [Header("通常時の音量")]
    [Tooltip("聞き耳をしていない時の基本音量倍率")]
    [SerializeField] private float normalVolumeRate = 0.25f;

    [Header("音量上限・変化速度")]
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float naturalVolumeLimit = 1.0f;
    [SerializeField] private float closeDistanceVolumeLimit = 1.0f;
    [SerializeField] private float directionBonusLimit = 0.0f;
    [SerializeField] private float maxVolumeRisePerSecond = 3.0f;
    [SerializeField] private float maxVolumeFallPerSecond = 3.0f;

    [Header("距離")]
    [SerializeField] private float distanceFalloffRange = 100.0f;
    [SerializeField] private float farMinimumVolume = 0.6f;

    [Header("左右の指向性・パン")]
    [SerializeField] private float frontBonusVolume = 0.0f;
    [SerializeField] private float backSideVolumeRate = 0.9f;
    [SerializeField] private float sideDirectionBonusVolume = 0.0f;
    [SerializeField] private bool useStereoPanAssist = true;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float spatialBlend = 0.85f;
    [SerializeField] private float stereoPanStrength = 1.5f;
    [SerializeField] private float panLerpSpeed = 14.0f;
    [SerializeField] private float clearAngle = 30.0f;
    [SerializeField] private float weakAngle = 130.0f;

    [Header("上下の指向性")]
    [SerializeField] private float verticalClearAngle = 18.0f;
    [SerializeField] private float verticalWeakAngle = 60.0f;
    [SerializeField] private float verticalMismatchRate = 0.5f;
    [SerializeField] private float verticalMismatchClarityRate = 0.1f;
    [SerializeField] private float verticalBonusVolume = 0.0f;

    [Header("真上・真下の無音防止")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float verticalExtremeDot = 0.92f;
    [SerializeField] private float verticalExtremeFacingMinimumVolume = 0.5f;
    [SerializeField] private float verticalExtremeColumnMinimumVolume = 0.4f;
    [SerializeField] private float verticalExtremeOppositeMinimumVolume = 0.3f;
    [SerializeField] private float verticalExtremeMinimumVolume = 0.3f;
    [SerializeField] private float verticalColumnHorizontalRadius = 4.0f;
    [SerializeField] private float verticalColumnHorizontalRate = 1.20f;
    [SerializeField] private float verticalColumnSafeMinimumVolume = 0.65f;
    [SerializeField] private float verticalColumnForwardMinimumVolume = 0.70f;
    [SerializeField] private float verticalColumnForwardMinimumClarity = 0.65f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float verticalColumnSpatialBlend = 0.0f;
    [SerializeField] private float verticalColumnSpread = 180.0f;
    [SerializeField] private float verticalColumnMinDistance = 50.0f;
    [SerializeField] private float verticalColumnMaxDistance = 500.0f;

    [Header("階層差")]
    [SerializeField] private float floorHeightThreshold = 1.5f;
    [SerializeField] private float differentFloorBaseVolumeRate = 0.6f;
    [SerializeField] private float differentFloorBaseClarityRate = 0.2f;
    [SerializeField] private float differentFloorWrongLookVolumeRate = 0.5f;
    [SerializeField] private float differentFloorWrongLookClarityRate = 0.1f;
    [SerializeField] private float differentFloorFullEffectHeight = 4.5f;
    [SerializeField] private float differentFloorWrongLookStartRate = 0.70f;
    [SerializeField] private float differentFloorWrongLookFullRate = 0.25f;
    [SerializeField] private float differentFloorPassingMinimumVolume = 0.3f;
    [SerializeField] private float differentFloorPassingMinimumClarity = 0.10f;
    [SerializeField] private float differentFloorVerticalColumnVolumeLimit = 0.6f;
    [SerializeField] private float differentFloorVerticalColumnClarityLimit = 0.20f;
    [SerializeField] private float differentFloorCeilingVolumeRate = 0.8f;
    [SerializeField] private float differentFloorCeilingClarityRate = 0.2f;

    [Header("壁越し")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private bool ignoreFloorAreaInWallOcclusion = true;
    [SerializeField] private float singleWallVolumeRate = 0.75f;
    [SerializeField] private float multiWallVolumeRate = 0.60f;
    [SerializeField] private float minimumWallVolumeRate = 0.45f;
    [SerializeField] private float singleWallClarityRate = 0.25f;
    [SerializeField] private float multiWallClarityRate = 0.05f;
    [SerializeField] private int maxWallHitCount = 2;

    [Header("音の濁り")]
    [SerializeField] private float clearCutoffFrequency = 22000.0f;
    [SerializeField] private float muffledCutoffFrequency = 300.0f;
    [SerializeField] private float filterLerpSpeed = 10.0f;

    private bool wasPlayingLastFrame = false;

    private struct AudioState
    {
        public float volume;
        public float clarity;
        public float pan;
        public float spatialBlend;
        public float spread;
        public float facingScore; // プレイヤーが音源を向いている度合い(0.0〜1.0)
    }

    private struct WallOcclusionState
    {
        public int wallCount;
        public float volumeRate;
        public float clarityRate;
    }

    private void Awake()
    {
        if (targetAudioSource == null) targetAudioSource = GetComponent<AudioSource>();
        if (lowPassFilter == null) lowPassFilter = GetComponent<AudioLowPassFilter>();
        if (lowPassFilter == null) lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();

        ApplyAudioSourceSettings();
        targetAudioSource.volume = 0.0f;
        wasPlayingLastFrame = false;
    }

    private void OnValidate()
    {
        distanceFalloffRange = Mathf.Max(0.01f, distanceFalloffRange);
        farMinimumVolume = Mathf.Clamp01(farMinimumVolume);
        normalVolumeRate = Mathf.Clamp01(normalVolumeRate);
        backSideVolumeRate = Mathf.Clamp01(backSideVolumeRate);
        verticalMismatchRate = Mathf.Clamp01(verticalMismatchRate);
        verticalMismatchClarityRate = Mathf.Clamp01(verticalMismatchClarityRate);
        ApplyAudioSourceSettings();
    }

    private void ApplyAudioSourceSettings()
    {
        if (targetAudioSource == null) return;
        targetAudioSource.spatialBlend = spatialBlend;
        targetAudioSource.dopplerLevel = 0.0f;
        targetAudioSource.spread = 0.0f;
        targetAudioSource.minDistance = Mathf.Max(targetAudioSource.minDistance, 1.0f);
        targetAudioSource.maxDistance = Mathf.Max(targetAudioSource.maxDistance, distanceFalloffRange);
    }

    private void Update()
    {
        if (listenerTransform == null || targetAudioSource == null) return;

        ApplyAudioSourceSettings();

        if (!targetAudioSource.isPlaying)
        {
            targetAudioSource.volume = 0.0f;
            wasPlayingLastFrame = false;
            return;
        }

        AudioState audioState = CalculateAudioState();

        targetAudioSource.spatialBlend = audioState.spatialBlend;
        targetAudioSource.spread = audioState.spread;

        if (audioState.spatialBlend <= 0.001f)
        {
            targetAudioSource.minDistance = verticalColumnMinDistance;
            targetAudioSource.maxDistance = verticalColumnMaxDistance;
        }

        // --- 聞き耳の連携と音量バランスの適用 ---
        float listenRate = normalVolumeRate;

        // ListenSkillでEキーが押されているかチェック
        if (ListenSkill.IsListening)
        {
            // 向いている度合い(0.0: 向いていない 〜 1.0: 向いている)に応じて音量倍率を変化させる
            listenRate = Mathf.Lerp(listenNotFacingVolumeRate, listenFacingVolumeRate, audioState.facingScore);
        }

        float globalMasterVolume = OrgelManager.Instance != null ? OrgelManager.Instance.MasterVolume : 1.0f;
        float targetVolume = audioState.volume * listenRate * maxVolume * globalMasterVolume;

        if (!wasPlayingLastFrame)
        {
            targetAudioSource.volume = targetVolume;
            wasPlayingLastFrame = true;
        }
        else
        {
            float volumeChangeSpeed = targetVolume > targetAudioSource.volume ? maxVolumeRisePerSecond : maxVolumeFallPerSecond;
            targetAudioSource.volume = Mathf.MoveTowards(targetAudioSource.volume, targetVolume, volumeChangeSpeed * Time.deltaTime);
        }

        float targetCutoff = Mathf.Lerp(muffledCutoffFrequency, clearCutoffFrequency, audioState.clarity);
        lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetCutoff, Time.deltaTime * filterLerpSpeed);

        float targetPan = useStereoPanAssist ? audioState.pan : 0.0f;
        targetAudioSource.panStereo = Mathf.Lerp(targetAudioSource.panStereo, targetPan, Time.deltaTime * panLerpSpeed);
    }

    private AudioState CalculateAudioState()
    {
        Vector3 listenerPosition = listenerTransform.position;
        Vector3 soundPosition = transform.position;
        Vector3 toSound = soundPosition - listenerPosition;
        float distance = toSound.magnitude;

        if (distance <= 0.001f)
        {
            return new AudioState { volume = 1.0f, clarity = 1.0f, pan = 0.0f, spatialBlend = spatialBlend, spread = 0.0f, facingScore = 1.0f };
        }

        Vector3 directionToSound = toSound.normalized;

        // 【新規追加】プレイヤーの視線とオルゴールの方向の角度差から「向いている度合い(0.0〜1.0)」を計算
        float angleToSound = Vector3.Angle(listenerTransform.forward, directionToSound);
        float facingScore = Mathf.InverseLerp(listenNotFacingAngle, listenFacingAngle, angleToSound);

        float horizontalDistance = new Vector2(toSound.x, toSound.z).magnitude;
        float verticalDistance = Mathf.Abs(toSound.y);
        float verticalColumnRadiusByHeight = verticalDistance * verticalColumnHorizontalRate;
        bool soundIsVerticalColumn = verticalDistance >= 0.25f && horizontalDistance <= Mathf.Max(verticalColumnHorizontalRadius, verticalColumnRadiusByHeight);

        float listenerForwardY = listenerTransform.forward.normalized.y;
        bool soundIsAlmostVertical = Mathf.Abs(directionToSound.y) >= verticalExtremeDot || soundIsVerticalColumn;
        bool listenerLooksAlmostVertical = Mathf.Abs(listenerForwardY) >= verticalExtremeDot;
        bool listenerLooksTowardVerticalSound = soundIsAlmostVertical && listenerLooksAlmostVertical && Mathf.Sign(listenerForwardY) == Mathf.Sign(directionToSound.y);
        bool listenerLooksOppositeVerticalSound = soundIsAlmostVertical && listenerLooksAlmostVertical && Mathf.Sign(listenerForwardY) != Mathf.Sign(directionToSound.y);

        float horizontalRate = CalculateHorizontalRate(directionToSound);
        float verticalRate = CalculateVerticalRate(directionToSound);
        float verticalClarityRate = CalculateVerticalClarityRate(directionToSound);
        float rawVerticalRate = verticalRate;
        float sideRate = CalculateSideRate(directionToSound);
        float pan = CalculateStereoPan(directionToSound);
        float dynamicSpatialBlend = spatialBlend;
        float dynamicSpread = 0.0f;

        if (soundIsVerticalColumn)
        {
            horizontalRate = 1.0f;
            sideRate = 0.0f;
            pan = 0.0f;
            verticalRate = 1.0f;
            dynamicSpatialBlend = verticalColumnSpatialBlend;
            dynamicSpread = verticalColumnSpread;
        }

        WallOcclusionState wallOcclusionState = CalculateWallOcclusionState(listenerPosition, directionToSound, distance);
        float wallVolume = wallOcclusionState.volumeRate;
        float wallClarity = wallOcclusionState.clarityRate;

        float distanceVolume = Mathf.Lerp(farMinimumVolume, 1.0f, 1.0f - Mathf.Clamp01(distance / distanceFalloffRange));
        distanceVolume = Mathf.Min(distanceVolume, closeDistanceVolumeLimit);

        float heightDifference = Mathf.Abs(toSound.y);
        bool differentFloor = heightDifference >= floorHeightThreshold;
        float floorEffectRate = differentFloor ? Mathf.InverseLerp(floorHeightThreshold, differentFloorFullEffectHeight, heightDifference) : 0.0f;
        float wrongLookRate = floorEffectRate * Mathf.InverseLerp(differentFloorWrongLookStartRate, differentFloorWrongLookFullRate, rawVerticalRate);

        float floorBaseVolumeRate = Mathf.Lerp(1.0f, differentFloorBaseVolumeRate, floorEffectRate);
        float floorBaseClarityRate = Mathf.Lerp(1.0f, differentFloorBaseClarityRate, floorEffectRate);
        float floorCeilingVolumeRate = Mathf.Lerp(1.0f, differentFloorCeilingVolumeRate, floorEffectRate);
        float floorCeilingClarityRate = Mathf.Lerp(1.0f, differentFloorCeilingClarityRate, floorEffectRate);
        float floorLookVolumeRate = Mathf.Lerp(1.0f, differentFloorWrongLookVolumeRate, wrongLookRate);
        float floorLookClarityRate = Mathf.Lerp(1.0f, differentFloorWrongLookClarityRate, wrongLookRate);

        float frontAndVerticalRate = horizontalRate * verticalRate;
        float directionBonus = (frontBonusVolume + verticalBonusVolume) * frontAndVerticalRate;
        float sideDirectionBonus = sideDirectionBonusVolume * sideRate * verticalRate;
        float limitedDirectionBonus = Mathf.Min(directionBonus + sideDirectionBonus, directionBonusLimit);

        float finalVolume = Mathf.Clamp01((distanceVolume + limitedDirectionBonus) * horizontalRate * verticalRate * floorBaseVolumeRate * floorCeilingVolumeRate * floorLookVolumeRate * wallVolume);
        finalVolume = Mathf.Min(finalVolume, naturalVolumeLimit);

        if (differentFloor && soundIsVerticalColumn)
        {
            finalVolume = Mathf.Max(finalVolume, differentFloorPassingMinimumVolume * floorEffectRate);
        }

        if (soundIsAlmostVertical || listenerLooksAlmostVertical)
        {
            float verticalExtremeMinimum = verticalExtremeMinimumVolume;

            if (soundIsAlmostVertical)
            {
                if (listenerLooksTowardVerticalSound) verticalExtremeMinimum = verticalExtremeFacingMinimumVolume;
                else if (listenerLooksOppositeVerticalSound) verticalExtremeMinimum = verticalExtremeOppositeMinimumVolume;
                else verticalExtremeMinimum = verticalExtremeColumnMinimumVolume;
            }

            if (soundIsVerticalColumn)
            {
                if (differentFloor)
                {
                    verticalExtremeMinimum = differentFloorPassingMinimumVolume * floorEffectRate;
                }
                else
                {
                    verticalExtremeMinimum = Mathf.Max(verticalExtremeMinimum, verticalColumnSafeMinimumVolume);
                    if (!listenerLooksAlmostVertical) verticalExtremeMinimum = Mathf.Max(verticalExtremeMinimum, verticalColumnForwardMinimumVolume);
                }
            }

            finalVolume = Mathf.Max(finalVolume, verticalExtremeMinimum);
            finalVolume = Mathf.Min(finalVolume, naturalVolumeLimit);

            if (differentFloor && soundIsVerticalColumn)
            {
                finalVolume = Mathf.Min(finalVolume, differentFloorVerticalColumnVolumeLimit);
            }
            pan = 0.0f;
        }

        float finalClarity = Mathf.Clamp01(horizontalRate * verticalClarityRate * floorBaseClarityRate * floorCeilingClarityRate * floorLookClarityRate * wallClarity);

        if (differentFloor && soundIsVerticalColumn)
        {
            finalClarity = Mathf.Max(finalClarity, differentFloorPassingMinimumClarity * floorEffectRate * wallClarity);
        }

        if (soundIsAlmostVertical || listenerLooksAlmostVertical)
        {
            float verticalExtremeMinimumClarity = 0.22f;

            if (soundIsAlmostVertical)
            {
                if (listenerLooksTowardVerticalSound) verticalExtremeMinimumClarity = 0.50f;
                else if (listenerLooksOppositeVerticalSound) verticalExtremeMinimumClarity = 0.16f;
                else verticalExtremeMinimumClarity = 0.32f;
            }

            if (soundIsVerticalColumn)
            {
                if (differentFloor)
                {
                    verticalExtremeMinimumClarity = differentFloorPassingMinimumClarity * floorEffectRate;
                }
                else
                {
                    verticalExtremeMinimumClarity = Mathf.Max(verticalExtremeMinimumClarity, 0.38f);
                    if (!listenerLooksAlmostVertical) verticalExtremeMinimumClarity = Mathf.Max(verticalExtremeMinimumClarity, verticalColumnForwardMinimumClarity);
                }
            }

            if (soundIsVerticalColumn)
            {
                finalClarity = Mathf.Max(finalClarity, verticalExtremeMinimumClarity);
                if (differentFloor) finalClarity = Mathf.Min(finalClarity, differentFloorVerticalColumnClarityLimit);
            }
            else
            {
                finalClarity = Mathf.Max(finalClarity, verticalExtremeMinimumClarity * wallClarity);
            }
        }

        return new AudioState
        {
            volume = finalVolume,
            clarity = finalClarity,
            pan = pan,
            spatialBlend = dynamicSpatialBlend,
            spread = dynamicSpread,
            facingScore = facingScore // 計算した向いている度合いを渡す
        };
    }

    private WallOcclusionState CalculateWallOcclusionState(Vector3 listenerPosition, Vector3 directionToSound, float distance)
    {
        if (wallLayer.value == 0) return new WallOcclusionState { wallCount = 0, volumeRate = 1.0f, clarityRate = 1.0f };

        RaycastHit[] hits = Physics.RaycastAll(listenerPosition, directionToSound, distance, wallLayer, QueryTriggerInteraction.Ignore);
        int wallCount = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null) continue;

            if (ignoreFloorAreaInWallOcclusion && hitCollider.GetComponentInParent<FloorArea>() != null) continue;

            wallCount++;
            if (wallCount >= maxWallHitCount) break;
        }

        if (wallCount <= 0) return new WallOcclusionState { wallCount = 0, volumeRate = 1.0f, clarityRate = 1.0f };
        if (wallCount == 1) return new WallOcclusionState { wallCount = 1, volumeRate = Mathf.Max(singleWallVolumeRate, minimumWallVolumeRate), clarityRate = singleWallClarityRate };

        return new WallOcclusionState { wallCount = wallCount, volumeRate = Mathf.Max(multiWallVolumeRate, minimumWallVolumeRate), clarityRate = multiWallClarityRate };
    }

    private float CalculateHorizontalRate(Vector3 directionToSound)
    {
        Vector3 listenerForward = listenerTransform.forward;
        listenerForward.y = 0.0f;
        Vector3 flatDirectionToSound = directionToSound;
        flatDirectionToSound.y = 0.0f;

        if (listenerForward.sqrMagnitude <= 0.0001f || flatDirectionToSound.sqrMagnitude <= 0.0001f) return 1.0f;

        listenerForward.Normalize();
        flatDirectionToSound.Normalize();

        float horizontalAngle = Vector3.Angle(listenerForward, flatDirectionToSound);
        if (horizontalAngle <= clearAngle) return 1.0f;
        if (horizontalAngle >= weakAngle) return backSideVolumeRate;

        return Mathf.Lerp(backSideVolumeRate, 1.0f, Mathf.InverseLerp(weakAngle, clearAngle, horizontalAngle));
    }

    private float CalculateSideRate(Vector3 directionToSound)
    {
        Vector3 listenerRight = listenerTransform.right;
        listenerRight.y = 0.0f;
        Vector3 flatDirectionToSound = directionToSound;
        flatDirectionToSound.y = 0.0f;

        if (listenerRight.sqrMagnitude <= 0.0001f || flatDirectionToSound.sqrMagnitude <= 0.0001f) return 0.0f;

        listenerRight.Normalize();
        flatDirectionToSound.Normalize();
        return Mathf.Abs(Vector3.Dot(listenerRight, flatDirectionToSound));
    }

    private float CalculateStereoPan(Vector3 directionToSound)
    {
        Vector3 listenerRight = listenerTransform.right;
        listenerRight.y = 0.0f;
        Vector3 flatDirectionToSound = directionToSound;
        flatDirectionToSound.y = 0.0f;

        if (listenerRight.sqrMagnitude <= 0.0001f || flatDirectionToSound.sqrMagnitude <= 0.0001f) return 0.0f;

        listenerRight.Normalize();
        flatDirectionToSound.Normalize();

        float rawPan = Vector3.Dot(listenerRight, flatDirectionToSound);
        return Mathf.Clamp(rawPan * stereoPanStrength, -1.0f, 1.0f);
    }

    private float CalculateVerticalRate(Vector3 directionToSound)
    {
        float verticalAngle = CalculateVerticalAngle(directionToSound);
        if (verticalAngle <= verticalClearAngle) return 1.0f;
        if (verticalAngle >= verticalWeakAngle) return verticalMismatchRate;

        return Mathf.Lerp(verticalMismatchRate, 1.0f, Mathf.InverseLerp(verticalWeakAngle, verticalClearAngle, verticalAngle));
    }

    private float CalculateVerticalClarityRate(Vector3 directionToSound)
    {
        float verticalAngle = CalculateVerticalAngle(directionToSound);
        if (verticalAngle <= verticalClearAngle) return 1.0f;
        if (verticalAngle >= verticalWeakAngle) return verticalMismatchClarityRate;

        return Mathf.Lerp(verticalMismatchClarityRate, 1.0f, Mathf.InverseLerp(verticalWeakAngle, verticalClearAngle, verticalAngle));
    }

    private float CalculateVerticalAngle(Vector3 directionToSound)
    {
        float listenerPitch = Mathf.Asin(Mathf.Clamp(listenerTransform.forward.normalized.y, -1.0f, 1.0f)) * Mathf.Rad2Deg;
        float soundPitch = Mathf.Asin(Mathf.Clamp(directionToSound.normalized.y, -1.0f, 1.0f)) * Mathf.Rad2Deg;
        return Mathf.Abs(Mathf.DeltaAngle(listenerPitch, soundPitch));
    }
}