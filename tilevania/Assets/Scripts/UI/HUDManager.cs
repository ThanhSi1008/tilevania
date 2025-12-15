using System.Collections;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;
    
    [Header("Rank Display (Phase 4)")]
    [SerializeField] private TextMeshProUGUI rankLabelText;
    [SerializeField] private TextMeshProUGUI rankValueText;
    
    [Header("Connection Status (Phase 4)")]
    [SerializeField] private GameObject connectionStatusIndicator;
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private Color onlineColor = Color.green;
    [SerializeField] private Color offlineColor = Color.red;
    [SerializeField] private Color syncingColor = Color.yellow;

    private float lastRankUpdateTime = 0f;
    private const float RANK_UPDATE_INTERVAL = 60f; // Update rank every 60 seconds

    void Start()
    {
        UpdateUsername();
        LoadPlayerRank();
    }

    void Update()
    {
        // Periodically update rank
        if (Time.time - lastRankUpdateTime > RANK_UPDATE_INTERVAL)
        {
            LoadPlayerRank();
            lastRankUpdateTime = Time.time;
        }
    }

    public void UpdateUsername()
    {
        var isAuthed = AuthManager.Instance != null && AuthManager.Instance.HasToken();
        var username = isAuthed && AuthManager.Instance.CurrentPlayer != null
            ? AuthManager.Instance.CurrentPlayer.username
            : "Guest";

        if (usernameText != null) usernameText.text = username;
    }

    private void LoadPlayerRank()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.HasToken() || AuthManager.Instance.CurrentPlayer == null)
        {
            if (rankValueText != null) rankValueText.text = "N/A";
            if (rankLabelText != null) rankLabelText.gameObject.SetActive(false);
            return;
        }

        if (rankLabelText != null) rankLabelText.gameObject.SetActive(true);

        var userId = AuthManager.Instance.CurrentPlayer.userId;
        if (LeaderboardManager.Instance != null)
        {
            StartCoroutine(LoadRankCoroutine(userId));
        }
    }

    private IEnumerator LoadRankCoroutine(string userId)
    {
        LeaderboardManager.LeaderboardEntry rankData = null;
        yield return LeaderboardManager.Instance.GetPlayerRank(userId, "ALLTIME", (rank) => rankData = rank);

        if (rankValueText != null)
        {
            if (rankData != null)
            {
                rankValueText.text = $"#{rankData.rank}";
                rankValueText.color = rankData.rank <= 3 ? Color.yellow : Color.white;
            }
            else
            {
                rankValueText.text = "N/A";
                rankValueText.color = Color.white;
            }
        }
    }

    public void SetConnectionStatus(bool online, bool syncing = false)
    {
        if (connectionStatusIndicator != null)
        {
            connectionStatusIndicator.SetActive(true);
            var image = connectionStatusIndicator.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                if (syncing)
                {
                    image.color = syncingColor;
                }
                else
                {
                    image.color = online ? onlineColor : offlineColor;
                }
            }
        }

        if (connectionStatusText != null)
        {
            connectionStatusText.gameObject.SetActive(true);
            if (syncing)
            {
                connectionStatusText.text = "Syncing...";
                connectionStatusText.color = syncingColor;
            }
            else
            {
                connectionStatusText.text = online ? "Online" : "Offline";
                connectionStatusText.color = online ? onlineColor : offlineColor;
            }
        }
    }
}

