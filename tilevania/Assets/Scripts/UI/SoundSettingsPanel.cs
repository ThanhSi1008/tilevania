using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the sound settings panel with volume controls and mute toggles.
/// </summary>
public class SoundSettingsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider musicVolumeSlider; // Main volume control (replaces master volume)
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private TextMeshProUGUI musicVolumeText; // Main volume text (replaces master volume text)
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private bool showVolumePercentages = true;

    private void Start()
    {
        InitializeUI();
        LoadSettings();
        SetupButtons();
    }

    private void InitializeUI()
    {
        // Set up sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            // Remove any existing listeners first to avoid duplicates
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            Debug.Log("[SoundSettingsPanel] Music volume slider initialized");
        }
        else
        {
            Debug.LogError("[SoundSettingsPanel] musicVolumeSlider is null! Make sure it's assigned in the Inspector.");
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (muteToggle != null)
        {
            muteToggle.onValueChanged.AddListener(OnMuteToggled);
        }
    }

    private void SetupButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    private void LoadSettings()
    {
        // Load settings from PlayerPrefs
        // Migrate from MasterVolume to MusicVolume if needed
        float musicVolume = 1f;
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
        
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        // Apply to UI
        if (musicVolumeSlider != null)
        {
            // Temporarily disable listener to avoid triggering during load
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            // Force update text
            UpdateMusicVolumeText(musicVolume);
        }
        else
        {
            Debug.LogError("[SoundSettingsPanel] musicVolumeSlider is null in LoadSettings!");
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            UpdateSFXVolumeText(sfxVolume);
        }

        if (muteToggle != null)
        {
            muteToggle.isOn = isMuted;
        }

        // Apply to AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
            AudioManager.Instance.SetMuted(isMuted);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        UpdateSFXVolumeText(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    private void OnMusicVolumeChanged(float value)
    {
        Debug.Log($"[SoundSettingsPanel] OnMusicVolumeChanged called with value: {value}");
        
        // Update text FIRST, before anything else
        UpdateMusicVolumeText(value);
        
        // Apply to AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        else
        {
            Debug.LogWarning("[SoundSettingsPanel] AudioManager.Instance is null!");
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    private void OnMuteToggled(bool isMuted)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMuted(isMuted);
        }

        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void UpdateSFXVolumeText(float value)
    {
        if (sfxVolumeText != null && showVolumePercentages)
        {
            sfxVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    private void UpdateMusicVolumeText(float value)
    {
        if (musicVolumeText != null)
        {
            string newText = showVolumePercentages 
                ? $"{Mathf.RoundToInt(value * 100)}%" 
                : value.ToString("F2");
            
            musicVolumeText.text = newText;
            Debug.Log($"[SoundSettingsPanel] Updated music volume text to: '{newText}' (value: {value}, text component: {musicVolumeText.name})");
        }
        else
        {
            Debug.LogError("[SoundSettingsPanel] musicVolumeText is NULL! Please assign it in the Inspector.");
        }
    }

    private void OnCloseClicked()
    {
        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Hide the panel and restore the previous panel
        HidePanel();
        
        // Find BootstrapAuth and restore the appropriate panel
        var bootstrapAuth = FindFirstObjectByType<BootstrapAuth>();
        if (bootstrapAuth != null)
        {
            // Check if user is logged in to determine which panel to show
            if (AuthManager.Instance != null && AuthManager.Instance.HasToken() && AuthManager.Instance.CurrentPlayer != null)
            {
                // User is logged in, show main menu
                bootstrapAuth.ShowMainMenu();
            }
            else
            {
                // User is not logged in, show login panel
                bootstrapAuth.ShowLoginPanel();
            }
        }
        else
        {
            // Fallback: just hide the panel
            gameObject.SetActive(false);
        }
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
        LoadSettings(); // Refresh settings when showing
        
        // Force update text after a frame to ensure UI is ready
        StartCoroutine(ForceUpdateTextAfterFrame());
    }
    
    private System.Collections.IEnumerator ForceUpdateTextAfterFrame()
    {
        yield return null; // Wait one frame
        
        if (musicVolumeSlider != null)
        {
            float currentValue = musicVolumeSlider.value;
            UpdateMusicVolumeText(currentValue);
            Debug.Log($"[SoundSettingsPanel] Force updated music volume text after ShowPanel: {currentValue}");
        }
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}

