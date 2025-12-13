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
    private DateTime sessionStartTime;
    private string currentLevelId;

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
        }
    }

    void Start()
    {
        livesText.text = playerLives.ToString();
        scoreText.text = score.ToString();
        
        // Start session when gameplay scene loads
        if (IsGameplayScene())
        {
            Debug.Log($"[GameSession] Start() called - Scene: {SceneManager.GetActiveScene().name}, BuildIndex: {SceneManager.GetActiveScene().buildIndex}");
            StartCoroutine(OnGameStart());
        }
        else
        {
            Debug.Log($"[GameSession] Start() called but not a gameplay scene - Scene: {SceneManager.GetActiveScene().name}");
        }
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
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        return sceneIndex > 0; // Assuming scene 0 is AuthScene/MainMenu
    }

    private IEnumerator OnGameStart()
    {
        Debug.Log("[GameSession] OnGameStart() called");
        
        if (AuthManager.Instance == null || !AuthManager.Instance.HasToken())
        {
            Debug.LogWarning("[GameSession] ❌ Cannot start session - user not authenticated");
            isSessionActive = false;
            yield break;
        }

        if (SessionManager.Instance == null)
        {
            Debug.LogError("[GameSession] ❌ SessionManager.Instance is null");
            isSessionActive = false;
            yield break;
        }

        sessionStartTime = DateTime.Now;
        lastSyncTime = Time.time;

        var userId = AuthManager.Instance.CurrentPlayer?.userId;
        Debug.Log($"[GameSession] Resolving levelId for userId={userId}...");
        
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
            isSessionActive = false;
            yield break;
        }

        var payload = new SessionStartRequest
        {
            userId = userId,
            levelId = levelId
        };

        var json = JsonUtility.ToJson(payload);
        APIResponse<string> apiResult = null;

        Debug.Log($"[GameSession] Starting session on server - userId={userId}, levelId={levelId}");
        yield return APIClient.Post(APIConfig.Sessions, json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var response = JsonUtility.FromJson<SessionStartResponse>(apiResult.data);
                if (response != null && response.session != null && !string.IsNullOrEmpty(response.session._id))
                {
                    SessionManager.Instance.SetActiveSession(response.session._id);
                    isSessionActive = true; // Only set to true after successful session creation
                    Debug.Log($"[GameSession] ✅ Session started successfully - sessionId={response.session._id}, isSessionActive={isSessionActive}");
                }
                else
                {
                    Debug.LogWarning("[GameSession] ❌ Failed to parse session start response");
                    Debug.LogWarning($"[GameSession] Response data: {apiResult.data}");
                    isSessionActive = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameSession] ❌ Error parsing session start response: {ex.Message}");
                Debug.LogError($"[GameSession] Response data: {apiResult.data}");
                isSessionActive = false;
            }
        }
        else
        {
            Debug.LogWarning($"[GameSession] ❌ Failed to start session - Status: {apiResult?.statusCode}, Error: {apiResult?.error}");
            Debug.LogWarning($"[GameSession] Response data: {apiResult?.data}");
            isSessionActive = false;
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
        Debug.Log($"[GameSession] isSessionActive={isSessionActive}, hasSessionManager={SessionManager.Instance != null}, sessionId={SessionManager.Instance?.ActiveSessionId}");

        bool sessionWasActive = isSessionActive;
        var sessionId = SessionManager.Instance?.ActiveSessionId;
        var durationSeconds = sessionWasActive ? Mathf.Max(0, (int)(DateTime.Now - sessionStartTime).TotalSeconds) : 0;

        // Try to end session on server if it was active
        if (sessionWasActive && SessionManager.Instance != null && !string.IsNullOrEmpty(sessionId))
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

            SessionManager.Instance.ClearSession();
            Debug.Log("[GameSession] Session cleared from SessionManager");
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

    void OnDestroy()
    {
        // End session when GameSession is destroyed
        if (isSessionActive)
        {
            StartCoroutine(OnGameEnd("ABANDONED"));
        }
    }

    private IEnumerator ResolveLevelId(Action<string> onResolved)
    {
        string resolvedId = null;

        if (LevelProgressManager.Instance != null)
        {
            yield return LevelProgressManager.Instance.ResolveLevelId(SceneManager.GetActiveScene(), id => resolvedId = id);
        }

        // Fallback to scene buildIndex as string if mapping fails
        if (string.IsNullOrEmpty(resolvedId))
        {
            resolvedId = SceneManager.GetActiveScene().buildIndex.ToString();
            Debug.LogWarning($"[GameSession] Falling back to scene index as levelId: {resolvedId}");
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
