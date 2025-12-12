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
    private float lastLevelsFetchTime = -999f;
    private bool isFetchingLevels;

    [Serializable]
    private class LevelsResponse
    {
        public LevelData[] levels;
    }

    [Serializable]
    private class LevelData
    {
        public string _id;
        public string levelName;
        public int levelNumber;
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
        if (string.IsNullOrEmpty(AuthManager.Instance?.Token))
        {
            onResolved?.Invoke(null);
            yield break;
        }

        yield return EnsureLevelsCached();

        string levelId = null;

        // Prefer levelNumber match with buildIndex
        if (levelNumberToLevelId.TryGetValue(scene.buildIndex, out var mappedByNumber))
        {
            levelId = mappedByNumber;
        }

        // Fallback to scene name match
        if (string.IsNullOrEmpty(levelId) && sceneNameToLevelId.TryGetValue(scene.name, out var mappedByName))
        {
            levelId = mappedByName;
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
        if (isFetchingLevels) yield break;

        if (Time.realtimeSinceStartup - lastLevelsFetchTime < levelsCacheTtlSeconds &&
            sceneNameToLevelId.Count > 0)
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
                if (parsed != null && parsed.levels != null)
                {
                    sceneNameToLevelId.Clear();
                    levelNumberToLevelId.Clear();
                    foreach (var lvl in parsed.levels)
                    {
                        if (string.IsNullOrEmpty(lvl._id)) continue;
                        levelNumberToLevelId[lvl.levelNumber] = lvl._id;
                        if (!string.IsNullOrEmpty(lvl.levelName))
                        {
                            sceneNameToLevelId[lvl.levelName] = lvl._id;
                        }
                    }
                    lastLevelsFetchTime = Time.realtimeSinceStartup;
                    Debug.Log($"[LevelProgress] Cached {parsed.levels.Length} levels");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LevelProgress] Failed to parse levels response: {ex.Message} body={apiResult.data}");
            }
        }
        else
        {
            Debug.LogWarning($"[LevelProgress] Fetch levels failed status={(int?)apiResult?.statusCode} err={apiResult?.error}");
        }
    }
}

