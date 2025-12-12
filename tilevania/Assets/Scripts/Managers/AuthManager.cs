using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [SerializeField] private string prefsKey = "auth_token";
    [SerializeField] private string xorKey = "tilevania-key";

    public string Token { get; private set; }
    public PlayerData CurrentPlayer { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Token = LoadToken();
        Debug.Log($"[AuthManager] Awake - loaded token null? {string.IsNullOrEmpty(Token)}");
    }

    public bool HasToken() => !string.IsNullOrEmpty(Token);

    public void SetAuth(string token, PlayerData player)
    {
        Token = token;
        CurrentPlayer = player;
        SaveToken(token);
        Debug.Log($"[AuthManager] SetAuth - token saved length={token?.Length ?? 0}, user={player?.username}");
    }

    public void ClearAuth()
    {
        Token = null;
        CurrentPlayer = null;
        PlayerPrefs.DeleteKey(prefsKey);
        Debug.Log("[AuthManager] ClearAuth - token cleared");
    }

    public Dictionary<string, string> BuildAuthHeaders()
    {
        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(Token))
        {
            headers["Authorization"] = $"Bearer {Token}";
        }
        return headers;
    }

    public IEnumerator ValidateTokenCoroutine(Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(Token))
        {
            onComplete?.Invoke(false);
            yield break;
        }

        APIResponse<string> apiResult = null;
        yield return APIClient.Get("/api/users/me", r => apiResult = r, BuildAuthHeaders());

        var ok = apiResult != null && apiResult.success;
        if (ok && !string.IsNullOrEmpty(apiResult.data))
        {
            try
            {
                // Server returns: { user: { id, username, email, ... } }
                // Need to parse wrapper object first
                var wrapper = JsonUtility.FromJson<UserResponseWrapper>(apiResult.data);
                if (wrapper != null && wrapper.user != null)
                {
                    CurrentPlayer = new PlayerData
                    {
                        userId = wrapper.user.id,
                        username = wrapper.user.username,
                        email = wrapper.user.email
                    };
                }
                else
                {
                    ok = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AuthManager] Failed to parse user response: {ex.Message}. Raw: {apiResult.data}");
                ok = false;
            }
        }
        else
        {
            Debug.LogWarning($"[AuthManager] Token validation failed. Success: {ok}, Status: {apiResult?.statusCode}, Error: {apiResult?.error}, Data: {apiResult?.data}");
        }

        if (!ok)
        {
            ClearAuth();
        }

        onComplete?.Invoke(ok);
    }

    [Serializable]
    private class UserResponseWrapper
    {
        public UserResponseData user;
    }

    [Serializable]
    private class UserResponseData
    {
        public string id;
        public string username;
        public string email;
    }

    private void SaveToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return;
        var encrypted = Encrypt(token);
        PlayerPrefs.SetString(prefsKey, encrypted);
        PlayerPrefs.Save();
        Debug.Log($"[AuthManager] SaveToken - encrypted length={encrypted.Length}");
    }

    private string LoadToken()
    {
        if (!PlayerPrefs.HasKey(prefsKey)) return null;
        var encrypted = PlayerPrefs.GetString(prefsKey);
        var decrypted = string.IsNullOrEmpty(encrypted) ? null : Decrypt(encrypted);
        Debug.Log($"[AuthManager] LoadToken - hasKey={PlayerPrefs.HasKey(prefsKey)}, encryptedLen={encrypted?.Length ?? 0}, decryptedNull={decrypted==null}");
        return decrypted;
    }

    private string Encrypt(string plain)
    {
        var bytes = Encoding.UTF8.GetBytes(plain);
        var keyBytes = Encoding.UTF8.GetBytes(xorKey);
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= keyBytes[i % keyBytes.Length];
        }
        return Convert.ToBase64String(bytes);
    }

    private string Decrypt(string encrypted)
    {
        var bytes = Convert.FromBase64String(encrypted);
        var keyBytes = Encoding.UTF8.GetBytes(xorKey);
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= keyBytes[i % keyBytes.Length];
        }
        return Encoding.UTF8.GetString(bytes);
    }
}

