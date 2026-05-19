using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform[] patronlPoints; //巡回するポイントの配列
    public float chaseRange = 5f; //プレイヤーを追いかける範囲

    private NavMeshAgent agent; //NavMeshAgentコンポーネント
    private int currentPointIndex = 0; //現在の巡回ポイントのインデックス
    private Transform player; //プレイヤーのTransform
    private bool isChasing = false; //プレイヤーを追いかけているかどうか

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        //player = GameObject.FindGameObjectWithTag("Player").transform; //プレイヤーのTransformを取得
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Playerオブジェクトが見つかりません。タグが正しく設定されているか確認してください。");
        }

        GoToNextPatrolPoint(); //最初の巡回ポイントに移動
    }
    void Update()
    {
        if (player == null) return; //プレイヤーが見つからない場合は処理をスキップ

        float distanceToPlayer = Vector3.Distance(transform.position, player.position); //プレイヤーとの距離を計算

        if (isChasing)
        {
            agent.SetDestination(player.position); //プレイヤーを追いかける

            if (distanceToPlayer > chaseRange)
            {
                isChasing = false; //追いかけるのをやめる
                GoToNextPatrolPoint(); //次の巡回ポイントに移動
            }
        }
        else
        {
            if (distanceToPlayer <= chaseRange)
            {
                isChasing = true; //プレイヤーを追いかける
            }
            else if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                GoToNextPatrolPoint(); //次の巡回ポイントに移動
            }
        }
    }
    void GoToNextPatrolPoint()
    {
        if (patronlPoints.Length == 0) return; //巡回ポイントがない場合は処理をスキップ

        agent.SetDestination(patronlPoints[currentPointIndex].position); //次の巡回ポイントに移動

        currentPointIndex = (currentPointIndex + 1) % patronlPoints.Length; //次のポイントのインデックスを更新
    }

    private void OnDrawGizmosSelected()
    {
        //追いかける範囲をシーンビューに表示(debug用)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}

