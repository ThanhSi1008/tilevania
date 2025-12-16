using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] int pointsForCoinPickup = 100;

    bool wasCollected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !wasCollected)
        {
            wasCollected = true;
            var gameSession = FindFirstObjectByType<GameSession>();
            if (gameSession != null)
            {
                gameSession.AddToScore(pointsForCoinPickup);
                gameSession.AddCoin(); // Track coin collection
            }
            
            // Play coin pickup sound through AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoinPickup();
            }
            
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
