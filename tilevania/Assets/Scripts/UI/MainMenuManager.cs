using System.Collections;
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

        // Start coroutine to load the appropriate level based on progress
        StartCoroutine(LoadNextLevelCoroutine());
    }

    private IEnumerator LoadNextLevelCoroutine()
    {
        Debug.Log("[MainMenuManager] LoadNextLevelCoroutine: Starting...");
        
        // Get LevelProgressManager instance
        if (LevelProgressManager.Instance == null)
        {
            Debug.LogError("[MainMenuManager] LevelProgressManager.Instance is null!");
            yield break;
        }

        // First, try to get current level from GameProfile (saved progress)
        Debug.Log("[MainMenuManager] Step 1: Getting current level from GameProfile...");
        LevelProgressManager.LevelData currentLevelData = null;
        yield return LevelProgressManager.Instance.GetCurrentLevel(data => currentLevelData = data);

        Debug.Log($"[MainMenuManager] Step 1 Result: currentLevelData={(currentLevelData != null ? $"{currentLevelData.levelName} (Level {currentLevelData.levelNumber})" : "NULL")}");

        LevelProgressManager.LevelData levelToLoad = currentLevelData;

        // If no current level saved, get next level to play (highest completed + 1, or Level 1)
        if (levelToLoad == null)
        {
            Debug.Log("[MainMenuManager] Step 2: No current level saved, determining next level to play...");
            yield return LevelProgressManager.Instance.GetNextLevelToPlay(data => levelToLoad = data);
            Debug.Log($"[MainMenuManager] Step 2 Result: levelToLoad={(levelToLoad != null ? $"{levelToLoad.levelName} (Level {levelToLoad.levelNumber})" : "NULL")}");
        }
        else
        {
            Debug.Log($"[MainMenuManager] ✅ Step 2: Found saved current level: {levelToLoad.levelName} (Level {levelToLoad.levelNumber})");
        }

        if (levelToLoad == null)
        {
            Debug.LogWarning("[MainMenuManager] Step 3: Could not determine level to load, using default: Level 1");
            // Fallback to default scene
            if (string.IsNullOrEmpty(gameplaySceneName))
            {
                Debug.LogError("Gameplay scene name is not set in MainMenuManager!");
                yield break;
            }
            Debug.Log($"[MainMenuManager] Loading default scene: {gameplaySceneName}");
            SceneManager.LoadScene(gameplaySceneName);
            yield break;
        }

        // Load the level's scene
        string sceneToLoad = levelToLoad.sceneName;
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            // Fallback to levelName if sceneName is not available
            sceneToLoad = levelToLoad.levelName;
            Debug.Log($"[MainMenuManager] Using levelName as sceneName: {sceneToLoad}");
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError($"[MainMenuManager] Level {levelToLoad.levelNumber} has no sceneName or levelName! Using default: {gameplaySceneName}");
            // Fallback to default
            sceneToLoad = gameplaySceneName;
        }

        Debug.Log($"[MainMenuManager] ✅ FINAL: Loading level scene: '{sceneToLoad}' (Level {levelToLoad.levelNumber}, levelName: {levelToLoad.levelName})");
        SceneManager.LoadScene(sceneToLoad);
    }
}

