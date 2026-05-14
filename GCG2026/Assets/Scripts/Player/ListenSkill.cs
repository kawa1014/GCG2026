using UnityEngine;

public class ListenSkill : MonoBehaviour
{
    [Header("透視用の聞き耳カメラ")]
    public GameObject ListenCamera; // インスペクターから ListenCamera をセットする

    [Header("白黒にするエフェクト")]
    public GameObject GrayscaleVolume; // インスペクターから GrayscaleVolume をセットする

    void Update()
    {
        // カメラかエフェクトがセットされていなければ何もしない（エラー防止）
        if (ListenCamera == null || GrayscaleVolume == null) return;

        // Eキー
        if (Input.GetKeyDown(KeyCode.E))
        {
            ListenCamera.SetActive(true);
            //GrayscaleVolume.SetActive(true); // 白黒エフェクトON
        }
        // Eキーを離したら
        else if (Input.GetKeyUp(KeyCode.E))
        {
            ListenCamera.SetActive(false);
            //GrayscaleVolume.SetActive(false); // 白黒エフェクトOFF
        }
    }
}