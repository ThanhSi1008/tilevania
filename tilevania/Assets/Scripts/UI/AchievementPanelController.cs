using System.Collections;
using UnityEngine;

/// <summary>
/// Controller for Achievement Panel. Handles showing/hiding panel and refreshing achievements.
/// </summary>
public class AchievementPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private AchievementListUI achievementListUI;
    [SerializeField] private GameObject mainMenuPanel; // Panel cần ẩn khi mở achievements
    
    [Header("Loading Indicator (Optional)")]
    [SerializeField] private GameObject loadingOverlay;
    
    [Header("Settings")]
    [SerializeField] private bool refreshOnShow = true;
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool hideMainMenuOnShow = true; // Ẩn MainMenu khi mở AchievementPanel

    void Start()
    {
        // Hide panel on start if enabled
        if (hideOnStart && achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Called when Show Achievements button is clicked.
    /// Shows panel and refreshes achievements from server.
    /// </summary>
    public void OnShowPanel()
    {
        if (achievementPanel == null)
        {
            Debug.LogWarning("[AchievementPanel] AchievementPanel GameObject is not assigned!");
            return;
        }

        // Ẩn MainMenuPanel nếu cần
        if (hideMainMenuOnShow && mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        achievementPanel.SetActive(true);

        if (refreshOnShow)
        {
            StartCoroutine(RefreshAndShow());
        }
        else if (achievementListUI != null)
        {
            // Just refresh the list with cached data
            achievementListUI.RefreshList();
        }
    }

    /// <summary>
    /// Called when Close button is clicked.
    /// Hides the achievement panel and shows MainMenu again.
    /// </summary>
    public void OnClosePanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }

        // Hiện lại MainMenuPanel
        if (hideMainMenuOnShow && mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Manually refresh achievements from server and update UI.
    /// Can be called from a Refresh button if needed.
    /// </summary>
    public void OnRefreshAchievements()
    {
        StartCoroutine(RefreshAndShow());
    }

    private IEnumerator RefreshAndShow()
    {
        SetLoading(true);

        // Refresh achievements from server
        if (AchievementManager.Instance != null)
        {
            yield return AchievementManager.Instance.RefreshAll();
        }
        else
        {
            Debug.LogWarning("[AchievementPanel] AchievementManager.Instance is null!");
        }

        // Update the list UI
        if (achievementListUI != null)
        {
            achievementListUI.RefreshList();
        }
        else
        {
            Debug.LogWarning("[AchievementPanel] AchievementListUI is not assigned!");
        }

        SetLoading(false);
    }

    private void SetLoading(bool isLoading)
    {
        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(isLoading);
        }
    }
}

