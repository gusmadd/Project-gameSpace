using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Level 1"); // langsung ke level 1
    }

    public void OpenHighScore()
    {
        SceneManager.LoadScene("HighScore");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void ExitGame()
    {
        Debug.Log("Game exited!");
        Application.Quit();
    }
}
