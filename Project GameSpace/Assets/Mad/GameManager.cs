using TMPro;
using UnityEngine;

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
            pacman.enabled = false;
            Invoke(nameof(RespawnRound), 1.5f);
        }
        else
        {
            GameOver();
        }
    }

    private void RespawnRound()
    {
        if (_isGameOver) return;

        pacman.ForceRecenter();
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
            Invoke(nameof(NewRound), 2f);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        if (_isGameOver) return;

        foreach (Ghost ghost in ghosts)
        {
            if (ghost.frightened != null)
                ghost.frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);

        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
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
}
