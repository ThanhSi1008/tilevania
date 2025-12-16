using UnityEngine;

public class ScenePersist : MonoBehaviour
{
    // Adjust these to match the persistent UI object names you want cleared
    // when returning to main menu. These are under the gameplay Canvas.
    private static readonly string[] DefaultCleanupNames = new[]
    {
        "Level Text",
        "Score Icon",
        "Score Text",
        "Lives Text",
        "MainMenuButton"
    };

    void Awake()
    {
        int count = FindObjectsByType<ScenePersist>(FindObjectsSortMode.None).Length;
        if (count > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public static void ResetAll()
    {
        // Clear (hide) specific UI objects (under the gameplay canvas) before removing ScenePersist
        HideObjectsByName(DefaultCleanupNames);

        ScenePersist[] persists =
            FindObjectsByType<ScenePersist>(FindObjectsSortMode.None);

        foreach (ScenePersist persist in persists)
        {
            Destroy(persist.gameObject);
        }
    }

    private static void HideObjectsByName(string[] names)
    {
        if (names == null || names.Length == 0) return;

        var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            foreach (var n in names)
            {
                if (go.name == n)
                {
                    // Thay vì Destroy, chỉ ẩn để khi quay lại gameplay có thể kích hoạt lại
                    go.SetActive(false);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Reactivate HUD objects that were hidden when returning to MainMenu.
    /// Call this when entering a gameplay scene before trying to find HUD references.
    /// </summary>
    public static void ShowObjectsByName(string[] names)
    {
        if (names == null || names.Length == 0) return;

        var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            foreach (var n in names)
            {
                if (go.name == n)
                {
                    go.SetActive(true);
                    break;
                }
            }
        }
    }

    public static void ReactivateHud()
    {
        ShowObjectsByName(DefaultCleanupNames);
    }

    public void ResetScenePersist()
    {
        ResetAll();
    }

}
