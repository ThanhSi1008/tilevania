using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image highlightBackground;
    [SerializeField] private Image avatarImage;

    [Header("Colors")]
    [SerializeField] private Color top3Color = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.3f); // Yellow with transparency

    private LeaderboardManager.LeaderboardEntry entry;
    private bool isCurrentPlayer = false;

    public void SetupEntry(LeaderboardManager.LeaderboardEntry entry, int displayRank, bool isCurrentPlayer = false)
    {
        this.entry = entry;
        this.isCurrentPlayer = isCurrentPlayer;

        // Set rank
        if (rankText != null)
        {
            rankText.text = $"#{displayRank}";
            
            // Color top 3
            if (displayRank <= 3)
            {
                rankText.color = top3Color;
                rankText.fontStyle = FontStyles.Bold;
            }
            else
            {
                rankText.color = normalColor;
                rankText.fontStyle = FontStyles.Normal;
            }
        }

        // Set player name
        if (playerNameText != null)
        {
            playerNameText.text = entry?.username ?? "Unknown";
            if (isCurrentPlayer)
            {
                playerNameText.fontStyle = FontStyles.Bold;
            }
        }

        // Set score
        if (scoreText != null)
        {
            scoreText.text = entry != null ? entry.totalScore.ToString("N0") : "0";
        }

        // Highlight current player
        if (highlightBackground != null)
        {
            highlightBackground.gameObject.SetActive(isCurrentPlayer);
            if (isCurrentPlayer)
            {
                highlightBackground.color = highlightColor;
            }
        }

        // Set avatar (if available)
        if (avatarImage != null && entry != null && !string.IsNullOrEmpty(entry.profileImage))
        {
            // TODO: Load image from URL if needed
            // For now, just show/hide based on availability
            avatarImage.gameObject.SetActive(false);
        }
    }

    public void OnEntryClicked()
    {
        if (entry != null && !string.IsNullOrEmpty(entry.userId))
        {
            // Open player profile
            var profileUI = FindFirstObjectByType<PlayerProfileUI>();
            if (profileUI != null)
            {
                profileUI.LoadProfile(entry.userId);
            }
        }
    }
}

