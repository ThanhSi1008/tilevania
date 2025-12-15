using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Toggle allTimeTab;
    [SerializeField] private Toggle weeklyTab;
    [SerializeField] private Toggle dailyTab;
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI emptyText;

    [Header("Settings")]
    [SerializeField] private bool autoRefreshOnEnable = true;

    private LeaderboardManager leaderboardManager;
    private string currentTab = "alltime";
    private bool isLoading = false;

    void Start()
    {
        leaderboardManager = LeaderboardManager.Instance;
        if (leaderboardManager == null)
        {
            Debug.LogError("[LeaderboardUI] LeaderboardManager.Instance is null!");
            return;
        }

        SetupButtons();
        
        if (autoRefreshOnEnable)
        {
            LoadLeaderboard("alltime");
        }
    }

    void OnEnable()
    {
        if (autoRefreshOnEnable && leaderboardManager != null)
        {
            LoadLeaderboard(currentTab);
        }
    }

    private void SetupButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(OnRefreshClicked);
        }

        if (allTimeTab != null)
        {
            allTimeTab.onValueChanged.AddListener((value) =>
            {
                if (value) LoadLeaderboard("alltime");
            });
        }

        if (weeklyTab != null)
        {
            weeklyTab.onValueChanged.AddListener((value) =>
            {
                if (value) LoadLeaderboard("weekly");
            });
        }

        if (dailyTab != null)
        {
            dailyTab.onValueChanged.AddListener((value) =>
            {
                if (value) LoadLeaderboard("daily");
            });
        }
    }

    private void LoadLeaderboard(string period)
    {
        if (isLoading)
        {
            Debug.Log("[LeaderboardUI] Already loading, skipping...");
            return;
        }

        currentTab = period;
        isLoading = true;

        // Clear existing entries
        ClearEntries();

        // Show loading
        SetLoadingState(true);

        // Get current user ID for highlighting
        string currentUserId = null;
        if (AuthManager.Instance != null && AuthManager.Instance.CurrentPlayer != null)
        {
            currentUserId = AuthManager.Instance.CurrentPlayer.userId;
        }

        // Fetch leaderboard
        StartCoroutine(LoadLeaderboardCoroutine(period, currentUserId));
    }

    private IEnumerator LoadLeaderboardCoroutine(string period, string currentUserId)
    {
        List<LeaderboardManager.LeaderboardEntry> entries = null;
        yield return leaderboardManager.FetchLeaderboard(period, (result) => entries = result);

        isLoading = false;
        SetLoadingState(false);

        if (entries == null || entries.Count == 0)
        {
            SetEmptyState(true);
            Debug.Log($"[LeaderboardUI] No entries found for {period}");
            yield break;
        }

        SetEmptyState(false);

        // Populate entries
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            bool isCurrentPlayer = !string.IsNullOrEmpty(currentUserId) && 
                                   entry.userId == currentUserId;

            if (leaderboardEntryPrefab != null && leaderboardContent != null)
            {
                var entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                var entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
                
                if (entryUI != null)
                {
                    // Use rank from entry, or fallback to index + 1
                    int displayRank = entry.rank > 0 ? entry.rank : (i + 1);
                    entryUI.SetupEntry(entry, displayRank, isCurrentPlayer);
                }
                else
                {
                    Debug.LogWarning("[LeaderboardUI] LeaderboardEntryUI component not found on prefab!");
                }
            }
        }

        Debug.Log($"[LeaderboardUI] ✅ Loaded {entries.Count} entries for {period}");
    }

    private void ClearEntries()
    {
        if (leaderboardContent == null) return;

        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetLoadingState(bool loading)
    {
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(loading);
        }
    }

    private void SetEmptyState(bool empty)
    {
        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(empty);
        }
    }

    private void OnBackClicked()
    {
        gameObject.SetActive(false);
        
        // Show main menu
        var mainMenu = FindFirstObjectByType<MainMenuManager>();
        if (mainMenu != null)
        {
            mainMenu.gameObject.SetActive(true);
        }
    }

    private void OnRefreshClicked()
    {
        // Clear cache and reload
        if (leaderboardManager != null)
        {
            leaderboardManager.ClearCache(currentTab);
        }
        LoadLeaderboard(currentTab);
    }
}

