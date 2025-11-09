using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    public AudioSource bgmSource;
    public AudioClip menuBGM;
    public AudioClip inGameBGM;
    public AudioClip levelCompleteBGM;
    public AudioClip pacmanDieBGM;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip pelletEatSFX;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // === BGM ===
    public void PlayMenuBGM()
    {
        PlayBGM(menuBGM, true);
    }

    public void PlayInGameBGM()
    {
        PlayBGM(inGameBGM, true);
    }

    public void PlayLevelCompleteBGM()
    {
        PlayBGM(levelCompleteBGM, false);
    }

    public void PlayPacmanDieBGM()
    {
        PlayBGM(pacmanDieBGM, false);
    }

    private void PlayBGM(AudioClip clip, bool loop)
    {
        if (bgmSource == null || clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    // === SFX ===
    public void PlayPelletEatSFX()
    {
        if (sfxSource == null || pelletEatSFX == null) return;
        sfxSource.PlayOneShot(pelletEatSFX);
    }
    public void PlayPacmanDieBGMThenResume()
    {
        if (bgmSource == null || pacmanDieBGM == null) return;
        StartCoroutine(PlayPacmanDieRoutine());
    }

    private IEnumerator PlayPacmanDieRoutine()
    {
        // Simpan BGM lama
        AudioClip previousClip = bgmSource.clip;

        // Mainkan BGM kematian
        bgmSource.Stop();
        bgmSource.loop = false;
        bgmSource.clip = pacmanDieBGM;
        bgmSource.Play();

        // Tunggu sampai selesai
        yield return new WaitForSeconds(pacmanDieBGM.length);

        // Lanjutkan BGM in-game
        if (previousClip == inGameBGM)
            PlayInGameBGM();
    }

}
