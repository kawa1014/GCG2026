using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class HighlightTarget : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
    }


    // ハイライトを有効にする処理
    // 外から呼び出す
    public void EnableHighlight()
    {
        // 縁をつける
        SetLayer(gameObject, LayerMask.NameToLayer("Outline"));
        //if (gameObject.layer != LayerMask.NameToLayer("Outline"))
        //{
        //    // 縁をつける
        //    SetLayer(gameObject, LayerMask.NameToLayer("Outline"));
        //}
    }

    public void DisableHighlight()
    {
        // 縁を消す
        SetLayer(gameObject, LayerMask.NameToLayer("Default"));
        //if (gameObject.layer != LayerMask.NameToLayer("Default"))
        //{
        //    // 縁を消す
        //    SetLayer(gameObject, LayerMask.NameToLayer("Default"));
        //}
    }

    private void SetLayer(GameObject obj, int layer)
    {
        if (obj == null)
            return;

        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayer(child.gameObject, layer);
        }
    }
}