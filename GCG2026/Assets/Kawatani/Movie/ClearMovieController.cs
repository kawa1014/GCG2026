using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    /// <summary>
    /// GameManagerから呼ばれる、クリアムービーの開始メソッド
    /// </summary>
    public void StartClearMovie()
    {
        StartCoroutine(MovieRoutine());
    }

    private IEnumerator MovieRoutine()
    {
        // プレイヤーの操作を完全に停止させる
        if (TargetPlayerController != null)
        {
            TargetPlayerController._isStop = true; // WASDやマウス操作を無効
        }

        // 黒色フェード処理
        yield return FadeImageRoutine(Color.black, 0.0f, 1.0f, 0.5f);

        // 画面があっくろの間に、特定の場所にワープ
        if (MoviewStartPosition != null)
        {
            // CharacterControllerがついていると直接Transformの変更が効かない場合があるため
            // 一時的に無効
            CharacterController characterController = TargetPlayerController.GetComponent<CharacterController>();
            if (characterController != null) characterController.enabled = false;

            PlayerTransform.position = MoviewStartPosition.position;
            PlayerTransform.rotation = MoviewStartPosition.rotation;

            if (characterController != null) characterController.enabled = true;
        }

        // 黒色フェードを明けて画面を表示(
        yield return FadeImageRoutine(Color.black, 1.0f, 0.0f, 0.5f);

        // 扉に向かって歩く
        if (DoorStandPosition != null)
        {
            yield return AutoWalkTo(DoorStandPosition.position);
        }

        // 扉の前に来たら、登録されている全ての扉を開く
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
            yield return AutoWalkTo(OutsidePosition.position);
        }
        

        // 白色のフェード処理
        yield return FadeImageRoutine(Color.white, 0.0f, 1.0f, 0.5f);

        // リザルトシーンへ遷移
        SceneManager.LoadScene("ResultScene");
    }

    /// <summary>
    /// 目的地点まで自動で歩く処理
    /// </summary>
    private IEnumerator AutoWalkTo(Vector3 targetPosition)
    {
        // 目的地との距離が0.1以上あるなら、近づき続ける
        while (Vector3.Distance(PlayerTransform.position, targetPosition) > 0.1f)
        {
            // CharacterControllerを使って移動
            CharacterController characterController = TargetPlayerController.GetComponent<CharacterController>();
            if (characterController != null)
            {
                // ターゲットへの方向ベクトル計算
                Vector3 direction = (targetPosition - PlayerTransform.position).normalized;

                // Move()は1フレームの移動量(速度×Time.deltaTime)を渡す
                characterController.Move(direction * WalkSpeed * Time.deltaTime);

                // 歩く方向にプレイヤーを少しずつ振り向かせる
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                PlayerTransform.rotation = Quaternion.Slerp(PlayerTransform.rotation, targetRotation, Time.deltaTime * 5.0f);
            }
            else
            {
                // characterControllerがない場合の保険
                PlayerTransform.position = Vector3.MoveTowards(PlayerTransform.position, targetPosition, WalkSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    /// <summary>
    /// 汎用フェード処理(指定した色・透明度から透明度への変化)
    /// </summary>
    private IEnumerator FadeImageRoutine(Color baseColor, float startAlpha, float targetAlpha, float duration)
    {
        if (FadeImage != null) yield break;

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
