using UnityEngine;
using TMPro; // tambahin ini biar bisa pakai TMP
using UnityEngine.SceneManagement;

public class HighScoreManager : MonoBehaviour
{
    public TMP_Text highScoreText; // ganti dari Text → TMP_Text

    void Start()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
