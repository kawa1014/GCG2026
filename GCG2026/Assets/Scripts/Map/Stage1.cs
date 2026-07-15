using UnityEngine;

public class TwoStoryMapGenerator : MonoBehaviour
{
    [Header("CSV File")]
    public TextAsset mapCsv;

    [Header("‹¤’ÊƒvƒŒƒnƒu“o˜^ (1ŠKE2ŠKE3ŠK‚Å“¯‚¶‚à‚Ì‚ðŽg‚¢‚Ü‚·)")]
    public GameObject[] floorPrefabs;   // ° (10‰­‚ÌˆÊ)
    public GameObject[] wallPrefabs;    // •Ç (1000–œ‚ÌˆÊ)
    public GameObject[] doorPrefabs;    // ƒhƒA (10–œ‚ÌˆÊ)
    public GameObject[] windowPrefabs;  // ‘‹ (1000‚ÌˆÊ)
    public GameObject[] furniturePrefabs;// ‰Æ‹ï (1‚ÌˆÊEÅ‘å999)

    [Header("Settings")]
    public float blockSize = 2.0f;   // ƒ}ƒX‚ÌL‚³
    public float floorHeight = 3.0f; // š 1‚Â‚ÌŠK‚Ì‚‚³iYŽ²j

    void Start() => GenerateMap();

    void GenerateMap()
    {
        if (mapCsv == null) return;

        string[] rows = mapCsv.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int z = 0; z < rows.Length; z++)
        {
            string[] columns = rows[z].Split(',');
            int adjustedZ = -z; // š1ƒ}ƒXƒYƒŒ–hŽ~‘ÎôÏ‚Ý

            for (int x = 0; x < columns.Length; x++)
            {
                string cellData = columns[x].Replace("\"", "").Trim(); // š"‚Ì•¶Žš‰»‚¯‘ÎôÏ‚Ý
                if (string.IsNullOrEmpty(cellData)) continue;

                // ŠeŠK‚ÌŠî€À•Wi3ŠK‚Í1ŠK‚Ì‚‚³{2ŠK•ªã‹ój
                Vector3 basePosF1 = new Vector3(x * blockSize, 0, adjustedZ * blockSize);
                Vector3 basePosF2 = basePosF1 + new Vector3(0, floorHeight, 0);
                Vector3 basePosF3 = basePosF1 + new Vector3(0, floorHeight * 2, 0); // š3ŠK‚ÌÀ•W‚ð’Ç‰Á

                long f1Value = 0;
                long f2Value = 0;
                long f3Value = 0; // š3ŠK‚Ì”’l‚ðŠi”[‚·‚é” ‚ð’Ç‰Á
                float offsetX = 0, offsetZ = 0, rotY = 0;

                // --- CSVƒf[ƒ^‚Ì•ª‰ð (Vƒ‹[ƒ‹: 1ŠK_2ŠK_3ŠK_XƒYƒŒ_ZƒYƒŒ_Y‰ñ“]) ---
                if (cellData.Contains("_"))
                {
                    string[] parts = cellData.Split('_');
                    if (parts.Length > 0) long.TryParse(parts[0], out f1Value);
                    if (parts.Length > 1) long.TryParse(parts[1], out f2Value);
                    if (parts.Length > 2) long.TryParse(parts[2], out f3Value); // š3ŠK‚ÌˆÃ†‚ð“Ç‚Ýž‚Þ
                    if (parts.Length > 3) float.TryParse(parts[3], out offsetX);
                    if (parts.Length > 4) float.TryParse(parts[4], out offsetZ);
                    if (parts.Length > 5) float.TryParse(parts[5], out rotY);
                }
                else
                {
                    long.TryParse(cellData, out f1Value);
                }

                // ‰Æ‹ï—p‚ÌƒYƒŒ‚Æ‰ñ“]
                Vector3 offsetVec = new Vector3(offsetX, 0, offsetZ);
                Quaternion furnRot = Quaternion.Euler(0, rotY, 0);

                // --- ŠeŠK‚ÌŽ©“®¶¬ ---
                DecodeAndSpawn(f1Value, basePosF1, basePosF1 + offsetVec, furnRot);
                DecodeAndSpawn(f2Value, basePosF2, basePosF2 + offsetVec, furnRot);
                DecodeAndSpawn(f3Value, basePosF3, basePosF3 + offsetVec, furnRot); // š3ŠK‚ð¶¬I
            }
        }
    }

    // 11Œ…‚Ì”Žš‚ð•ª‰ð‚µ‚ÄAŠeƒp[ƒc‚ð¶¬‚·‚é‹¤’Êƒƒ\ƒbƒh
    void DecodeAndSpawn(long rawValue, Vector3 basePos, Vector3 furnPos, Quaternion furnRot)
    {
        if (rawValue == 0) return;

        int floorIndex = (int)((rawValue / 1000000000L) % 100); // 10‰­‚ÌˆÊ
        int wallIndex = (int)((rawValue / 10000000L) % 100);   // 1000–œ‚ÌˆÊ
        int doorIndex = (int)((rawValue / 100000L) % 100);     // 10–œ‚ÌˆÊ
        int windowIndex = (int)((rawValue / 1000L) % 100);       // 1000‚ÌˆÊ
        int furnIndex = (int)(rawValue % 1000);                // 1‚ÌˆÊ

        SpawnObject(floorPrefabs, floorIndex, basePos, Quaternion.identity);
        SpawnObject(wallPrefabs, wallIndex, basePos, Quaternion.identity);
        SpawnObject(doorPrefabs, doorIndex, basePos, Quaternion.identity);
        SpawnObject(windowPrefabs, windowIndex, basePos, Quaternion.identity);
        SpawnObject(furniturePrefabs, furnIndex, furnPos, furnRot);
    }

    void SpawnObject(GameObject[] prefabs, int index, Vector3 position, Quaternion rotation)
    {
        if (index > 0 && index <= prefabs.Length)
        {
            GameObject prefab = prefabs[index - 1];
            if (prefab != null) Instantiate(prefab, position, rotation, transform);
        }
    }
}