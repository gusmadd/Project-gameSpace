using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private PlayerController pacman;
    [SerializeField] private Transform pellets;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private bool showGameOverUI = false;

    [Header("Settings")]
    [SerializeField] private int startingLives = 3;

    private int ghostMultiplier = 1;
    private bool _isGameOver = false;
    private bool isRespawning = false;
    [SerializeField] private GameObject portalPrefab;
    private GameObject activePortal;
    [SerializeField] private Transform portalSpawnPoint;


    public int Score { get; private set; } = 0;
    public int Lives { get; private set; }
    public bool IsGameOver => _isGameOver;

    private void Awake()
    {
        if (Instance != null)
            DestroyImmediate(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        gameOverText.enabled = false;
        NewGame();
    }

    private void Update()
    {
        // restart game logika optional jika mau
        // disini tidak ada apa-apa
    }

    private void NewGame()
    {
        _isGameOver = false;
        isRespawning = false;
        ghostMultiplier = 1;
        SetScore(0);
        SetLives(startingLives);
        NewRound();
    }

    private void NewRound()
    {
        foreach (Transform pellet in pellets)
            pellet.gameObject.SetActive(true);

        ResetState();
    }

    private void ResetState()
    {
        // Aktifkan player
        pacman.gameObject.SetActive(true);
        pacman.enabled = true;
        pacman.ForceRecenter();

        // Aktifkan ghost & AI
        foreach (Ghost ghost in ghosts)
        {
            ghost.gameObject.SetActive(true);
            ghost.ResetState();
            var enemyAI = ghost.GetComponent<EnemyAI>();
            if (enemyAI != null)
                enemyAI.enabled = true;
        }
    }

    private void GameOver()
    {
        _isGameOver = true;

        // Hanya stop input player
        gameOverText.enabled = true;
        pacman.enabled = false;
        pacman.gameObject.SetActive(false);

        // ghost tetap terlihat, tapi movement dimatikan
        foreach (Ghost ghost in ghosts)
        {
            var enemyAI = ghost.GetComponent<EnemyAI>();
            if (enemyAI != null)
                enemyAI.enabled = false;
        }

        // tidak ada Time.timeScale = 0
        if (showGameOverUI && gameOverText != null)
            gameOverText.enabled = true;
    }

    private void SetLives(int lives)
    {
        Lives = lives;
        if (livesText != null)
            livesText.text = "x" + Lives.ToString();
    }

    private void SetScore(int score)
    {
        Score = score;
        if (scoreText != null)
            scoreText.text = Score.ToString().PadLeft(2, '0');
    }

    public void PacmanEaten()
    {
        if (_isGameOver || isRespawning) return;

        SetLives(Mathf.Max(Lives - 1, 0));

        if (Lives > 0)
        {
            isRespawning = true;

            // Nonaktifkan player dulu
            pacman.enabled = false;

            // Hentikan gerakan ghost sementara
            foreach (Ghost ghost in ghosts)
            {
                var enemyAI = ghost.GetComponent<EnemyAI>();
                if (enemyAI != null)
                    enemyAI.enabled = false;
            }

            // Jalankan delay respawn
            StartCoroutine(RespawnDelayCoroutine());
        }
        else
        {
            GameOver();
        }
    }
    private IEnumerator RespawnDelayCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        // aktifkan kembali AI ghost sebelum respawn round
        foreach (Ghost ghost in ghosts)
        {
            var enemyAI = ghost.GetComponent<EnemyAI>();
            if (enemyAI != null)
                enemyAI.enabled = true;
        }

        RespawnRound();
    }
    private void RespawnRound()
    {
        if (_isGameOver) return;

        pacman.RespawnToStart(); // respawn ke titik semula
        pacman.enabled = true;
        pacman.gameObject.SetActive(true);

        foreach (Ghost ghost in ghosts)
        {
            ghost.ResetState();
            ghost.gameObject.SetActive(true);
            var enemyAI = ghost.GetComponent<EnemyAI>();
            if (enemyAI != null)
                enemyAI.enabled = true;
        }

        isRespawning = false;
    }

    public void GhostEaten(Ghost ghost)
    {
        if (_isGameOver) return;

        int points = ghost.points * ghostMultiplier;
        SetScore(Score + points);
        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        if (_isGameOver) return;

        pellet.gameObject.SetActive(false);
        SetScore(Score + pellet.points);

        if (!HasRemainingPellets())
        {
            // semua pellet sudah diambil → panggil portal spawn
            SpawnPortal();
        }
    }
    private void SpawnPortal()
    {
        if (portalPrefab == null)
        {
            Debug.LogWarning("Portal prefab belum di-assign di Inspector!");
            return;
        }

        portalPrefab.SetActive(true);
        Debug.Log("✨ Portal diaktifkan karena semua pellet sudah diambil!");
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf)
                return true;
        }
        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }
    public void NextLevel()
    {
        Debug.Log("Next level dimulai!");

        // hapus portal lama
        if (activePortal != null)
            Destroy(activePortal);

        // opsional: efek fade, delay, dsb bisa ditambah di sini
        StartCoroutine(NextLevelCoroutine());
    }

    private IEnumerator NextLevelCoroutine()
    {
        // hentikan input & gerakan player
        pacman.enabled = false;
        var pacmanRb = pacman.GetComponent<Rigidbody2D>();
        if (pacmanRb != null)
            pacmanRb.velocity = Vector2.zero; // langsung berhenti

        // hentikan semua ghost sementara
        foreach (Ghost ghost in ghosts)
        {
            var ai = ghost.GetComponent<EnemyAI>();
            if (ai != null)
                ai.enabled = false;
        }
        // delay dikit biar kelihatan efeknya
        yield return new WaitForSeconds(1f);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Memuat scene berikutnya: " + nextSceneIndex);
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Tidak ada scene berikutnya, kembali ke menu utama atau restart game.");
            SceneManager.LoadScene(0); // misalnya balik ke main menu
        }
    }

}
