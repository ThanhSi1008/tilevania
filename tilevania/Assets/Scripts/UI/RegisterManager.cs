using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("UI")]
    [SerializeField] private Button registerButton;
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainMenuPanel;
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

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            SetLoading(false, "Email, username, password không được trống");
            yield break;
        }

        var payload = new RegisterRequest { email = email, username = user, password = pass };

        var json = JsonUtility.ToJson(payload);
        APIResponse<string> apiResult = null;
        // Auth headers không cần cho register
        yield return APIClient.Post("/api/auth/register", json, r => apiResult = r, null);

        HandleRegisterResponse(apiResult);
        SetLoading(false, string.Empty);
    }

    private void HandleRegisterResponse(APIResponse<string> apiResult)
    {
        if (apiResult == null)
        {
            SetStatus("Register failed: no response");
            return;
        }

        if (!apiResult.success)
        {
            var msg = !string.IsNullOrEmpty(apiResult.error)
                ? apiResult.error
                : apiResult.data;
            SetStatus($"Register error ({(int)apiResult.statusCode}): {msg}");
            Debug.LogWarning($"Register failed status={(int)apiResult.statusCode} body={apiResult.data}");
            return;
        }

        var response = JsonUtility.FromJson<RegisterResponse>(apiResult.data ?? "{}");
        if (response == null || string.IsNullOrEmpty(response.token))
        {
            SetStatus("Register failed: invalid server response");
            Debug.LogWarning($"Register parse failed. Raw body: {apiResult.data}");
            return;
        }

        var player = new PlayerData
        {
            userId = response.user != null ? response.user.id : null,
            username = response.user != null ? response.user.username : null,
            email = response.user != null ? response.user.email : null
        };

        AuthManager.Instance?.SetAuth(response.token, player);
        SetStatus("Register successful");
        Debug.Log($"[Register] success user={player.username} tokenLen={response.token?.Length ?? 0}");

        // Sau khi đăng ký thành công, đã có token nên chuyển thẳng sang MainMenu
        if (registerPanel != null) registerPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        mainMenuManager?.RefreshUI();
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

