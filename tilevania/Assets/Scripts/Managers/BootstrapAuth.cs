using System.Collections;
using UnityEngine;

/// <summary>
/// Handles auto-login on scene start. Checks for valid token and switches UI panels accordingly.
/// </summary>
public class BootstrapAuth : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject soundSettingsPanel;
    
    [Header("Managers")]
    [SerializeField] private MainMenuManager mainMenuManager;

    void Start()
    {
        StartCoroutine(AutoLoginRoutine());
    }

    private IEnumerator AutoLoginRoutine()
    {
        // Show loading overlay
        SetLoading(true);

        // Check if AuthManager exists and has token
        if (AuthManager.Instance == null)
        {
            ShowLoginPanel();
            SetLoading(false);
            yield break;
        }

        if (!AuthManager.Instance.HasToken())
        {
            ShowLoginPanel();
            SetLoading(false);
            yield break;
        }

        // Validate token with server
        bool isValid = false;
        yield return AuthManager.Instance.ValidateTokenCoroutine(success =>
        {
            isValid = success;
        });

        // Handle validation result
        if (isValid && AuthManager.Instance.CurrentPlayer != null)
        {
            ShowMainMenu();
        }
        else
        {
            AuthManager.Instance?.ClearAuth();
            ShowLoginPanel();
        }

        SetLoading(false);
    }

    public void ShowLoginPanel()
    {
        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
            // Clear login input fields when showing login panel (e.g., after logout)
            var loginManager = loginPanel.GetComponent<LoginManager>();
            if (loginManager != null)
            {
                loginManager.ClearInputs();
            }
        }
        else
        {
        }
        
        if (registerPanel != null)
        {
            registerPanel.SetActive(false);
        }
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        else
        {
        }

        // Hide sound settings panel when switching to login
        if (soundSettingsPanel != null)
        {
            soundSettingsPanel.SetActive(false);
        }
    }

    public void ShowMainMenu()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
        // Hide sound settings panel when showing main menu
        if (soundSettingsPanel != null)
        {
            soundSettingsPanel.SetActive(false);
        }
        
        // Refresh UI to show username
        if (mainMenuManager != null)
        {
            mainMenuManager.RefreshUI();
        }
    }

    public void ShowSoundSettings()
    {
        Debug.Log("[BootstrapAuth] ShowSoundSettings() called");
        
        // Hide all other panels first
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        
        // Show sound settings panel
        if (soundSettingsPanel != null)
        {
            Debug.Log("[BootstrapAuth] Sound settings panel found, showing it");
            var soundSettings = soundSettingsPanel.GetComponent<SoundSettingsPanel>();
            if (soundSettings != null)
            {
                soundSettings.ShowPanel();
            }
            else
            {
                Debug.LogWarning("[BootstrapAuth] SoundSettingsPanel component not found, activating GameObject directly");
                soundSettingsPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("[BootstrapAuth] Sound settings panel is not assigned! Please assign it in the Inspector.");
        }
    }

    public void HideSoundSettings()
    {
        if (soundSettingsPanel != null)
        {
            var soundSettings = soundSettingsPanel.GetComponent<SoundSettingsPanel>();
            if (soundSettings != null)
            {
                soundSettings.HidePanel();
            }
            else
            {
                soundSettingsPanel.SetActive(false);
            }
        }
    }

    private void SetLoading(bool state)
    {
        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(state);
        }
    }
}

