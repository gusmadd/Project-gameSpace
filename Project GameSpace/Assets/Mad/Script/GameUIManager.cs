using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject pausePanel;
    private bool isPaused = false;

    void Update()
    {
        // Tekan ESC untuk pause/unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void ShowWin()
    {
        winPanel.SetActive(true);
    }

    public void WinGame()
    {
        // Load Victory Scene
        SceneManager.LoadScene("Akhir");
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Pause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f; // freeze semua aktivitas game
        isPaused = true;
        Debug.Log("Game paused");
    }

    public void Resume()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f; // lanjutkan waktu
        isPaused = false;
        Debug.Log("Game resumed");
    }
}
