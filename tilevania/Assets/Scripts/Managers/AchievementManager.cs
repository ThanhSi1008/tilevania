using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Notification")]
    [SerializeField] private GameObject achievementNotificationPrefab;
    [SerializeField] private Transform notificationParent;

    public IReadOnlyList<AchievementData> AllAchievements => achievements;
    public IReadOnlyList<PlayerAchievementData> UnlockedAchievements => unlocked;

    private readonly List<AchievementData> achievements = new List<AchievementData>();
    private readonly List<PlayerAchievementData> unlocked = new List<PlayerAchievementData>();

    [Serializable]
    public class AchievementData
    {
        public string _id;
        public string name;
        public string description;
        public string condition;
        public int points;
        public string rarity;
    }

    [Serializable]
    public class PlayerAchievementData
    {
        public string _id;
        public string unlockedAt;
        public int progress;
        public AchievementData achievementId;
    }

    [Serializable]
    private class AchievementListResponse
    {
        public AchievementData[] achievements;
    }

    [Serializable]
    private class PlayerAchievementListResponse
    {
        public int count;
        public PlayerAchievementData[] achievements;
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

    public IEnumerator RefreshAll()
    {
        if (!HasAuth()) yield break;

        yield return FetchAchievements();
        yield return RefreshUnlocked(false);
    }

    public IEnumerator RefreshUnlocked(bool showNotifications)
    {
        Debug.Log($"[Achievement] RefreshUnlocked called - showNotifications={showNotifications}");
        
        if (!HasAuth())
        {
            Debug.LogWarning("[Achievement] ❌ Cannot refresh - user not authenticated");
            yield break;
        }

        var previousUnlockedIds = new HashSet<string>(unlocked.Where(u => u.achievementId != null).Select(u => u.achievementId._id));
        Debug.Log($"[Achievement] Previous unlocked count: {previousUnlockedIds.Count}");

        APIResponse<string> apiResult = null;
        var userId = AuthManager.Instance.CurrentPlayer.userId;
        Debug.Log($"[Achievement] Fetching unlocked achievements for userId={userId}...");
        yield return APIClient.Get(APIConfig.PlayerAchievements(userId), r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<PlayerAchievementListResponse>(apiResult.data);
                unlocked.Clear();
                if (parsed != null && parsed.achievements != null)
                {
                    unlocked.AddRange(parsed.achievements);
                    Debug.Log($"[Achievement] ✅ Fetched {unlocked.Count} unlocked achievements");
                    
                    // Debug: Log achievement details to verify structure
                    foreach (var item in unlocked)
                    {
                        if (item?.achievementId != null)
                        {
                            Debug.Log($"[Achievement] Found achievement: {item.achievementId.name} (id: {item.achievementId._id})");
                        }
                        else
                        {
                            Debug.LogWarning($"[Achievement] ⚠️ Achievement item has null achievementId! Item id: {item?._id}");
                        }
                    }
                }
                else
                {
                    Debug.Log("[Achievement] No achievements in response");
                    if (parsed == null)
                    {
                        Debug.LogWarning($"[Achievement] ⚠️ Parsed response is null. Raw data: {apiResult.data}");
                    }
                }

                if (showNotifications)
                {
                    int newUnlocks = 0;
                    foreach (var item in unlocked)
                    {
                        var id = item?.achievementId?._id;
                        if (string.IsNullOrEmpty(id))
                        {
                            Debug.LogWarning($"[Achievement] ⚠️ Skipping achievement with null/empty id. Item: {item?._id}");
                            continue;
                        }
                        
                        if (!previousUnlockedIds.Contains(id))
                        {
                            newUnlocks++;
                            var name = item.achievementId?.name ?? "Unknown";
                            var desc = item.achievementId?.description ?? "";
                            Debug.Log($"[Achievement] 🎉 NEW ACHIEVEMENT UNLOCKED: {name} - {desc}");
                            ShowNotification(name, desc);
                        }
                        else
                        {
                            Debug.Log($"[Achievement] Achievement {id} was already unlocked, skipping notification");
                        }
                    }
                    
                    if (newUnlocks > 0)
                    {
                        Debug.Log($"[Achievement] ✅ Showed {newUnlocks} achievement notification(s)");
                    }
                    else
                    {
                        Debug.Log($"[Achievement] No new achievements to show (previous: {previousUnlockedIds.Count}, current: {unlocked.Count})");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Achievement] ❌ Failed to parse unlocked list: {ex.Message} body={apiResult.data}");
            }
        }
        else
        {
            Debug.LogWarning($"[Achievement] ❌ Fetch unlocked failed - status={(int?)apiResult?.statusCode}, error={apiResult?.error}, data={apiResult?.data}");
        }
    }

    private IEnumerator FetchAchievements()
    {
        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.Achievements, r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<AchievementListResponse>(apiResult.data);
                achievements.Clear();
                if (parsed != null && parsed.achievements != null)
                {
                    achievements.AddRange(parsed.achievements);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Achievement] Failed to parse achievements: {ex.Message} body={apiResult.data}");
            }
        }
        else
        {
            Debug.LogWarning($"[Achievement] Fetch achievements failed status={(int?)apiResult?.statusCode} err={apiResult?.error}");
        }
    }

    private void ShowNotification(string title, string description)
    {
        Debug.Log($"[Achievement] ShowNotification called - title={title}, description={description}");
        
        if (achievementNotificationPrefab == null)
        {
            Debug.LogWarning("[Achievement] ❌ Cannot show notification - prefab is null! Check AchievementManager settings.");
            return;
        }

        var parent = notificationParent != null ? notificationParent : transform;
        Debug.Log($"[Achievement] Spawning notification prefab at parent: {parent.name}");
        
        var go = Instantiate(achievementNotificationPrefab, parent);
        var notification = go.GetComponent<AchievementNotification>();
        
        if (notification != null)
        {
            notification.SetContent(title, description);
            Debug.Log($"[Achievement] ✅ Notification spawned and content set");
        }
        else
        {
            Debug.LogWarning("[Achievement] ❌ AchievementNotification component not found on prefab!");
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

