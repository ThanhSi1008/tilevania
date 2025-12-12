using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] AudioClip coinPickupSFX;
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
            AudioSource.PlayClipAtPoint(coinPickupSFX, transform.position);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
