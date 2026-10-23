using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ドアのオブジェクトと、再追加時の開く角度をセットで管理するクラス
/// </summary>
[System.Serializable]
public class DoorConfig
{
    [Tooltip("開ける対象のドアのGameObject")]
    public GameObject DoorObject;

    [Tooltip("ドアを開ける角度")]
    public float OpenAngle;
}

/// <summary>
/// ゲームクリア時のムービー演出を管理するクラス
/// </summary>
public class ClearMovieController : MonoBehaviour
{
    [Header("フェード用UI設定")]
    [SerializeField, Tooltip("ムービー専用のフェード用Image")]
    private Image FadeImage;

    [Header("プレイヤー・扉の参照")]
    [SerializeField, Tooltip("プレイヤーの操作を止めるため、PlayerControllerを指定します")]
    private PlayerController TargetPlayerController;

    [SerializeField, Tooltip("プレイヤー本体(移動させる対象)")]
    private Transform PlayerTransform;

    [Header("扉の設定")]
    [SerializeField, Tooltip("開ける対象の扉のリスト")]
    private List<DoorConfig> TargetDoors = new List<DoorConfig>();

    [Header("ムービーの移動ポイント")]
    [SerializeField, Tooltip("ムービー開始時にワープする初期位置")]
    private Transform MoviewStartPosition;

    [SerializeField, Tooltip("扉の前に立つ目標位置")]
    private Transform DoorStandPosition;

    [SerializeField, Tooltip("扉を開けて外に出る目標位置")]
    private Transform OutsidePosition;

    [Header("演出設定")]
    [SerializeField, Tooltip("自動で歩くスピード")]
    private float WalkSpeed = 2.0f;

    [Header("朝になる演出設定")]

    [SerializeField, Tooltip("夜の暗闇を作っているVolume(これの影響力を0にして暗闇を消します)")]
    private Volume NightVolume;

    [SerializeField, Tooltip("太陽の光になるDirectionLight(これの光を強くして明るくする)")]
    private Light SunLight;

    [SerializeField, Tooltip("完全に朝になった時の太陽の明るさ")]
    private float MorningLightIntensity = 1.0f;

    [SerializeField, Tooltip("朝になるまでにかける時間(秒)。この秒数をかけて徐々に明るくなっていく")]
    private float SunriseDuration = 3.0f;

    [Header("シーン遷移設定")]
    [SerializeField, Tooltip("ムービー終了後に遷移するクリアシーン名")]
    private string ClearSceneName = "GameclearScene";

    /// <summary>
    /// GameManagerから呼ばれる、クリアムービーの開始メソッド
    /// </summary>
    public void StartClearMovie()
    {
        Debug.Log("<color=yellow>ムービー開始メソッドが呼ばれました</color>");
        StartCoroutine(MovieRoutine());
    }

    private IEnumerator MovieRoutine()
    {
        // プレイヤーの操作を完全に停止させる
        if (TargetPlayerController != null)
        {
            TargetPlayerController._isStop = true; // WASDやマウス操作を無効
            Debug.Log("1. プレイヤーの操作を停止しました");
        }

        // 黒フェードに入る前に、空を明るくする処理を呼び出します
        Debug.Log("朝になる演出を開始します");
        yield return SunriseRoutine();
        Debug.Log("朝になりました");

        // 黒色のフェード処理
        if (FadeImage != null)
        {
            yield return FadeImageRoutine(Color.black, 0f, 1f, 0.5f);
            Debug.Log("2. 黒フェードアウト完了");
        }
        else
        {
            Debug.LogWarning(" FadeImageが未設定のためフェードをスキップしました");
        }

        // 画面があっくろの間に、特定の場所にワープ
        if (MoviewStartPosition != null)
        {
            // CharacterControllerがついていると直接Transformの変更が効かない場合があるため
            // 一時的に無効
            CharacterController characterController = TargetPlayerController.GetComponent<CharacterController>();
            if (characterController != null) characterController.enabled = false;
            
            // 位置をワープさせる
            PlayerTransform.position = MoviewStartPosition.position;

            // 画面が真っ暗な瞬間に、一足先に「次の目的地」と同じ向き（Y軸のみ）にセットしておく
            if (DoorStandPosition != null)
            {
                Vector3 targetEuler = DoorStandPosition.rotation.eulerAngles;
                PlayerTransform.rotation = Quaternion.Euler(0, targetEuler.y, 0);
            }
            else
            {
                Vector3 startEuler = MoviewStartPosition.rotation.eulerAngles;
                PlayerTransform.rotation = Quaternion.Euler(0, startEuler.y, 0);
            }

            // プレイヤーの子オブジェクトにある「カメラ」を見つけ出し、上下の角度を完全に正面(0,0,0)にリセットする
            Camera playerCamera = TargetPlayerController.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.identity; // 視点を真正面で固定
            }

            if (characterController != null) characterController.enabled = true;
        }
        else
        {
            Debug.LogWarning(" MovieStartPositionが未設定のためワープをスキップしました");
        }

        // 黒フェード明け
        if (FadeImage != null)
        {
            yield return FadeImageRoutine(Color.black, 1f, 0f, 0.5f);
            Debug.Log("4. 黒フェードイン（画面表示）完了");
        }

        // 扉に向かって歩く
        if (DoorStandPosition != null)
        {
            Debug.Log("5. 扉の前へ移動開始");
            yield return AutoWalkTo(DoorStandPosition, 5.0f);
            Debug.Log("5. 扉の前へ移動完了");
        }
        else
        {
            Debug.LogWarning("DoorStandPositionが未設定のため扉前への移動をスキップしました");
        }

        // 扉の前に来たら、登録されている全ての扉を開く
        Debug.Log($"6. 扉を開ける処理を開始します（対象: {TargetDoors.Count}個）");
        foreach (DoorConfig doorConfig in TargetDoors)
        {
            if (doorConfig != null)
            {
                // オブジェクト自体がオフになっていたらオンにする
                if (!doorConfig.DoorObject.activeSelf)
                {
                    doorConfig.DoorObject.SetActive(true);
                }

                // DoorSystemコンポーネントの取得を試みる
                DoorSystem door = doorConfig.DoorObject.GetComponent<DoorSystem>();

                // コンポーネントが削除されている場合は新たに追加する
                if (door == null)
                {
                    door = doorConfig.DoorObject.AddComponent<DoorSystem>();

                    // ここでインスペクターで設定した角度を反映する
                    door.OpenAngle = doorConfig.OpenAngle;

                    // ムービー中に勝手に閉まらないようにAutoCloseをオフにする
                    door.AutoClose = false;
                }
                else if (!door.enabled)
                {
                    // コンポーネントがあるけどチェックが外れているだけのパターン
                    door.enabled = true;
                }

                // 扉が開いていなければ開ける
                if (!door.IsOpen)
                {
                    door.ExecuteInteraction();
                }
            }
        }

        // 扉が開くのを少し待つ
        yield return new WaitForSeconds(1.0f);

        // 扉を開けながら外に出る
        if (OutsidePosition != null)
        {
            Debug.Log("7. 外へ移動開始");
            yield return AutoWalkTo(OutsidePosition, 10.0f);
            Debug.Log("7. 外へ移動完了");
        }
        else
        {
            Debug.LogWarning("OutsidePositionが未設定のため外への移動をスキップしました");
        }


        // 白色のフェード処理
        if (FadeImage != null)
        {
            yield return FadeImageRoutine(Color.white, 0f, 1f, 0.5f);
            Debug.Log("8. 白フェード完了");
        }

        // リザルトシーンへ遷移
        Debug.Log("<color=yellow>9. リザルトシーンへ遷移します</color>");
        SceneManager.LoadScene("GameclearScene");
    }

    /// <summary>
    /// 徐々に朝にする処理
    /// (夜のVolume効果を消しつつ、太陽の光を強くする)
    /// </summary>
    private IEnumerator SunriseRoutine()
    {
        // 設定漏れ防止：インスペクターでどちらもセットされていなければスキップする
        if (NightVolume == null && SunLight == null)
        {
            Debug.LogWarning("朝の演出用の設定が空のため、明るくする処理をスキップします");
            yield break;
        }

        float time = 0.0f;

        // 現在の夜の暗さと光の強さを記憶しておく
        float startVolumeWeight = (NightVolume != null) ? NightVolume.weight : 0.0f;
        float startLightIntensity = (SunLight != null) ? SunLight.intensity : 0.0f;

        // 指定した時間をかけて少しずつ変化させる
        while(time < SunriseDuration)
        {
            time += Time.deltaTime;

            // 今何パーセント終わったかを表す用
            float t = time / SunriseDuration;

            if (NightVolume != null)
            {
                // 最初の暗さから、0に向かって変化させます
                NightVolume.weight = Mathf.Lerp(startVolumeWeight, 0.0f, t);
            }

            if (SunLight != null)
            {
                // 光の強さを、最初の状態から朝の明るさへ変化させます
                SunLight.intensity = Mathf.Lerp(startLightIntensity, MorningLightIntensity, t);
            }

            // 1フレーム待ってからループを繰り返す
            yield return null;
        }

        // 時間が来たら、最後に年のためキッチリ目標の数値を入れる
        if (NightVolume != null) NightVolume.weight = 0f;
        if (SunLight != null) SunLight.intensity = MorningLightIntensity;
    }

    /// <summary>
    /// 目標地点の Transform を受け取り、その「向き」に視点を固定して歩く
    /// </summary>
    /// <param name="targetPosition">目標地点</param>
    /// <param name="timeout">何秒経ったら諦めて次の処理に進むか</param>
    private IEnumerator AutoWalkTo(Transform targetPoint, float timeout)
    {
        if (PlayerTransform == null || TargetPlayerController == null)
        {
            Debug.LogError("PlayerTransform または TargetPlayerController が未設定のため、歩行処理をスキップします。");
            yield break; // エラーで止まらずにすぐ次の処理（扉開け）へ行く
        }

        Vector3 playerPosNoY = new Vector3(PlayerTransform.position.x, 0, PlayerTransform.position.z);
        Vector3 targetPosNoY = new Vector3(targetPoint.position.x, 0, targetPoint.position.z);

        Vector3 targetEuler = targetPoint.rotation.eulerAngles;
        PlayerTransform.rotation = Quaternion.Euler(0, targetEuler.y, 0);

        // 歩行中も視点リセットをキープするためにカメラを取得
        Camera playerCamera = TargetPlayerController.GetComponentInChildren<Camera>();

        // ムービー中は壁や扉に引っかからないよう、物理演算の当たり判定をオフにする
        CharacterController characterController = TargetPlayerController.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        float timer = 0.0f;

        // 目的地との距離が0.3以上あるなら、近づき続ける
        while (Vector3.Distance(playerPosNoY, targetPosNoY) > 1.0f)
        {
            // 経過時間をカウント
            timer += Time.deltaTime;

            // タイムアウト時間を超えたら強制的にループを抜けて次へ進む
            if (timer > timeout)
            {
                Debug.LogWarning("目的地に辿り着けなかったため、移動を強制終了して次へ進みます。");
                break;
            }


            PlayerTransform.rotation = Quaternion.Euler(0, targetEuler.y, 0);
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.identity;
            }

            // 物理演算を無視して、直接「等速」で目的地へ移動させる（MoveTowards）
            // 高さがズレないように、目的地側のY座標をプレイヤーの今のY座標に合わせて計算します
            Vector3 currentPos = PlayerTransform.position;
            Vector3 destination = new Vector3(targetPoint.position.x, currentPos.y, targetPoint.position.z);

            PlayerTransform.position = Vector3.MoveTowards(currentPos, destination, WalkSpeed * Time.deltaTime);

            yield return null;

            playerPosNoY = new Vector3(PlayerTransform.position.x, 0, PlayerTransform.position.z);
        }
    }

    /// <summary>
    /// 汎用フェード処理(指定した色・透明度から透明度への変化)
    /// </summary>
    private IEnumerator FadeImageRoutine(Color baseColor, float startAlpha, float targetAlpha, float duration)
    {
        if (FadeImage == null) yield break;

        FadeImage.gameObject.SetActive(true);
        baseColor.a = startAlpha;
        FadeImage.color = baseColor;

        float time = 0.0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            baseColor.a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            FadeImage.color = baseColor;
            yield return null;
        }

        baseColor.a = targetAlpha;
        FadeImage.color = baseColor;
        
        // 完全に透明になったらUIを非アクティブにしておく
        if (targetAlpha <= 0.0f)
        {
            FadeImage.gameObject.SetActive(false);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
