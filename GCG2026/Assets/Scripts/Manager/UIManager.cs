using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// UIや画面演出を専門に管理するクラス
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("ゲームオーバー演出設定")]
    [Tooltip("アニメーションんを表示する全画面のUI Image")]
    public Image GameOverImage;

    [Tooltip("再生する連番スプライトの配列")]
    public Sprite[] GameOverAnimationSprites;

    [Tooltip("1秒間に何故の画像を切り替えるか")]
    public float AnimationFPS = 30.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 普段は非表示にしておく
        if (GameOverImage != null)
        {
            GameOverImage.enabled = false;
        }

        // GameManagerのイベントを購読
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverEvent += PlayGameOverEffect;
        }
        
    }

    private void OnDestroy()
    {
        // オブジェクト破棄時にイベント登録を解除
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverEvent -= PlayGameOverEffect;
        }
    }

    /// <summary>
    /// ゲームオーバーのイベントを受け取って実行されるメソッド
    /// </summary>
    private void PlayGameOverEffect()
    {
        Debug.Log("<color=yellow>UIManager: イベント受信！アニメーション開始します</color>");

        StartCoroutine(PlaySequentialAnimationCoroutine());
    }

    /// <summary>
    /// 連番画像を順番に切り替えるコルーチン
    /// </summary>
    private System.Collections.IEnumerator PlaySequentialAnimationCoroutine()
    {
        if (GameOverImage == null || GameOverAnimationSprites == null || GameOverAnimationSprites.Length == 0)
        yield break;

        // Imageを有効化して表示開始
        GameOverImage.enabled = true;

        // 1フレームあたりの待機時間を計算(FPSが30なら約0.33秒)
        float frameDelay = 1.0f / AnimationFPS;

        // 配列の0番目から最後の画像まで順番に差し替える
        for (int i = 0; i < GameOverAnimationSprites.Length; i++)
        {
            GameOverImage.sprite = GameOverAnimationSprites[i];

            // 指定した時間だけ待機して次の画像へ
            yield return new WaitForSeconds(frameDelay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
