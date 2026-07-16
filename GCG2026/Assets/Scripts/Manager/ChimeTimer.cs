using System.Collections;
using UnityEngine;

public class ChimeTimer : MonoBehaviour
{
    [Header("オーディオ設定")]
    [SerializeField] private AudioSource audioSource; // 音を鳴らすコンポーネント
    [SerializeField] private AudioClip chimeSound;   // 鐘の音のファイル

    [Header("タイマー設定（秒）")]
    [SerializeField] private float interval = 60f;    // 間隔（1分 = 60秒）

    private void Start()
    {
        // コンポーネントの付け忘れチェック
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // 1分ごとに鐘を鳴らすループ処理（コルーチン）を開始
        StartCoroutine(ChimeLoop());
    }

    private IEnumerator ChimeLoop()
    {
        // ゲームが動いている間、ずーーーっと繰り返す
        while (true)
        {
            // 指定した秒数（60秒）待つ
            yield return new WaitForSeconds(interval);

            // 鐘の音を鳴らす
            if (audioSource != null && chimeSound != null)
            {
                audioSource.PlayOneShot(chimeSound);
            }
        }
    }
}