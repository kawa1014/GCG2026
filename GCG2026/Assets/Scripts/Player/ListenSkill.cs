using UnityEngine;

public class ListenSkill : MonoBehaviour
{
    [Header("透視用の聞き耳カメラ")]
    public GameObject listenCamera; // インスペクターから ListenCamera をセットする

    [Header("白黒にするエフェクト")]
    public GameObject grayscaleVolume; // インスペクターから GrayscaleVolume をセットする

    void Update()
    {
        // カメラかエフェクトがセットされていなければ何もしない（エラー防止）
        if (listenCamera == null || grayscaleVolume == null) return;

        // Eキー
        if (Input.GetKeyDown(KeyCode.E))
        {
            listenCamera.SetActive(true);
            //grayscaleVolume.SetActive(true); // 白黒エフェクトON
        }
        // Eキーを離したら
        else if (Input.GetKeyUp(KeyCode.E))
        {
            listenCamera.SetActive(false);
            //grayscaleVolume.SetActive(false); // 白黒エフェクトOFF
        }
    }
}