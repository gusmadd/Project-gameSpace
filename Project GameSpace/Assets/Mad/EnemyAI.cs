using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Tilemap wallTilemap; // Tilemap tembok (Obstacle)

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float arriveThreshold = 0.02f;

    private Vector3 targetWorldPos;
    private bool isMoving = false;
    private Vector2Int currentDir = Vector2Int.right; // arah awal

    private Transform player;
    public void SetPlayer(Transform p)
    {
        player = p;
    }
    void Start()
    {
        // sisanya tetap sama
        Vector3Int startCell = wallTilemap.WorldToCell(transform.position);
        targetWorldPos = wallTilemap.CellToWorld(startCell) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = targetWorldPos;

        TryStartMove(currentDir);
    }


    void FixedUpdate()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(transform.position, targetWorldPos) <= arriveThreshold)
            {
                transform.position = targetWorldPos;
                isMoving = false;
                ChooseNextDirection();
            }
        }
    }

    void ChooseNextDirection()
    {
        // arah yang mungkin (atas, bawah, kiri, kanan)
        List<Vector2Int> possibleDirs = new List<Vector2Int>()
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        // prioritas: jangan langsung balik arah
        Vector2Int opposite = -currentDir;
        possibleDirs.Remove(opposite);

        // filter arah yang bisa dilewati (bukan tembok)
        List<Vector2Int> validDirs = new List<Vector2Int>();
        foreach (var dir in possibleDirs)
        {
            if (CanMove(dir))
                validDirs.Add(dir);
        }

        if (validDirs.Count == 0)
        {
            // kalau semua mentok, balik arah
            TryStartMove(opposite);
        }
        else
        {
            // pilih acak salah satu arah valid
            Vector2Int chosen = validDirs[Random.Range(0, validDirs.Count)];
            TryStartMove(chosen);
        }
    }

    bool CanMove(Vector2Int dir)
    {
        Vector3Int currentCell = wallTilemap.WorldToCell(transform.position);
        Vector3Int nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);
        return !wallTilemap.HasTile(nextCell);
    }

    void TryStartMove(Vector2Int dir)
    {
        if (CanMove(dir))
        {
            currentDir = dir;
            isMoving = true;

            Vector3Int nextCell = wallTilemap.WorldToCell(transform.position) + new Vector3Int(dir.x, dir.y, 0);
            targetWorldPos = wallTilemap.CellToWorld(nextCell) + (Vector3)wallTilemap.cellSize * 0.5f;
        }
    }
    public void ForceRecenterAfterTeleport()
    {
        if (wallTilemap == null) return;

        // Snap ke tengah cell setelah teleport
        Vector3Int cell = wallTilemap.WorldToCell(transform.position);
        targetWorldPos = wallTilemap.CellToWorld(cell) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = targetWorldPos;

        // Reset agar bisa langsung gerak
        isMoving = false;
        ChooseNextDirection();
    }


}
