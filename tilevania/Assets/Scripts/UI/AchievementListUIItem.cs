using TMPro;
using UnityEngine;

public class AchievementListUIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private GameObject unlockedBadge;

    public void SetData(string title, string description, int points, bool unlocked)
    {
        if (titleText != null) titleText.text = title ?? "Achievement";
        if (descriptionText != null) descriptionText.text = description ?? string.Empty;
        if (pointsText != null) pointsText.text = $"+{points} pts";
        if (unlockedBadge != null) unlockedBadge.SetActive(unlocked);
    }
}

