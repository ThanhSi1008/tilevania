using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public string ActiveSessionId { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetActiveSession(string sessionId)
    {
        ActiveSessionId = sessionId;
    }

    public void ClearSession()
    {
        ActiveSessionId = null;
    }
}

