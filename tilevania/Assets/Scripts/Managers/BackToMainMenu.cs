using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    private const string AUTH_SCENE = "AuthScene";

    public void GoToMainMenu()
    {
        Debug.Log("Returning to Main Menu...");

        ScenePersist.ResetAll();
        SessionManager.Instance?.ClearSession();

        SceneManager.LoadScene(AUTH_SCENE);
    }

}
