using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementListUIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private GameObject unlockedBadge;
    [SerializeField] private TextMeshProUGUI badgeText; // Reference to BadgeText inside UnlockedBadge
    [SerializeField] private TextMeshProUGUI progressText; // Text to show progress (e.g., "3/100 enemies")
    [SerializeField] private Slider progressBar; // Optional progress bar
    [SerializeField] private Image progressBarFill; // Optional progress bar fill image
    
    public void SetData(string title, string description, int points, bool unlocked, int progress = 0, string condition = null)
    {
        if (titleText != null) titleText.text = title ?? "Achievement";
        if (descriptionText != null) descriptionText.text = description ?? string.Empty;
        if (pointsText != null) pointsText.text = $"+{points} pts";
        if (unlockedBadge != null) unlockedBadge.SetActive(unlocked);
        
        // Set badge text with a safe character that exists in most fonts
        // Using "[OK]" or "UNLOCKED" instead of checkmark (✓) which may not be in font
        if (badgeText != null)
        {
            // Use "[OK]" or just "UNLOCKED" - both are safe ASCII characters
            badgeText.text = "UNLOCKED";
        }
        else if (unlockedBadge != null)
        {
            // Try to find BadgeText as a child if not assigned
            var badgeTextChild = unlockedBadge.transform.Find("BadgeText");
            if (badgeTextChild != null)
            {
                var textComponent = badgeTextChild.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = "UNLOCKED";
                }
            }
        }
        
        // Display progress for unlocked achievements
        if (unlocked)
        {
            // Hide progress UI for unlocked achievements
            if (progressText != null) progressText.gameObject.SetActive(false);
            if (progressBar != null) progressBar.gameObject.SetActive(false);
        }
        else
        {
            // Show progress for locked achievements
            UpdateProgress(progress, condition);
        }
    }
    
    private void UpdateProgress(int progress, string condition)
    {
        // Progress is 0-100 from server
        float progressPercent = Mathf.Clamp01(progress / 100f);
        
        // Update progress text
        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
            
            // Try to parse condition to show meaningful progress (e.g., "3/100 enemies")
            if (!string.IsNullOrEmpty(condition))
            {
                // Parse condition string to extract target number
                // Example conditions: "Collect 100 coins", "Kill 50 enemies", "Score 1000 points"
                int targetNumber = ExtractTargetNumber(condition);
                if (targetNumber > 0)
                {
                    int currentProgress = Mathf.RoundToInt(progressPercent * targetNumber);
                    string progressDisplay = FormatProgressText(condition, currentProgress, targetNumber);
                    progressText.text = progressDisplay;
                }
                else
                {
                    // Fallback to percentage
                    progressText.text = $"{progress}%";
                }
            }
            else
            {
                // Fallback to percentage
                progressText.text = $"{progress}%";
            }
        }
        
        // Update progress bar
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = progressPercent;
        }
        
        // Update progress bar fill image
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progressPercent;
        }
    }
    
    private int ExtractTargetNumber(string condition)
    {
        if (string.IsNullOrEmpty(condition)) return 0;
        
        // Try to find numbers in the condition string
        // Examples: 
        // - "Collect 100 coins" -> 100
        // - "Kill 50 enemies" -> 50
        // - "COIN_COLLECTOR_100" -> 100
        // - "KILLER_100" -> 100
        // - "SCORE_MASTER_1000" -> 1000
        
        // First, try splitting by space (for human-readable format)
        string[] words = condition.Split(' ');
        foreach (var word in words)
        {
            if (int.TryParse(word, out int number) && number > 0)
            {
                return number;
            }
        }
        
        // If no number found in spaces, try splitting by underscore (for enum format like COIN_COLLECTOR_100)
        string[] parts = condition.Split('_');
        foreach (var part in parts)
        {
            if (int.TryParse(part, out int number) && number > 0)
            {
                return number;
            }
        }
        
        // Try to extract number from anywhere in the string (fallback)
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\d+");
        var match = regex.Match(condition);
        if (match.Success && int.TryParse(match.Value, out int extractedNumber) && extractedNumber > 0)
        {
            return extractedNumber;
        }
        
        return 0;
    }
    
    private string FormatProgressText(string condition, int current, int target)
    {
        if (string.IsNullOrEmpty(condition)) return $"{current}/{target}";
        
        string conditionLower = condition.ToLower();
        
        // Format progress text based on condition type
        // Handle both enum format (COIN_COLLECTOR_100) and human-readable format (Collect 100 coins)
        if (conditionLower.Contains("coin") || conditionLower.Contains("collector"))
        {
            return $"{current}/{target} coins";
        }
        else if (conditionLower.Contains("enem") || conditionLower.Contains("kill") || conditionLower.Contains("killer"))
        {
            return $"{current}/{target} enemies";
        }
        else if (conditionLower.Contains("score") || conditionLower.Contains("master"))
        {
            return $"{current}/{target} points";
        }
        else if (conditionLower.Contains("level"))
        {
            return $"{current}/{target} levels";
        }
        else if (conditionLower.Contains("death"))
        {
            return $"{current}/{target} deaths";
        }
        else if (conditionLower.Contains("playtime") || conditionLower.Contains("time") || conditionLower.Contains("hour") || conditionLower.Contains("day"))
        {
            return $"{current}/{target} seconds";
        }
        else
        {
            // Generic format
            return $"{current}/{target}";
        }
    }
}

