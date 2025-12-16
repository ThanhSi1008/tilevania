using System;
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
    
    [Header("Player Stats UI (Phase 4)")]
    [SerializeField] private TextMeshProUGUI rankValueText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private TextMeshProUGUI achievementsText;
    
    [Header("Navigation Buttons (Phase 4)")]
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button achievementsButton;
    
    [Header("Panel References")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject achievementsPanel;
    
    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "Level 1"; // Tên scene gameplay trong Build Settings

    void Start()
    {
        RefreshUI();
        SetupButtons();
        LoadPlayerStats();
    }

    void OnEnable()
    {
        // Refresh stats when menu is shown
        if (AuthManager.Instance != null && AuthManager.Instance.HasToken())
        {
            LoadPlayerStats();
        }
    }

    private void SetupButtons()
    {
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        }
        if (achievementsButton != null)
        {
            achievementsButton.onClick.AddListener(OnAchievementsClicked);
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

    private void LoadPlayerStats()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.HasToken() || AuthManager.Instance.CurrentPlayer == null)
        {
            // Clear stats if not logged in
            if (rankValueText != null) rankValueText.text = "N/A";
            if (totalScoreText != null) totalScoreText.text = "0";
            if (totalCoinsText != null) totalCoinsText.text = "0";
            if (achievementsText != null) achievementsText.text = "0/0";
            return;
        }

        var userId = AuthManager.Instance.CurrentPlayer.userId;
        StartCoroutine(LoadPlayerStatsCoroutine(userId));
    }

    private IEnumerator LoadPlayerStatsCoroutine(string userId)
    {
        // Load rank
        if (rankValueText != null && LeaderboardManager.Instance != null)
        {
            LeaderboardManager.LeaderboardEntry rankData = null;
            yield return LeaderboardManager.Instance.GetPlayerRank(userId, "ALLTIME", (rank) => rankData = rank);
            
            if (rankData != null)
            {
                rankValueText.text = $"#{rankData.rank}";
                rankValueText.color = rankData.rank <= 3 ? Color.yellow : Color.white;
            }
            else
            {
                rankValueText.text = "N/A";
            }
        }

        // Load game profile stats
        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.GameProfile(userId), r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<GameProfileResponse>(apiResult.data);
                if (parsed != null && parsed.gameProfile != null)
                {
                    var profile = parsed.gameProfile;
                    
                    if (totalScoreText != null)
                    {
                        totalScoreText.text = profile.totalScore.ToString("N0");
                    }
                    
                    if (totalCoinsText != null)
                    {
                        totalCoinsText.text = profile.totalCoinsCollected.ToString("N0");
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // Load achievements count
        if (achievementsText != null && AchievementManager.Instance != null)
        {
            int totalAchievements = AchievementManager.Instance.AllAchievements?.Count ?? 0;
            int unlockedCount = AchievementManager.Instance.UnlockedAchievements?.Count ?? 0;
            achievementsText.text = $"{unlockedCount}/{totalAchievements}";
            
            // Color green if all unlocked
            if (unlockedCount == totalAchievements && totalAchievements > 0)
            {
                achievementsText.color = Color.green;
            }
            else
            {
                achievementsText.color = Color.white;
            }
        }
    }

    private void OnLeaderboardClicked()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
        }
    }

    private void OnAchievementsClicked()
    {
        if (achievementsPanel != null)
        {
            achievementsPanel.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
        }
    }

    public void OnLogoutClicked()
    {
        // Ensure we always have a reference to the main menu object (fallback to this)
        var mainMenuObj = mainMenuPanel != null ? mainMenuPanel : gameObject;
        
        // Clear authentication FIRST
        AuthManager.Instance?.ClearAuth();
        
        // ALWAYS use BootstrapAuth to switch panels to ensure consistency
        var bootstrapAuth = FindFirstObjectByType<BootstrapAuth>();
        if (bootstrapAuth != null)
        {
            bootstrapAuth.ShowLoginPanel();
        }
        else
        {
            // Fallback: Try using direct panel references
            if (loginPanel != null)
            {
                loginPanel.SetActive(true);
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
                mainMenuObj.SetActive(false);
            }
        }
        
        // Refresh UI AFTER panels are switched (this should not interfere now)
        RefreshUI();
        
        // DOUBLE CHECK: Ensure mainMenuPanel is hidden after RefreshUI (in case something interfered)
        // Try both direct reference and via BootstrapAuth
        if (mainMenuObj != null && mainMenuObj.activeSelf)
        {
            mainMenuObj.SetActive(false);
        }
        
        // Also try to hide via BootstrapAuth's reference if available
        if (bootstrapAuth != null)
        {
            bootstrapAuth.ShowLoginPanel(); // Call again to ensure panels are in correct state
        }
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

    [Serializable]
    private class GameProfileResponse
    {
        public GameProfileData gameProfile;
    }

    [Serializable]
    private class GameProfileData
    {
        public int totalScore;
        public int totalCoinsCollected;
        public int totalEnemiesDefeated;
        public int totalDeaths;
        public float totalPlayTime;
    }
}

