using UnityEngine;

public class ReticleHighlight : MonoBehaviour
{
    /// <summary>
    /// プレイヤーがオブジェクトに手が届く距離
    /// </summary>
    private float InteractRange = 3.0f;

    /// <summary>
    /// プレイヤーのカメラ(ここから視線の光線を飛ばします)
    /// </summary>
    private Camera PlayerCamera;

    private HighlightTarget currentTarget = null;

    void Start()
    {
        // プレイヤーインタラクタを取得
        PlayerInteractor playerInteractor = GetComponent<PlayerInteractor>();

        // プレイヤーインタラクタから必要なメンバ変数を取得する
        InteractRange = playerInteractor.InteractRange;
        PlayerCamera = playerInteractor.PlayerCamera;
    }

    void Update()
    {
        IsLookDoor();
    }


    private void IsLookDoor()
    {
        if (PlayerCamera == null) return;

        //Debug.DrawRay(ray.origin, ray.direction * InteractRange, Color.green, 1, false);

        // ビットで返したレイヤー番号を、~でビット反転する
        // これで、プレイヤー以外のレイヤーを指定できる
        int layerMask = ~LayerMask.GetMask("Player");

        // プレイヤー以外のオブジェクトを対象にレイを飛ばす
        Ray ray = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, InteractRange, layerMask))
        {
            IInteractable interactableObj = hit.collider.GetComponent<IInteractable>();

            // 看板(IInteractable)を持っていて、有効なら即座に実行
            if (interactableObj != null && interactableObj.IsInteractable)
            {
                HighlightTarget target = hit.collider.GetComponent<HighlightTarget>();

                if (!currentTarget && target)
                {
                    currentTarget = target;
                    currentTarget.EnableHighlight();
                }

                //Debug.Log("<color=yellow>【Ray】ドアを見ている</color>");

                return;
            }

            //Debug.Log("<color=blue>【Ray】ドアを見ていない1</color>");

        }

        //Debug.Log("<color=blue>【Ray】ドアを見ていない2</color>");


        if (currentTarget)
        {
            currentTarget.DisableHighlight();
            currentTarget = null;

            return;
        }
    }
}