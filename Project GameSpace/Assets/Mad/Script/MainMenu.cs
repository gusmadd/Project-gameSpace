using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Audio")]
    public bool playMenuBGM = true;

    private void Start()
    {
        // Mainkan BGM menu saat scene ini dibuka
        if (playMenuBGM && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuBGM();
        }
    }

    public void PlayGame()
    {
        // Stop BGM menu, ganti ke BGM in-game
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayInGameBGM();
        }

        SceneManager.LoadScene("Level 1");
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