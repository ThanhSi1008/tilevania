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

    [Serializable]
    private class UpdateCurrentLevelRequest
    {
        public string currentLevel;
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
        if (string.IsNullOrEmpty(AuthManager.Instance?.Token))
        {
            onResolved?.Invoke(null);
            yield break;
        }

        // Skip invalid scenes (buildIndex < 0 or empty scene name)
        // This can happen when scene is not fully loaded yet
        bool isInvalidScene = scene.buildIndex < 0 || string.IsNullOrEmpty(scene.name);
        if (isInvalidScene)
        {
            onResolved?.Invoke(null);
            yield break;
        }

        yield return EnsureLevelsCached();

        // Re-validate scene after EnsureLevelsCached (scene might have changed during async operation)
        if (scene.buildIndex < 0 || string.IsNullOrEmpty(scene.name))
        {
            onResolved?.Invoke(null);
            yield break;
        }

        // Double-check that levels were cached successfully
        if (sceneNameToLevelId.Count == 0 && levelNumberToLevelId.Count == 0)
        {
            onResolved?.Invoke(null);
            yield break;
        }

        string levelId = null;
        
        // Try scene name match first (more reliable than buildIndex)
        // This is the preferred method since scene names are explicit
        if (!string.IsNullOrEmpty(scene.name))
        {
            if (sceneNameToLevelId.TryGetValue(scene.name, out var mappedByName))
            {
                levelId = mappedByName;
            }
            else
            {
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
            }
            else
            {
            }
        }

        onResolved?.Invoke(levelId);
    }

    public IEnumerator CompleteLevel(string userId, string levelId, int score, int coins, int enemies, int durationSeconds)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(levelId))
        {
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
        APIResponse<string> apiResult = null;
        yield return APIClient.Post(APIConfig.CompleteLevel(userId, levelId), json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success)
        {
        }
        else
        {
        }
    }

    public IEnumerator EnsureLevelsCached()
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
            yield break;
        }

        // Check if cache is still valid
        if (Time.realtimeSinceStartup - lastLevelsFetchTime < levelsCacheTtlSeconds &&
            sceneNameToLevelId.Count > 0 && levelNumberToLevelId.Count > 0)
        {
            yield break;
        }

        isFetchingLevels = true;
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
                        }
                        // Fallback to levelName if sceneName is not available
                        else if (!string.IsNullOrEmpty(lvl.levelName))
                        {
                            sceneNameToLevelId[lvl.levelName] = lvl._id;
                        }
                    }
                    lastLevelsFetchTime = Time.realtimeSinceStartup;
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }
        }
        else
        {
            if (apiResult != null && !string.IsNullOrEmpty(apiResult.data))
            {
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

    [Serializable]
    private class LevelProgressResponse
    {
        public int count;
        public LevelProgressData[] levelProgresses;
    }

    [Serializable]
    private class LevelProgressData
    {
        public string _id;
        public bool isCompleted;
        public LevelData levelId;
        public int bestScore;
        public int coinsCollected;
        public int enemiesDefeated;
        public string completedAt;
    }

    /// <summary>
    /// Get the highest completed level number. Returns 0 if no levels completed.
    /// </summary>
    public IEnumerator GetHighestCompletedLevelNumber(Action<int> onResult)
    {
        if (!HasAuth())
        {
            onResult?.Invoke(0);
            yield break;
        }

        var userId = AuthManager.Instance.CurrentPlayer.userId;
        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.LevelProgress(userId), r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<LevelProgressResponse>(apiResult.data);
                if (parsed != null && parsed.levelProgresses != null && parsed.levelProgresses.Length > 0)
                {
                    int highestLevelNumber = 0;
                    foreach (var progress in parsed.levelProgresses)
                    {
                        if (progress.isCompleted && progress.levelId != null)
                        {
                            highestLevelNumber = Mathf.Max(highestLevelNumber, progress.levelId.levelNumber);
                        }
                    }
                    onResult?.Invoke(highestLevelNumber);
                    yield break;
                }
            }
            catch (Exception)
            {
            }
        }
        
        onResult?.Invoke(0);
    }

    /// <summary>
    /// Get the next level to play (first uncompleted level, or Level 1 if all completed or none started).
    /// Returns level data or null if error.
    /// </summary>
    public IEnumerator GetNextLevelToPlay(Action<LevelData> onResult)
    {
        if (!HasAuth())
        {
            onResult?.Invoke(null);
            yield break;
        }

        // Ensure levels are cached
        yield return EnsureLevelsCached();

        // Get all level progress to find first uncompleted level
        var userId = AuthManager.Instance.CurrentPlayer.userId;
        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.LevelProgress(userId), r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        HashSet<int> completedLevelNumbers = new HashSet<int>();
        
        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<LevelProgressResponse>(apiResult.data);
                if (parsed != null && parsed.levelProgresses != null && parsed.levelProgresses.Length > 0)
                {
                    foreach (var progress in parsed.levelProgresses)
                    {
                        if (progress.isCompleted && progress.levelId != null)
                        {
                            completedLevelNumbers.Add(progress.levelId.levelNumber);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        
        // Find first uncompleted level, starting from Level 1
        LevelData nextLevelData = null;
        int maxLevelNumber = 0;
        
        // Find max level number from cache
        foreach (var kvp in levelNumberToLevelId)
        {
            maxLevelNumber = Mathf.Max(maxLevelNumber, kvp.Key);
        }
        
        for (int levelNum = 1; levelNum <= maxLevelNumber; levelNum++)
        {
            if (!completedLevelNumbers.Contains(levelNum))
            {
                nextLevelData = GetLevelDataByNumber(levelNum);
                if (nextLevelData != null)
                {
                    onResult?.Invoke(nextLevelData);
                    yield break;
                }
            }
        }

        // If all levels are completed, return the last level (or Level 1 if no levels found)
        if (nextLevelData == null)
        {
            if (maxLevelNumber > 0)
            {
                nextLevelData = GetLevelDataByNumber(maxLevelNumber);
            }
            if (nextLevelData == null)
            {
                nextLevelData = GetLevelDataByNumber(1);
            }
        }

        if (nextLevelData == null)
        {
            onResult?.Invoke(null);
            yield break;
        }

        onResult?.Invoke(nextLevelData);
    }

    private bool HasAuth()
    {
        return AuthManager.Instance != null && AuthManager.Instance.HasToken() && AuthManager.Instance.CurrentPlayer != null;
    }

    /// <summary>
    /// Update currentLevel in GameProfile. Called when starting a level.
    /// </summary>
    public IEnumerator UpdateCurrentLevel(string levelId, System.Action<bool> onResult = null)
    {
        if (!HasAuth() || string.IsNullOrEmpty(levelId))
        {
            onResult?.Invoke(false);
            yield break;
        }

        var userId = AuthManager.Instance.CurrentPlayer.userId;
        var updateData = new UpdateCurrentLevelRequest { currentLevel = levelId };
        var json = JsonUtility.ToJson(updateData);

        APIResponse<string> apiResult = null;
        yield return APIClient.Put(APIConfig.GameProfile(userId), json, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success)
        {
            onResult?.Invoke(true);
        }
        else
        {
            if (apiResult != null && !string.IsNullOrEmpty(apiResult.data))
            {
            }
            onResult?.Invoke(false);
        }
    }

    /// <summary>
    /// Get current level from GameProfile. Returns null if not set.
    /// </summary>
    public IEnumerator GetCurrentLevel(Action<LevelData> onResult)
    {
        if (!HasAuth())
        {
            onResult?.Invoke(null);
            yield break;
        }

        var userId = AuthManager.Instance.CurrentPlayer.userId;
        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.GameProfile(userId), r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            // Ensure levels are cached before parsing (moved outside try-catch to avoid CS1626)
            yield return EnsureLevelsCached();
            
            try
            {
                var profile = JsonUtility.FromJson<GameProfileResponse>(apiResult.data);
                
                if (profile != null && profile.gameProfile != null)
                {
                    // Check if currentLevel exists and is not an empty object
                    // Unity JsonUtility may create empty objects with default values when field is null in JSON
                        bool hasValidCurrentLevel = profile.gameProfile.currentLevel != null &&
                        (!string.IsNullOrEmpty(profile.gameProfile.currentLevel._id) || 
                         profile.gameProfile.currentLevel.levelNumber > 0);
                    
                    if (hasValidCurrentLevel)
                    {
                        LevelData levelData = null;
                        
                        // Try to get level data by _id first
                        if (!string.IsNullOrEmpty(profile.gameProfile.currentLevel._id))
                        {
                            levelData = GetLevelData(profile.gameProfile.currentLevel._id);
                            if (levelData != null)
                            {
                                onResult?.Invoke(levelData);
                                yield break;
                            }
                        }
                        
                        // Fallback: Try to get level data by levelNumber
                        if (levelData == null && profile.gameProfile.currentLevel.levelNumber > 0)
                        {
                            levelData = GetLevelDataByNumber(profile.gameProfile.currentLevel.levelNumber);
                            if (levelData != null)
                            {
                                onResult?.Invoke(levelData);
                                yield break;
                            }
                        }
                        
                        // If still not found, treat as null
                        if (levelData == null)
                        {
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }
        }
        else
        {
        }
        
        onResult?.Invoke(null);
    }

    [Serializable]
    private class GameProfileResponse
    {
        public GameProfileData gameProfile;
    }

    [Serializable]
    private class GameProfileData
    {
        public CurrentLevelData currentLevel;
    }

    [Serializable]
    private class CurrentLevelData
    {
        public string _id;
        public string levelName;
        public string sceneName;
        public int levelNumber;
    }
}

