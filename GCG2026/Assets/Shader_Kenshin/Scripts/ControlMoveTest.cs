using System.IO;
using UnityEngine;

public class ControlMoveTest : MonoBehaviour
{
    [SerializeField]
    float _moveSpeed = 1.0f;
    [SerializeField]
    GameObject _camera;

    Rigidbody rb;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        // カメラを基準に移動（sincostan）
        // キー入力受け付け
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            direction.z += 1.0f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction.z -= 1.0f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction.x -= 1.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction.x += 1.0f;
        }

        // キー入力していたら移動処理
        if (direction.x != 0.0f || direction.z != 0.0f)
        {
            // カメラの角度
            Quaternion quaternion = _camera.transform.rotation;
            Vector3 rotation = quaternion.eulerAngles;

            // 移動する角度を計算
            float angle = rotation.y;
            float adding_angle = Mathf.Atan2(direction.x, direction.z);
            float radian = angle * (Mathf.PI / 180.0f) + adding_angle;

            // 移動
            Vector3 directionNormalized = new Vector3(Mathf.Sin(radian), 0.0f, Mathf.Cos(radian));
            //rb.AddForce(directionNormalized);
            transform.position += directionNormalized;
        }
        //else
        //{
        //    // 入力していなければ減速させていく
        //    rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y, rb.linearVelocity.z * 0.9f);
        //}
    }
}
