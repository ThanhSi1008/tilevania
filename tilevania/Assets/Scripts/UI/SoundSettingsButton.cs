using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to attach to a button to open the sound settings panel.
/// </summary>
public class SoundSettingsButton : MonoBehaviour
{
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
            Debug.Log($"[SoundSettingsButton] Attached to button: {gameObject.name}");
        }
        else
        {
            Debug.LogError($"[SoundSettingsButton] No Button component found on {gameObject.name}!");
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log("[SoundSettingsButton] Button clicked!");
        
        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        else
        {
            Debug.LogWarning("[SoundSettingsButton] AudioManager.Instance is null!");
        }

        // Find BootstrapAuth and show sound settings
        var bootstrapAuth = FindFirstObjectByType<BootstrapAuth>();
        if (bootstrapAuth != null)
        {
            Debug.Log("[SoundSettingsButton] Found BootstrapAuth, calling ShowSoundSettings()");
            bootstrapAuth.ShowSoundSettings();
        }
        else
        {
            Debug.LogError("[SoundSettingsButton] BootstrapAuth not found in scene! Make sure BootstrapAuth exists in the AuthScene.");
            
            // Try alternative: FindAnyObjectByType as fallback
            var fallbackAuth = FindAnyObjectByType<BootstrapAuth>();
            if (fallbackAuth != null)
            {
                Debug.Log("[SoundSettingsButton] Found BootstrapAuth using fallback method");
                fallbackAuth.ShowSoundSettings();
            }
            else
            {
                Debug.LogError("[SoundSettingsButton] BootstrapAuth not found even with fallback method!");
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

