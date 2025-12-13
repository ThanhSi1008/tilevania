using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
        
        // Validate ScrollRect components in the achievement panel
        ValidateScrollRects();
    }
    
    /// <summary>
    /// Validates all ScrollRect components in the achievement panel to ensure Content is assigned.
    /// This prevents UnassignedReferenceException errors.
    /// Disables ScrollRect if Content cannot be found to prevent runtime errors.
    /// </summary>
    private void ValidateScrollRects()
    {
        if (achievementPanel == null) return;
        
        var scrollRects = achievementPanel.GetComponentsInChildren<ScrollRect>(true);
        foreach (var scrollRect in scrollRects)
        {
            if (scrollRect.content == null)
            {
                Debug.LogWarning($"[AchievementPanel] ScrollRect on '{scrollRect.gameObject.name}' has no Content assigned! " +
                              "Attempting to auto-find Content...");
                
                // Try to find Content automatically
                var viewport = scrollRect.viewport;
                if (viewport != null)
                {
                    // Look for a child named "Content" or any direct child
                    Transform contentTransform = null;
                    foreach (Transform child in viewport.transform)
                    {
                        if (child.name.Contains("Content") || child.name.Contains("content"))
                        {
                            contentTransform = child;
                            break;
                        }
                    }
                    
                    // If not found, use first child
                    if (contentTransform == null && viewport.transform.childCount > 0)
                    {
                        contentTransform = viewport.transform.GetChild(0);
                    }
                    
                    if (contentTransform != null)
                    {
                        scrollRect.content = contentTransform.GetComponent<RectTransform>();
                        Debug.Log($"[AchievementPanel] ✅ Auto-assigned Content for ScrollRect on '{scrollRect.gameObject.name}': {contentTransform.name}");
                    }
                }
                
                // If still no content, disable the ScrollRect to prevent errors
                if (scrollRect.content == null)
                {
                    Debug.LogError($"[AchievementPanel] ❌ Could not find Content for ScrollRect on '{scrollRect.gameObject.name}'. " +
                                  "Disabling ScrollRect to prevent errors. Please manually assign Content in the Inspector.");
                    scrollRect.enabled = false;
                }
            }
            else
            {
                // Ensure ScrollRect is enabled if Content is assigned
                if (!scrollRect.enabled)
                {
                    scrollRect.enabled = true;
                    Debug.Log($"[AchievementPanel] ✅ Re-enabled ScrollRect on '{scrollRect.gameObject.name}' (Content is now assigned)");
                }
            }
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

        // Validate ScrollRects before showing panel to prevent errors
        ValidateScrollRects();

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

