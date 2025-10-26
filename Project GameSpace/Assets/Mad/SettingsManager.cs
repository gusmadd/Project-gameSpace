using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public AudioSource musicSource; // nanti diisi kalau udah punya musik

    void Start()
    {
        // Ambil nilai volume tersimpan, kalau belum ada pakai default 1
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (musicSlider != null)
            musicSlider.value = musicVolume;

        if (sfxSlider != null)
            sfxSlider.value = sfxVolume;

        // Kalau udah ada AudioSource (musicSource), atur volumenya
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void ChangeMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = musicSlider.value;

        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
    }

    public void ChangeSFXVolume()
    {
        // Ini buat efek suara (kalau nanti kamu tambahin)
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
    }

    public void BackToMenu()
    {
        // Simpan dulu sebelum pindah scene
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }
}
