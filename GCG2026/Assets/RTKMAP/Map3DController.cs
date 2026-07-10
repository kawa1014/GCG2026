using UnityEngine;

/// <summary>
/// Tabキーを押している間、3Dマップを表示します。
/// マップはカメラの前に開き、Tabキーを離すまで画面上の同じ位置に固定されます。
/// </summary>
public class Map3DController : MonoBehaviour
{
    [Header("Map Object")]
    [Tooltip("移動させるマップのTransformです。未設定の場合は、このオブジェクトのTransformを使用します。")]
    [SerializeField]
    private Transform mapObject;

    [Header("Camera Anchor")]
    [Tooltip("マップを開閉する位置の基準となるカメラです。未設定の場合はCamera.mainを使用します。")]
    [SerializeField]
    private Camera anchorCamera;

    [Header("Open Position")]
    [Tooltip("Tabキーを押したときにマップを表示する、カメラから見たローカル座標です。")]
    [SerializeField]
    private Vector3 openPosition = new Vector3(0.0f, -0.15f, 0.7f);

    [Header("Closed Position")]
    [Tooltip("Tabキーを離しているときにマップを収納する、カメラから見たローカル座標です。")]
    [SerializeField]
    private Vector3 closePosition = new Vector3(0.0f, -1.2f, 0.7f);

    [Header("Animation")]
    [Tooltip("マップが開閉するときの移動速度です。")]
    [SerializeField]
    private float animationSpeed = 8.0f;

    [Header("Sound")]
    [SerializeField]
    private AudioSource audioSource;

    [Tooltip("マップを開いたときに再生する効果音です。")]
    [SerializeField]
    private AudioClip openSound;

    [Tooltip("マップを閉じたときに再生する効果音です。")]
    [SerializeField]
    private AudioClip closeSound;

    private Transform closedParent;
    private Quaternion initialLocalRotation;
    private Renderer[] mapRenderers;
    private Collider[] mapColliders;
    private bool isOpen;
    private bool wasOpen;

    private void Awake()
    {
        if (mapObject == null)
        {
            mapObject = transform;
        }

        closedParent = mapObject.parent;
        initialLocalRotation = mapObject.localRotation;
        mapRenderers = mapObject.GetComponentsInChildren<Renderer>(true);
        mapColliders = mapObject.GetComponentsInChildren<Collider>(true);

        SnapClosed();
        SetMapVisible(false);
    }
    private void Update()
    {
        isOpen = Input.GetKey(KeyCode.Tab);

        if (isOpen && !wasOpen)
        {
            BeginOpen();
            SetMapVisible(true);
            PlaySound(openSound);
        }

        if (!isOpen && wasOpen)
        {
            BeginClose();
            PlaySound(closeSound);
        }

        wasOpen = isOpen;
    }

    private void LateUpdate()
    {
        float moveRate = 1.0f - Mathf.Exp(-animationSpeed * Time.deltaTime);

        if (isOpen)
        {
            Camera openCamera = GetAnchorCamera();
            if (openCamera == null)
            {
                MoveLocal(openPosition, initialLocalRotation, moveRate);
                return;
            }

            MoveWorld(
                openCamera.transform.TransformPoint(openPosition),
                openCamera.transform.rotation * initialLocalRotation,
                moveRate
            );
            return;
        }

        Camera cameraToUse = GetAnchorCamera();
        if (cameraToUse == null)
        {
            mapObject.localPosition = Vector3.Lerp(
                mapObject.localPosition,
                closePosition,
                moveRate
            );

            mapObject.localRotation = initialLocalRotation;

            if (Vector3.Distance(mapObject.localPosition, closePosition) < 0.01f)
            {
                SetMapVisible(false);
            }

            return;
        }

        Vector3 closeWorldPosition =
            cameraToUse.transform.TransformPoint(closePosition);

        Quaternion closeWorldRotation =
            cameraToUse.transform.rotation * initialLocalRotation;

        MoveWorld(closeWorldPosition, closeWorldRotation, moveRate);

        if (Vector3.Distance(mapObject.position, closeWorldPosition) < 0.01f)
        {
            AttachClosed(cameraToUse.transform);
            SetMapVisible(false);
        }
    }

    private void BeginOpen()
    {
        mapObject.SetParent(null, true);
    }

    private void BeginClose()
    {
        mapObject.SetParent(null, true);
    }

    private void SnapClosed()
    {
        Camera cameraToUse = GetAnchorCamera();

        if (cameraToUse != null)
        {
            AttachClosed(cameraToUse.transform);
            return;
        }

        mapObject.localPosition = closePosition;
        mapObject.localRotation = initialLocalRotation;
    }

    private void AttachClosed(Transform cameraTransform)
    {
        // 元の親に戻す。最初から親がなければルートに戻す
        if (mapObject.parent != closedParent)
        {
            mapObject.SetParent(closedParent, true);
        }

        Vector3 closeWorldPosition =
            cameraTransform.TransformPoint(closePosition);

        Quaternion closeWorldRotation =
            cameraTransform.rotation * initialLocalRotation;

        mapObject.position = closeWorldPosition;
        mapObject.rotation = closeWorldRotation;
    }
    private void MoveWorld(
        Vector3 targetPosition,
        Quaternion targetRotation,
        float moveRate
    )
    {
        mapObject.position = Vector3.Lerp(
            mapObject.position,
            targetPosition,
            moveRate
        );

        mapObject.rotation = Quaternion.Slerp(
            mapObject.rotation,
            targetRotation,
            moveRate
        );
    }

    private void MoveLocal(
        Vector3 targetPosition,
        Quaternion targetRotation,
        float moveRate
    )
    {
        mapObject.localPosition = Vector3.Lerp(
            mapObject.localPosition,
            targetPosition,
            moveRate
        );

        mapObject.localRotation = Quaternion.Slerp(
            mapObject.localRotation,
            targetRotation,
            moveRate
        );
    }

    private void SetMapVisible(bool visible)
    {
        foreach (Renderer mapRenderer in mapRenderers)
        {
            if (mapRenderer != null)
            {
                mapRenderer.enabled = visible;
            }
        }

        foreach (Collider mapCollider in mapColliders)
        {
            if (mapCollider != null)
            {
                mapCollider.enabled = visible;
            }
        }
    }

    private Camera GetAnchorCamera()
    {
        if (anchorCamera != null)
        {
            return anchorCamera;
        }

        return Camera.main;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}
