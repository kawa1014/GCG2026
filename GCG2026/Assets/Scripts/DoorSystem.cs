using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen = false; // ドアが開いているか
    public float openAngle = 90f; // 開く角度
    public float smooth = 2f;    // 動きの滑らかさ

    private Quaternion targetRotation;
    private Quaternion defaultRotation;

    void Start()
    {
        // 最初の角度を保存
        defaultRotation = transform.localRotation;
    }

    void Update()
    {
        // ターゲットとなる角度を決定
        if (isOpen)
        {
            targetRotation = defaultRotation * Quaternion.Euler(0, openAngle, 0);
        }
        else
        {
            targetRotation = defaultRotation;
        }

        // スムーズに回転させる
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smooth);

        // テスト用：スペースキーで開閉を切り替え
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleDoor();
        }
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }
}
