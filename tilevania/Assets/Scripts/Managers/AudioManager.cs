using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusicClip;

    [Header("Gameplay SFX")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip coinPickupClip;
    [SerializeField] private AudioClip levelExitClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip deathClip;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClickClip;

    // Volume settings
    private float musicVolume = 1f; // Main volume control (replaces master volume)
    private float sfxVolume = 1f;
    private bool isMuted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }

        // Create music source if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        // Load volume settings from PlayerPrefs
        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        // Load music volume (migrate from MasterVolume if exists, otherwise use MusicVolume)
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            musicVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.DeleteKey("MasterVolume");
            PlayerPrefs.Save();
        }
        else
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }
        
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        // Apply settings
        UpdateAudioSourceVolume();
    }

    private void UpdateAudioSourceVolume()
    {
        if (sfxSource != null)
        {
            float finalVolume = isMuted ? 0f : (musicVolume * sfxVolume);
            sfxSource.volume = finalVolume;
            Debug.Log($"[AudioManager] SFX volume set to: {finalVolume} (musicVolume: {musicVolume}, sfxVolume: {sfxVolume}, muted: {isMuted})");
        }

        if (musicSource != null)
        {
            float finalMusicVolume = isMuted ? 0f : musicVolume;
            musicSource.volume = finalMusicVolume;
            Debug.Log($"[AudioManager] Music volume set to: {finalMusicVolume} (musicVolume: {musicVolume}, muted: {isMuted})");
            
            // If volume is 0, stop the music to ensure it's completely silent
            if (finalMusicVolume <= 0f && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
            else if (finalMusicVolume > 0f && !musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.UnPause();
            }
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolume();
    }

    public void SetMusicVolume(float volume)
    {
        float oldVolume = musicVolume;
        musicVolume = Mathf.Clamp01(volume);
        Debug.Log($"[AudioManager] SetMusicVolume called: {oldVolume} -> {musicVolume}");
        UpdateAudioSourceVolume();
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    // Legacy method for compatibility - now sets music volume
    public void SetMasterVolume(float volume)
    {
        SetMusicVolume(volume);
    }

    public void SetMuted(bool muted)
    {
        isMuted = muted;
        UpdateAudioSourceVolume();
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    // Legacy method for compatibility - returns music volume
    public float GetMasterVolume()
    {
        return musicVolume;
    }

    public bool IsMuted()
    {
        return isMuted;
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusicClip != null)
        {
            if (musicSource.clip != backgroundMusicClip || !musicSource.isPlaying)
            {
                musicSource.clip = backgroundMusicClip;
                musicSource.Play();
            }
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PlayJump()
    {
        PlayOneShot(jumpClip);
    }

    public void PlayShoot()
    {
        PlayOneShot(shootClip);
    }

    public void PlayCoinPickup()
    {
        PlayOneShot(coinPickupClip);
    }

    public void PlayLevelExit()
    {
        PlayOneShot(levelExitClip);
    }

    public void PlayGameOver()
    {
        PlayOneShot(gameOverClip);
    }

    public void PlayDeath()
    {
        PlayOneShot(deathClip);
    }

    public void PlayButtonClick()
    {
        PlayOneShot(buttonClickClip);
    }

    public void PlayButtonClick(AudioClip overrideClip)
    {
        if (overrideClip != null)
        {
            PlayOneShot(overrideClip);
        }
        else
        {
            PlayButtonClick();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}

