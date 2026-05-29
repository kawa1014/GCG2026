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
    [SerializeField] private float farMinimumVolume = 0.14f;

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
    [SerializeField] private float verticalMismatchRate = 0.08f;

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
    /// 音源がほぼ真上・真下、またはカメラがほぼ真上・真下を向いている時の判定角度。
    /// 0.92なら、だいたい上下23度以内を特殊処理する。
    /// </summary>
    [Header("真上・真下の無音防止")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float verticalExtremeDot = 0.92f;

    /// <summary>
    /// 真上・真下の音源を正しく向いている時に、最低限残す音量倍率。
    /// 上下判定や階層差が重なっても、完全に無音になるのを防ぐ。
    /// </summary>
    [SerializeField] private float verticalExtremeFacingMinimumVolume = 0.30f;

    /// <summary>
    /// オルゴールがほぼ真上・真下にあり、プレイヤーが正面を向いている時に残す最低音量倍率。
    /// 真上・真下を向いていなくても、真上/真下に音源があることに気づけるようにする。
    /// </summary>
    [SerializeField] private float verticalExtremeColumnMinimumVolume = 0.22f;

    /// <summary>
    /// オルゴールがほぼ真上・真下にあるのに、プレイヤーが逆方向の真上・真下を向いた時に残す最低音量倍率。
    /// 例：音源が真下なのに真上を向いている場合。
    /// </summary>
    [SerializeField] private float verticalExtremeOppositeMinimumVolume = 0.10f;

    /// <summary>
    /// 真上・真下の音源を向けていない時でも、完全に消えないように残す最低音量倍率。
    /// 主に「カメラだけが真上/真下を向いている」場合の保険。
    /// </summary>
    [SerializeField] private float verticalExtremeMinimumVolume = 0.12f;

    /// <summary>
    /// [Fix] 音源がほぼ真上/真下にある時、Unity標準の3D音が極端に弱くなるのを避けるための判定半径。
    /// プレイヤーとオルゴールのXZ距離がこの値以下なら、上下柱判定として扱う。
    /// </summary>
    [SerializeField] private float verticalColumnHorizontalRadius = 4.0f;

    /// <summary>
    /// [Fix] 高さ差に対してXZ距離がこの割合以下なら、ほぼ真上/真下として扱う。
    /// 例：高さ差4m、値0.45なら、XZ距離1.8m以内を上下柱判定にする。
    /// </summary>
    [SerializeField] private float verticalColumnHorizontalRate = 1.20f;

    /// <summary>
    /// [Fix] 音源が真上/真下にある時の最低音量。
    /// 正面を向いていても、完全に聞こえなくなるのを防ぐ。
    /// </summary>
    [SerializeField] private float verticalColumnSafeMinimumVolume = 0.65f;

    /// <summary>
    /// [Fix] 音源が真上/真下にある時、正面を向いていても最低限この音量を残す。
    /// 「真下にいる状態で前を向くと音が消える」問題への直接対策。
    /// </summary>
    [SerializeField] private float verticalColumnForwardMinimumVolume = 0.70f;

    /// <summary>
    /// [Fix] 音源が真上/真下にある時、正面を向いていても最低限この鮮明度を残す。
    /// 完全にクリアにはしないが、無音に感じない程度にする。
    /// </summary>
    [SerializeField] private float verticalColumnForwardMinimumClarity = 0.65f;

    /// <summary>
    /// [Fix] 音源が真上/真下にある時だけ、Unity標準3Dの上下方向で音が消える問題を避けるためにSpatial Blendを下げる。
    /// 0.0がおすすめ。真上/真下だけ2D寄りにして、Unity標準3Dの上下方向で音が消える問題を止める。
    /// </summary>
    [Range(0.0f, 1.0f)]
    [SerializeField] private float verticalColumnSpatialBlend = 0.0f;

    /// <summary>
    /// [Fix] 音源が真上/真下にある時だけ音の広がりを増やし、聞こえなくなる位置を減らす。
    /// </summary>
    [SerializeField] private float verticalColumnSpread = 180.0f;

    /// <summary>
    /// [Fix] 真上/真下対策中は、AudioSource側の距離減衰が原因で音が消えないように最小距離を広げる。
    /// </summary>
    [SerializeField] private float verticalColumnMinDistance = 50.0f;

    /// <summary>
    /// [Fix] 真上/真下対策中は、AudioSource側の最大距離不足で音が消えないように最大距離を広げる。
    /// </summary>
    [SerializeField] private float verticalColumnMaxDistance = 500.0f;

    /// <summary>
    /// プレイヤーと音源の高さ差がこれ以上ある場合、階層違いとして扱う。
    /// </summary>
    [Header("階層差")]
    [SerializeField] private float floorHeightThreshold = 1.5f;

    /// <summary>
    /// 階層差がある時に、常に全体音量へ掛ける倍率。
    /// 例：1階で鳴っているのに2階を探している時は、音を全体的に小さくする。
    /// </summary>
    [SerializeField] private float differentFloorBaseVolumeRate = 0.55f;

    /// <summary>
    /// 階層差がある時に、常に鮮明度へ掛ける倍率。
    /// 違う階にいる時の「目の前で鳴っている感じ」を弱める。
    /// </summary>
    [SerializeField] private float differentFloorBaseClarityRate = 0.60f;

    /// <summary>
    /// 階層差があるのに上下方向を見ていない時の追加音量倍率。
    /// 小さいほど、違う階を探している時に音がさらに小さくなる。
    /// </summary>
    [SerializeField] private float differentFloorWrongLookVolumeRate = 0.35f;

    /// <summary>
    /// 階層差があるのに上下方向を見ていない時の追加鮮明度倍率。
    /// 小さいほど、違う階を見ている時にさらにこもる。
    /// </summary>
    [SerializeField] private float differentFloorWrongLookClarityRate = 0.25f;

    /// <summary>
    /// [Fix] 階層差の効果が最大になる高さ差。
    /// Floor Height Thresholdを超えた瞬間に音が急落しないよう、ここまでの高さ差でなめらかに補正を強くする。
    /// </summary>
    [SerializeField] private float differentFloorFullEffectHeight = 4.5f;

    /// <summary>
    /// [Fix] 上下方向を見ていない判定を始める上下一致率。
    /// この値より下がると、違う階で見ていない補正が少しずつ入り始める。
    /// </summary>
    [SerializeField] private float differentFloorWrongLookStartRate = 0.70f;

    /// <summary>
    /// [Fix] 上下方向を完全に見ていない扱いにする上下一致率。
    /// この値以下で、Different Floor Wrong Look系の倍率が最大まで効く。
    /// </summary>
    [SerializeField] private float differentFloorWrongLookFullRate = 0.25f;

    /// <summary>
    /// [Fix] 違う階のオルゴールの真上/真下付近を通り過ぎる時に残す最低音量。
    /// 床をまたいだ瞬間や真下を通った瞬間に、音が極端に小さくなるのを防ぐ。
    /// </summary>
    [SerializeField] private float differentFloorPassingMinimumVolume = 0.28f;

    /// <summary>
    /// [Fix] 違う階のオルゴールの真上/真下付近を通り過ぎる時に残す最低鮮明度。
    /// 完全にクリアにはしないが、急に消えたようなこもり方を防ぐ。
    /// </summary>
    [SerializeField] private float differentFloorPassingMinimumClarity = 0.28f;

    /// <summary>
    /// [Fix] 違う階で真上/真下付近にいる時の最大音量。
    /// 真上/真下の無音防止が強すぎて、違う階なのに近くで鳴っているように聞こえる問題を防ぐ。
    /// </summary>
    [SerializeField] private float differentFloorVerticalColumnVolumeLimit = 0.48f;

    /// <summary>
    /// [Fix] 違う階で真上/真下付近にいる時の最大鮮明度。
    /// 違う階の音は少しこもらせ、同じ部屋で鳴っている感じを弱める。
    /// </summary>
    [SerializeField] private float differentFloorVerticalColumnClarityLimit = 0.46f;

    /// <summary>
    /// 壁として扱うLayer。
    /// Wall Layerに指定したレイヤーだけを壁越し判定に使う。
    /// </summary>
    [Header("壁越し")]
    [SerializeField] private LayerMask wallLayer;

    /// <summary>
    /// [Fix] FloorAreaが付いている床・天井を壁越し判定から除外する。
    /// 1階と2階の床をWall Layerにしている場合、Raycastが床を壁として数えて音が急に小さくなるため。
    /// 階数差の聞こえ方はFloorArea側の階数補正で作る。
    /// </summary>
    [SerializeField] private bool ignoreFloorAreaInWallOcclusion = true;

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

        /// <summary>
        /// [Fix] 状況ごとのSpatial Blend。真上/真下の時だけ3D比率を下げて無音化を防ぐ。
        /// </summary>
        public float spatialBlend;

        /// <summary>
        /// [Fix] 状況ごとのSpread。真上/真下の時だけ広げて音抜けを防ぐ。
        /// </summary>
        public float spread;
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
        verticalExtremeDot = Mathf.Clamp01(verticalExtremeDot);
        verticalExtremeFacingMinimumVolume = Mathf.Clamp01(verticalExtremeFacingMinimumVolume);
        verticalExtremeColumnMinimumVolume = Mathf.Clamp01(verticalExtremeColumnMinimumVolume);
        verticalExtremeOppositeMinimumVolume = Mathf.Clamp01(verticalExtremeOppositeMinimumVolume);
        verticalExtremeMinimumVolume = Mathf.Clamp01(verticalExtremeMinimumVolume);
        verticalColumnHorizontalRadius = Mathf.Max(0.0f, verticalColumnHorizontalRadius);
        verticalColumnHorizontalRate = Mathf.Max(0.0f, verticalColumnHorizontalRate);
        // [Fix] 既にシーンへ置いたコンポーネントは古いInspector値を保持するため、
        // 無音対策に必要な最低値はここで強制的に引き上げる。
        verticalColumnSafeMinimumVolume = Mathf.Max(Mathf.Clamp01(verticalColumnSafeMinimumVolume), 0.65f);
        verticalColumnForwardMinimumVolume = Mathf.Max(Mathf.Clamp01(verticalColumnForwardMinimumVolume), 0.70f);
        verticalColumnForwardMinimumClarity = Mathf.Max(Mathf.Clamp01(verticalColumnForwardMinimumClarity), 0.65f);
        verticalColumnSpatialBlend = 0.0f;
        verticalColumnSpread = Mathf.Clamp(Mathf.Max(verticalColumnSpread, 180.0f), 0.0f, 360.0f);
        verticalColumnHorizontalRadius = Mathf.Max(verticalColumnHorizontalRadius, 4.0f);
        verticalColumnHorizontalRate = Mathf.Max(verticalColumnHorizontalRate, 1.20f);
        verticalColumnMinDistance = Mathf.Max(verticalColumnMinDistance, 50.0f);
        verticalColumnMaxDistance = Mathf.Max(verticalColumnMaxDistance, verticalColumnMinDistance + 100.0f);
        differentFloorBaseVolumeRate = Mathf.Clamp01(differentFloorBaseVolumeRate);
        differentFloorBaseClarityRate = Mathf.Clamp01(differentFloorBaseClarityRate);
        differentFloorWrongLookVolumeRate = Mathf.Clamp01(differentFloorWrongLookVolumeRate);
        differentFloorWrongLookClarityRate = Mathf.Clamp01(differentFloorWrongLookClarityRate);
        differentFloorFullEffectHeight = Mathf.Max(floorHeightThreshold + 0.01f, differentFloorFullEffectHeight);
        differentFloorWrongLookStartRate = Mathf.Clamp01(differentFloorWrongLookStartRate);
        differentFloorWrongLookFullRate = Mathf.Clamp01(differentFloorWrongLookFullRate);
        if (differentFloorWrongLookFullRate > differentFloorWrongLookStartRate)
        {
            differentFloorWrongLookFullRate = differentFloorWrongLookStartRate;
        }
        differentFloorPassingMinimumVolume = Mathf.Clamp01(differentFloorPassingMinimumVolume);
        differentFloorPassingMinimumClarity = Mathf.Clamp01(differentFloorPassingMinimumClarity);
        differentFloorVerticalColumnVolumeLimit = Mathf.Clamp(differentFloorVerticalColumnVolumeLimit, 0.05f, naturalVolumeLimit);
        differentFloorVerticalColumnClarityLimit = Mathf.Clamp01(differentFloorVerticalColumnClarityLimit);
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

        // AudioSource側の距離設定が小さいと、こちらでvolumeを上げても最終的に聞こえないことがある。
        // 通常時も最低限の聞こえを確保するため、極端に小さい値だけ補正する。
        targetAudioSource.minDistance = Mathf.Max(targetAudioSource.minDistance, 1.0f);
        targetAudioSource.maxDistance = Mathf.Max(targetAudioSource.maxDistance, distanceFalloffRange);
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

        // [Fix] 真上/真下のようにUnity標準3D音が弱くなりやすい場面では、
        // このフレームだけSpatial BlendとSpreadを切り替える。
        targetAudioSource.spatialBlend = audioState.spatialBlend;
        targetAudioSource.spread = audioState.spread;

        // [Fix] 真上/真下の時は、AudioSourceの3D距離減衰も一時的に広げる。
        // これで「スクリプト上は音量があるのにUnity側の距離減衰で聞こえない」を防ぐ。
        if (audioState.spatialBlend <= 0.001f)
        {
            targetAudioSource.minDistance = verticalColumnMinDistance;
            targetAudioSource.maxDistance = verticalColumnMaxDistance;
        }

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
                pan = 0.0f,
                spatialBlend = spatialBlend,
                spread = 0.0f
            };
        }

        Vector3 directionToSound = toSound.normalized;

        float horizontalDistance = new Vector2(toSound.x, toSound.z).magnitude;
        float verticalDistance = Mathf.Abs(toSound.y);
        float verticalColumnRadiusByHeight = verticalDistance * verticalColumnHorizontalRate;
        bool soundIsVerticalColumn =
            verticalDistance >= 0.25f
            && horizontalDistance <= Mathf.Max(verticalColumnHorizontalRadius, verticalColumnRadiusByHeight);

        float listenerForwardY = listenerTransform.forward.normalized.y;
        bool soundIsAlmostVertical = Mathf.Abs(directionToSound.y) >= verticalExtremeDot || soundIsVerticalColumn;
        bool listenerLooksAlmostVertical = Mathf.Abs(listenerForwardY) >= verticalExtremeDot;
        bool listenerLooksTowardVerticalSound =
            soundIsAlmostVertical
            && listenerLooksAlmostVertical
            && Mathf.Sign(listenerForwardY) == Mathf.Sign(directionToSound.y);
        bool listenerLooksOppositeVerticalSound =
            soundIsAlmostVertical
            && listenerLooksAlmostVertical
            && Mathf.Sign(listenerForwardY) != Mathf.Sign(directionToSound.y);

        float horizontalRate = CalculateHorizontalRate(directionToSound);
        float verticalRate = CalculateVerticalRate(directionToSound);
        float verticalClarityRate = CalculateVerticalClarityRate(directionToSound);

        // [Fix] 真上/真下の無音防止で verticalRate を底上げする前の値。
        // 階数違いの「上下を見ていない」判定にはこの生の値を使う。
        // これにより、補正が急にON/OFFせず自然に変化する。
        float rawVerticalRate = verticalRate;

        float sideRate = CalculateSideRate(directionToSound);
        float pan = CalculateStereoPan(directionToSound);
        float dynamicSpatialBlend = spatialBlend;
        float dynamicSpread = 0.0f;

        if (soundIsVerticalColumn)
        {
            // [Fix] 真上/真下の音源では、水平角度・左右パンを無理に使わない。
            // 真上/真下にいるのに前を向いた時、水平判定で音が消えるのを防ぐ。
            horizontalRate = 1.0f;
            sideRate = 0.0f;
            pan = 0.0f;

            // [Fix] 真下/真上にいる時は、上下の向きズレによる極端な音量低下を防ぐ。
            // 前を向いているだけで verticalMismatchRate まで落ちると、無音に感じるため強めに保証する。
            verticalRate = 1.0f;

            // [Fix] Unity標準の3D音は真上/真下で聞こえ方が不安定になりやすい。
            // この時だけ2D寄りにして、音量・こもり・方向感はこのスクリプト側で制御する。
            dynamicSpatialBlend = verticalColumnSpatialBlend;
            dynamicSpread = verticalColumnSpread;
        }

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

        // [Fix] 階数差を bool で急に切り替えると、
        // オルゴールの真下/真上を通り過ぎた瞬間に音が極端に小さくなる。
        // そのため、高さ差に応じて0〜1でなめらかに階層補正を強くする。
        bool differentFloor = heightDifference >= floorHeightThreshold;
        float floorEffectRate = differentFloor
            ? Mathf.InverseLerp(floorHeightThreshold, differentFloorFullEffectHeight, heightDifference)
            : 0.0f;

        // [Fix] 「違う階を見ていない」補正も急にONにせず、
        // 上下方向の一致率が下がるほど少しずつ効かせる。
        float wrongLookRate = floorEffectRate * Mathf.InverseLerp(
            differentFloorWrongLookStartRate,
            differentFloorWrongLookFullRate,
            rawVerticalRate
        );

        float floorBaseVolumeRate = Mathf.Lerp(1.0f, differentFloorBaseVolumeRate, floorEffectRate);
        float floorBaseClarityRate = Mathf.Lerp(1.0f, differentFloorBaseClarityRate, floorEffectRate);

        float floorLookVolumeRate = Mathf.Lerp(1.0f, differentFloorWrongLookVolumeRate, wrongLookRate);
        float floorLookClarityRate = Mathf.Lerp(1.0f, differentFloorWrongLookClarityRate, wrongLookRate);

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

        if (differentFloor && soundIsVerticalColumn)
        {
            // [Fix] 違う階のオルゴールの真上/真下付近を通る時だけ、
            // 階層差・上下方向・壁越し補正が重なっても極端には小さくしない。
            // floorEffectRateを掛けることで、高さ差が小さい時は保証も弱くなる。
            finalVolume = Mathf.Max(finalVolume, differentFloorPassingMinimumVolume * floorEffectRate);
        }

        // 真上・真下の特殊対策。
        // 真上/真下は左右方向の情報がほぼ0になり、上下補正・階層差・壁補正が重なると
        // 音量が極端に小さくなりすぎることがある。
        // そのため、上下の極端なケースだけ最低音量を保証する。
        if (soundIsAlmostVertical || listenerLooksAlmostVertical)
        {
            float verticalExtremeMinimum = verticalExtremeMinimumVolume;

            if (soundIsAlmostVertical)
            {
                if (listenerLooksTowardVerticalSound)
                {
                    // 音源が真上/真下で、その方向をちゃんと向いている時。
                    verticalExtremeMinimum = verticalExtremeFacingMinimumVolume;
                }
                else if (listenerLooksOppositeVerticalSound)
                {
                    // 音源が真下なのに真上を見る、または音源が真上なのに真下を見る時。
                    // 完全無音にはしないが、方向違いとして弱める。
                    verticalExtremeMinimum = verticalExtremeOppositeMinimumVolume;
                }
                else
                {
                    // [Fix] 今回の重要修正。
                    // 音源が真上/真下にある状態でプレイヤーが正面を向くと、
                    // 上下補正が弱判定になって音が消えやすかった。
                    // 正面を向いていても、真上/真下に音源があることが分かる最低音量を残す。
                    verticalExtremeMinimum = verticalExtremeColumnMinimumVolume;
                }
            }

            // 真上・真下の時は、距離減衰・階層差・壁越し補正が重なっても
            // 完全に無音にならないように、最終音量そのものへ最低保証をかける。
            if (soundIsVerticalColumn)
            {
                if (differentFloor)
                {
                    // [Fix] 違う階では「真上/真下の無音防止」を弱める。
                    // ここで verticalColumnSafeMinimumVolume / verticalColumnForwardMinimumVolume を使うと、
                    // 違う階なのに音量0.65〜0.70が保証されて大きすぎる。
                    verticalExtremeMinimum = Mathf.Max(
                        verticalExtremeMinimum,
                        differentFloorPassingMinimumVolume * floorEffectRate
                    );
                }
                else
                {
                    verticalExtremeMinimum = Mathf.Max(verticalExtremeMinimum, verticalColumnSafeMinimumVolume);

                    if (!listenerLooksAlmostVertical)
                    {
                        // [Fix] 同じ階の真上/真下にいて、カメラを前へ向けている時の最低保証。
                        // 違う階では使わない。階数差の聞こえ方が消えるため。
                        verticalExtremeMinimum = Mathf.Max(verticalExtremeMinimum, verticalColumnForwardMinimumVolume);
                    }
                }
            }

            finalVolume = Mathf.Max(finalVolume, verticalExtremeMinimum);
            finalVolume = Mathf.Min(finalVolume, naturalVolumeLimit);

            if (differentFloor && soundIsVerticalColumn)
            {
                // [Fix] 違う階の真上/真下付近では最大音量にも上限をかける。
                // これで「上の部屋で鳴っているのに、真下を通ると同じ部屋みたいにデカい」を防ぐ。
                finalVolume = Mathf.Min(finalVolume, differentFloorVerticalColumnVolumeLimit);
            }

            // 真上・真下は左右差が存在しないため、パンを中央に戻す。
            // これでUnityの3Dパン補助が極端な上下方向で不安定になるのを防ぐ。
            pan = 0.0f;
        }

        // 鮮明度は、真正面・上下一致・壁なしの時に最も高くなる。
        float finalClarity = Mathf.Clamp01(
            horizontalRate
            * verticalClarityRate
            * floorBaseClarityRate
            * floorLookClarityRate
            * wallClarity
        );

        if (differentFloor && soundIsVerticalColumn)
        {
            // [Fix] 違う階の真上/真下付近で、LowPassが急に下がりすぎるのを防ぐ。
            // 壁越しのこもりは残すため、wallClarityは掛ける。
            finalClarity = Mathf.Max(
                finalClarity,
                differentFloorPassingMinimumClarity * floorEffectRate * wallClarity
            );
        }

        if (soundIsAlmostVertical || listenerLooksAlmostVertical)
        {
            // [Fix] 真上・真下で音が完全にこもりすぎて消えたように感じるのを防ぐ。
            // 壁越しや違う階の濁りは残しつつ、最低限の聞こえを保証する。
            float verticalExtremeMinimumClarity = 0.22f;

            if (soundIsAlmostVertical)
            {
                if (listenerLooksTowardVerticalSound)
                {
                    verticalExtremeMinimumClarity = 0.50f;
                }
                else if (listenerLooksOppositeVerticalSound)
                {
                    verticalExtremeMinimumClarity = 0.16f;
                }
                else
                {
                    // 音源が真上/真下で、正面を向いている時。
                    // 少しこもるが、消えたようには感じない程度に残す。
                    verticalExtremeMinimumClarity = 0.32f;
                }
            }

            if (soundIsVerticalColumn)
            {
                if (differentFloor)
                {
                    // [Fix] 違う階では真上/真下でもクリアにしすぎない。
                    // verticalColumnForwardMinimumClarity = 0.65 を使うと、
                    // 上の部屋の音が同じ部屋のように鮮明になる。
                    verticalExtremeMinimumClarity = Mathf.Max(
                        verticalExtremeMinimumClarity,
                        differentFloorPassingMinimumClarity * floorEffectRate
                    );
                }
                else
                {
                    verticalExtremeMinimumClarity = Mathf.Max(verticalExtremeMinimumClarity, 0.38f);

                    if (!listenerLooksAlmostVertical)
                    {
                        // [Fix] 同じ階の真上/真下の音源を前向きで聞いた時、
                        // こもりすぎて消えたように感じるのを防ぐ。
                        verticalExtremeMinimumClarity = Mathf.Max(verticalExtremeMinimumClarity, verticalColumnForwardMinimumClarity);
                    }
                }
            }

            if (soundIsVerticalColumn)
            {
                // [Fix] 真上/真下の無音問題を優先する。
                // ただし、違う階では後で最大鮮明度の上限をかける。
                finalClarity = Mathf.Max(finalClarity, verticalExtremeMinimumClarity);

                if (differentFloor)
                {
                    finalClarity = Mathf.Min(finalClarity, differentFloorVerticalColumnClarityLimit);
                }
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
            spread = dynamicSpread
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

        int wallCount = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;

            if (hitCollider == null)
            {
                continue;
            }

            // [Fix] FloorArea付きの床・天井は壁として数えない。
            // オルゴールが上の部屋にある時、1階と2階の床をWallとして数えると、
            // 真下付近を通った瞬間に壁2枚以上扱いになり、音が急に小さくなる。
            // 階数違いの音量差はfloorBaseVolumeRate側で処理する。
            if (ignoreFloorAreaInWallOcclusion
                && hitCollider.GetComponentInParent<FloorArea>() != null)
            {
                continue;
            }

            wallCount++;

            if (wallCount >= maxWallHitCount)
            {
                break;
            }
        }

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
