using System.Collections;
using TMPro;
using UnityEngine;

public class AchievementNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private float lifetimeSeconds = 3f;
    [SerializeField] private CanvasGroup canvasGroup;

    void OnEnable()
    {
        StartCoroutine(AutoHide());
    }

    public void SetContent(string title, string description)
    {
        if (titleText != null) titleText.text = string.IsNullOrEmpty(title) ? "Achievement Unlocked" : title;
        if (descriptionText != null) descriptionText.text = description ?? string.Empty;
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(lifetimeSeconds);

        // Simple fade out
        if (canvasGroup != null)
        {
            const float fadeDuration = 0.5f;
            float t = 0f;
            var startAlpha = canvasGroup.alpha;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}

