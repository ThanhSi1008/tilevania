using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;

    void Start()
    {
        UpdateUsername();
    }

    public void UpdateUsername()
    {
        var isAuthed = AuthManager.Instance != null && AuthManager.Instance.HasToken();
        var username = isAuthed && AuthManager.Instance.CurrentPlayer != null
            ? AuthManager.Instance.CurrentPlayer.username
            : "Guest";

        if (usernameText != null) usernameText.text = username;
    }
}

