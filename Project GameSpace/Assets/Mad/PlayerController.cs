using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Tilemap wallTilemap;       // Tilemap berisi dinding (Obstacle)
    public Tilemap pelletTilemap;     // (optional) pellet tilemap untuk dihapus

    [Header("Movement")]
    public float moveSpeed = 6f;      // unit per second (movesmooth)
    public float arriveThreshold = 0.02f; // tolerance to snap to center

    // runtime
    private Vector3 targetWorldPos;
    private bool isMoving = false;

    // input buffering
    private Vector2Int currentDir = Vector2Int.zero; // arah saat ini (grid)
    private Vector2Int queuedDir = Vector2Int.zero;  // arah yang ditekan saat bergerak

    void Start()
    {
        // Snap start position to nearest cell center
        Vector3Int startCell = wallTilemap.WorldToCell(transform.position);
        targetWorldPos = wallTilemap.CellToWorld(startCell) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = targetWorldPos;
    }

    void Update()
    {
        ReadInput();

        if (!isMoving)
        {
            // jika ada queuedDir, pake itu; jika tidak, pakai currentDir
            Vector2Int dirToTry = queuedDir != Vector2Int.zero ? queuedDir : currentDir;

            if (dirToTry != Vector2Int.zero)
            {
                if (CanMove(dirToTry))
                {
                    StartMove(dirToTry);
                    // clear queued if used
                    if (queuedDir == dirToTry) queuedDir = Vector2Int.zero;
                }
                else
                {
                    // jika masih tidak bisa, dan kita belum bergerak, coba lihat arah currentDir (fallback)
                    if (currentDir != dirToTry && currentDir != Vector2Int.zero && CanMove(currentDir))
                        StartMove(currentDir);
                }
            }
        }
        else
        {
            // while moving, if there's a queued direction that becomes available when at next cell, we will try after arrival
        }

        // check pellet every frame (tile at current cell)
        CheckAndEatPellet();
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            // move towards targetWorldPos
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(transform.position, targetWorldPos) <= arriveThreshold)
            {
                transform.position = targetWorldPos; // snap
                isMoving = false;

                // after arriving, if queuedDir is available, immediately start next move
                if (queuedDir != Vector2Int.zero && CanMove(queuedDir))
                {
                    StartMove(queuedDir);
                    queuedDir = Vector2Int.zero;
                }
                else if (currentDir != Vector2Int.zero && CanMove(currentDir))
                {
                    // continue moving in same direction automatically (classic Pac-Man behavior)
                    StartMove(currentDir);
                }
            }
        }
    }

    void ReadInput()
    {
        // raw input so no smoothing
        float hx = Input.GetAxisRaw("Horizontal");
        float hy = Input.GetAxisRaw("Vertical");

        Vector2Int inputDir = Vector2Int.zero;
        if (Mathf.Abs(hx) > 0.1f) inputDir = new Vector2Int((int)Mathf.Sign(hx), 0);
        else if (Mathf.Abs(hy) > 0.1f) inputDir = new Vector2Int(0, (int)Mathf.Sign(hy));

        if (inputDir != Vector2Int.zero)
        {
            // store as queued direction; prefer latest press
            queuedDir = inputDir;

            // if not moving, set currentDir immediately so Update will try to move
            if (!isMoving)
            {
                currentDir = inputDir;
            }
        }
    }

    bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;

        // compute target cell from current cell center
        Vector3Int currentCell = wallTilemap.WorldToCell(transform.position);
        Vector3Int nextCell = new Vector3Int(currentCell.x + dir.x, currentCell.y + dir.y, currentCell.z);

        // if tilemap has a tile at nextCell (wall), cannot move
        if (wallTilemap.HasTile(nextCell)) return false;

        return true;
    }

    void StartMove(Vector2Int dir)
    {
        currentDir = dir;
        isMoving = true;

        Vector3Int currentCell = wallTilemap.WorldToCell(transform.position);
        Vector3Int nextCell = new Vector3Int(currentCell.x + dir.x, currentCell.y + dir.y, currentCell.z);
        targetWorldPos = wallTilemap.CellToWorld(nextCell) + (Vector3)wallTilemap.cellSize * 0.5f;
    }

    void CheckAndEatPellet()
    {
        if (pelletTilemap == null) return;

        Vector3Int cell = pelletTilemap.WorldToCell(transform.position);
        if (pelletTilemap.HasTile(cell))
        {
            pelletTilemap.SetTile(cell, null);
            // TODO: tambah scoring & SFX di sini
        }
    }
    public void ForceRecenter()
    {
        // Recalculate posisi grid setelah teleport
        Vector3Int currentCell = wallTilemap.WorldToCell(transform.position);
        targetWorldPos = wallTilemap.CellToWorld(currentCell) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = targetWorldPos;

        // Boleh langsung lanjutkan ke arah sebelumnya
        isMoving = false;

        // Jika arah saat ini valid, langsung lanjut jalan otomatis
        if (currentDir != Vector2Int.zero && CanMove(currentDir))
        {
            StartMove(currentDir);
        }
    }

}

