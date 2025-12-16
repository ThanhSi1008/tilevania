using System;
using System.Collections;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Inputs")]
    [Tooltip("Email input field that will be sent to /api/auth/login.")]
    [SerializeField] private TMP_InputField emailInput;
    [Tooltip("Password input field.")]
    [SerializeField] private TMP_InputField passwordInput;

    [Header("UI")]
    [Tooltip("Login button. Should call OnLoginButtonClicked() on click.")]
    [SerializeField] private Button loginButton;
    [Tooltip("Overlay shown while waiting for the login API (typically a fullscreen panel).")]
    [SerializeField] private GameObject loadingOverlay;
    [Tooltip("Text used to display login status / error messages to the player.")]
    [SerializeField] private TextMeshProUGUI statusText;
    [Tooltip("Login panel (will be hidden after successful login).")]
    [SerializeField] private GameObject loginPanel;
    [Tooltip("Register panel (will be shown when player clicks the 'Register' button).")]
    [SerializeField] private GameObject registerPanel;
    [Tooltip("Main Menu panel (will be shown after successful login).")]
    [SerializeField] private GameObject mainMenuPanel;
    [Tooltip("Reference to MainMenuManager to call RefreshUI after login.")]
    [SerializeField] private MainMenuManager mainMenuManager;

    [Header("Usage Instructions (Editor)")]
    [TextArea(3, 6)]
    [SerializeField] private string usageInstructions =
        "- Attach LoginManager to a GameObject in the AuthScene.\n" +
        "- Assign emailInput and passwordInput to the two TMP_InputField components on your UI.\n" +
        "- Assign loginButton and add an OnClick event -> LoginManager.OnLoginButtonClicked.\n" +
        "- Assign loadingOverlay (a fullscreen panel). It should be disabled by default.\n" +
        "- Assign statusText (TextMeshProUGUI) to display errors: invalid credentials, network issues, etc.\n" +
        "- Assign loginPanel, registerPanel, and mainMenuPanel to the corresponding UI panels.\n" +
        "- Make sure AuthManager exists in the scene and APIConfig.API_BASE_URL points to your GameServer.";

    // This is only used in the Inspector as documentation; this read avoids CS0414 warning.
    private void Awake()
    {
        _ = usageInstructions;
    }

    [Serializable]
    private class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    private class LoginResponseUser
    {
        public string id;
        public string username;
        public string email;
    }

    [Serializable]
    private class LoginResponse
    {
        public bool success;
        public string token;
        public LoginResponseUser user;
        public string error;
    }

    public void OnLoginButtonClicked()
    {
        if (loadingOverlay != null && loadingOverlay.activeSelf) return;
        StartCoroutine(LoginRoutine());
    }

    // Called by GoRegisterButton OnClick to switch panels
    public void OnGoRegister()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        SetStatus(string.Empty);
    }

    /// <summary>
    /// Clear all input fields (email and password). Called when user logs out.
    /// </summary>
    public void ClearInputs()
    {
        if (emailInput != null)
        {
            emailInput.text = string.Empty;
        }
        if (passwordInput != null)
        {
            passwordInput.text = string.Empty;
        }
        SetStatus(string.Empty);
    }

    private IEnumerator LoginRoutine()
    {
        SetLoading(true);
        SetStatus("");

        var email = emailInput != null ? emailInput.text.Trim() : string.Empty;
        var pass = passwordInput != null ? passwordInput.text : string.Empty;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            SetLoading(false);
            SetStatus("Email and password must not be empty.");
            yield break;
        }

        // Basic client-side validation to catch common input errors early
        if (!email.Contains("@") || !email.Contains("."))
        {
            SetLoading(false);
            SetStatus("Please enter a valid email address.");
            yield break;
        }

        if (pass.Length < 6)
        {
            SetLoading(false);
            SetStatus("Password must be at least 6 characters long.");
            yield break;
        }

        var payload = new LoginRequest { email = email, password = pass };
        
        var json = JsonUtility.ToJson(payload);
        
        APIResponse<string> apiResult = null;
        // Auth headers không cần cho login; gửi request trần để tránh token cũ ảnh hưởng
        yield return APIClient.Post("/api/auth/login", json, r => apiResult = r, null);

        HandleLoginResponse(apiResult);
        SetLoading(false);
    }

    private void HandleLoginResponse(APIResponse<string> apiResult)
    {
        if (apiResult == null)
        {
            SetStatus("Login failed: no response from server.");
            return;
        }

        if (!apiResult.success)
        {
            // Provide user-friendly error messages
            string userMessage;
            if (apiResult.statusCode == HttpStatusCode.ServiceUnavailable || 
                apiResult.statusCode == HttpStatusCode.BadGateway ||
                (int)apiResult.statusCode == 0)
            {
                userMessage = "Cannot connect to server. Please check your internet connection or GameServer.";
            }
            else if (apiResult.statusCode == HttpStatusCode.RequestTimeout)
            {
                userMessage = "Request timeout. Server may be slow or unreachable.";
            }
            // Many backends use 401 or 400 for invalid credentials
            else if (apiResult.statusCode == HttpStatusCode.Unauthorized ||
                     apiResult.statusCode == HttpStatusCode.BadRequest)
            {
                userMessage = "Invalid email or password.";
            }
            else if (apiResult.statusCode == HttpStatusCode.NotFound)
            {
                userMessage = "Login endpoint not found. Please verify the API configuration.";
            }
            else
            {
                // Try to detect invalid credentials from server message
                var raw = !string.IsNullOrEmpty(apiResult.error)
                    ? apiResult.error
                    : (apiResult.data ?? string.Empty);

                if (!string.IsNullOrEmpty(raw) &&
                    raw.IndexOf("invalid", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (raw.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     raw.IndexOf("credential", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    userMessage = "Invalid email or password.";
                }
                else
                {
                    userMessage = !string.IsNullOrEmpty(apiResult.error)
                        ? apiResult.error
                        : (!string.IsNullOrEmpty(apiResult.data) ? apiResult.data : "Unknown error from server.");
                }
            }

            SetStatus($"Login error: {userMessage}");
            return;
        }

        var response = JsonUtility.FromJson<LoginResponse>(apiResult.data ?? "{}");
        if (response == null || string.IsNullOrEmpty(response.token))
        {
            // Treat missing token as invalid credentials from player perspective
            SetStatus("Login failed: invalid email or password.");
            return;
        }

        var player = new PlayerData
        {
            userId = response.user != null ? response.user.id : null,
            username = response.user != null ? response.user.username : null,
            email = response.user != null ? response.user.email : null
        };

        AuthManager.Instance?.SetAuth(response.token, player);
        SetStatus("Login successful!");

        // Play background music on successful login
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBackgroundMusic();
        }

        // Switch to main menu
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        mainMenuManager?.RefreshUI();
    }

    private void SetLoading(bool isLoading)
    {
        if (loadingOverlay != null) loadingOverlay.SetActive(isLoading);
        if (loginButton != null) loginButton.interactable = !isLoading;
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}

