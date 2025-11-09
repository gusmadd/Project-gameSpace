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
    public Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
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
        if (player == null)
        {
            Debug.LogWarning($"{name} tidak punya player reference!");
            return;
        }

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

        // cari arah mendekati player
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

        Vector3Int cell = wallTilemap.WorldToCell(transform.position);
        targetWorldPos = wallTilemap.CellToWorld(cell) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = targetWorldPos;

        isMoving = false;
        ChooseNextDirection();
    }

    public void SetPlayer(Transform p)
    {
        player = p;
    }

    public void RespawnToStart()
    {
        transform.position = startPos; // kembalikan ke posisi awal
        isMoving = false;

        // Recenter posisi ke grid
        SnapToGrid();
        if (player == null && GameManager.Instance != null)
            SetPlayer(GameManager.Instance.Pacman.transform);

        // Mulai gerak lagi
        ChooseNextDirection();
    }

    public void RestartMovement()
    {
        SnapToGrid();
        ChooseNextDirection();
    }

}
