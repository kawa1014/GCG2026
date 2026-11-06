using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// エネミーに捕まった際の専用演出を管理するクラス
/// アニメーションや視点固定や赤フェードやシーン遷移などを行います
/// </summary>
public class EnemyCatchController : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField, Tooltip("シーン上に配置した赤色フェード用のImageのオブジェクト名")]
    private string fadeImageObjectName = "RedFadeImage";

    private Image redFadeImage;

    [Header("遷移設定")]
    [SerializeField, Tooltip("ゲームオーバー後に遷移するシーン名")]
    private string GameOverSceneName = "GameOverScene";

    [SerializeField, Tooltip("赤フェードにかかる秒数")]
    private float FadeDuration = 1.0f;

    [SerializeField, Tooltip("捕獲アニメーション再生後フェード開始までの待機秒数")]
    private float CatchAnimWaitTime = 1.5f;

    [Header("ローザ捕獲時の視点設定")]

    // 変更点1 頭のボーンではなく本体を基準にするため名前を変更
    [Tooltip("ローザの本体のTransformを指定します")]
    [SerializeField]
    private Transform RosaTransform;

    private Camera PlayerCamera;

    [Tooltip("視点をローザに向けるスピードの秒数です一瞬で向かせる場合は0に少しヌルっと向かせる場合は0.1などにします")]
    [SerializeField]
    private float LookAtSmoothTime = 0.1f;

    // SmoothDamp用の速度保持変数
    private float CatchVelocityX;
    private float CatchVelocityY;

    // 変更点2 捕まっている最中かどうかを判定するフラグ
    private bool isCaught = false;

    // 変更点2 Update関数を追加し捕まったら毎フレーム視点を補間し続けるようにする
    private void Update()
    {
        if (isCaught)
        {
            LookAtRosaFlat();
        }
    }

    /// <summary>
    /// エネミーから呼び出される捕獲演出の開始メソッド
    /// </summary>
    public IEnumerator PlayCatchRoutine(Transform enemyTransform, Animator enemyAnimator, Transform playerTransform, PlayerController playerController)
    {
        PlayerCamera = Camera.main;

        // プレイヤーの操作を完全に停止
        if (playerController != null)
        {
            playerController._isStop = true;
        }

        // プレイヤーをローザの方向に向かせる
        if (playerTransform != null && enemyTransform != null)
        {
            Vector3 direction = (enemyTransform.position - playerTransform.position).normalized;
            direction.y = 0; // Y軸は固定
            if (direction != Vector3.zero)
            {
                playerTransform.rotation = Quaternion.LookRotation(direction);
            }
        }

        // 変更点2 フラグをオンにしてUpdate内でカメラをローザに向け始める
        isCaught = true;

        // ローザ側の捕獲アニメーションを再生
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Catch");
            Debug.Log("AnimatorのCatchトリガーを送信しました");
        }
        else
        {
            Debug.LogError("エネミーのAnimatorがアタッチされていません");
        }

        Debug.Log("ローザの捕獲アニメーション再生開始");

        // アニメーションの再生が進むのを少し待つ
        yield return new WaitForSeconds(CatchAnimWaitTime);

        // 赤色のフェードイン画面が真っ赤になっていく処理
        yield return RedFadeRoutine();

        // ゲームオーバーシーンへ遷移
        SceneManager.LoadScene(GameOverSceneName);
    }

    /// <summary>
    /// 赤色のフェードを徐々に不透明にするコルーチンインスペクターの色を維持する仕様
    /// </summary>
    private IEnumerator RedFadeRoutine()
    {
        if (redFadeImage == null)
        {
            GameObject foundObj = GameObject.Find(fadeImageObjectName);
            if (foundObj != null)
            {
                redFadeImage = foundObj.GetComponent<Image>();
            }

            if (redFadeImage == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    GameObject imgObj = new GameObject("AutoRedFadeImage");
                    imgObj.transform.SetParent(canvas.transform, false);

                    RectTransform rect = imgObj.AddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.sizeDelta = Vector2.zero;
                    rect.anchoredPosition = Vector2.zero;

                    redFadeImage = imgObj.AddComponent<Image>();
                    redFadeImage.color = Color.red; // 自動生成の場合のみデフォルトで赤にする
                }
            }
        }

        if (redFadeImage != null)
        {
            redFadeImage.gameObject.SetActive(true);

            // インスペクターで設定された色をそのまま取得し最初は透明にする
            Color baseColor = redFadeImage.color;
            baseColor.a = 0f;
            redFadeImage.color = baseColor;

            float time = 0f;
            while (time < FadeDuration)
            {
                time += Time.deltaTime;
                baseColor.a = Mathf.Clamp01(time / FadeDuration);
                redFadeImage.color = baseColor;
                yield return null;
            }

            baseColor.a = 1f;
            redFadeImage.color = baseColor;
        }
        else
        {
            Debug.LogWarning("RedFadeImageが見つからないためフェードをスキップします");
            yield return new WaitForSeconds(FadeDuration);
        }
    }

    // 変更点3 LookAtRosaFaceを削除し水平を向くLookAtRosaFlatに差し替え
    /// <summary>
    /// 視点をローザの方向に向けつつ上下のX軸は平面に戻す処理
    /// </summary>
    private void LookAtRosaFlat()
    {
        if (RosaTransform == null || PlayerCamera == null) return;

        // ローザへの方向ベクトルを計算
        Vector3 DirectionToRosa = RosaTransform.position - PlayerCamera.transform.position;

        // Y軸の高さを0にして強制的に完全な平面のベクトルにします
        DirectionToRosa.y = 0f;

        // その方向を向く角度を計算平面ベクトルなので自動的に上下の角度は0を向く計算になります
        Quaternion TargetRotation = Quaternion.LookRotation(DirectionToRosa);
        Vector3 TargetEulerAngles = TargetRotation.eulerAngles;

        // 現在の角度から目標の角度へ滑らかに回転させます
        // 上下のX軸は強制的に0度に向かって補間します
        float CurrentEulerX = Mathf.SmoothDampAngle(PlayerCamera.transform.eulerAngles.x, 0f, ref CatchVelocityX, LookAtSmoothTime);
        float CurrentEulerY = Mathf.SmoothDampAngle(PlayerCamera.transform.eulerAngles.y, TargetEulerAngles.y, ref CatchVelocityY, LookAtSmoothTime);

        // 実際のカメラに適用Z軸も傾かないように0で固定
        PlayerCamera.transform.rotation = Quaternion.Euler(CurrentEulerX, CurrentEulerY, 0f);
    }
}