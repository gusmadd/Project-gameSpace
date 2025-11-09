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
    public PlayerController Pacman => pacman;
    [SerializeField] private Transform pellets;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;
    private GameUIManager uiManager;

    [Header("Settings")]
    [SerializeField] private int startingLives = 3;

    private bool _isGameOver = false;
    private bool isRespawning = false;

    [Header("Portal Settings")]
    [SerializeField] private GameObject portalPrefab;
    private GameObject activePortal;
    [SerializeField] private Transform portalSpawnPoint;

    [Header("Pause Settings")]
    [SerializeField] private GameObject pauseMenuUI;
    private bool isPaused = false;

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
        uiManager = FindObjectOfType<GameUIManager>();
        NewGame();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // === GAME FLOW ===
    private void NewGame()
    {
        _isGameOver = false;
        isRespawning = false;

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
        pacman.gameObject.SetActive(true);
        pacman.enabled = true;
        pacman.ForceRecenter();

        foreach (Ghost ghost in ghosts)
        {
            if (ghost == null) continue;
            ghost.gameObject.SetActive(true);
            ghost.ResetState();
            if (ghost.movement != null)
                ghost.movement.enabled = true;
        }
    }

    private void GameOver()
    {
        _isGameOver = true;

        pacman.enabled = false;
        pacman.gameObject.SetActive(false);

        foreach (Ghost ghost in ghosts)
        {
            if (ghost != null && ghost.movement != null)
                ghost.movement.enabled = false;
        }

        if (uiManager != null)
            uiManager.ShowGameOver();
    }

    // === SCORE & LIVES ===
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

    // === PLAYER DEATH & RESPAWN ===
    public void PacmanEaten()
    {
        if (_isGameOver || isRespawning) return;

        SetLives(Mathf.Max(Lives - 1, 0));

        if (Lives > 0)
        {
            isRespawning = true;
            pacman.enabled = false;

            foreach (Ghost ghost in ghosts)
            {
                if (ghost != null && ghost.movement != null)
                    ghost.movement.enabled = false;
            }

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

        foreach (Ghost ghost in ghosts)
        {
            if (ghost != null && ghost.movement != null)
                ghost.movement.enabled = true;
        }

        RespawnRound();
    }

    private void RespawnRound()
    {
        if (_isGameOver) return;

        pacman.RespawnToStart();
        pacman.enabled = true;
        pacman.gameObject.SetActive(true);

        foreach (Ghost ghost in ghosts)
        {
            if (ghost == null) continue;
            ghost.enabled = true; // hanya aktifkan lagi, jangan ubah posisi
        }

        isRespawning = false;
    }

    // === PELLET & PORTAL ===
    public void PelletEaten(Pellet pellet)
    {
        if (_isGameOver) return;

        pellet.gameObject.SetActive(false);
        SetScore(Score + pellet.points);

        if (!HasRemainingPellets())
        {
            SpawnPortal();
            Debug.Log("Semua pellet sudah diambil! Portal aktif!");
        }
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

    private void SpawnPortal()
    {
        if (portalPrefab == null)
        {
            Debug.LogWarning("Portal prefab belum di-assign!");
            return;
        }

        portalPrefab.SetActive(true);
        Debug.Log("✨ Portal diaktifkan!");
    }

    // === LEVEL MANAGEMENT ===
    public void LevelCompleted()
    {
        _isGameOver = true;
        pacman.enabled = false;

        foreach (Ghost ghost in ghosts)
        {
            if (ghost != null && ghost.movement != null)
                ghost.movement.enabled = false;
        }

        Debug.Log("Level selesai!");
        if (uiManager != null)
            uiManager.ShowWin();

        StartCoroutine(NextLevelDelay());
    }

    private IEnumerator NextLevelDelay()
    {
        yield return new WaitForSeconds(2f);
        NextLevel();
    }

    public void NextLevel()
    {
        Debug.Log("Next level dimulai!");

        if (activePortal != null)
            Destroy(activePortal);

        StartCoroutine(NextLevelCoroutine());
    }

    private IEnumerator NextLevelCoroutine()
    {
        pacman.enabled = false;
        var rb = pacman.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        foreach (Ghost ghost in ghosts)
        {
            if (ghost != null && ghost.movement != null)
                ghost.movement.enabled = false;
        }

        yield return new WaitForSeconds(1f);

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;

        if (nextScene < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextScene);
        else
            SceneManager.LoadScene(0);
    }

    // === GAME STATE ===
    public bool IsLastLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        return currentScene + 1 >= SceneManager.sceneCountInBuildSettings;
    }

    public void WinGame()
    {
        PlayerPrefs.SetInt("LastLevelIndex", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.SetInt("LastLives", Lives);
        PlayerPrefs.SetInt("LastScore", Score);
        SceneManager.LoadScene("Akhir");
    }

    // === PAUSE MENU ===
    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
        Debug.Log("Game paused!");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        Debug.Log("Game resumed!");
    }

    public void RetryGame()
    {
        Debug.Log("Retry pressed!");
        Time.timeScale = 1f;
        isPaused = false;
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(1);
    }
}
