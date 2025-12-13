using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }

    [Header("Caching")]
    [SerializeField] private float levelsCacheTtlSeconds = 300f;

    private readonly Dictionary<string, string> sceneNameToLevelId = new Dictionary<string, string>();
    private readonly Dictionary<int, string> levelNumberToLevelId = new Dictionary<int, string>();
    private readonly Dictionary<string, LevelData> levelIdToData = new Dictionary<string, LevelData>(); // Cache full level data
    private float lastLevelsFetchTime = -999f;
    private bool isFetchingLevels;

    [Serializable]
    private class LevelsResponse
    {
        public LevelData[] levels;
    }

    [Serializable]
    public class LevelData
    {
        public string _id;
        public string levelName;
        public string sceneName; // Server field for scene name
        public int levelNumber;
        public bool isUnlocked;
        public int requiredScoreToUnlock;
    }

    [Serializable]
    private class CompleteLevelRequest
    {
        public int score;
        public int coins;
        public int enemies;
        public int time;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator ResolveLevelId(Scene scene, Action<string> onResolved)
    {
        // Log early to debug - NEW CODE VERSION 2.0
        Debug.Log($"[LevelProgress] ResolveLevelId called (V2.0) - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
        
        if (string.IsNullOrEmpty(AuthManager.Instance?.Token))
        {
            Debug.LogWarning("[LevelProgress] No auth token, skipping ResolveLevelId");
            onResolved?.Invoke(null);
            yield break;
        }

        // Skip invalid scenes (buildIndex < 0 or empty scene name)
        // This can happen when scene is not fully loaded yet
        bool isInvalidScene = scene.buildIndex < 0 || string.IsNullOrEmpty(scene.name);
        Debug.Log($"[LevelProgress] Checking scene validity - buildIndex={scene.buildIndex}, scene.name='{scene.name}', isInvalidScene={isInvalidScene}");
        
        if (isInvalidScene)
        {
            Debug.LogWarning($"[LevelProgress] ⚠️ Skipping invalid scene - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
            onResolved?.Invoke(null);
            yield break;
        }
        
        Debug.Log($"[LevelProgress] Scene passed validation, proceeding with EnsureLevelsCached");

        yield return EnsureLevelsCached();

        // Re-validate scene after EnsureLevelsCached (scene might have changed during async operation)
        if (scene.buildIndex < 0 || string.IsNullOrEmpty(scene.name))
        {
            Debug.LogWarning($"[LevelProgress] ⚠️ Scene became invalid after EnsureLevelsCached - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
            onResolved?.Invoke(null);
            yield break;
        }

        // Double-check that levels were cached successfully
        if (sceneNameToLevelId.Count == 0 && levelNumberToLevelId.Count == 0)
        {
            Debug.LogError("[LevelProgress] ❌ Levels cache is empty after EnsureLevelsCached!");
            onResolved?.Invoke(null);
            yield break;
        }

        string levelId = null;
        
        // Debug: Log dictionary contents (scene is valid at this point)
        Debug.Log($"[LevelProgress] Processing valid scene - scene.name='{scene.name}', buildIndex={scene.buildIndex}");
        Debug.Log($"[LevelProgress] Dictionary sizes - sceneNameToLevelId: {sceneNameToLevelId.Count}, levelNumberToLevelId: {levelNumberToLevelId.Count}");
        if (sceneNameToLevelId.Count > 0)
        {
            Debug.Log($"[LevelProgress] Available sceneNames: {string.Join(", ", sceneNameToLevelId.Keys)}");
        }

        // Try scene name match first (more reliable than buildIndex)
        // This is the preferred method since scene names are explicit
        if (!string.IsNullOrEmpty(scene.name))
        {
            if (sceneNameToLevelId.TryGetValue(scene.name, out var mappedByName))
            {
                levelId = mappedByName;
                Debug.Log($"[LevelProgress] ✅ Matched levelId by sceneName: '{scene.name}' -> levelId={levelId}");
            }
            else
            {
                Debug.Log($"[LevelProgress] ❌ No match by sceneName: '{scene.name}' (not in dictionary)");
            }
        }

        // Fallback to levelNumber match with buildIndex
        // Note: Server levelNumber starts at 1, Unity buildIndex for Level 1 is 0
        // So we need to map: buildIndex + 1 = levelNumber
        if (string.IsNullOrEmpty(levelId) && scene.buildIndex >= 0)
        {
            int expectedLevelNumber = scene.buildIndex + 1;
            if (levelNumberToLevelId.TryGetValue(expectedLevelNumber, out var mappedByNumber))
            {
                levelId = mappedByNumber;
                Debug.Log($"[LevelProgress] ✅ Matched levelId by levelNumber: buildIndex={scene.buildIndex} -> levelNumber={expectedLevelNumber} -> levelId={levelId}");
            }
            else
            {
                Debug.Log($"[LevelProgress] ❌ No match by levelNumber: buildIndex={scene.buildIndex} -> expectedLevelNumber={expectedLevelNumber} (not in dictionary)");
            }
        }
        
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogError($"[LevelProgress] ❌ Failed to resolve levelId for scene: '{scene.name}' (buildIndex: {scene.buildIndex})");
            Debug.LogError($"[LevelProgress] Available sceneNames: {string.Join(", ", sceneNameToLevelId.Keys)}");
            Debug.LogError($"[LevelProgress] Available levelNumbers: {string.Join(", ", levelNumberToLevelId.Keys)}");
        }

        onResolved?.Invoke(levelId);
    }

    public IEnumerator CompleteLevel(string userId, string levelId, int score, int coins, int enemies, int durationSeconds)
    {
        Debug.Log($"[LevelProgress] CompleteLevel called - userId={userId}, levelId={levelId}, score={score}, coins={coins}, enemies={enemies}, time={durationSeconds}s");
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning($"[LevelProgress] ❌ Cannot complete level - userId={userId}, levelId={levelId}");
            yield break;
        }

        var payload = new CompleteLevelRequest
        {
            score = score,
            coins = coins,
            enemies = enemies,
            time = durationSeconds
        };

        var json = JsonUtility.ToJson(payload);
        Debug.Log($"[LevelProgress] Sending complete level request: {json}");
        
        APIResponse<string> apiResult = null;
        yield return APIClient.Post(APIConfig.CompleteLevel(userId, levelId), json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success)
        {
            Debug.Log($"[LevelProgress] ✅ CompleteLevel success for levelId={levelId}, score={score}, coins={coins}, enemies={enemies}");
        }
        else
        {
            Debug.LogWarning($"[LevelProgress] ❌ CompleteLevel failed - status={(int?)apiResult?.statusCode}, error={apiResult?.error}, data={apiResult?.data}");
        }
    }

    private IEnumerator EnsureLevelsCached()
    {
        // Wait if another coroutine is already fetching
        int waitCount = 0;
        while (isFetchingLevels && waitCount < 50) // Wait up to 5 seconds
        {
            yield return new WaitForSeconds(0.1f);
            waitCount++;
        }

        if (isFetchingLevels)
        {
            Debug.LogWarning("[LevelProgress] Timeout waiting for levels fetch to complete");
            yield break;
        }

        // Check if cache is still valid
        if (Time.realtimeSinceStartup - lastLevelsFetchTime < levelsCacheTtlSeconds &&
            sceneNameToLevelId.Count > 0 && levelNumberToLevelId.Count > 0)
        {
            Debug.Log($"[LevelProgress] Using cached levels (cached {sceneNameToLevelId.Count} levels)");
            yield break;
        }

        isFetchingLevels = true;
        Debug.Log("[LevelProgress] Fetching levels from server...");
        
        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.Levels, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());
        isFetchingLevels = false;

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<LevelsResponse>(apiResult.data);
                if (parsed != null && parsed.levels != null && parsed.levels.Length > 0)
                {
                    sceneNameToLevelId.Clear();
                    levelNumberToLevelId.Clear();
                    levelIdToData.Clear();
                    foreach (var lvl in parsed.levels)
                    {
                        if (string.IsNullOrEmpty(lvl._id)) continue;
                        
                        // Cache full level data
                        levelIdToData[lvl._id] = lvl;
                        
                        // Map by levelNumber
                        levelNumberToLevelId[lvl.levelNumber] = lvl._id;
                        
                        // Map by sceneName (preferred, more reliable)
                        if (!string.IsNullOrEmpty(lvl.sceneName))
                        {
                            sceneNameToLevelId[lvl.sceneName] = lvl._id;
                            Debug.Log($"[LevelProgress] Mapped sceneName '{lvl.sceneName}' -> levelId={lvl._id} (levelNumber={lvl.levelNumber}, isUnlocked={lvl.isUnlocked})");
                        }
                        // Fallback to levelName if sceneName is not available
                        else if (!string.IsNullOrEmpty(lvl.levelName))
                        {
                            sceneNameToLevelId[lvl.levelName] = lvl._id;
                            Debug.Log($"[LevelProgress] Mapped levelName '{lvl.levelName}' -> levelId={lvl._id} (levelNumber={lvl.levelNumber}, isUnlocked={lvl.isUnlocked})");
                        }
                    }
                    lastLevelsFetchTime = Time.realtimeSinceStartup;
                    Debug.Log($"[LevelProgress] ✅ Cached {parsed.levels.Length} levels successfully");
                }
                else
                {
                    Debug.LogWarning($"[LevelProgress] Levels response is empty or invalid - levels={parsed?.levels?.Length ?? 0}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LevelProgress] Failed to parse levels response: {ex.Message}");
                Debug.LogError($"[LevelProgress] Response body: {apiResult.data}");
            }
        }
        else
        {
            Debug.LogError($"[LevelProgress] ❌ Fetch levels failed - status={(int?)apiResult?.statusCode}, err={apiResult?.error}");
            if (apiResult != null && !string.IsNullOrEmpty(apiResult.data))
            {
                Debug.LogError($"[LevelProgress] Response data: {apiResult.data}");
            }
        }
    }

    /// <summary>
    /// Get level data by levelId. Returns null if not found.
    /// </summary>
    public LevelData GetLevelData(string levelId)
    {
        if (string.IsNullOrEmpty(levelId)) return null;
        return levelIdToData.TryGetValue(levelId, out var data) ? data : null;
    }

    /// <summary>
    /// Get level data by scene name. Returns null if not found.
    /// </summary>
    public LevelData GetLevelDataBySceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;
        if (!sceneNameToLevelId.TryGetValue(sceneName, out var levelId)) return null;
        return GetLevelData(levelId);
    }

    /// <summary>
    /// Get level data by level number. Returns null if not found.
    /// </summary>
    public LevelData GetLevelDataByNumber(int levelNumber)
    {
        if (!levelNumberToLevelId.TryGetValue(levelNumber, out var levelId)) return null;
        return GetLevelData(levelId);
    }

    /// <summary>
    /// Check if a level is unlocked. Also checks if player has required score.
    /// </summary>
    public IEnumerator CheckLevelUnlocked(string levelId, int playerTotalScore, Action<bool, string> onResult)
    {
        if (string.IsNullOrEmpty(levelId))
        {
            onResult?.Invoke(false, "Level ID is invalid");
            yield break;
        }

        // Ensure levels are cached
        yield return EnsureLevelsCached();

        var levelData = GetLevelData(levelId);
        if (levelData == null)
        {
            onResult?.Invoke(false, "Level not found");
            yield break;
        }

        // Check if level is unlocked by default
        if (levelData.isUnlocked)
        {
            onResult?.Invoke(true, null);
            yield break;
        }

        // Check if player has required score
        if (levelData.requiredScoreToUnlock > 0 && playerTotalScore >= levelData.requiredScoreToUnlock)
        {
            onResult?.Invoke(true, null);
            yield break;
        }

        // Level is locked
        string message = levelData.requiredScoreToUnlock > 0
            ? $"Level requires {levelData.requiredScoreToUnlock} total score. Your score: {playerTotalScore}"
            : "Level is locked";
        onResult?.Invoke(false, message);
    }
}

