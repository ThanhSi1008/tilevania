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
        "Lives Text"
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
        // Clear specific UI objects (under the gameplay canvas) before removing ScenePersist
        DestroyObjectsByName(DefaultCleanupNames);

        ScenePersist[] persists =
            FindObjectsByType<ScenePersist>(FindObjectsSortMode.None);

        foreach (ScenePersist persist in persists)
        {
            Destroy(persist.gameObject);
        }
    }

    private static void DestroyObjectsByName(string[] names)
    {
        if (names == null || names.Length == 0) return;

        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            foreach (var n in names)
            {
                if (go.name == n)
                {
                    Destroy(go);
                    break;
                }
            }
        }
    }

    public void ResetScenePersist()
    {
        ResetAll();
    }

}
