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
    
    [Header("Managers")]
    [SerializeField] private MainMenuManager mainMenuManager;

    void Start()
    {
        Debug.Log("[BootstrapAuth] Start - begin auto-login check");
        StartCoroutine(AutoLoginRoutine());
    }

    private IEnumerator AutoLoginRoutine()
    {
        // Show loading overlay
        SetLoading(true);

        // Check if AuthManager exists and has token
        if (AuthManager.Instance == null)
        {
            Debug.LogWarning("AuthManager.Instance is null. Showing login panel.");
            ShowLoginPanel();
            SetLoading(false);
            yield break;
        }

        if (!AuthManager.Instance.HasToken())
        {
            Debug.Log("[BootstrapAuth] No token found. Showing login panel.");
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
            Debug.Log($"[BootstrapAuth] Auto-login successful. User: {AuthManager.Instance.CurrentPlayer.username}");
            ShowMainMenu();
        }
        else
        {
            Debug.Log("[BootstrapAuth] Auto-login failed. Token invalid or expired. Showing login panel.");
            AuthManager.Instance?.ClearAuth();
            ShowLoginPanel();
        }

        SetLoading(false);
    }

    public void ShowLoginPanel()
    {
        Debug.Log($"[BootstrapAuth] ShowLoginPanel called - loginPanel: {loginPanel?.name ?? "NULL"}, registerPanel: {registerPanel?.name ?? "NULL"}, mainMenuPanel: {mainMenuPanel?.name ?? "NULL"}");
        
        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
            Debug.Log($"[BootstrapAuth] Set loginPanel active: {loginPanel.name}");
            
            // Clear login input fields when showing login panel (e.g., after logout)
            var loginManager = loginPanel.GetComponent<LoginManager>();
            if (loginManager != null)
            {
                loginManager.ClearInputs();
            }
        }
        else
        {
            Debug.LogWarning("[BootstrapAuth] loginPanel is NULL!");
        }
        
        if (registerPanel != null)
        {
            registerPanel.SetActive(false);
            Debug.Log($"[BootstrapAuth] Set registerPanel inactive: {registerPanel.name}");
        }
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            Debug.Log($"[BootstrapAuth] Set mainMenuPanel inactive: {mainMenuPanel.name}");
        }
        else
        {
            Debug.LogError("[BootstrapAuth] mainMenuPanel is NULL! Cannot hide main menu panel.");
        }
    }

    private void ShowMainMenu()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
        // Refresh UI to show username
        if (mainMenuManager != null)
        {
            mainMenuManager.RefreshUI();
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

