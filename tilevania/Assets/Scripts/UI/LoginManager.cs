using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("UI")]
    [SerializeField] private Button loginButton;
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private MainMenuManager mainMenuManager;

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

    private IEnumerator LoginRoutine()
    {
        SetLoading(true, "Signing in...");

        var email = emailInput != null ? emailInput.text.Trim() : string.Empty;
        var pass = passwordInput != null ? passwordInput.text : string.Empty;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            SetLoading(false, "Email và mật khẩu không được trống");
            yield break;
        }

        var payload = new LoginRequest { email = email, password = pass };

        var json = JsonUtility.ToJson(payload);
        APIResponse<string> apiResult = null;
        // Auth headers không cần cho login; gửi request trần để tránh token cũ ảnh hưởng
        yield return APIClient.Post("/api/auth/login", json, r => apiResult = r, null);

        HandleLoginResponse(apiResult);
        SetLoading(false, string.Empty);
    }

    private void HandleLoginResponse(APIResponse<string> apiResult)
    {
        if (apiResult == null)
        {
            SetStatus("Login failed: no response");
            return;
        }

        if (!apiResult.success)
        {
            var msg = !string.IsNullOrEmpty(apiResult.error)
                ? apiResult.error
                : apiResult.data;
            SetStatus($"Login error ({(int)apiResult.statusCode}): {msg}");
            Debug.LogWarning($"Login failed status={(int)apiResult.statusCode} body={apiResult.data}");
            return;
        }

        var response = JsonUtility.FromJson<LoginResponse>(apiResult.data ?? "{}");
        if (response == null || string.IsNullOrEmpty(response.token))
        {
            SetStatus("Login failed: invalid server response");
            Debug.LogWarning($"Login parse failed. Raw body: {apiResult.data}");
            return;
        }

        var player = new PlayerData
        {
            userId = response.user != null ? response.user.id : null,
            username = response.user != null ? response.user.username : null,
            email = response.user != null ? response.user.email : null
        };

        AuthManager.Instance?.SetAuth(response.token, player);
        SetStatus("Login successful");
        Debug.Log($"[Login] success user={player.username} tokenLen={response.token?.Length ?? 0}");

        // Switch to main menu
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        mainMenuManager?.RefreshUI();
    }

    private void SetLoading(bool isLoading, string message)
    {
        if (loadingOverlay != null) loadingOverlay.SetActive(isLoading);
        if (loginButton != null) loginButton.interactable = !isLoading;
        SetStatus(message);
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}

