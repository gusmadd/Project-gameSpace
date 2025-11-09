using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Tilemap wallTilemap;       // Tilemap berisi dinding (Obstacle)
    public Tilemap pelletTilemap;     // (optional) pellet tilemap untuk dihapus

    [Header("Movement")]
    public float moveSpeed = 6f;      // unit per second (movesmooth)
    public float arriveThreshold = 0.02f; // tolerance to snap to center
    public int score = 0;              // contoh skor
    public int lives = 3;              // contoh nyawa

    // runtime
    private Vector3 targetWorldPos;
    private bool isMoving = false;
    public Vector3 startPos;

    // input buffering
    private Vector2Int currentDir = Vector2Int.zero; // arah saat ini (grid)
    private Vector2Int queuedDir = Vector2Int.zero;  // arah yang ditekan saat bergerak
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // Snap start position to nearest cell center
        Vector3Int startCell = wallTilemap.WorldToCell(transform.position);
        targetWorldPos = wallTilemap.CellToWorld(startCell) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = targetWorldPos;
    }
    public void RespawnToStart()
    {
        // Pastikan respawn ke posisi tengah cell awal (bukan posisi interpolasi)
        Vector3Int startCell = wallTilemap.WorldToCell(startPos);
        Vector3 cellCenter = wallTilemap.CellToWorld(startCell) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = cellCenter;

        isMoving = false;
        currentDir = Vector2Int.zero;
        queuedDir = Vector2Int.zero;
        targetWorldPos = cellCenter; // <— penting supaya arah berikutnya benar

        Debug.Log("Player respawn ke posisi awal: " + cellCenter);
    }


    // method untuk menambah skor
    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score: " + score);
        // Kalau mau, bisa update UI di sini
    }

    public void LoseLife()
    {
        lives--;
        Debug.Log("Player hit by ghost! Lives left: " + lives);

        if (lives <= 0)
            OnDeath();
        else
            ForceRecenter(); // kembali ke grid atau start position
    }


    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
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
            if (GameManager.Instance.IsGameOver) return;

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
        if (GameManager.Instance.IsGameOver) return;
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
        // --- Arahkan sprite ---
        if (spriteRenderer != null)
        {
            // Gerak ke atas
            if (dir == Vector2Int.up)
            {
                spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, 90);
                spriteRenderer.flipX = false; // reset flip
            }
            // Gerak ke bawah
            else if (dir == Vector2Int.down)
            {
                spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, -90);
                spriteRenderer.flipX = false;
            }
            // Gerak ke kanan
            else if (dir == Vector2Int.right)
            {
                spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                spriteRenderer.flipX = false;
            }
            // Gerak ke kiri
            else if (dir == Vector2Int.left)
            {
                spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, 0); // tetap rotasi normal
                spriteRenderer.flipX = true; // flip horizontal
            }
        }
    }

    void CheckAndEatPellet()
    {
        if (pelletTilemap == null) return;

        Vector3Int cell = pelletTilemap.WorldToCell(transform.position);
        if (pelletTilemap.HasTile(cell))
        {
            pelletTilemap.SetTile(cell, null);

            // ✅ Panggil SFX hanya kalau benar-benar makan pellet
            AudioManager.Instance?.PlayPelletEatSFX();

            // Tambah skor
            score += 10;
            Debug.Log("Makan pellet! Skor: " + score);
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
    public void OnDeath()
    {
        // 1. Reset posisi player
        Vector3 startPos = wallTilemap.CellToWorld(Vector3Int.zero) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = startPos;

        // 2. Matikan sementara movement/input
        isMoving = false;
        enabled = false; // disable PlayerController sementara

        // 3. Optional: bisa kasih animasi kedip atau efek flash
        StartCoroutine(RespawnDelay());

        Debug.Log("Player mati!");
        if (GameManager.Instance.IsGameOver)
            return;

    }

    private IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(2f);
        if (!GameManager.Instance.IsGameOver)
            enabled = true;
    }

    public void Respawn()
    {
        // Snap ke posisi awal (misal start cell)
        Vector3Int startCell = wallTilemap.WorldToCell(Vector3.zero);
        Vector3 startPos = wallTilemap.CellToWorld(startCell) + (Vector3)wallTilemap.cellSize * 0.5f;
        transform.position = startPos;

        // Reset arah & movement
        currentDir = Vector2Int.zero;
        queuedDir = Vector2Int.zero;
        isMoving = false;
    }
}

