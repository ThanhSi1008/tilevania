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

    private float lastSyncTime = 0f;
    private const float SYNC_INTERVAL = 10f; // Sync every 10 seconds
    private bool isSessionActive = false;
    private bool isStartingSession = false; // Track if session is currently being started
    private DateTime sessionStartTime;
    private string currentLevelId;
    private string lastStartedSceneName = null; // Track which scene we last started a session for
    private string cachedSessionId = null; // Cache sessionId in case SessionManager.Instance becomes null

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
                Debug.Log($"[GameSession] Awake() - Already in gameplay scene, will start session in Start()");
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
            Debug.Log("[GameSession] OnDisable called - ending session as ABANDONED (using helper method)");
            isEndingSession = true;
            EndSessionWhenInactive("ABANDONED");
        }
        else if (isEndingSessionStatic)
        {
            Debug.Log("[GameSession] OnDisable called but session already ending (static flag), skipping");
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
            Debug.Log("[GameSession] OnDestroy - cleaning up temporary GameObject immediately with DestroyImmediate");
            var tempGO = currentTempGO;
            currentTempGO = null;
            isEndingSessionStatic = false;
            
            if (tempGO != null)
            {
                DestroyImmediate(tempGO);
                Debug.Log("[GameSession] ✅ Temporary GameObject destroyed immediately in OnDestroy");
            }
        }
        
        // Don't end session in OnDestroy if already ended in OnDisable
        // OnDisable is called before OnDestroy, so if isEndingSession or isEndingSessionStatic is true, session was already ended
        if (isSessionActive && !isEndingSession && !isEndingSessionStatic)
        {
            if (gameObject != null && gameObject.activeInHierarchy)
            {
                Debug.Log("[GameSession] OnDestroy called - ending session as ABANDONED");
                isEndingSession = true;
                StartCoroutine(OnGameEnd("ABANDONED"));
            }
            else if (gameObject != null)
            {
                // If GameObject is inactive, try to end session using a helper
                Debug.LogWarning("[GameSession] Cannot end session in OnDestroy - GameObject is inactive, trying alternative method");
                isEndingSession = true;
                EndSessionWhenInactive("ABANDONED");
            }
        }
        else
        {
            Debug.Log($"[GameSession] OnDestroy called but session already ending/ended (isEndingSession={isEndingSession}, isEndingSessionStatic={isEndingSessionStatic}), skipping");
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
            Debug.Log("[GameSession] Session is already being ended, skipping duplicate call");
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
            Debug.Log($"[GameSession] Cleaning up temporary GameObject: {tempGO.name}");
            
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
                Debug.Log("[GameSession] ✅ Temporary GameObject destroyed immediately (scene unloading)");
            }
            else if (Application.isPlaying)
            {
                Destroy(tempGO);
                Debug.Log("[GameSession] ✅ Temporary GameObject destroyed after session end");
            }
            else
            {
                DestroyImmediate(tempGO);
                Debug.Log("[GameSession] ✅ Temporary GameObject destroyed immediately (edit mode)");
            }
        }
        else
        {
            Debug.LogWarning("[GameSession] CleanupTemporaryGameObject called with null GameObject");
        }
    }

    // Helper MonoBehaviour to run coroutines when main GameObject is inactive
    private class CoroutineRunner : MonoBehaviour { }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameSession] OnSceneLoaded - Scene: {scene.name}, BuildIndex: {scene.buildIndex}, Mode: {mode}");
        
        // Reset loading flag when new scene is loaded
        LevelExit.IsLoading = false;
        
        // Hide loading overlay when new scene is loaded (with a small delay to ensure scene is fully initialized)
        StartCoroutine(HideLoadingOverlayDelayed());
        
        // Skip invalid scenes (buildIndex < 0 or empty scene name)
        // This can happen when scene is not fully loaded yet
        if (scene.buildIndex < 0 || string.IsNullOrEmpty(scene.name))
        {
            Debug.LogWarning($"[GameSession] Skipping invalid scene in OnSceneLoaded - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
            return;
        }
        
        // Prevent duplicate calls for the same scene
        // Unity may call OnSceneLoaded multiple times, or when scene is already loaded
        if (lastStartedSceneName == scene.name && (isStartingSession || isSessionActive))
        {
            Debug.Log($"[GameSession] OnSceneLoaded called for already processed scene '{scene.name}', skipping");
            return;
        }
        
        StartSessionForScene(scene);
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
            Debug.LogWarning("[GameSession] LoadingOverlay not found in any scene - it may not exist or has a different name");
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
        
        Debug.Log($"[GameSession] Loading overlay '{overlay.name}' hidden (Scene: {overlay.scene.name})");
    }

    void Start()
    {
        livesText.text = playerLives.ToString();
        scoreText.text = score.ToString();
        
        // Also try to start session in Start() in case OnSceneLoaded wasn't called
        // (e.g., if scene was already loaded when GameSession was created)
        if (IsGameplayScene())
        {
            Debug.Log($"[GameSession] Start() called - Scene: {SceneManager.GetActiveScene().name}, BuildIndex: {SceneManager.GetActiveScene().buildIndex}");
            StartSessionForScene(SceneManager.GetActiveScene());
        }
        else
        {
            Debug.Log($"[GameSession] Start() called but not a gameplay scene - Scene: {SceneManager.GetActiveScene().name}");
        }
    }

    private void StartSessionForScene(Scene scene)
    {
        if (!IsGameplaySceneForScene(scene))
        {
            return;
        }

        Debug.Log($"[GameSession] StartSessionForScene - Scene: {scene.name}, BuildIndex: {scene.buildIndex}");
        
        // Don't start a new session if we're already starting one
        // This prevents duplicate session starts when both OnSceneLoaded and Start() are called
        if (isStartingSession)
        {
            Debug.Log("[GameSession] Session is already being started, skipping duplicate start");
            return;
        }
        
        // Reset session state when loading a new gameplay scene
        // This ensures we start fresh for each level
        // Also reset lastStartedSceneName if we're loading a different scene
        if (lastStartedSceneName != null && lastStartedSceneName != scene.name)
        {
            Debug.Log($"[GameSession] Loading different scene - previous: {lastStartedSceneName}, new: {scene.name}");
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
            Debug.LogWarning("[GameSession] Previous session was still active when loading same scene - clearing it");
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
        Debug.Log("[GameSession] OnGameStart() called");
        
        // Mark that we're starting a session
        isStartingSession = true;
        
        // Ensure we start with a clean state
        isSessionActive = false;
        
        if (AuthManager.Instance == null || !AuthManager.Instance.HasToken())
        {
            Debug.LogWarning("[GameSession] ❌ Cannot start session - user not authenticated");
            isStartingSession = false;
            yield break;
        }

        if (SessionManager.Instance == null)
        {
            Debug.LogError("[GameSession] ❌ SessionManager.Instance is null");
            isStartingSession = false;
            yield break;
        }

        // Clear any existing session first
        if (!string.IsNullOrEmpty(SessionManager.Instance.ActiveSessionId))
        {
            Debug.Log($"[GameSession] Clearing previous session: {SessionManager.Instance.ActiveSessionId}");
            SessionManager.Instance.ClearSession();
        }

        sessionStartTime = DateTime.Now;
        lastSyncTime = Time.time;

        // Wait a frame to ensure scene is fully loaded
        yield return null;

        var userId = AuthManager.Instance.CurrentPlayer?.userId;
        var currentScene = SceneManager.GetActiveScene();
        Debug.Log($"[GameSession] Resolving levelId for userId={userId}, scene='{currentScene.name}', buildIndex={currentScene.buildIndex}...");
        
        // Resolve levelId from server definitions
        yield return StartCoroutine(ResolveLevelId(levelId =>
        {
            currentLevelId = levelId;
        }));

        var levelId = currentLevelId;
        Debug.Log($"[GameSession] Resolved levelId={levelId}");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(levelId))
        {
            Debug.LogError($"[GameSession] ❌ Cannot start session - userId={userId}, levelId={levelId}");
            isStartingSession = false;
            yield break;
        }

        // Update currentLevel in GameProfile when starting a level
        if (LevelProgressManager.Instance != null)
        {
            yield return LevelProgressManager.Instance.UpdateCurrentLevel(levelId);
        }

        var payload = new SessionStartRequest
        {
            userId = userId,
            levelId = levelId
        };

        var json = JsonUtility.ToJson(payload);
        APIResponse<string> apiResult = null;

        Debug.Log($"[GameSession] Starting session on server - userId={userId}, levelId={levelId}");
        var startTime = Time.realtimeSinceStartup;
        yield return APIClient.Post(APIConfig.Sessions, json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());
        var elapsed = Time.realtimeSinceStartup - startTime;
        Debug.Log($"[GameSession] Session start request completed in {elapsed:F2}s");

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var response = JsonUtility.FromJson<SessionStartResponse>(apiResult.data);
                if (response != null && response.session != null && !string.IsNullOrEmpty(response.session._id))
                {
                    SessionManager.Instance.SetActiveSession(response.session._id);
                    cachedSessionId = response.session._id; // Cache sessionId in case SessionManager.Instance becomes null
                    isSessionActive = true; // Only set to true after successful session creation
                    isStartingSession = false; // Mark that session start is complete
                    Debug.Log($"[GameSession] ✅ Session started successfully - sessionId={response.session._id}, isSessionActive={isSessionActive}");
                }
                else
                {
                    isStartingSession = false; // Mark that session start failed
                    Debug.LogWarning("[GameSession] ❌ Failed to parse session start response");
                    Debug.LogWarning($"[GameSession] Response data: {apiResult.data}");
                }
            }
            catch (Exception ex)
            {
                isStartingSession = false; // Mark that session start failed
                Debug.LogError($"[GameSession] ❌ Error parsing session start response: {ex.Message}");
                Debug.LogError($"[GameSession] Response data: {apiResult.data}");
            }
        }
        else
        {
            isStartingSession = false; // Mark that session start failed
            Debug.LogWarning($"[GameSession] ❌ Failed to start session - Status: {apiResult?.statusCode}, Error: {apiResult?.error}");
            Debug.LogWarning($"[GameSession] Response data: {apiResult?.data}");
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
            Debug.Log($"[GameSession] Stats synced - Score: {score}, Coins: {coinsCollected}, Deaths: {deathCount}");
        }
        else
        {
            Debug.LogWarning($"[GameSession] Failed to sync stats - Status: {apiResult?.statusCode}");
        }
    }

    public IEnumerator EndSession(string status = "COMPLETED")
    {
        yield return StartCoroutine(OnGameEnd(status));
    }

    private IEnumerator OnGameEnd(string status = "COMPLETED")
    {
        Debug.Log($"[GameSession] OnGameEnd called with status={status}");
        Debug.Log($"[GameSession] isSessionActive={isSessionActive}, isStartingSession={isStartingSession}, hasSessionManager={SessionManager.Instance != null}, sessionId={SessionManager.Instance?.ActiveSessionId}");

        // If session is currently being started, wait a bit for it to complete
        // This handles the case where player completes level very quickly
        if (isStartingSession && !isSessionActive)
        {
            Debug.Log("[GameSession] Session is still starting, waiting up to 2 seconds for it to complete...");
            float waitTime = 0f;
            float maxWaitTime = 2f;
            while (isStartingSession && !isSessionActive && waitTime < maxWaitTime)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }
            
            if (isSessionActive)
            {
                Debug.Log("[GameSession] ✅ Session started while waiting, proceeding with session end");
            }
            else
            {
                Debug.LogWarning("[GameSession] ⚠️ Session did not start in time, proceeding without session end");
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
            
            Debug.Log($"[GameSession] Final stats - Score: {score}, Coins: {coinsCollected}, Enemies: {enemiesDefeated}, Deaths: {deathCount}, Duration: {durationSeconds}s");
            Debug.Log($"[GameSession] Current levelId: {currentLevelId}");

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

            Debug.Log($"[GameSession] Ending session - sessionId={sessionId}, status={status}");
            yield return APIClient.Post(APIConfig.EndSession(sessionId), json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

            if (apiResult != null && apiResult.success)
            {
                Debug.Log("[GameSession] ✅ Session ended successfully on server");
            }
            else
            {
                Debug.LogWarning($"[GameSession] ❌ Failed to end session - Status: {apiResult?.statusCode}, Error: {apiResult?.error}");
            }

            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.ClearSession();
                Debug.Log("[GameSession] Session cleared from SessionManager");
            }
            cachedSessionId = null; // Clear cached sessionId
        }
        else
        {
            Debug.LogWarning("[GameSession] Session was not active or missing sessionId - skipping session end, but will still process level completion and achievements");
        }

        // ALWAYS process level completion and achievements when status is COMPLETED, even if session wasn't active
        // This ensures achievements are checked and notifications shown when player completes a level
        if (status == "COMPLETED")
        {
            Debug.Log("[GameSession] Status is COMPLETED, proceeding with level progress and achievements...");
            
            if (LevelProgressManager.Instance != null && AuthManager.Instance?.CurrentPlayer != null)
            {
                // Resolve levelId if not already set
                if (string.IsNullOrEmpty(currentLevelId))
                {
                    Debug.Log("[GameSession] currentLevelId is empty, resolving...");
                    yield return StartCoroutine(ResolveLevelId(levelId =>
                    {
                        currentLevelId = levelId;
                    }));
                }
                
                Debug.Log($"[GameSession] Calling CompleteLevel - userId={AuthManager.Instance.CurrentPlayer.userId}, levelId={currentLevelId}");
                yield return LevelProgressManager.Instance.CompleteLevel(
                    AuthManager.Instance.CurrentPlayer.userId,
                    currentLevelId,
                    score,
                    coinsCollected,
                    enemiesDefeated,
                    durationSeconds);
                Debug.Log("[GameSession] ✅ CompleteLevel finished");
            }
            else
            {
                Debug.LogWarning($"[GameSession] Cannot complete level - LevelProgressManager={LevelProgressManager.Instance != null}, CurrentPlayer={AuthManager.Instance?.CurrentPlayer != null}");
            }

            // Refresh achievements and show notifications for new unlocks
            if (AchievementManager.Instance != null)
            {
                Debug.Log("[GameSession] Refreshing achievements and checking for new unlocks...");
                yield return AchievementManager.Instance.RefreshUnlocked(true);
                Debug.Log("[GameSession] ✅ Achievement refresh finished");
            }
            else
            {
                Debug.LogWarning("[GameSession] AchievementManager.Instance is null, cannot refresh achievements");
            }
        }
        else
        {
            Debug.Log($"[GameSession] Status is {status}, skipping level progress and achievements");
        }
        
        // Reset ending flag after session end is complete
        isEndingSession = false;
    }

    public void ProcessPlayerDeath()
    {
        deathCount++;
        
        // Sync death immediately
        StartCoroutine(SyncDeathToServer());

        if (playerLives > 1)
        {
            TakeLife();
        }
        else
        {
            StartCoroutine(OnGameEnd("FAILED"));
            ResetGameSession();
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

        if (apiResult != null && apiResult.success)
        {
            Debug.Log($"[GameSession] Death synced - Total deaths: {deathCount}");
        }
    }

    public void AddToScore(int pointsToAdd)
    {
        score += pointsToAdd;
        scoreText.text = score.ToString();
        
        // Sync score change immediately (or queue for batch sync)
        // For now, rely on periodic sync
    }

    public void AddCoin()
    {
        coinsCollected++;
        Debug.Log($"[GameSession] Coin collected - Total: {coinsCollected}");
    }

    public void AddEnemyDefeated()
    {
        enemiesDefeated++;
    }

    void TakeLife()
    {
        playerLives--;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        livesText.text = playerLives.ToString();
    }

    void ResetGameSession()
    {
        StartCoroutine(OnGameEnd("FAILED"));
        FindFirstObjectByType<ScenePersist>().ResetScenePersist();
        SceneManager.LoadScene(0);
        Destroy(gameObject);
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
            Debug.Log($"[GameSession] Waiting for scene to load... (frame {waitCount + 1}/{maxWaitFrames}) - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
            yield return null; // Wait one frame
            scene = SceneManager.GetActiveScene();
            waitCount++;
        }

        // Log early to debug
        Debug.Log($"[GameSession] ResolveLevelId called - scene.name='{scene.name}', buildIndex={scene.buildIndex}");

        // Skip invalid scenes (after waiting)
        if (scene.buildIndex < 0 || string.IsNullOrEmpty(scene.name))
        {
            Debug.LogError($"[GameSession] ⚠️ Cannot resolve levelId for invalid scene after waiting - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
            onResolved?.Invoke(null);
            yield break;
        }

        if (LevelProgressManager.Instance != null)
        {
            yield return LevelProgressManager.Instance.ResolveLevelId(scene, id => resolvedId = id);
        }
        else
        {
            Debug.LogError("[GameSession] ❌ LevelProgressManager.Instance is null!");
            onResolved?.Invoke(null);
            yield break;
        }

        // Don't fallback to buildIndex - server requires valid ObjectId
        // If we can't resolve, we need to wait for levels to be cached or handle the error
        if (string.IsNullOrEmpty(resolvedId))
        {
            Debug.LogWarning($"[GameSession] ❌ Failed to resolve levelId for scene: {scene.name} (buildIndex: {scene.buildIndex})");
            Debug.LogWarning("[GameSession] This usually means levels haven't been cached from server yet. Retrying...");
            
            // Wait a bit longer and retry once more
            yield return new WaitForSeconds(1f);
            
            if (LevelProgressManager.Instance != null)
            {
                yield return LevelProgressManager.Instance.ResolveLevelId(scene, id => resolvedId = id);
            }
            
            if (string.IsNullOrEmpty(resolvedId))
            {
                Debug.LogError("[GameSession] ❌ Still failed to resolve levelId after retry. Cannot start session without valid levelId.");
            }
            else
            {
                Debug.Log($"[GameSession] ✅ Successfully resolved levelId after retry: {resolvedId}");
            }
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
