using UnityEngine;

public class ListenSkill : MonoBehaviour
{
    // どこからでも「聞き耳中かどうか」を取得できるようにする魔法の変数
    public static bool IsListening { get; private set; }

    [Header("透視用の聞き耳カメラ")]
    public GameObject ListenCamera;

    [Header("ぼやけエフェクト(Depth of Field等を設定したVolume)")]
    // ※インスペクターの変数名がGrayscaleVolumeからBlurVolumeに変わるので、
    // Unity上で再度Volumeオブジェクトをアタッチし直してください。
    public GameObject BlurVolume;

    void Update()
    {
        //if (ListenCamera == null || BlurVolume == null) return;

        // Eキーを押した瞬間
        if (Input.GetKeyDown(KeyCode.E))
        {
            IsListening = true; // 聞き耳ONを知らせる
            // オブジェクトがアタッチされている時だけエフェクトをONにする
            if (ListenCamera != null) ListenCamera.SetActive(true);
            if (BlurVolume != null) BlurVolume.SetActive(true);
        }
        // Eキーを離した瞬間
        else if (Input.GetKeyUp(KeyCode.E))
        {
            IsListening = false; // 聞き耳OFFを知らせる
            // オブジェクトがアタッチされている時だけエフェクトをOFFにする
            if (ListenCamera != null) ListenCamera.SetActive(false);
            if (BlurVolume != null) BlurVolume.SetActive(false);
        }
    }
}