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
                var unlocked = manager.UnlockedAchievements != null &&
                               manager.UnlockedAchievements.Any(p => p.achievementId != null && p.achievementId._id == ach._id);
                item.SetData(ach.name, ach.description, ach.points, unlocked);
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

