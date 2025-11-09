using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AbilityManager : MonoBehaviour
{
    [Header("References")]
    public Tilemap wallTilemap;
    public GameObject bombPrefab;
    public GameObject chiliPrefab;
    public GameObject firePrefab;
    public GameObject icePrefab;   // prefab es (bongkahan pertama)
    public GameObject snowPrefab;  // prefab salju (yang menyebar)

    [Header("UI References")]
    public AbilityUI bombUI;
    public AbilityUI chiliUI;
    public AbilityUI iceUI;

    [Header("Cooldown Settings")]
    public float bombCooldown = 7f;
    public float chiliCooldown = 10f;
    public float iceCooldown = 8f;

    private bool canUseBomb = true;
    private bool canUseChili = true;
    private bool canUseIce = true;

    void Start()
    {
        canUseBomb = false;
        canUseChili = false;
        canUseIce = false;

        StartCoroutine(InitialCooldown(bombCooldown, () => canUseBomb = true, bombUI));
        StartCoroutine(InitialCooldown(chiliCooldown, () => canUseChili = true, chiliUI));
        StartCoroutine(InitialCooldown(iceCooldown, () => canUseIce = true, iceUI));
    }

    IEnumerator InitialCooldown(float duration, System.Action onFinish, AbilityUI ui)
    {
        ui?.TriggerCooldown(duration);
        yield return new WaitForSeconds(duration);
        onFinish?.Invoke();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) TryUseBomb();
        if (Input.GetKeyDown(KeyCode.N)) TryUseChili();
        if (Input.GetKeyDown(KeyCode.B)) TryUseIce();
    }

    // ---------- BOMB ----------
    void TryUseBomb()
    {
        if (!canUseBomb) return;
        PlaceBomb();
        StartCoroutine(CooldownRoutine(bombCooldown, () => canUseBomb = true, bombUI));
    }

    void PlaceBomb()
    {
        Vector3Int cell = wallTilemap.WorldToCell(transform.position);
        Vector3 spawnPos = wallTilemap.CellToWorld(cell) + (Vector3)wallTilemap.cellSize * 0.5f;
        Instantiate(bombPrefab, spawnPos, Quaternion.identity);
    }

    // ---------- CHILI ----------
    void TryUseChili()
    {
        if (!canUseChili) return;
        StartCoroutine(UseChiliRoutine());
        StartCoroutine(CooldownRoutine(chiliCooldown, () => canUseChili = true, chiliUI));
    }

    IEnumerator UseChiliRoutine()
    {
        Vector3Int cell = wallTilemap.WorldToCell(transform.position);
        Vector3 spawnPos = wallTilemap.CellToWorld(cell) + (Vector3)wallTilemap.cellSize * 0.5f;

        GameObject chili = Instantiate(chiliPrefab, spawnPos, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        SpawnFireRow(spawnPos);
        Destroy(chili);
    }

    void SpawnFireRow(Vector3 spawnPos)
    {
        Vector3Int startCell = wallTilemap.WorldToCell(spawnPos);
        int y = startCell.y;

        // kanan
        for (int x = startCell.x; x <= startCell.x + 3; x++)
        {
            Vector3Int cellPos = new Vector3Int(x, y, 0);
            if (wallTilemap.HasTile(cellPos)) break;
            Vector3 worldPos = wallTilemap.CellToWorld(cellPos) + (Vector3)wallTilemap.cellSize * 0.5f;
            GameObject fire = Instantiate(firePrefab, worldPos, Quaternion.identity);
            Destroy(fire, 1f);
        }

        // kiri
        for (int x = startCell.x - 1; x >= startCell.x - 3; x--)
        {
            Vector3Int cellPos = new Vector3Int(x, y, 0);
            if (wallTilemap.HasTile(cellPos)) break;
            Vector3 worldPos = wallTilemap.CellToWorld(cellPos) + (Vector3)wallTilemap.cellSize * 0.5f;
            GameObject fire = Instantiate(firePrefab, worldPos, Quaternion.identity);
            Destroy(fire, 1f);
        }
    }

    // ---------- ICE ----------
    void TryUseIce()
    {
        if (!canUseIce) return;
        StartCoroutine(UseIceRoutine());
        StartCoroutine(CooldownRoutine(iceCooldown, () => canUseIce = true, iceUI));
    }

    IEnumerator UseIceRoutine()
    {
        Vector3Int cell = wallTilemap.WorldToCell(transform.position);
        Vector3 spawnPos = wallTilemap.CellToWorld(cell) + (Vector3)wallTilemap.cellSize * 0.5f;

        // 1️⃣ Munculkan es utama
        GameObject ice = Instantiate(icePrefab, spawnPos, Quaternion.identity);

        yield return new WaitForSeconds(1f); // tunggu 1 detik sebelum jadi salju

        // 2️⃣ Es berubah jadi salju
        Destroy(ice);
        SpawnSnowCross(spawnPos);
    }

    void SpawnSnowCross(Vector3 spawnPos)
    {
        Vector3Int startCell = wallTilemap.WorldToCell(spawnPos);
        Vector3 halfCell = (Vector3)wallTilemap.cellSize * 0.5f;

        List<GameObject> spawnedSnow = new List<GameObject>();

        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        foreach (var dir in directions)
        {
            for (int i = 1; i <= 3; i++)
            {
                Vector3Int nextCell = startCell + dir * i;
                if (wallTilemap.HasTile(nextCell))
                    break;

                Vector3 worldPos = wallTilemap.CellToWorld(nextCell) + halfCell;
                GameObject snow = Instantiate(snowPrefab, worldPos, Quaternion.identity);
                spawnedSnow.Add(snow);
            }
        }

        // ❄️ Hapus semua salju setelah 4 detik
        foreach (var snow in spawnedSnow)
            Destroy(snow, 4f);
    }

    // ---------- GENERIC COOLDOWN ----------
    IEnumerator CooldownRoutine(float duration, System.Action onFinish, AbilityUI ui)
    {
        ui?.TriggerCooldown(duration);
        if (ui == bombUI) canUseBomb = false;
        if (ui == chiliUI) canUseChili = false;
        if (ui == iceUI) canUseIce = false;
        yield return new WaitForSeconds(duration);
        onFinish?.Invoke();
    }
}