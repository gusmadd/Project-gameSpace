using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    public Tilemap wallTilemap;
    public float moveSpeed = 5f;
    public float arriveThreshold = 0.02f;

    private Vector3 targetWorldPos;
    private bool isMoving = false;
    private Vector2Int currentDir = Vector2Int.right;
    public Transform player;

    private void Start()
    {
        SnapToGrid();
        TryStartMove(currentDir);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetWorldPos) <= arriveThreshold)
        {
            transform.position = targetWorldPos;
            isMoving = false;
            ChooseNextDirection();
        }
    }

    private void SnapToGrid()
    {
        Vector3Int cell = wallTilemap.WorldToCell(transform.position);
        targetWorldPos = wallTilemap.CellToWorld(cell) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = targetWorldPos;
    }

    void ChooseNextDirection()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (player == null) return;

        List<Vector2Int> possibleDirs = new List<Vector2Int>()
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

        Vector2Int opposite = -currentDir;
        possibleDirs.Remove(opposite);

        List<Vector2Int> validDirs = new List<Vector2Int>();
        foreach (var dir in possibleDirs)
        {
            if (CanMove(dir))
                validDirs.Add(dir);
        }

        if (validDirs.Count == 0)
        {
            TryStartMove(opposite);
            return;
        }

        // Pilih arah yang mendekati player
        Vector2Int bestDir = validDirs[0];
        float minDist = float.MaxValue;

        foreach (var dir in validDirs)
        {
            Vector3Int nextCell = wallTilemap.WorldToCell(transform.position) + new Vector3Int(dir.x, dir.y, 0);
            Vector3 nextPos = wallTilemap.CellToWorld(nextCell) + (Vector3)wallTilemap.cellSize * 0.5f;

            float dist = Vector3.Distance(nextPos, player.position);
            if (dist < minDist)
            {
                minDist = dist;
                bestDir = dir;
            }
        }

        TryStartMove(bestDir);
    }


    private bool CanMove(Vector2Int dir)
    {
        Vector3Int nextCell = wallTilemap.WorldToCell(transform.position) + new Vector3Int(dir.x, dir.y, 0);
        return !wallTilemap.HasTile(nextCell);
    }

    private void TryStartMove(Vector2Int dir)
    {
        if (!CanMove(dir)) return;

        currentDir = dir;
        isMoving = true;

        Vector3Int nextCell = wallTilemap.WorldToCell(transform.position) + new Vector3Int(dir.x, dir.y, 0);
        targetWorldPos = wallTilemap.CellToWorld(nextCell) + (Vector3)wallTilemap.cellSize * 0.5f;
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
        // pilih arah berikutnya (acak)
        ChooseNextDirection();
    }

    public void SetPlayer(Transform p)
    {
        player = p;
    }



}
