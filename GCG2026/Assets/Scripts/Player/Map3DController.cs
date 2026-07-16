using UnityEngine;

/// <summary>
/// Tabキーを押している間、3Dマップをカメラ前方へ表示します。
/// カメラが上下左右を向いても、マップが常に画面内へ追従します。
/// Tabキーを離すと、マップをカメラ付近へ収納して非表示にします。
/// </summary>
public class Map3DController : MonoBehaviour
{
    [Header("Camera Follow Target")]

    [Tooltip("追従するカメラです。未設定の場合はMain Cameraを自動取得します。")]
    [SerializeField]
    private Camera targetCamera;

    [Header("Map Position")]

    [Tooltip("収納時のカメラから見た相対位置です。")]
    [SerializeField]
    private Vector3 closedOffset =
        new Vector3(0.0f, -0.35f, 0.05f);

    [Tooltip("表示時のカメラから見た相対位置です。Zを小さくすると近くなります。")]
    [SerializeField]
    private Vector3 openOffset =
        new Vector3(0.0f, -0.15f, 0.8f);

    [Header("Map Rotation")]

    [Tooltip("マップモデルの向きを調整します。向きが逆ならYを180にします。")]
    [SerializeField]
    private Vector3 rotationOffset =
        new Vector3(0.0f, 0.0f, 0.0f);

    [Header("Move Speed")]

    [Tooltip("マップが表示位置へ移動する速度です。")]
    [Min(0.01f)]
    [SerializeField]
    private float openSpeed = 2.5f;

    [Tooltip("マップが収納位置へ戻る速度です。")]
    [Min(0.01f)]
    [SerializeField]
    private float closeSpeed = 3.0f;

    [Header("Hidden Setting")]

    [Tooltip("収納完了後にRendererとColliderを無効にします。")]
    [SerializeField]
    private bool disableWhenClosed = true;

    [Tooltip("収納完了と判定する距離です。基本的に変更不要です。")]
    [Min(0.0001f)]
    [SerializeField]
    private float closeThreshold = 0.001f;

    /// <summary>
    /// 現在のカメラから見た相対位置です。
    /// </summary>
    private Vector3 currentOffset;

    /// <summary>
    /// マップ内にある全Rendererです。
    /// </summary>
    private Renderer[] mapRenderers;

    /// <summary>
    /// マップ内にある全Colliderです。
    /// </summary>
    private Collider[] mapColliders;

    /// <summary>
    /// 現在マップを開いているかどうかです。
    /// </summary>
    private bool isOpen;

    /// <summary>
    /// 初期設定を行います。
    /// </summary>
    private void Awake()
    {
        // Inspectorでカメラが未設定なら、Main Cameraを自動取得します。
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError(
                "Map3DController: Target Cameraが設定されておらず、" +
                "Main Cameraも見つかりません。"
            );

            enabled = false;
            return;
        }

        // プレイヤーやカメラのScaleを引き継がないよう、独立させます。
        transform.SetParent(null, true);

        mapRenderers = GetComponentsInChildren<Renderer>(true);
        mapColliders = GetComponentsInChildren<Collider>(true);

        currentOffset = closedOffset;
        isOpen = false;

        UpdateMapTransform();

        if (disableWhenClosed)
        {
            SetMapVisible(false);
        }
    }

    /// <summary>
    /// Tab入力と、開閉アニメーションを更新します。
    /// </summary>
    private void Update()
    {
        isOpen = Input.GetKey(KeyCode.Tab);

        Vector3 targetOffset = isOpen
            ? openOffset
            : closedOffset;

        float moveSpeed = isOpen
            ? openSpeed
            : closeSpeed;

        // 開き始める瞬間に表示を有効にします。
        if (isOpen)
        {
            SetMapVisible(true);
        }

        currentOffset = Vector3.MoveTowards(
            currentOffset,
            targetOffset,
            moveSpeed * Time.deltaTime
        );

        // 完全に収納されたら非表示にします。
        if (!isOpen &&
            Vector3.Distance(currentOffset, closedOffset) <= closeThreshold)
        {
            currentOffset = closedOffset;

            if (disableWhenClosed)
            {
                SetMapVisible(false);
            }
        }
    }

    /// <summary>
    /// カメラ移動後にマップの位置と向きを更新します。
    /// LateUpdateを使うことで、視点移動による遅れを減らします。
    /// </summary>
    private void LateUpdate()
    {
        UpdateMapTransform();
    }

    /// <summary>
    /// カメラの位置と回転を基準に、マップを配置します。
    /// カメラがどこを向いても、マップは同じ画面位置へ追従します。
    /// </summary>
    private void UpdateMapTransform()
    {
        if (targetCamera == null)
        {
            return;
        }

        Transform cameraTransform = targetCamera.transform;

        // カメラのScaleは使わず、位置と回転だけで相対位置を計算します。
        transform.position =
            cameraTransform.position +
            cameraTransform.rotation * currentOffset;

        // カメラと同じ方向を向かせ、モデル固有の角度を追加します。
        transform.rotation =
            cameraTransform.rotation *
            Quaternion.Euler(rotationOffset);
    }

    /// <summary>
    /// マップ内のRendererとColliderをまとめて切り替えます。
    /// </summary>
    /// <param name="visible">表示する場合はtrueです。</param>
    private void SetMapVisible(bool visible)
    {
        if (mapRenderers != null)
        {
            foreach (Renderer mapRenderer in mapRenderers)
            {
                if (mapRenderer != null)
                {
                    mapRenderer.enabled = visible;
                }
            }
        }

        if (mapColliders != null)
        {
            foreach (Collider mapCollider in mapColliders)
            {
                if (mapCollider != null)
                {
                    mapCollider.enabled = visible;
                }
            }
        }
    }
}