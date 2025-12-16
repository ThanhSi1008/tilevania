using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    [Header("Inputs")]
    [Tooltip("Email input field for registration.")]
    [SerializeField] private TMP_InputField emailInput;
    [Tooltip("Username input field for registration.")]
    [SerializeField] private TMP_InputField usernameInput;
    [Tooltip("Password input field for registration.")]
    [SerializeField] private TMP_InputField passwordInput;

    [Header("UI")]
    [Tooltip("Register button. Should call OnRegisterButtonClicked() on click.")]
    [SerializeField] private Button registerButton;
    [Tooltip("Overlay shown while waiting for the register API.")]
    [SerializeField] private GameObject loadingOverlay;
    [Tooltip("Text used to display registration status / error messages.")]
    [SerializeField] private TextMeshProUGUI statusText;
    [Tooltip("Register panel (will be hidden after successful registration).")]
    [SerializeField] private GameObject registerPanel;
    [Tooltip("Login panel (will be shown when user clicks 'Back to Login').")]
    [SerializeField] private GameObject loginPanel;
    [Tooltip("Main Menu panel (will be shown after successful registration).")]
    [SerializeField] private GameObject mainMenuPanel;
    [Tooltip("Reference to MainMenuManager to refresh UI after registration.")]
    [SerializeField] private MainMenuManager mainMenuManager;

    [Serializable]
    private class RegisterRequest
    {
        public string email;
        public string username;
        public string password;
    }

    [Serializable]
    private class RegisterResponseUser
    {
        public string id;
        public string username;
        public string email;
    }

    [Serializable]
    private class RegisterResponse
    {
        public bool success;
        public string token;
        public RegisterResponseUser user;
        public string error;
    }

    public void OnRegisterButtonClicked()
    {
        if (loadingOverlay != null && loadingOverlay.activeSelf) return;
        StartCoroutine(RegisterRoutine());
    }

    // Called by GoLoginButton to switch back to login panel
    public void OnGoLogin()
    {
        if (registerPanel != null) registerPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(true);
        SetStatus(string.Empty);
    }

    private IEnumerator RegisterRoutine()
    {
        SetLoading(true, "Creating account...");

        var email = emailInput != null ? emailInput.text.Trim() : string.Empty;
        var user = usernameInput != null ? usernameInput.text.Trim() : string.Empty;
        var pass = passwordInput != null ? passwordInput.text : string.Empty;

        // Basic client-side validation to catch common input errors early
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            SetLoading(false, "Email, username, and password must not be empty.");
            yield break;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            SetLoading(false, "Please enter a valid email address.");
            yield break;
        }

        if (user.Length < 3 || user.Length > 20)
        {
            SetLoading(false, "Username must be between 3 and 20 characters.");
            yield break;
        }

        if (pass.Length < 6)
        {
            SetLoading(false, "Password must be at least 6 characters long.");
            yield break;
        }

        var payload = new RegisterRequest { email = email, username = user, password = pass };

        var json = JsonUtility.ToJson(payload);
        APIResponse<string> apiResult = null;
        // Auth headers are not required for register
        yield return APIClient.Post("/api/auth/register", json, r => apiResult = r, null);

        HandleRegisterResponse(apiResult);
        SetLoading(false, string.Empty);
    }

    private void HandleRegisterResponse(APIResponse<string> apiResult)
    {
        if (apiResult == null)
        {
            SetStatus("Register failed: no response from server.");
            return;
        }

        if (!apiResult.success)
        {
            // Try to provide user-friendly error messages
            string userMessage;
            if (!string.IsNullOrEmpty(apiResult.error))
            {
                userMessage = apiResult.error;
            }
            else if (!string.IsNullOrEmpty(apiResult.data))
            {
                userMessage = apiResult.data;
            }
            else
            {
                userMessage = "Unknown error from server.";
            }

            SetStatus($"Register error ({(int)apiResult.statusCode}): {userMessage}");
            return;
        }

        var response = JsonUtility.FromJson<RegisterResponse>(apiResult.data ?? "{}");
        if (response == null || string.IsNullOrEmpty(response.token))
        {
            SetStatus("Register failed: invalid server response.");
            return;
        }

        var player = new PlayerData
        {
            userId = response.user != null ? response.user.id : null,
            username = response.user != null ? response.user.username : null,
            email = response.user != null ? response.user.email : null
        };

        AuthManager.Instance?.SetAuth(response.token, player);
        SetStatus("Register successful. Please login with your new account.");

        // Play background music on successful registration
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBackgroundMusic();
        }

        // After successful registration, switch to Login panel instead of MainMenu
        if (registerPanel != null) registerPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(true);
    }

    private void SetLoading(bool isLoading, string message)
    {
        if (loadingOverlay != null) loadingOverlay.SetActive(isLoading);
        if (registerButton != null) registerButton.interactable = !isLoading;
        SetStatus(message);
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}

