using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject winPanel;

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
}
