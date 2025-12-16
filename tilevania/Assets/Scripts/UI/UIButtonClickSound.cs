using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically plays a button click sound when attached to a UI Button.
/// </summary>
public class UIButtonClickSound : MonoBehaviour
{
    [Header("Optional Override")]
    [SerializeField] private AudioClip overrideClip; // Optional: use a different sound for this specific button

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning($"[UIButtonClickSound] No Button component found on {gameObject.name}");
        }
    }

    private void OnButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            if (overrideClip != null)
            {
                AudioManager.Instance.PlayButtonClick(overrideClip);
            }
            else
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}

