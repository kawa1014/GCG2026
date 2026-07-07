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

        Ray ray = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, InteractRange))
        {
            IInteractable interactableObj = hit.collider.GetComponent<IInteractable>();

            // 看板(IInteractable)を持っていて、有効なら即座に実行（ドアが開く）
            if (interactableObj != null && interactableObj.IsInteractable)
            {
                //hit.collider.GetComponent<HighlightTarget).EnableHighlight();
                Debug.Log("<color=yellow>【Ray】ドアを見ている</color>");
            }
        }
    }
}
