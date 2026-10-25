using UnityEngine;

/// <summary>
/// シーン間でデータを共有・保持するための静的クラス
/// </summary>
public static class RetryManager
{
    /// <summary>
    /// プレイヤーが最後に死亡したシーン名
    /// </summary>
    public static string LastSceneName = "";
}
