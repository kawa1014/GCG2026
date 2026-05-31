using UnityEngine;

public class SpawnSensor : MonoBehaviour
{
    [Header("このセンサーが連動する出現ポイント（複数登録可能)")]
    public Transform[] spawnPoints;

    [Header("一度反応した後のクールタイム（秒）")]
    public float cooldownTime = 5f;
    private float nextEnableTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーがセンサーに入って、クールタイムが経過している場合
        if (other.CompareTag("Player") && Time.time >= nextEnableTime)
        {
            // クールタイムを設定
            nextEnableTime = Time.time + cooldownTime;

            //マネージャーに出現ポイントのリストを渡して、抽選
            EnemySpawnManager.Instance.TrySpawnEnemy(spawnPoints);
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
