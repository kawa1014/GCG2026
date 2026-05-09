using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool isOpen = false;
    public float openAngle = 90f;
    public float smooth = 2f;

    private Quaternion targetRotation;
    private Quaternion defaultRotation;

    // プレイヤーが近くにいるかどうかを判定するフラグ
    private bool isPlayerNear = false;

    void Start()
    {
        defaultRotation = transform.localRotation;
    }

    void Update()
    {
        if (isOpen)
        {
            targetRotation = defaultRotation * Quaternion.Euler(0, openAngle, 0);
        }
        else
        {
            targetRotation = defaultRotation;
        }

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smooth);

        // プレイヤーが近くにいて、かつスペースキーが押された時に開閉する
        if (isPlayerNear && Input.GetKeyDown(KeyCode.Space))
        {
            isOpen = !isOpen;
        }
    }

    // プレイヤーが判定エリア（トリガー）に入った時
    private void OnTriggerEnter(Collider other)
    {
        // ぶつかった相手のタグが"Player"ならフラグをオンにする
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    // プレイヤーが判定エリア（トリガー）から出た時
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}