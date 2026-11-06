using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// エネミーに捕まった際の専用演出（アニメーション、視点固定、赤フェード、シーン遷移）を管理するクラス
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

    [SerializeField, Tooltip("赤フェードにかかる時間（秒）")]
    private float FadeDuration = 1.0f;

    [SerializeField, Tooltip("捕獲アニメーション再生後、フェード開始までの待機時間")]
    private float CatchAnimWaitTime = 1.5f;

    /// <summary>
    /// エネミーから呼び出される捕獲演出の開始メソッド
    /// </summary>
    public IEnumerator PlayCatchRoutine(Transform enemyTransform, Animator enemyAnimator, Transform playerTransform, PlayerController playerController)
    {
        // 1. プレイヤーの操作を完全に停止
        if (playerController != null)
        {
            playerController._isStop = true;
        }

        // 2. プレイヤーをローザ（エネミー）の方向に向かせる
        if (playerTransform != null && enemyTransform != null)
        {
            Vector3 direction = (enemyTransform.position - playerTransform.position).normalized;
            direction.y = 0; // Y軸は固定
            if (direction != Vector3.zero)
            {
                playerTransform.rotation = Quaternion.LookRotation(direction);
            }
        }

        // 3. ローザ側の `Roza_catch` アニメーションを再生
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Catch");
            Debug.Log("【デバッグ】Animatorの 'Catch' トリガーを送信しました！");
        }
        else
        {
            Debug.LogError("【エラー】エネミーの Animator がアタッチされていません！");
        }

        Debug.Log("【演出】ローザの捕獲アニメーション再生開始");

        // 4. アニメーションの再生が進むのを少し待つ
        yield return new WaitForSeconds(CatchAnimWaitTime);

        // 5. 赤色のフェードイン（画面が真っ赤になっていく）
        yield return RedFadeRoutine();

        // 6. ゲームオーバーシーンへ遷移
        SceneManager.LoadScene(GameOverSceneName);
    }

    /// <summary>
    /// 赤色のフェードを徐々に不透明にするコルーチン（インスペクターの色を維持する仕様）
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

            // 【修正】インスペクターで設定された色（RGB）をそのまま取得し、最初は透明（Alpha=0）にする
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
            Debug.LogWarning("【警告】RedFadeImageが見つからないため、フェードをスキップします。");
            yield return new WaitForSeconds(FadeDuration);
        }
    }
}