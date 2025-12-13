using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private GameObject loggedInPanel;
    [SerializeField] private GameObject loggedOutPanel;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button playButton;
    
    [Header("Panel References")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject mainMenuPanel;
    
    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "Level 1"; // Tên scene gameplay trong Build Settings

    void Start()
    {
        RefreshUI();
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
    }

    public void RefreshUI()
    {
        var isAuthed = AuthManager.Instance != null && AuthManager.Instance.HasToken();
        
        // Show/hide panels based on auth status
        if (loggedInPanel != null) loggedInPanel.SetActive(isAuthed);
        if (loggedOutPanel != null) loggedOutPanel.SetActive(!isAuthed);
        
        // If only one panel is set, use it for both states
        if (loggedInPanel == null && loggedOutPanel != null)
        {
            loggedOutPanel.SetActive(true); // Always show if no loggedInPanel
        }
        else if (loggedOutPanel == null && loggedInPanel != null)
        {
            loggedInPanel.SetActive(true); // Always show if no loggedOutPanel
        }

        var username = isAuthed && AuthManager.Instance.CurrentPlayer != null
            ? AuthManager.Instance.CurrentPlayer.username
            : "Guest";

        if (usernameText != null) usernameText.text = $"Welcome, {username}";
    }

    public void OnLogoutClicked()
    {
        Debug.Log("[MainMenuManager] Logout clicked - clearing auth and returning to login");
        
        // Clear authentication FIRST
        AuthManager.Instance?.ClearAuth();
        
        // ALWAYS use BootstrapAuth to switch panels to ensure consistency
        var bootstrapAuth = FindFirstObjectByType<BootstrapAuth>();
        if (bootstrapAuth != null)
        {
            Debug.Log("[MainMenuManager] Using BootstrapAuth.ShowLoginPanel() to switch panels");
            bootstrapAuth.ShowLoginPanel();
        }
        else
        {
            // Fallback: Try using direct panel references
            Debug.LogWarning("[MainMenuManager] BootstrapAuth not found, using direct panel references");
            if (loginPanel != null)
            {
                Debug.Log($"[MainMenuManager] Setting loginPanel active: {loginPanel.name}");
                loginPanel.SetActive(true);
            }
            if (registerPanel != null)
            {
                Debug.Log($"[MainMenuManager] Setting registerPanel inactive: {registerPanel.name}");
                registerPanel.SetActive(false);
            }
            if (mainMenuPanel != null)
            {
                Debug.Log($"[MainMenuManager] Setting mainMenuPanel inactive: {mainMenuPanel.name}");
                mainMenuPanel.SetActive(false);
            }
            else
            {
                Debug.LogError("[MainMenuManager] mainMenuPanel reference is NULL! Cannot hide main menu panel.");
            }
        }
        
        // Refresh UI AFTER panels are switched (this should not interfere now)
        RefreshUI();
        
        // DOUBLE CHECK: Ensure mainMenuPanel is hidden after RefreshUI (in case something interfered)
        // Try both direct reference and via BootstrapAuth
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            Debug.LogWarning($"[MainMenuManager] mainMenuPanel was still active after RefreshUI! Forcing it inactive: {mainMenuPanel.name}");
            mainMenuPanel.SetActive(false);
        }
        
        // Also try to hide via BootstrapAuth's reference if available
        if (bootstrapAuth != null)
        {
            bootstrapAuth.ShowLoginPanel(); // Call again to ensure panels are in correct state
        }
        
        Debug.Log("[MainMenuManager] Logout complete - loginPanel should be visible, mainMenuPanel should be hidden");
    }

    // Called when PlayButton is clicked - loads gameplay scene
    public void OnPlayClicked()
    {
        // Check if user is logged in (optional - remove if you want guest play)
        if (AuthManager.Instance == null || !AuthManager.Instance.HasToken())
        {
            Debug.LogWarning("Please login before playing");
            // Optionally show error message to user
            return;
        }

        // Load gameplay scene
        if (string.IsNullOrEmpty(gameplaySceneName))
        {
            Debug.LogError("Gameplay scene name is not set in MainMenuManager!");
            return;
        }

        Debug.Log($"Loading scene: {gameplaySceneName}");
        SceneManager.LoadScene(gameplaySceneName);
    }
}

