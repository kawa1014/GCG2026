using UnityEngine;

/// <summary>
/// プレイヤーの恐怖度(内部数値)を管理し、限界に達すると死亡するクラス
/// オルゴールの状態を監視して数値を増減させます
/// </summary>
public class FearManager : MonoBehaviour
{
    /// <summary>
    /// 恐怖の最大値(この数値に達すると死亡します)
    /// </summary>
    [Tooltip("恐怖度の最大値")]
    public float maxFear = 100.0f;

    /// <summary>
    /// 1秒間に増加する恐怖度の量
    /// </summary>
    [Tooltip("1秒間に増加する恐怖度")]
    public float fearIncreaseRate = 10.0f;

    // 現在の恐怖度を保存する変数(目に見えない内部数値)
    private float currentFear = 0.0f;

    // シーン内にあるオルゴールを記憶しておくための変数
    private OrgelSystem orgelSystem;

    private void Start()
    {
        // ゲーム開始時に、シーン内にあるオルゴールを自動で探し出して記憶します
        // これにより、プレハブのインスペクタ―で毎回設定する手間が省けます
        orgelSystem = FindAnyObjectByType<OrgelSystem>();
    }

    private void Update()
    {
        if (orgelSystem == null) return;

        // オルゴールが鳴っている間だけ数値を増やす
        if(orgelSystem.isPlaying)
        {
            // Timer.deltatimeを掛けることで、1秒間にFearIncreaseRateの分だけ正確に増えます
            currentFear += fearIncreaseRate * Time.deltaTime;

            // 目に見えない内部地ですが、プロトタイプ確認用にコンソールに表示します
            Debug.Log($"<color=purple> 【Fear】恐怖が迫っている…恐怖度:{currentFear:F1} / {maxFear}</color>");

            // 恐怖度がMAXに達したかチェック
            if(currentFear >= maxFear)
            {
                Die(); // 死亡処理を呼ぶ
            }
        }
        else 
        {
            // オルゴールが止まっている間は、徐々に恐怖度が回復するようにしています
            if(currentFear > 0)
            {
                currentFear -= (fearIncreaseRate * 0.5f) * Time.deltaTime; // 回復は少し遅めに設定
                currentFear = Mathf.Max(0, currentFear); // 0未満には鳴らないように制限
            }
        }
    }

    /// <summary>
    /// プレイヤーの死亡
    /// </summary>
    private void Die()
    {
        Debug.Log("<color=red> 【Game Over】恐怖が限界に達し、プレイヤーは消滅した…</color>");

        // プレイヤー地震を完全に削除する
        Destroy(gameObject);
    }
}