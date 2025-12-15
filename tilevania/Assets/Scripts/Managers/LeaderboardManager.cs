using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Cache Settings")]
    [SerializeField] private float cacheDuration = 300f; // 5 minutes

    private Dictionary<string, LeaderboardCache> cache = new Dictionary<string, LeaderboardCache>();

    [Serializable]
    public class LeaderboardEntry
    {
        public string _id;
        public string userId;
        public string username;
        public string profileImage;
        public int rank;
        public int totalScore;
        public string period;
        public string calculatedAt;
    }

    [Serializable]
    public class LeaderboardRank
    {
        public LeaderboardEntry rank;
    }

    [Serializable]
    private class LeaderboardResponse
    {
        public int count;
        public LeaderboardEntry[] leaderboard;
    }

    private class LeaderboardCache
    {
        public List<LeaderboardEntry> entries;
        public float timestamp;
        public bool isValid(float duration) => Time.time - timestamp < duration;
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

    public IEnumerator FetchLeaderboard(string period, Action<List<LeaderboardEntry>> onComplete)
    {
        string endpoint = period switch
        {
            "weekly" => APIConfig.LeaderboardWeekly,
            "daily" => APIConfig.LeaderboardDaily,
            _ => APIConfig.Leaderboard
        };

        // Check cache first
        if (cache.ContainsKey(period) && cache[period].isValid(cacheDuration))
        {
            Debug.Log($"[Leaderboard] Using cached data for {period}");
            onComplete?.Invoke(cache[period].entries);
            yield break;
        }

        APIResponse<string> apiResult = null;
        yield return APIClient.Get(endpoint, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<LeaderboardResponse>(apiResult.data);
                var entries = new List<LeaderboardEntry>();

                if (parsed != null && parsed.leaderboard != null)
                {
                    entries.AddRange(parsed.leaderboard);
                    Debug.Log($"[Leaderboard] ✅ Fetched {entries.Count} entries for {period}");
                }

                // Update cache
                cache[period] = new LeaderboardCache
                {
                    entries = entries,
                    timestamp = Time.time
                };

                onComplete?.Invoke(entries);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Leaderboard] ❌ Failed to parse leaderboard: {ex.Message} body={apiResult.data}");
                onComplete?.Invoke(new List<LeaderboardEntry>());
            }
        }
        else
        {
            Debug.LogWarning($"[Leaderboard] ❌ Fetch failed - status={(int?)apiResult?.statusCode}, error={apiResult?.error}");
            onComplete?.Invoke(new List<LeaderboardEntry>());
        }
    }

    public IEnumerator GetPlayerRank(string userId, string period = "ALLTIME", Action<LeaderboardEntry> onComplete = null)
    {
        APIResponse<string> apiResult = null;
        string endpoint = $"{APIConfig.LeaderboardRank(userId)}?period={period}";
        yield return APIClient.Get(endpoint, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<LeaderboardRank>(apiResult.data);
                if (parsed != null && parsed.rank != null)
                {
                    Debug.Log($"[Leaderboard] ✅ Player rank: #{parsed.rank.rank} (score: {parsed.rank.totalScore})");
                    onComplete?.Invoke(parsed.rank);
                }
                else
                {
                    Debug.LogWarning($"[Leaderboard] Player not found in leaderboard for period {period}");
                    onComplete?.Invoke(null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Leaderboard] ❌ Failed to parse player rank: {ex.Message} body={apiResult.data}");
                onComplete?.Invoke(null);
            }
        }
        else
        {
            Debug.LogWarning($"[Leaderboard] ❌ Get player rank failed - status={(int?)apiResult?.statusCode}, error={apiResult?.error}");
            onComplete?.Invoke(null);
        }
    }

    public void ClearCache(string period = null)
    {
        if (period != null)
        {
            cache.Remove(period);
            Debug.Log($"[Leaderboard] Cleared cache for {period}");
        }
        else
        {
            cache.Clear();
            Debug.Log("[Leaderboard] Cleared all cache");
        }
    }

    private bool HasAuth()
    {
        return AuthManager.Instance != null &&
               AuthManager.Instance.HasToken() &&
               AuthManager.Instance.CurrentPlayer != null &&
               !string.IsNullOrEmpty(AuthManager.Instance.CurrentPlayer.userId);
    }
}

