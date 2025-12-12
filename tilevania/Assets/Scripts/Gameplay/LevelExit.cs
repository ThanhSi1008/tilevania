using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] float levelLoadDelay = 1f;
    private bool isProcessing = false; // Prevent multiple triggers

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isProcessing)
        {
            isProcessing = true;
            Debug.Log("[LevelExit] Player reached level exit! Starting level completion process...");
            StartCoroutine(LoadNextLevel());
        }
        else if (isProcessing)
        {
            Debug.Log("[LevelExit] Level completion already in progress, ignoring trigger");
        }
    }

    IEnumerator LoadNextLevel()
    {
        Debug.Log($"[LevelExit] LoadNextLevel started - Waiting {levelLoadDelay} seconds...");
        yield return new WaitForSecondsRealtime(levelLoadDelay);
        
        // End current session before loading next level
        var gameSession = FindFirstObjectByType<GameSession>();
        if (gameSession != null)
        {
            Debug.Log("[LevelExit] Found GameSession, calling EndSession(COMPLETED)...");
            // Sync final stats and end session (this will also check achievements and show notifications)
            yield return gameSession.EndSession("COMPLETED");
            Debug.Log("[LevelExit] EndSession completed!");
        }
        else
        {
            Debug.LogWarning("[LevelExit] GameSession not found! Cannot end session properly.");
        }
        
        // Wait a bit more to let players see achievement notifications
        Debug.Log("[LevelExit] Waiting 2 seconds for achievement notifications...");
        yield return new WaitForSecondsRealtime(2f);
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("[LevelExit] Reached last level, returning to scene 0 (MainMenu)");
            nextSceneIndex = 0;
        }
        else
        {
            Debug.Log($"[LevelExit] Loading next level: Scene {nextSceneIndex}");
        }

        FindFirstObjectByType<ScenePersist>().ResetScenePersist();
        SceneManager.LoadScene(nextSceneIndex);
    }
}
