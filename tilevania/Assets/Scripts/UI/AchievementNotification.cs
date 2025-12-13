using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class AchievementNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private float lifetimeSeconds = 3f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AudioClip unlockSound; // Sound to play when achievement unlocks
    [SerializeField] private float animationDuration = 0.5f; // Duration of entrance animation
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Scale animation curve
    [SerializeField] private AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Bounce animation curve
    
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector2 originalPosition;
    
    public event Action OnDestroyed; // Event fired when notification is destroyed

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;
            originalPosition = rectTransform.anchoredPosition;
        }
    }

    void OnEnable()
    {
        // Play sound when notification appears
        PlayUnlockSound();
        
        // Start entrance animation
        StartCoroutine(EntranceAnimation());
        
        // Start auto-hide after animation
        StartCoroutine(AutoHide());
    }

    public void SetContent(string title, string description)
    {
        if (titleText != null) titleText.text = string.IsNullOrEmpty(title) ? "Achievement Unlocked" : title;
        if (descriptionText != null) descriptionText.text = description ?? string.Empty;
    }

    private void PlayUnlockSound()
    {
        if (unlockSound != null)
        {
            // Play sound at camera position (2D sound)
            AudioSource.PlayClipAtPoint(unlockSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }
    }

    private IEnumerator EntranceAnimation()
    {
        if (rectTransform == null) yield break;
        
        // Reset scale to 0 for entrance effect
        rectTransform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        
        // Scale up animation with bounce
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Scale animation (0 -> 1.1 -> 1.0 for bounce effect)
            float scaleValue = scaleCurve.Evaluate(t);
            if (t < 0.7f)
            {
                // Bounce effect: overshoot then settle
                scaleValue = Mathf.Lerp(0f, 1.15f, t / 0.7f);
            }
            else
            {
                // Settle to normal scale
                scaleValue = Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f);
            }
            
            rectTransform.localScale = originalScale * scaleValue;
            
            // Optional: Add slight vertical bounce
            if (bounceCurve != null && bounceCurve.length > 0)
            {
                float bounceOffset = bounceCurve.Evaluate(t) * 20f; // 20 pixels bounce
                rectTransform.anchoredPosition = originalPosition + Vector2.up * bounceOffset;
            }
            
            yield return null;
        }
        
        // Ensure final scale and position
        rectTransform.localScale = originalScale;
        rectTransform.anchoredPosition = originalPosition;
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

        // Fire event before destroying
        OnDestroyed?.Invoke();
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        // Fire event in OnDestroy as backup (in case coroutine doesn't complete)
        OnDestroyed?.Invoke();
    }
}

