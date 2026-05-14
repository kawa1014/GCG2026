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
    public float MoveSpeed = 5.0f;

    /// <summary>
    /// マウス感度
    /// </summary>
    public float MouseSensitivity = 200.0f;
}