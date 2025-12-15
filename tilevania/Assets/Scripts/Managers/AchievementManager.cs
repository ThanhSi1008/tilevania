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
    [SerializeField] private float notificationSpacing = 120f; // Vertical spacing between notifications
    [SerializeField] private Vector2 notificationStartPosition = new Vector2(0, 200f); // Starting position (top center)

    public IReadOnlyList<AchievementData> AllAchievements => achievements;
    public IReadOnlyList<PlayerAchievementData> UnlockedAchievements => unlocked;

    private readonly List<AchievementData> achievements = new List<AchievementData>();
    private readonly List<PlayerAchievementData> unlocked = new List<PlayerAchievementData>();
    private readonly List<AchievementNotification> activeNotifications = new List<AchievementNotification>(); // Track active notifications

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
        if (!HasAuth())
        {
            yield break;
        }

        var previousUnlockedIds = new HashSet<string>(unlocked.Where(u => u.achievementId != null).Select(u => u.achievementId._id));

        APIResponse<string> apiResult = null;
        var userId = AuthManager.Instance.CurrentPlayer.userId;
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
                }
                else
                {
                }

                if (showNotifications)
                {
                    int newUnlocks = 0;
                    foreach (var item in unlocked)
                    {
                                var id = item?.achievementId?._id;
                                if (string.IsNullOrEmpty(id))
                                {
                                    continue;
                                }
                                
                        if (!previousUnlockedIds.Contains(id))
                        {
                            newUnlocks++;
                            var name = item.achievementId?.name ?? "Unknown";
                            var desc = item.achievementId?.description ?? "";
                            ShowNotification(name, desc);
                        }
                                else
                                {
                                }
                    }
                    
                        if (newUnlocks > 0)
                        {
                        }
                        else
                        {
                        }
                }
            }
            catch (Exception)
            {
            }
        }
        else
        {
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

        // Find the best parent for the notification
        Transform parent = null;
        
        // Priority 1: Use explicitly set notificationParent
        if (notificationParent != null)
        {
            parent = notificationParent;
            Debug.Log($"[Achievement] Using explicitly set notificationParent: {parent.name}");
        }
        else
        {
            // Priority 2: Find Canvas in current scene
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                parent = canvas.transform;
                Debug.Log($"[Achievement] Found Canvas in current scene: {canvas.name}");
            }
            else
            {
                // Priority 3: Fallback to AchievementManager transform (may not be visible in gameplay scenes)
                parent = transform;
                Debug.LogWarning($"[Achievement] ⚠️ No Canvas found in scene, using AchievementManager transform ({parent.name}). Notification may not be visible!");
            }
        }
        
        Debug.Log($"[Achievement] Spawning notification prefab at parent: {parent.name}");
        
        var go = Instantiate(achievementNotificationPrefab, parent);
        var notification = go.GetComponent<AchievementNotification>();
        
        if (notification != null)
        {
            notification.SetContent(title, description);
            
            // Calculate position based on number of active notifications
            int notificationIndex = activeNotifications.Count;
            Vector2 position = new Vector2(
                notificationStartPosition.x,
                notificationStartPosition.y - (notificationIndex * notificationSpacing)
            );
            
            // Set position
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
                Debug.Log($"[Achievement] Notification positioned at index {notificationIndex}, position: {position}");
            }
            
            // Track this notification
            activeNotifications.Add(notification);
            
            // Register callback to remove from list when destroyed
            notification.OnDestroyed += () => {
                activeNotifications.Remove(notification);
                UpdateNotificationPositions(); // Reposition remaining notifications
            };
            
            Debug.Log($"[Achievement] ✅ Notification spawned and content set (active count: {activeNotifications.Count})");
        }
        else
        {
            Debug.LogWarning("[Achievement] ❌ AchievementNotification component not found on prefab!");
        }
    }
    
    private void UpdateNotificationPositions()
    {
        // Reposition all active notifications
        for (int i = 0; i < activeNotifications.Count; i++)
        {
            var notification = activeNotifications[i];
            if (notification != null)
            {
                RectTransform rectTransform = notification.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector2 position = new Vector2(
                        notificationStartPosition.x,
                        notificationStartPosition.y - (i * notificationSpacing)
                    );
                    rectTransform.anchoredPosition = position;
                }
            }
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

