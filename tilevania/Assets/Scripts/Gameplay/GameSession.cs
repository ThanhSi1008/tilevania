using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameSession : MonoBehaviour
{
    [SerializeField] int playerLives = 3;
    [SerializeField] int score = 0;
    [SerializeField] int coinsCollected = 0;
    [SerializeField] int enemiesDefeated = 0;
    [SerializeField] int deathCount = 0;

    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI levelText;

    private float lastSyncTime = 0f;
    private const float SYNC_INTERVAL = 10f; // Sync every 10 seconds
    private bool isSessionActive = false;
    private bool isStartingSession = false; // Track if session is currently being started
    private DateTime sessionStartTime;
    private string currentLevelId;
    private string lastStartedSceneName = null; // Track which scene we last started a session for
    private string cachedSessionId = null; // Cache sessionId in case SessionManager.Instance becomes null
    [SerializeField] private GameObject gameOverModalPrefab;
    [SerializeField] private GameObject activeGameOverModal;

    void Awake()
    {
        int numberGameSessions = FindObjectsByType<GameSession>(FindObjectsSortMode.None).Length;
        if (numberGameSessions > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            // Subscribe to scene loaded event to start session when scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // If we're already in a gameplay scene when GameSession is created,
            // start session immediately (OnSceneLoaded won't be called for already-loaded scenes)
            if (IsGameplayScene())
            {
                // Debug.Log($"[GameSession] Awake() - Already in gameplay scene, will start session in Start()");
            }
        }
    }

    private bool isEndingSession = false; // Flag to prevent multiple end calls

    void OnDisable()
    {
        // End session when GameObject is disabled (e.g., when pause menu is shown)
        // Note: OnDisable is called when GameObject becomes inactive, so we can't start coroutines directly
        // Use helper method with temporary GameObject instead
        // Only end if we're actually being disabled (not destroyed)
        if (isSessionActive && !isEndingSession && !isEndingSessionStatic && this != null && gameObject != null)
        {
            // Debug.Log("[GameSession] OnDisable called - ending session as ABANDONED (using helper method)");
            isEndingSession = true;
            EndSessionWhenInactive("ABANDONED");
        }
        else if (isEndingSessionStatic)
        {
            // Debug.Log("[GameSession] OnDisable called but session already ending (static flag), skipping");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Always cleanup temporary GameObject in OnDestroy to prevent Unity warning
        // This ensures cleanup happens before Unity checks for leftover objects
        // Use DestroyImmediate since scene is unloading
        if (currentTempGO != null)
        {
            // Debug.Log("[GameSession] OnDestroy - cleaning up temporary GameObject immediately with DestroyImmediate");
            var tempGO = currentTempGO;
            currentTempGO = null;
            isEndingSessionStatic = false;
            
            if (tempGO != null)
            {
                DestroyImmediate(tempGO);
                // Debug.Log("[GameSession] ✅ Temporary GameObject destroyed immediately in OnDestroy");
            }
        }
        
        // Don't end session in OnDestroy if already ended in OnDisable
        // OnDisable is called before OnDestroy, so if isEndingSession or isEndingSessionStatic is true, session was already ended
        if (isSessionActive && !isEndingSession && !isEndingSessionStatic)
        {
            if (gameObject != null && gameObject.activeInHierarchy)
            {
                // Debug.Log("[GameSession] OnDestroy called - ending session as ABANDONED");
                isEndingSession = true;
                StartCoroutine(OnGameEnd("ABANDONED"));
            }
            else if (gameObject != null)
            {
                // If GameObject is inactive, try to end session using a helper
                // Debug.LogWarning("[GameSession] Cannot end session in OnDestroy - GameObject is inactive, trying alternative method");
                isEndingSession = true;
                EndSessionWhenInactive("ABANDONED");
            }
        }
        else
        {
            // Debug.Log($"[GameSession] OnDestroy called but session already ending/ended (isEndingSession={isEndingSession}, isEndingSessionStatic={isEndingSessionStatic}), skipping");
        }
    }
    
    private IEnumerator DelayedCleanup()
    {
        // Wait a bit for coroutine to finish
        yield return new WaitForSeconds(1f);
        CleanupTemporaryGameObjects();
    }

    void OnApplicationQuit()
    {
        // Clean up temporary GameObjects when application quits
        CleanupTemporaryGameObjects();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // Clean up temporary GameObjects when application is paused (e.g., on mobile)
        if (pauseStatus)
        {
            CleanupTemporaryGameObjects();
        }
    }

    private void CleanupTemporaryGameObjects()
    {
        // Clean up tracked temporary GameObject (only use static reference, don't use Find)
        if (currentTempGO != null)
        {
            if (Application.isPlaying)
            {
                Destroy(currentTempGO);
            }
            else
            {
                DestroyImmediate(currentTempGO);
            }
            currentTempGO = null;
            isEndingSessionStatic = false; // Reset flag when cleaning up
        }
    }

    private static GameObject currentTempGO = null; // Track current temporary GameObject
    private static bool isEndingSessionStatic = false; // Static flag to prevent multiple calls across instances

    // Helper method to end session even when GameObject is inactive
    // Uses a temporary GameObject to run the coroutine
    private void EndSessionWhenInactive(string status)
    {
        if (!isSessionActive) return;
        
        // Prevent multiple calls - if already ending, don't create another temporary GameObject
        if (isEndingSessionStatic && currentTempGO != null)
        {
            // Debug.Log("[GameSession] Session is already being ended, skipping duplicate call");
            return;
        }

        // Clean up any existing temporary GameObjects first
        CleanupTemporaryGameObjects();

        // Set static flag to prevent multiple calls
        isEndingSessionStatic = true;

        // Create a temporary GameObject to run the coroutine
        currentTempGO = new GameObject("TempGameSessionEnd");
        DontDestroyOnLoad(currentTempGO); // Prevent it from being destroyed when scene changes
        var runner = currentTempGO.AddComponent<CoroutineRunner>();
        runner.StartCoroutine(EndSessionCoroutine(status, currentTempGO));
    }

    private IEnumerator EndSessionCoroutine(string status, GameObject tempGO)
    {
        try
        {
            // Call OnGameEnd directly (it's a coroutine method)
            yield return OnGameEnd(status);
            
            // Wait a bit to ensure all operations are complete before cleanup
            yield return new WaitForSeconds(0.1f);
        }
        finally
        {
            // Always clean up temporary GameObject after session end is complete (even if error occurs)
            // Note: Cannot use yield in finally block, so cleanup immediately
            // Check if GameObject still exists and is tracked before cleanup (may have been cleaned up in OnDestroy)
            if (tempGO != null && currentTempGO == tempGO)
            {
                CleanupTemporaryGameObject(tempGO);
            }
            else if (tempGO != null)
            {
                // GameObject exists but not tracked (already cleaned up), just destroy it
                if (Application.isPlaying)
                {
                    Destroy(tempGO);
                }
                else
                {
                    DestroyImmediate(tempGO);
                }
            }
        }
    }
    
    private void CleanupTemporaryGameObject(GameObject tempGO)
    {
        if (tempGO != null)
        {
            // Debug.Log($"[GameSession] Cleaning up temporary GameObject: {tempGO.name}");
            
            // Clear static reference first (before destroy) to prevent double cleanup
            bool wasTracked = (currentTempGO == tempGO);
            if (wasTracked)
            {
                currentTempGO = null;
            }
            isEndingSessionStatic = false;
            
            // Always destroy - use DestroyImmediate if scene is unloading or not playing
            // This ensures cleanup happens even when scene is closing
            if (Application.isPlaying && !tempGO.scene.isLoaded)
            {
                // Scene is unloading, use DestroyImmediate
                DestroyImmediate(tempGO);
                // Debug.Log("[GameSession] ✅ Temporary GameObject destroyed immediately (scene unloading)");
            }
            else if (Application.isPlaying)
            {
                Destroy(tempGO);
                // Debug.Log("[GameSession] ✅ Temporary GameObject destroyed after session end");
            }
            else
            {
                DestroyImmediate(tempGO);
                // Debug.Log("[GameSession] ✅ Temporary GameObject destroyed immediately (edit mode)");
            }
        }
        else
        {
            // Debug.LogWarning("[GameSession] CleanupTemporaryGameObject called with null GameObject");
        }
    }

    // Helper MonoBehaviour to run coroutines when main GameObject is inactive
    private class CoroutineRunner : MonoBehaviour { }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Debug.Log($"[GameSession] OnSceneLoaded - Scene: {scene.name}, BuildIndex: {scene.buildIndex}, Mode: {mode}");
        
        // Reset loading flag when new scene is loaded
        LevelExit.IsLoading = false;
        
        // Hide loading overlay when new scene is loaded (with a small delay to ensure scene is fully initialized)
        StartCoroutine(HideLoadingOverlayDelayed());
        
        // Skip invalid scenes (buildIndex < 0 or empty scene name)
        // This can happen when scene is not fully loaded yet
        if (scene.buildIndex < 0 || string.IsNullOrEmpty(scene.name))
        {
            // Debug.LogWarning($"[GameSession] Skipping invalid scene in OnSceneLoaded - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
            return;
        }
        
        // Recover text references when scene loads (they might have been destroyed when returning to main menu)
        if (IsGameplaySceneForScene(scene))
        {
            StartCoroutine(RecoverTextReferencesDelayed());
        }
        
        // Prevent duplicate calls for the same scene
        // Unity may call OnSceneLoaded multiple times, or when scene is already loaded
        if (lastStartedSceneName == scene.name && (isStartingSession || isSessionActive))
        {
            // Debug.Log($"[GameSession] OnSceneLoaded called for already processed scene '{scene.name}', skipping");
            return;
        }
        
        StartSessionForScene(scene);
    }
    
    private IEnumerator RecoverTextReferencesDelayed()
    {
        // Wait a few frames to ensure scene is fully loaded and UI elements are created
        yield return null;
        yield return null;
        yield return null;
        
        // Recover all text references
        EnsureScoreTextReference();
        EnsureLevelTextReference();
        EnsureLivesTextReference();
        
        // Update text values if references were found
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        
        if (livesText != null)
        {
            livesText.text = playerLives.ToString();
        }
        
        // Update level text if we have a current level ID
        if (levelText != null && !string.IsNullOrEmpty(currentLevelId))
        {
            UpdateLevelText(currentLevelId);
        }
    }
    
    private void EnsureLivesTextReference()
    {
        if (livesText != null) return;

        // Try to find by common name in the active scene hierarchy
        var allText = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var t in allText)
        {
            if (t != null && t.name == "Lives Text")
            {
                livesText = t;
                break;
            }
        }
    }
    
    private IEnumerator HideLoadingOverlayDelayed()
    {
        // Wait a few frames to ensure scene is fully loaded and Canvas is initialized
        yield return null;
        yield return null;
        yield return null;
        
        HideLoadingOverlay();
    }
    
    private void HideLoadingOverlay()
    {
        // Find ALL LoadingOverlay objects in ALL loaded scenes and hide them
        // This handles cases where overlay from previous scene might still exist
        bool foundOverlay = false;
        
        // Search through all loaded scenes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            
            var rootObjects = scene.GetRootGameObjects();
            
            foreach (var rootObj in rootObjects)
            {
                // Check if this root object is a Canvas
                var canvas = rootObj.GetComponent<Canvas>();
                if (canvas != null)
                {
                    var overlay = canvas.transform.Find("LoadingOverlay");
                    if (overlay != null)
                    {
                        HideOverlay(overlay.gameObject);
                        foundOverlay = true;
                    }
                }
                
                // Also search recursively for any LoadingOverlay in the scene
                var overlayInScene = FindInChildren(rootObj.transform, "LoadingOverlay");
                if (overlayInScene != null && overlayInScene.name == "LoadingOverlay")
                {
                    HideOverlay(overlayInScene.gameObject);
                    foundOverlay = true;
                }
            }
        }
        
        // Also try FindFirstObjectByType as fallback (searches all scenes)
        if (!foundOverlay)
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                var overlay = canvas.transform.Find("LoadingOverlay");
                if (overlay != null)
                {
                    HideOverlay(overlay.gameObject);
                    foundOverlay = true;
                }
            }
        }
        
        // Also search through all GameObjects with name "LoadingOverlay" as last resort
        if (!foundOverlay)
        {
            var allOverlays = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allOverlays)
            {
                if (obj.name == "LoadingOverlay")
                {
                    HideOverlay(obj);
                    foundOverlay = true;
                }
            }
        }
        
        if (!foundOverlay)
        {
            // Debug.LogWarning("[GameSession] LoadingOverlay not found in any scene - it may not exist or has a different name");
        }
    }
    
    private Transform FindInChildren(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;
            
        foreach (Transform child in parent)
        {
            var found = FindInChildren(child, name);
            if (found != null)
                return found;
        }
        
        return null;
    }
    
    private void HideOverlay(GameObject overlay)
    {
        overlay.SetActive(false);
        
        // Disable raycast blocking
        var canvasGroup = overlay.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        else
        {
            var image = overlay.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }
        }
        
        // Debug.Log($"[GameSession] Loading overlay '{overlay.name}' hidden (Scene: {overlay.scene.name})");
    }

    void Start()
    {
        // Initialize UI with current values (will be updated when lives are loaded from server)
        // For gameplay scenes, ensure lives is at least 3 (default) if not loaded yet
        if (IsGameplayScene() && playerLives <= 0)
        {
            playerLives = 3; // Default to 3 lives for new game session
            // Debug.Log("[GameSession] Start() - Setting default lives to 3 for gameplay scene");
        }
        
        if (livesText != null)
        {
            livesText.text = playerLives.ToString();
            // Debug.Log($"[GameSession] Start() - Initialized livesText to {playerLives}");
        }
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        // Try to find level text reference if not assigned
        if (levelText == null)
        {
            EnsureLevelTextReference();
        }

        // Update level text if we have a current level ID
        if (!string.IsNullOrEmpty(currentLevelId))
        {
            UpdateLevelText(currentLevelId);
        }
        
        // Also try to start session in Start() in case OnSceneLoaded wasn't called
        // (e.g., if scene was already loaded when GameSession was created)
        if (IsGameplayScene())
        {
            // Debug.Log($"[GameSession] Start() called - Scene: {SceneManager.GetActiveScene().name}, BuildIndex: {SceneManager.GetActiveScene().buildIndex}");
            StartSessionForScene(SceneManager.GetActiveScene());
        }
        else
        {
            // Debug.Log($"[GameSession] Start() called but not a gameplay scene - Scene: {SceneManager.GetActiveScene().name}");
        }
    }

    private void StartSessionForScene(Scene scene)
    {
        if (!IsGameplaySceneForScene(scene))
        {
            return;
        }

        // Debug.Log($"[GameSession] StartSessionForScene - Scene: {scene.name}, BuildIndex: {scene.buildIndex}");
        
        // Don't start a new session if we're already starting one
        // This prevents duplicate session starts when both OnSceneLoaded and Start() are called
        if (isStartingSession)
        {
            // Debug.Log("[GameSession] Session is already being started, skipping duplicate start");
            return;
        }
        
        // Reset session state when loading a new gameplay scene
        // This ensures we start fresh for each level
        // Also reset lastStartedSceneName if we're loading a different scene
        if (lastStartedSceneName != null && lastStartedSceneName != scene.name)
        {
            // Debug.Log($"[GameSession] Loading different scene - previous: {lastStartedSceneName}, new: {scene.name}");
            // Reset flags when switching to a different scene
            isStartingSession = false;
            isSessionActive = false;
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.ClearSession();
            }
        }
        else if (isSessionActive && lastStartedSceneName == scene.name)
        {
            // Debug.LogWarning("[GameSession] Previous session was still active when loading same scene - clearing it");
            isSessionActive = false;
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.ClearSession();
            }
        }
        
        // Reset stats for new level (keep lives and score across levels)
        // coinsCollected, enemiesDefeated, deathCount will be reset per level
        coinsCollected = 0;
        enemiesDefeated = 0;
        // Note: We keep score and lives across levels
        
        // Track which scene we're starting session for
        lastStartedSceneName = scene.name;
        
        StartCoroutine(OnGameStart());
    }

    private bool IsGameplaySceneForScene(Scene scene)
    {
        var sceneName = scene.name;
        
        // Check by scene name first (more reliable)
        // Level scenes are named "Level 1", "Level 2", "Level 3", etc.
        if (sceneName.StartsWith("Level", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // If scene name contains "Auth" or "Menu", it's not a gameplay scene
        if (sceneName.Contains("Auth", StringComparison.OrdinalIgnoreCase) || 
            sceneName.Contains("Menu", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        
        // Otherwise, if it's not AuthScene/MainMenu, assume it's a gameplay scene
        // This handles cases where Level 1 might be at index 0
        return true;
    }

    void Update()
    {
        // Periodic sync every SYNC_INTERVAL seconds
        if (isSessionActive && Time.time - lastSyncTime >= SYNC_INTERVAL)
        {
            lastSyncTime = Time.time;
            StartCoroutine(SyncStatsToServer());
        }
    }

    private bool IsGameplayScene()
    {
        // Check if current scene is a gameplay scene (not AuthScene or MainMenu)
        var scene = SceneManager.GetActiveScene();
        var sceneName = scene.name;
        
        // Check by scene name first (more reliable)
        // Level scenes are named "Level 1", "Level 2", "Level 3", etc.
        if (sceneName.StartsWith("Level", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Fallback: check by buildIndex (if AuthScene is at index 0)
        // But if Level 1 is at index 0, we need to check by name instead
        int sceneIndex = scene.buildIndex;
        
        // If scene name contains "Auth" or "Menu", it's not a gameplay scene
        if (sceneName.Contains("Auth", StringComparison.OrdinalIgnoreCase) || 
            sceneName.Contains("Menu", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        
        // Otherwise, if it's not AuthScene/MainMenu, assume it's a gameplay scene
        // This handles cases where Level 1 might be at index 0
        return true;
    }

    private IEnumerator OnGameStart()
    {
        // Debug.Log("[GameSession] OnGameStart() called");
        
        // Mark that we're starting a session
        isStartingSession = true;
        
        // Ensure we start with a clean state
        isSessionActive = false;

        // Reactivate HUD objects that were hidden when returning to MainMenu
        ScenePersist.ReactivateHud();
        
        if (AuthManager.Instance == null || !AuthManager.Instance.HasToken())
        {
            // Debug.LogWarning("[GameSession] ❌ Cannot start session - user not authenticated");
            isStartingSession = false;
            yield break;
        }

        if (SessionManager.Instance == null)
        {
            // Debug.LogError("[GameSession] ❌ SessionManager.Instance is null");
            isStartingSession = false;
            yield break;
        }

        // Clear any existing session first
        if (!string.IsNullOrEmpty(SessionManager.Instance.ActiveSessionId))
        {
            // Debug.Log($"[GameSession] Clearing previous session: {SessionManager.Instance.ActiveSessionId}");
            SessionManager.Instance.ClearSession();
        }

        sessionStartTime = DateTime.Now;
        lastSyncTime = Time.time;

        // Wait a frame to ensure scene is fully loaded
        yield return null;

        var userId = AuthManager.Instance.CurrentPlayer?.userId;
        var currentScene = SceneManager.GetActiveScene();
        // Debug.Log($"[GameSession] Resolving levelId for userId={userId}, scene='{currentScene.name}', buildIndex={currentScene.buildIndex}...");
        
        // Resolve levelId from server definitions
        yield return StartCoroutine(ResolveLevelId(levelId =>
        {
            currentLevelId = levelId;
        }));

        var levelId = currentLevelId;
        // Debug.Log($"[GameSession] Resolved levelId={levelId}");

        // Ensure level text reference is found before updating
        EnsureLevelTextReference();
        
        // Update level text with the current level name
        if (!string.IsNullOrEmpty(levelId))
        {
            UpdateLevelText(levelId);
        }
        else
        {
            Debug.LogWarning("[GameSession] levelId is empty, cannot update level text");
        }

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(levelId))
        {
            // Debug.LogError($"[GameSession] ❌ Cannot start session - userId={userId}, levelId={levelId}");
            isStartingSession = false;
            yield break;
        }

        // Update currentLevel in GameProfile when starting a level
        Debug.Log($"[GameSession] OnGameStart: About to update currentLevel - levelId={levelId}, scene={SceneManager.GetActiveScene().name}");
        if (LevelProgressManager.Instance != null)
        {
            // Debug.Log($"[GameSession] OnGameStart: Updating currentLevel to levelId={levelId} before starting session");
            bool updateSuccess = false;
            yield return LevelProgressManager.Instance.UpdateCurrentLevel(levelId, success => updateSuccess = success);
            if (updateSuccess)
            {
                Debug.Log($"[GameSession] ✅ OnGameStart: Successfully updated currentLevel to {levelId}");
            }
            else
            {
                Debug.LogWarning($"[GameSession] ❌ OnGameStart: Failed to update currentLevel to {levelId}");
            }
        }
        else
        {
            Debug.LogWarning("[GameSession] OnGameStart: LevelProgressManager.Instance is null, cannot update currentLevel");
        }

        // Load currentLives from server BEFORE starting session
        // This ensures lives are loaded and displayed before player can interact
        yield return StartCoroutine(LoadLivesFromServer());
        
        // Ensure livesText is updated after loading (in case it wasn't updated in LoadLivesFromServer)
        if (livesText != null)
        {
            livesText.text = playerLives.ToString();
            // Debug.Log($"[GameSession] OnGameStart: Updated livesText to {playerLives} after loading from server");
        }

        var payload = new SessionStartRequest
        {
            userId = userId,
            levelId = levelId
        };

        var json = JsonUtility.ToJson(payload);
        APIResponse<string> apiResult = null;

        // Debug.Log($"[GameSession] Starting session on server - userId={userId}, levelId={levelId}");
        var startTime = Time.realtimeSinceStartup;
        yield return APIClient.Post(APIConfig.Sessions, json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());
        var elapsed = Time.realtimeSinceStartup - startTime;
        // Debug.Log($"[GameSession] Session start request completed in {elapsed:F2}s");

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            SessionStartResponse response = null;
            bool parseSuccess = false;
            
            try
            {
                response = JsonUtility.FromJson<SessionStartResponse>(apiResult.data);
                parseSuccess = true;
            }
            catch (Exception)
            {
                isStartingSession = false; // Mark that session start failed
                // Debug.LogError($"[GameSession] ❌ Error parsing session start response");
                // Debug.LogError($"[GameSession] Response data: {apiResult.data}");
            }
            
            if (parseSuccess && response != null && response.session != null && !string.IsNullOrEmpty(response.session._id))
            {
                SessionManager.Instance.SetActiveSession(response.session._id);
                cachedSessionId = response.session._id; // Cache sessionId in case SessionManager.Instance becomes null
                isSessionActive = true; // Only set to true after successful session creation
                isStartingSession = false; // Mark that session start is complete
                // Debug.Log($"[GameSession] ✅ Session started successfully - sessionId={response.session._id}, isSessionActive={isSessionActive}");
                
                // Ensure level text is updated after session starts (text might have been recreated)
                yield return null; // Wait a frame for UI to be ready
                EnsureLevelTextReference();
                if (!string.IsNullOrEmpty(currentLevelId))
                {
                    UpdateLevelText(currentLevelId);
                }
            }
            else if (parseSuccess)
            {
                isStartingSession = false; // Mark that session start failed
                // Debug.LogWarning("[GameSession] ❌ Failed to parse session start response");
                // Debug.LogWarning($"[GameSession] Response data: {apiResult.data}");
            }
        }
        else
        {
            isStartingSession = false; // Mark that session start failed
            // Debug.LogWarning($"[GameSession] ❌ Failed to start session - Status: {apiResult?.statusCode}, Error: {apiResult?.error}");
            // Debug.LogWarning($"[GameSession] Response data: {apiResult?.data}");
        }
    }

    private IEnumerator SyncStatsToServer()
    {
        if (!isSessionActive || SessionManager.Instance == null || string.IsNullOrEmpty(SessionManager.Instance.ActiveSessionId))
        {
            yield break;
        }

        var sessionId = SessionManager.Instance.ActiveSessionId;
        var payload = new SessionUpdateRequest
        {
            finalScore = score,
            coinsCollected = coinsCollected,
            enemiesDefeated = enemiesDefeated,
            deathCount = deathCount,
            livesRemaining = playerLives
        };

        var json = JsonUtility.ToJson(payload);
        APIResponse<string> apiResult = null;

        yield return APIClient.Put(APIConfig.Session(sessionId), json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success)
        {
            // Debug.Log($"[GameSession] Stats synced - Score: {score}, Coins: {coinsCollected}, Deaths: {deathCount}");
        }
        else
        {
            // Debug.LogWarning($"[GameSession] Failed to sync stats - Status: {apiResult?.statusCode}");
        }
    }

    public IEnumerator EndSession(string status = "COMPLETED")
    {
        yield return StartCoroutine(OnGameEnd(status));
    }

    private IEnumerator OnGameEnd(string status = "COMPLETED")
    {
        // Debug.Log($"[GameSession] OnGameEnd called with status={status}");
        // Debug.Log($"[GameSession] isSessionActive={isSessionActive}, isStartingSession={isStartingSession}, hasSessionManager={SessionManager.Instance != null}, sessionId={SessionManager.Instance?.ActiveSessionId}");

        // If session is currently being started, wait a bit for it to complete
        // This handles the case where player completes level very quickly
        if (isStartingSession && !isSessionActive)
        {
            // Debug.Log("[GameSession] Session is still starting, waiting up to 2 seconds for it to complete...");
            float waitTime = 0f;
            float maxWaitTime = 2f;
            while (isStartingSession && !isSessionActive && waitTime < maxWaitTime)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }
            
            if (isSessionActive)
            {
                // Debug.Log("[GameSession] ✅ Session started while waiting, proceeding with session end");
            }
            else
            {
                // Debug.LogWarning("[GameSession] ⚠️ Session did not start in time, proceeding without session end");
            }
        }

        bool sessionWasActive = isSessionActive;
        // Use cached sessionId if SessionManager.Instance is null (can happen when GameObject is disabled)
        var sessionId = SessionManager.Instance?.ActiveSessionId ?? cachedSessionId;
        var durationSeconds = sessionWasActive ? Mathf.Max(0, (int)(DateTime.Now - sessionStartTime).TotalSeconds) : 0;

        // Try to end session on server if it was active
        if (sessionWasActive && !string.IsNullOrEmpty(sessionId))
        {
            isSessionActive = false;
            
            // Debug.Log($"[GameSession] Final stats - Score: {score}, Coins: {coinsCollected}, Enemies: {enemiesDefeated}, Deaths: {deathCount}, Duration: {durationSeconds}s");
            // Debug.Log($"[GameSession] Current levelId: {currentLevelId}");

            var payload = new SessionEndRequest
            {
                status = status,
                finalScore = score,
                coinsCollected = coinsCollected,
                enemiesDefeated = enemiesDefeated,
                deathCount = deathCount,
                livesRemaining = playerLives
            };

            var json = JsonUtility.ToJson(payload);
            APIResponse<string> apiResult = null;

            // Debug.Log($"[GameSession] Ending session - sessionId={sessionId}, status={status}");
            yield return APIClient.Post(APIConfig.EndSession(sessionId), json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

            if (apiResult != null && apiResult.success)
            {
                // Debug.Log("[GameSession] ✅ Session ended successfully on server");
            }
            else
            {
                // Debug.LogWarning($"[GameSession] ❌ Failed to end session - Status: {apiResult?.statusCode}, Error: {apiResult?.error}");
            }

            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.ClearSession();
                // Debug.Log("[GameSession] Session cleared from SessionManager");
            }
            cachedSessionId = null; // Clear cached sessionId
        }
        else
        {
            // Debug.LogWarning("[GameSession] Session was not active or missing sessionId - skipping session end, but will still process level completion and achievements");
        }

        // ALWAYS process level completion and achievements when status is COMPLETED, even if session wasn't active
        // This ensures achievements are checked and notifications shown when player completes a level
        if (status == "COMPLETED")
        {
            
            if (LevelProgressManager.Instance != null && AuthManager.Instance?.CurrentPlayer != null)
            {
                // Resolve levelId if not already set
                if (string.IsNullOrEmpty(currentLevelId))
                {
                    // Debug.Log("[GameSession] currentLevelId is empty, resolving...");
                    yield return StartCoroutine(ResolveLevelId(levelId =>
                    {
                        currentLevelId = levelId;
                    }));
                }
                
                // Debug.Log($"[GameSession] Calling CompleteLevel - userId={AuthManager.Instance.CurrentPlayer.userId}, levelId={currentLevelId}");
                yield return LevelProgressManager.Instance.CompleteLevel(
                    AuthManager.Instance.CurrentPlayer.userId,
                    currentLevelId,
                    score,
                    coinsCollected,
                    enemiesDefeated,
                    durationSeconds);
                // Debug.Log("[GameSession] ✅ CompleteLevel finished");
            }
            else
            {
                // Debug.LogWarning($"[GameSession] Cannot complete level - LevelProgressManager={LevelProgressManager.Instance != null}, CurrentPlayer={AuthManager.Instance?.CurrentPlayer != null}");
            }

            // Refresh achievements and show notifications for new unlocks
            if (AchievementManager.Instance != null)
            {
                // Debug.Log("[GameSession] Refreshing achievements and checking for new unlocks...");
                yield return AchievementManager.Instance.RefreshUnlocked(true);
                // Debug.Log("[GameSession] ✅ Achievement refresh finished");
            }
            else
            {
                // Debug.LogWarning("[GameSession] AchievementManager.Instance is null, cannot refresh achievements");
            }
        }
        else
        {
            // Debug.Log($"[GameSession] Status is {status}, skipping level progress and achievements");
        }
        
        // Reset ending flag after session end is complete
        isEndingSession = false;
    }

    public void ProcessPlayerDeath()
    {
        deathCount++;
        
        // Play death sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeath();
        }
        
        // Sync death immediately
        StartCoroutine(SyncDeathToServer());

        if (playerLives > 1)
        {
            // Update currentLevel to the level where player died before taking life
            StartCoroutine(ProcessDeathWithLevelUpdate(true));
        }
        else
        {
            // Update currentLevel to the level where player died before resetting
            StartCoroutine(ProcessDeathWithLevelUpdate(false));
        }
    }
    
    private IEnumerator ProcessDeathWithLevelUpdate(bool shouldTakeLife)
    {
        if (shouldTakeLife)
        {
            // Player still has lives left
            yield return StartCoroutine(UpdateCurrentLevelOnDeath());
            TakeLife();
        }
        else
        {
            Debug.Log("[GameSession] Game Over! Showing modal.");

            // Play game over sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOver();
            }

            // ✅ Spawn the GameOver modal if not already active
            if (activeGameOverModal == null && gameOverModalPrefab != null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    activeGameOverModal = Instantiate(gameOverModalPrefab, canvas.transform);
                    Debug.Log("[GameSession] Spawned GameOver modal under canvas");

                    // Hook up the Restart button
                    var restartButton = activeGameOverModal.GetComponentInChildren<UnityEngine.UI.Button>();
                    if (restartButton != null)
                    {
                        restartButton.onClick.AddListener(() =>
                        {
                            Debug.Log("[GameSession] Restart button clicked — resetting game session.");
                            StartCoroutine(HandleRestartButton());
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[GameSession] GameOverModal prefab has no Button component in children!");
                    }
                }
                else
                {
                    Debug.LogWarning("[GameSession] No Canvas found — cannot show GameOver modal");
                }
            }
            else if (gameOverModalPrefab == null)
            {
                Debug.LogWarning("[GameSession] GameOverModal prefab not assigned!");
            }
        }
    }

    private IEnumerator HandleRestartButton()
    {
        // Optionally, disable the button so user can't spam it
        if (activeGameOverModal != null)
        {
            var button = activeGameOverModal.GetComponentInChildren<UnityEngine.UI.Button>();
            if (button != null)
                button.interactable = false;
        }

        // Hide modal
        if (activeGameOverModal != null)
        {
            Destroy(activeGameOverModal);
            activeGameOverModal = null;
        }

        // ✅ Now perform reset
        yield return StartCoroutine(ResetGameSession());
    }
    
    private IEnumerator UpdateCurrentLevelOnDeath()
    {
        // Update currentLevel to the level where player died
            if (LevelProgressManager.Instance != null && AuthManager.Instance?.CurrentPlayer != null && !string.IsNullOrEmpty(currentLevelId))
            {
            bool updateSuccess = false;
            yield return LevelProgressManager.Instance.UpdateCurrentLevel(currentLevelId, success => updateSuccess = success);
                if (updateSuccess)
                {
                }
                else
                {
                }
            }
            else
            {
        }
    }

    private IEnumerator SyncDeathToServer()
    {
        if (AuthManager.Instance?.CurrentPlayer == null) yield break;

        var userId = AuthManager.Instance.CurrentPlayer.userId;
        if (string.IsNullOrEmpty(userId)) yield break;

        var payload = "{}"; // Empty body, server increments death count
        APIResponse<string> apiResult = null;

        yield return APIClient.Post(APIConfig.GameProfileDeath(userId), payload, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());
    }

    public void AddToScore(int pointsToAdd)
    {
        score += pointsToAdd;
        EnsureScoreTextReference();
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        
        // Sync score change immediately (or queue for batch sync)
        // For now, rely on periodic sync
    }

    public void AddCoin()
    {
        coinsCollected++;
        // Debug.Log($"[GameSession] Coin collected - Total: {coinsCollected}");
    }

    public void AddEnemyDefeated()
    {
        enemiesDefeated++;
    }

    /// <summary>
    /// Try to recover the Score Text reference if it was destroyed (e.g., when returning to main menu and back).
    /// </summary>
    private void EnsureScoreTextReference()
    {
        if (scoreText != null) return;

        // Try to find by common name in the active scene hierarchy
        var allText = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var t in allText)
        {
            if (t != null && t.name == "Score Text")
            {
                scoreText = t;
                break;
            }
        }
    }

    /// <summary>
    /// Try to recover the Level Text reference if it was destroyed (e.g., when returning to main menu and back).
    /// </summary>
    private void EnsureLevelTextReference()
    {
        if (levelText != null) return;

        // Try to find by common name in the active scene hierarchy
        var allText = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var t in allText)
        {
            if (t != null && t.name == "Level Text")
            {
                levelText = t;
                break;
            }
        }
    }

    private void UpdateLevelText(string levelId)
    {
        // Try to find level text if not already found
        if (levelText == null)
        {
            EnsureLevelTextReference();
        }
        
        if (levelText == null)
        {
            Debug.LogWarning("[GameSession] Level Text not found! Make sure 'Level Text' GameObject exists in the scene.");
            return;
        }

        if (string.IsNullOrEmpty(levelId))
        {
            // Try to get level from current scene name as fallback
            var scene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(scene.name))
            {
                levelText.text = scene.name;
                Debug.Log($"[GameSession] Level Text set to scene name: {scene.name} (no levelId)");
            }
            else
            {
                levelText.text = "Level";
            }
            return;
        }

        // Get level data from LevelProgressManager
        if (LevelProgressManager.Instance != null)
        {
            var levelData = LevelProgressManager.Instance.GetLevelData(levelId);
            if (levelData != null && !string.IsNullOrEmpty(levelData.levelName))
            {
                levelText.text = levelData.levelName;
                Debug.Log($"[GameSession] Level Text updated to: {levelData.levelName} (levelId: {levelId})");
            }
            else
            {
                // Fallback to scene name
                var scene = SceneManager.GetActiveScene();
                levelText.text = !string.IsNullOrEmpty(scene.name) ? scene.name : "Level";
                Debug.LogWarning($"[GameSession] Level data not found for levelId: {levelId}, using scene name: {scene.name}");
            }
        }
        else
        {
            // Fallback to scene name if LevelProgressManager not available
            var scene = SceneManager.GetActiveScene();
            levelText.text = !string.IsNullOrEmpty(scene.name) ? scene.name : "Level";
            Debug.LogWarning("[GameSession] LevelProgressManager.Instance is null, using scene name as fallback");
        }
    }

        void TakeLife()
        {
        playerLives--;
        if (livesText != null)
        {
            livesText.text = playerLives.ToString();
        }
        
        // Update lives on server
        StartCoroutine(UpdateLivesOnServer(playerLives));
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private IEnumerator ResetGameSession()
    {
        // Reset local lives to 3 first (before ending session to avoid sending wrong livesRemaining)
        playerLives = 3;
        if (livesText != null)
        {
            livesText.text = playerLives.ToString();
        }
        Debug.Log("[GameSession] ResetGameSession: Reset local lives to 3");
        
        // End session first (will send livesRemaining = 3)
        yield return StartCoroutine(OnGameEnd("FAILED"));
        
        // Reset lives to 3 on server (ensure server has correct value)
        yield return StartCoroutine(UpdateLivesOnServer(3));
        
        // Reset currentLevel to Level 1 when gameover
        // Player should start from Level 1 when they restart after gameover
        Debug.Log($"[GameSession] ResetGameSession: Resetting currentLevel to Level 1");
        if (LevelProgressManager.Instance != null && AuthManager.Instance?.CurrentPlayer != null)
        {
            // Get Level 1 data
            yield return LevelProgressManager.Instance.EnsureLevelsCached();
            var level1Data = LevelProgressManager.Instance.GetLevelDataByNumber(1);
            
            if (level1Data != null && !string.IsNullOrEmpty(level1Data._id))
            {
                // Debug.Log($"[GameSession] ResetGameSession: Updating currentLevel to Level 1 (levelId={level1Data._id})");
                bool updateSuccess = false;
                yield return LevelProgressManager.Instance.UpdateCurrentLevel(level1Data._id, success => updateSuccess = success);
                if (updateSuccess)
                {
                    Debug.Log($"[GameSession] ✅ ResetGameSession: Successfully updated currentLevel to Level 1");
                }
                else
                {
                    Debug.LogWarning($"[GameSession] ❌ ResetGameSession: Failed to update currentLevel to Level 1");
                }
            }
            else
            {
                Debug.LogWarning("[GameSession] ResetGameSession: Level 1 data not found, cannot update currentLevel");
            }
        }
        else
        {
            Debug.LogWarning($"[GameSession] ResetGameSession: Cannot update currentLevel - LevelProgressManager={LevelProgressManager.Instance != null}, AuthManager={AuthManager.Instance?.CurrentPlayer != null}");
        }
        
        // Reset scene persist
        var scenePersist = FindFirstObjectByType<ScenePersist>();
        if (scenePersist != null)
        {
            scenePersist.ResetScenePersist();
        }
        
        // Reset session state but DON'T destroy GameSession
        // GameSession should persist across scenes to handle deaths properly
        // IMPORTANT: Don't reset currentLevelId here - it was already updated in UpdateCurrentLevelOnDeath()
        // currentLevelId should remain so player can continue from where they died
        Debug.Log($"[GameSession] ResetGameSession: Resetting session state but keeping currentLevelId={currentLevelId}");
        isSessionActive = false;
        isStartingSession = false;
        lastStartedSceneName = null;
        // currentLevelId is NOT reset here - it should persist so player continues from death level
        cachedSessionId = null;
        score = 0;
        coinsCollected = 0;
        enemiesDefeated = 0;
        deathCount = 0;
        
        // Clear session from SessionManager
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.ClearSession();
        }
        
        // Load MainMenu (scene 0)
        Debug.Log("[GameSession] ResetGameSession: Loading MainMenu (scene 0)");
        SceneManager.LoadScene(0);
        
        // Don't destroy GameSession - it will be reused when entering a new level
        Debug.Log("[GameSession] ResetGameSession: GameSession preserved for next game session");
    }


    private IEnumerator LoadLivesFromServer()
    {
        if (AuthManager.Instance?.CurrentPlayer == null)
        {
            // Debug.LogWarning("[GameSession] LoadLivesFromServer: Not authenticated, using default lives (3)");
            playerLives = 3;
            livesText.text = playerLives.ToString();
            yield break;
        }

        var userId = AuthManager.Instance.CurrentPlayer.userId;
        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.GameProfile(userId), r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            bool needsReset = false;
            try
            {
                var profile = JsonUtility.FromJson<GameProfileResponse>(apiResult.data);
                if (profile != null && profile.gameProfile != null)
                {
                    int serverLives = profile.gameProfile.currentLives;
                    if (serverLives > 0)
                    {
                        playerLives = serverLives;
                        // Debug.Log($"[GameSession] LoadLivesFromServer: Loaded lives from server: {playerLives}");
                    }
                    else
                    {
                        // If server has 0 or negative lives, reset to 3
                        // This can happen after gameover if lives weren't properly reset
                        playerLives = 3;
                        // Debug.LogWarning($"[GameSession] LoadLivesFromServer: Server has invalid lives ({serverLives}), resetting to 3");
                        needsReset = true;
                    }
                }
            }
            catch (Exception)
            {
                // Debug.LogError($"[GameSession] LoadLivesFromServer: Failed to parse game profile");
                playerLives = 3; // Default fallback
            }
            
            // Update lives on server if needed (moved outside try-catch to avoid CS1626)
            if (needsReset)
            {
                yield return StartCoroutine(UpdateLivesOnServer(3));
            }
        }
        else
        {
            // Debug.LogWarning("[GameSession] LoadLivesFromServer: Failed to load lives from server, using default (3)");
            playerLives = 3;
        }

        // Ensure lives is at least 1 (safety check)
        if (playerLives < 1)
        {
            // Debug.LogWarning($"[GameSession] LoadLivesFromServer: Lives is invalid ({playerLives}), resetting to 3");
            playerLives = 3;
            yield return StartCoroutine(UpdateLivesOnServer(3));
        }

        // Always update livesText after loading (ensure it's displayed)
        if (livesText != null)
        {
            livesText.text = playerLives.ToString();
            // Debug.Log($"[GameSession] LoadLivesFromServer: Updated livesText to {playerLives}");
        }
        else
        {
            // Debug.LogWarning("[GameSession] LoadLivesFromServer: livesText is null, cannot update UI");
        }
    }

    private IEnumerator UpdateLivesOnServer(int lives)
    {
        if (AuthManager.Instance?.CurrentPlayer == null)
        {
            // Debug.LogWarning("[GameSession] UpdateLivesOnServer: Not authenticated, cannot update lives");
            yield break;
        }

        var userId = AuthManager.Instance.CurrentPlayer.userId;
        var updateData = new UpdateLivesRequest { lives = lives };
        var json = JsonUtility.ToJson(updateData);
        // Debug.Log($"[GameSession] UpdateLivesOnServer: Sending request - lives={lives}, json={json}");

        APIResponse<string> apiResult = null;
        yield return APIClient.Put(APIConfig.GameProfileLives(userId), json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success)
        {
            // Debug.Log($"[GameSession] ✅ UpdateLivesOnServer: Successfully updated lives to {lives}");
        }
        else
        {
            // Debug.LogWarning($"[GameSession] ❌ UpdateLivesOnServer: Failed to update lives - success={apiResult?.success}, statusCode={apiResult?.statusCode}, error={apiResult?.error}");
            if (apiResult != null && !string.IsNullOrEmpty(apiResult.data))
            {
                // Debug.LogWarning($"[GameSession] UpdateLivesOnServer: Response body: {apiResult.data}");
            }
        }
    }

    [Serializable]
    private class UpdateLivesRequest
    {
        public int lives;
    }

    [Serializable]
    private class GameProfileResponse
    {
        public GameProfileData gameProfile;
    }

    [Serializable]
    private class GameProfileData
    {
        public int currentLives;
    }

    private IEnumerator ResolveLevelId(Action<string> onResolved)
    {
        string resolvedId = null;
        
        // Wait for scene to be fully loaded
        // Scene might not be ready immediately after SceneManager.LoadScene()
        var scene = SceneManager.GetActiveScene();
        int waitCount = 0;
        const int maxWaitFrames = 10; // Wait up to 10 frames for scene to load
        
        while ((scene.buildIndex < 0 || string.IsNullOrEmpty(scene.name)) && waitCount < maxWaitFrames)
        {
            // Debug.Log($"[GameSession] Waiting for scene to load... (frame {waitCount + 1}/{maxWaitFrames}) - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
            yield return null; // Wait one frame
            scene = SceneManager.GetActiveScene();
            waitCount++;
        }

        // Skip invalid scenes (after waiting)
        if (scene.buildIndex < 0 || string.IsNullOrEmpty(scene.name))
        {
            Debug.LogError($"[GameSession] ⚠️ ResolveLevelId: Cannot resolve levelId for invalid scene after waiting - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
            onResolved?.Invoke(null);
            yield break;
        }

        if (LevelProgressManager.Instance != null)
        {
            yield return LevelProgressManager.Instance.ResolveLevelId(scene, id => resolvedId = id);
            Debug.Log($"[GameSession] ResolveLevelId: First attempt - resolvedId={resolvedId}");
        }
        else
        {
            Debug.LogError("[GameSession] ❌ ResolveLevelId: LevelProgressManager.Instance is null!");
            onResolved?.Invoke(null);
            yield break;
        }

        // Don't fallback to buildIndex - server requires valid ObjectId
        // If we can't resolve, we need to wait for levels to be cached or handle the error
        if (string.IsNullOrEmpty(resolvedId))
        {
            Debug.LogWarning($"[GameSession] ❌ ResolveLevelId: Failed to resolve levelId for scene: {scene.name} (buildIndex: {scene.buildIndex})");
            Debug.LogWarning("[GameSession] ResolveLevelId: This usually means levels haven't been cached from server yet. Retrying...");
            
            // Wait a bit longer and retry once more
            yield return new WaitForSeconds(1f);
            
            if (LevelProgressManager.Instance != null)
            {
                yield return LevelProgressManager.Instance.ResolveLevelId(scene, id => resolvedId = id);
                Debug.Log($"[GameSession] ResolveLevelId: Retry attempt - resolvedId={resolvedId}");
            }
            
            if (string.IsNullOrEmpty(resolvedId))
            {
                Debug.LogError("[GameSession] ❌ ResolveLevelId: Still failed to resolve levelId after retry. Cannot start session without valid levelId.");
            }
            else
            {
                Debug.Log($"[GameSession] ✅ ResolveLevelId: Successfully resolved levelId after retry: {resolvedId}");
            }
        }
        else
        {
            Debug.Log($"[GameSession] ✅ ResolveLevelId: Successfully resolved levelId: {resolvedId}");
        }

        onResolved?.Invoke(resolvedId);
    }

    [Serializable]
    private class SessionStartRequest
    {
        public string userId;
        public string levelId;
    }

    [Serializable]
    private class SessionUpdateRequest
    {
        public int finalScore;
        public int coinsCollected;
        public int enemiesDefeated;
        public int deathCount;
        public int livesRemaining;
    }

    [Serializable]
    private class SessionEndRequest
    {
        public string status;
        public int finalScore;
        public int coinsCollected;
        public int enemiesDefeated;
        public int deathCount;
        public int livesRemaining;
    }

    [Serializable]
    private class SessionStartResponse
    {
        public SessionData session;
    }

    [Serializable]
    private class SessionData
    {
        public string _id;
        public string userId;
        public string levelId;
    }
}

