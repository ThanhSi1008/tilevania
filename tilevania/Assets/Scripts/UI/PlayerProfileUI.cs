using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerRankText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private TextMeshProUGUI enemiesDefeatedText;
    [SerializeField] private TextMeshProUGUI deathsText;
    [SerializeField] private TextMeshProUGUI playtimeText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private Transform achievementsContainer;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI errorText;

    private string currentUserId;
    private bool isLoading = false;

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    public void LoadProfile(string userId)
    {
        if (isLoading)
        {
            Debug.Log("[PlayerProfile] Already loading, skipping...");
            return;
        }

        currentUserId = userId;
        gameObject.SetActive(true);
        StartCoroutine(LoadProfileCoroutine(userId));
    }

    private IEnumerator LoadProfileCoroutine(string userId)
    {
        isLoading = true;
        SetLoadingState(true);
        SetErrorState(false);

        // Load game profile
        GameProfileData profileData = null;
        yield return FetchGameProfile(userId, (data) => profileData = data);

        // Load player rank
        LeaderboardManager.LeaderboardEntry rankData = null;
        if (LeaderboardManager.Instance != null)
        {
            yield return LeaderboardManager.Instance.GetPlayerRank(userId, "ALLTIME", (rank) => rankData = rank);
        }

        // Load achievements
        List<AchievementManager.PlayerAchievementData> achievements = null;
        if (AchievementManager.Instance != null)
        {
            yield return FetchAchievements(userId, (data) => achievements = data);
        }

        isLoading = false;
        SetLoadingState(false);

        if (profileData == null)
        {
            SetErrorState(true, "Failed to load player profile");
            yield break;
        }

        // Update UI with profile data
        UpdateProfileUI(profileData, rankData, achievements);
    }

    private IEnumerator FetchGameProfile(string userId, Action<GameProfileData> onComplete)
    {
        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.GameProfile(userId), r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<GameProfileResponse>(apiResult.data);
                if (parsed != null && parsed.gameProfile != null)
                {
                    onComplete?.Invoke(parsed.gameProfile);
                }
                else
                {
                    Debug.LogWarning("[PlayerProfile] Failed to parse game profile response");
                    onComplete?.Invoke(null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayerProfile] ❌ Failed to parse game profile: {ex.Message} body={apiResult.data}");
                onComplete?.Invoke(null);
            }
        }
        else
        {
            Debug.LogWarning($"[PlayerProfile] ❌ Fetch game profile failed - status={(int?)apiResult?.statusCode}, error={apiResult?.error}");
            onComplete?.Invoke(null);
        }
    }

    private IEnumerator FetchAchievements(string userId, Action<List<AchievementManager.PlayerAchievementData>> onComplete)
    {
        APIResponse<string> apiResult = null;
        yield return APIClient.Get(APIConfig.PlayerAchievements(userId), r => apiResult = r, AuthManager.Instance?.BuildAuthHeaders());

        if (apiResult != null && apiResult.success && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                var parsed = JsonUtility.FromJson<PlayerAchievementListResponse>(apiResult.data);
                var achievements = new List<AchievementManager.PlayerAchievementData>();
                
                if (parsed != null && parsed.achievements != null)
                {
                    achievements.AddRange(parsed.achievements);
                }

                onComplete?.Invoke(achievements);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayerProfile] ❌ Failed to parse achievements: {ex.Message}");
                onComplete?.Invoke(new List<AchievementManager.PlayerAchievementData>());
            }
        }
        else
        {
            onComplete?.Invoke(new List<AchievementManager.PlayerAchievementData>());
        }
    }

    private void UpdateProfileUI(GameProfileData profile, LeaderboardManager.LeaderboardEntry rank, List<AchievementManager.PlayerAchievementData> achievements)
    {
        // Player name
        if (playerNameText != null && profile.userId != null)
        {
            playerNameText.text = profile.userId.username ?? "Unknown Player";
        }

        // Rank
        if (playerRankText != null)
        {
            if (rank != null)
            {
                playerRankText.text = $"Rank: #{rank.rank}";
                playerRankText.color = rank.rank <= 3 ? Color.yellow : Color.white;
            }
            else
            {
                playerRankText.text = "Rank: N/A";
            }
        }

        // Stats
        if (totalScoreText != null)
        {
            totalScoreText.text = profile.totalScore.ToString("N0");
        }

        if (totalCoinsText != null)
        {
            totalCoinsText.text = profile.totalCoinsCollected.ToString("N0");
        }

        if (enemiesDefeatedText != null)
        {
            enemiesDefeatedText.text = profile.totalEnemiesDefeated.ToString("N0");
        }

        if (deathsText != null)
        {
            deathsText.text = profile.totalDeaths.ToString("N0");
        }

        if (playtimeText != null)
        {
            // Convert seconds to hours:minutes:seconds
            int hours = (int)(profile.totalPlayTime / 3600);
            int minutes = (int)((profile.totalPlayTime % 3600) / 60);
            int seconds = (int)(profile.totalPlayTime % 60);
            playtimeText.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        if (currentLevelText != null)
        {
            if (profile.currentLevel != null && !string.IsNullOrEmpty(profile.currentLevel.levelName))
            {
                currentLevelText.text = profile.currentLevel.levelName;
            }
            else
            {
                currentLevelText.text = "Not started";
            }
        }

        // Achievements
        if (achievementsContainer != null && achievementItemPrefab != null)
        {
            // Clear existing
            foreach (Transform child in achievementsContainer)
            {
                Destroy(child.gameObject);
            }

            // Populate achievements
            if (achievements != null && achievements.Count > 0)
            {
                foreach (var achievement in achievements)
                {
                    if (achievement?.achievementId != null)
                    {
                        var itemObj = Instantiate(achievementItemPrefab, achievementsContainer);
                        var itemUI = itemObj.GetComponent<AchievementListUIItem>();
                        if (itemUI != null)
                        {
                            // Setup achievement item
                            itemUI.SetData(
                                achievement.achievementId.name,
                                achievement.achievementId.description,
                                achievement.achievementId.points,
                                true, // unlocked
                                100,  // progress = 100% for unlocked
                                achievement.achievementId.condition
                            );
                        }
                    }
                }
            }
        }
    }

    private void SetLoadingState(bool loading)
    {
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(loading);
        }
    }

    private void SetErrorState(bool error, string message = "")
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(error);
            if (error && !string.IsNullOrEmpty(message))
            {
                errorText.text = message;
            }
        }
    }

    private void OnBackClicked()
    {
        gameObject.SetActive(false);
    }

    [Serializable]
    private class GameProfileResponse
    {
        public GameProfileData gameProfile;
    }

    [Serializable]
    private class GameProfileData
    {
        public string _id;
        public UserData userId;
        public int totalScore;
        public int totalCoinsCollected;
        public int totalEnemiesDefeated;
        public int totalDeaths;
        public float totalPlayTime;
        public int currentLives;
        public int highestScoreAchieved;
        public LevelData currentLevel;
    }

    [Serializable]
    private class UserData
    {
        public string _id;
        public string username;
        public string email;
    }

    [Serializable]
    private class LevelData
    {
        public string _id;
        public string levelName;
        public string sceneName;
        public int levelNumber;
    }

    [Serializable]
    private class PlayerAchievementListResponse
    {
        public int count;
        public AchievementManager.PlayerAchievementData[] achievements;
    }
}

