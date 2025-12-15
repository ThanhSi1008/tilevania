using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    private const string AUTH_SCENE = "AuthScene";

    public void GoToMainMenu()
    {
        Debug.Log("[BackToMainMenu] Loading AuthScene (Main Menu)");
        SessionManager.Instance?.ClearSession();
        SceneManager.LoadScene(AUTH_SCENE);
    }
}
