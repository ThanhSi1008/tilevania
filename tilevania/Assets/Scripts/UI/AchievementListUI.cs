using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AchievementListUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private TextMeshProUGUI emptyStateText;

    private readonly List<GameObject> spawnedItems = new List<GameObject>();

    void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        ClearExisting();

        var manager = AchievementManager.Instance;
        if (manager == null || manager.AllAchievements == null || manager.AllAchievements.Count == 0)
        {
            ShowEmpty("No achievements available.");
            return;
        }

        foreach (var ach in manager.AllAchievements)
        {
            var go = Instantiate(itemPrefab, contentParent != null ? contentParent : transform);
            spawnedItems.Add(go);

            var item = go.GetComponent<AchievementListUIItem>();
            if (item != null)
            {
                var unlockedAchievement = manager.UnlockedAchievements != null
                    ? manager.UnlockedAchievements.FirstOrDefault(p => p.achievementId != null && p.achievementId._id == ach._id)
                    : null;
                
                // Determine unlocked status: progress >= 100 OR unlockedAt is not null
                bool unlocked = unlockedAchievement != null && 
                               (unlockedAchievement.progress >= 100 || !string.IsNullOrEmpty(unlockedAchievement.unlockedAt));
                int progress = unlockedAchievement != null ? unlockedAchievement.progress : 0;
                
                // Server now returns progress for ALL achievements (including locked ones)
                // So unlockedAchievement should always be found, but we check progress to determine unlock status
                item.SetData(ach.name, ach.description, ach.points, unlocked, progress, ach.condition);
            }
        }

        ShowEmpty(spawnedItems.Count == 0 ? "No achievements available." : string.Empty);
    }

    private void ClearExisting()
    {
        foreach (var go in spawnedItems)
        {
            if (go != null) Destroy(go);
        }
        spawnedItems.Clear();
    }

    private void ShowEmpty(string message)
    {
        if (emptyStateText != null)
        {
            emptyStateText.text = message;
            emptyStateText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }
}

