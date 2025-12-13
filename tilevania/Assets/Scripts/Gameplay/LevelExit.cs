using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] float levelLoadDelay = 1f;
    [SerializeField] private GameObject loadingOverlay; // Loading overlay to show during level transition
    private bool isProcessing = false; // Prevent multiple triggers
    
    // Static flag to track loading state - accessible by other scripts
    public static bool IsLoading { get; set; } = false;
    
    void Start()
    {
        // Reset loading flag when scene starts (in case it was left true from previous scene)
        IsLoading = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isProcessing)
        {
            isProcessing = true;
            Debug.Log("[LevelExit] Player reached level exit! Starting level completion process...");
            StartCoroutine(LoadNextLevel());
        }
        else if (isProcessing)
        {
            Debug.Log("[LevelExit] Level completion already in progress, ignoring trigger");
        }
    }

    IEnumerator LoadNextLevel()
    {
        Debug.Log($"[LevelExit] LoadNextLevel started - Waiting {levelLoadDelay} seconds...");
        yield return new WaitForSecondsRealtime(levelLoadDelay);
        
        // Set loading flag to disable player input
        IsLoading = true;
        
        // Show loading overlay FIRST, before any other operations
        // This ensures it's visible during the entire transition
        ShowLoadingOverlay(true);
        
        // Wait a frame to ensure UI updates
        yield return null;
        
        // End current session before loading next level
        var gameSession = FindFirstObjectByType<GameSession>();
        if (gameSession != null)
        {
            Debug.Log("[LevelExit] Found GameSession, calling EndSession(COMPLETED)...");
            // Sync final stats and end session (this will also check achievements and show notifications)
            yield return gameSession.EndSession("COMPLETED");
            Debug.Log("[LevelExit] EndSession completed!");
        }
        else
        {
            Debug.LogWarning("[LevelExit] GameSession not found! Cannot end session properly.");
        }
        
        // Wait a bit more to let players see achievement notifications (but keep loading overlay visible)
        Debug.Log("[LevelExit] Waiting 2 seconds for achievement notifications...");
        yield return new WaitForSecondsRealtime(2f);
        
        // Ensure loading overlay is still visible before loading
        ShowLoadingOverlay(true);
        yield return null; // Wait another frame to ensure UI update
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("[LevelExit] Reached last level, returning to scene 0 (MainMenu)");
            nextSceneIndex = 0;
        }
        else
        {
            Debug.Log($"[LevelExit] Loading next level: Scene {nextSceneIndex}");
            
            // Check if next level is unlocked before loading
            yield return StartCoroutine(CheckAndLoadNextLevel(nextSceneIndex));
            yield break; // Exit early if level check handled loading
        }

        FindFirstObjectByType<ScenePersist>().ResetScenePersist();
        
        // Hide overlay in current scene before loading new scene
        // This prevents overlay from persisting into the new scene
        ShowLoadingOverlay(false);
        yield return null; // Wait a frame to ensure overlay is hidden
        
        // Load scene asynchronously to allow UI to update
        var asyncOperation = SceneManager.LoadSceneAsync(nextSceneIndex);
        asyncOperation.allowSceneActivation = false;
        
        // Wait until scene is almost loaded (90%)
        while (asyncOperation.progress < 0.9f)
        {
            yield return null;
        }
        
        // Allow scene activation
        asyncOperation.allowSceneActivation = true;
        
        // Wait for scene to fully load
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        
        // Wait a few frames to ensure new scene is fully initialized
        yield return null;
        yield return null;
        yield return null;
        
        // Reset loading flag to re-enable player input
        IsLoading = false;
        
        // Hide overlay in the new scene (try to find it in the new scene)
        ShowLoadingOverlay(false);
        Debug.Log("[LevelExit] Scene loaded, loading overlay hidden, input re-enabled");
    }
    
    private void ShowLoadingOverlay(bool show)
    {
        GameObject overlayGO = null;
        
        if (loadingOverlay != null)
        {
            overlayGO = loadingOverlay;
        }
        else
        {
            // Try to find loading overlay in current active scene
            var activeScene = SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();
            
            foreach (var rootObj in rootObjects)
            {
                var canvas = rootObj.GetComponent<Canvas>();
                if (canvas != null)
                {
                    var overlay = canvas.transform.Find("LoadingOverlay");
                    if (overlay != null)
                    {
                        overlayGO = overlay.gameObject;
                        break;
                    }
                }
            }
            
            // Fallback: try FindFirstObjectByType
            if (overlayGO == null)
            {
                var overlay = FindFirstObjectByType<Canvas>()?.transform?.Find("LoadingOverlay");
                if (overlay != null)
                {
                    overlayGO = overlay.gameObject;
                }
            }
        }
        
        if (overlayGO != null)
        {
            overlayGO.SetActive(show);
            
            // Block raycasts when showing overlay to prevent UI input
            var canvasGroup = overlayGO.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = show;
                canvasGroup.interactable = show;
            }
            else
            {
                // If no CanvasGroup, try to add one or use Image component
                var image = overlayGO.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.raycastTarget = show;
                }
            }
            
            Debug.Log($"[LevelExit] Loading overlay {(show ? "shown" : "hidden")} (blocksRaycasts: {show}, Scene: {overlayGO.scene.name})");
        }
        else
        {
            Debug.LogWarning("[LevelExit] Loading overlay not found! Please assign it in Inspector or create one in Canvas.");
        }
    }

    private IEnumerator CheckAndLoadNextLevel(int nextSceneIndex)
    {
        // Get next level's scene name
        string nextSceneName = null;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            nextSceneName = SceneManager.GetSceneByBuildIndex(nextSceneIndex).name;
            // If scene not loaded yet, try to get name from build settings
            if (string.IsNullOrEmpty(nextSceneName))
            {
                nextSceneName = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(nextSceneIndex));
            }
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[LevelExit] Cannot determine next scene name, loading anyway");
            yield break;
        }

        // Get player's total score from GameProfile (we'll fetch it)
        int playerTotalScore = 0;
        if (AuthManager.Instance?.CurrentPlayer != null)
        {
            var userId = AuthManager.Instance.CurrentPlayer.userId;
            yield return StartCoroutine(GetPlayerTotalScore(userId, score => playerTotalScore = score));
        }

        // Get next level data
        var levelProgressManager = LevelProgressManager.Instance;
        if (levelProgressManager == null)
        {
            Debug.LogWarning("[LevelExit] LevelProgressManager not found, loading level anyway");
            yield break;
        }

        // Resolve next level's levelId
        string nextLevelId = null;
        var nextLevelData = levelProgressManager.GetLevelDataBySceneName(nextSceneName);
        if (nextLevelData != null)
        {
            nextLevelId = nextLevelData._id;
        }
        else
        {
            // Try by level number (nextSceneIndex + 1 = levelNumber, assuming Level 1 is at index 1)
            int nextLevelNumber = nextSceneIndex; // Adjust based on your scene setup
            nextLevelData = levelProgressManager.GetLevelDataByNumber(nextLevelNumber);
            if (nextLevelData != null)
            {
                nextLevelId = nextLevelData._id;
            }
        }

        if (string.IsNullOrEmpty(nextLevelId) || nextLevelData == null)
        {
            Debug.LogWarning($"[LevelExit] Cannot find level data for scene '{nextSceneName}', loading anyway");
            yield break;
        }

        // Check if level is unlocked
        bool isUnlocked = nextLevelData.isUnlocked;
        if (!isUnlocked && nextLevelData.requiredScoreToUnlock > 0)
        {
            isUnlocked = playerTotalScore >= nextLevelData.requiredScoreToUnlock;
        }

        if (!isUnlocked)
        {
            // Level is locked - show message and return to main menu
            string message = nextLevelData.requiredScoreToUnlock > 0
                ? $"Level '{nextLevelData.levelName}' is locked. Requires {nextLevelData.requiredScoreToUnlock} total score. Your score: {playerTotalScore}"
                : $"Level '{nextLevelData.levelName}' is locked.";
            
            Debug.LogWarning($"[LevelExit] {message}");
            
            // Hide loading overlay
            ShowLoadingOverlay(false);
            IsLoading = false;
            
            // Wait a bit for player to see message (if we add UI later)
            yield return new WaitForSecondsRealtime(2f);
            
            // Return to main menu (scene 0)
            FindFirstObjectByType<ScenePersist>()?.ResetScenePersist();
            SceneManager.LoadScene(0);
            yield break;
        }

        // Level is unlocked - proceed with loading
        Debug.Log($"[LevelExit] Next level '{nextLevelData.levelName}' is unlocked, loading...");
        
        // Update currentLevel in GameProfile to the next level
        if (LevelProgressManager.Instance != null && !string.IsNullOrEmpty(nextLevelId))
        {
            yield return LevelProgressManager.Instance.UpdateCurrentLevel(nextLevelId);
        }
        
        FindFirstObjectByType<ScenePersist>()?.ResetScenePersist();
        
        // Hide overlay in current scene before loading new scene
        ShowLoadingOverlay(false);
        yield return null;
        
        // Load scene asynchronously
        var asyncOperation = SceneManager.LoadSceneAsync(nextSceneIndex);
        asyncOperation.allowSceneActivation = false;
        
        while (asyncOperation.progress < 0.9f)
        {
            yield return null;
        }
        
        asyncOperation.allowSceneActivation = true;
        
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        
        yield return null;
        yield return null;
        yield return null;
        
        IsLoading = false;
        ShowLoadingOverlay(false);
        Debug.Log("[LevelExit] Scene loaded, loading overlay hidden, input re-enabled");
    }

    private IEnumerator GetPlayerTotalScore(string userId, System.Action<int> onResult)
    {
        if (string.IsNullOrEmpty(userId))
        {
            onResult?.Invoke(0);
            yield break;
        }

        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.GameProfile(userId), r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var profile = JsonUtility.FromJson<GameProfileResponse>(apiResult.data);
                if (profile != null && profile.gameProfile != null)
                {
                    onResult?.Invoke(profile.gameProfile.totalScore);
                    yield break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[LevelExit] Failed to parse game profile: {ex.Message}");
            }
        }

        onResult?.Invoke(0);
    }

    [System.Serializable]
    private class GameProfileResponse
    {
        public GameProfileData gameProfile;
    }

    [System.Serializable]
    private class GameProfileData
    {
        public int totalScore;
    }
}
