using UnityEngine;

/// <summary>
/// プレイヤー設定データ
/// </summary>
[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/Player Settings")]
public class PlayerSettings : ScriptableObject
{
    /// <summary>
    /// プレイヤーの移動速度
    /// </summary>
    public float moveSpeed = 5.0f;

    /// <summary>
    /// マウス感度
    /// </summary>
    public float mouseSensitivity = 200.0f;
}