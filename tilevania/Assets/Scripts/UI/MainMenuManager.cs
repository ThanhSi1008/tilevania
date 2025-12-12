using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private GameObject loggedInPanel;
    [SerializeField] private GameObject loggedOutPanel;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button playButton;
    
    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "Level 1"; // Tên scene gameplay trong Build Settings

    void Start()
    {
        RefreshUI();
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
    }

    public void RefreshUI()
    {
        var isAuthed = AuthManager.Instance != null && AuthManager.Instance.HasToken();
        
        // Show/hide panels based on auth status
        if (loggedInPanel != null) loggedInPanel.SetActive(isAuthed);
        if (loggedOutPanel != null) loggedOutPanel.SetActive(!isAuthed);
        
        // If only one panel is set, use it for both states
        if (loggedInPanel == null && loggedOutPanel != null)
        {
            loggedOutPanel.SetActive(true); // Always show if no loggedInPanel
        }
        else if (loggedOutPanel == null && loggedInPanel != null)
        {
            loggedInPanel.SetActive(true); // Always show if no loggedOutPanel
        }

        var username = isAuthed && AuthManager.Instance.CurrentPlayer != null
            ? AuthManager.Instance.CurrentPlayer.username
            : "Guest";

        if (usernameText != null) usernameText.text = $"Welcome, {username}";
    }

    public void OnLogoutClicked()
    {
        AuthManager.Instance?.ClearAuth();
        RefreshUI();
    }

    // Called when PlayButton is clicked - loads gameplay scene
    public void OnPlayClicked()
    {
        // Check if user is logged in (optional - remove if you want guest play)
        if (AuthManager.Instance == null || !AuthManager.Instance.HasToken())
        {
            Debug.LogWarning("Please login before playing");
            // Optionally show error message to user
            return;
        }

        // Load gameplay scene
        if (string.IsNullOrEmpty(gameplaySceneName))
        {
            Debug.LogError("Gameplay scene name is not set in MainMenuManager!");
            return;
        }

        Debug.Log($"Loading scene: {gameplaySceneName}");
        SceneManager.LoadScene(gameplaySceneName);
    }
}

