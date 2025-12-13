using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] Vector2 deathKick = new Vector2(10f, 20f);
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;

    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    float gravityScaleAtStart;
    BoxCollider2D myFeetCollider;

    bool isAlive = true;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        gravityScaleAtStart = myRigidbody.gravityScale;
        myFeetCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (!isAlive) { return; }
        // Block all movement if level is loading
        if (LevelExit.IsLoading) { return; }
        Run();
        FlipSprite();
        ClimbLadder();
        Die();
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) { return; }
        // Block input if level is loading
        if (LevelExit.IsLoading) 
        { 
            moveInput = Vector2.zero;
            return; 
        }
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        // Block input if level is loading
        if (LevelExit.IsLoading) { return; }
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) { return; }
        if (value.isPressed)
        {
            // do stuff
            myRigidbody.linearVelocity += new Vector2(0f, jumpSpeed);
        }
    }

    void Run()
    {
        // Block movement if level is loading
        if (LevelExit.IsLoading) 
        { 
            // Stop player movement during loading
            myRigidbody.linearVelocity = new Vector2(0f, myRigidbody.linearVelocity.y);
            myAnimator.SetBool("isRunning", false);
            return; 
        }
        Vector2 playerVelocity = new Vector2(moveInput.x * moveSpeed, myRigidbody.linearVelocity.y);
        myRigidbody.linearVelocity = playerVelocity;
        bool hasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", hasHorizontalSpeed);

    }

    void FlipSprite()
    {
        bool hasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        if (hasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.linearVelocity.x), 1f);
        }
    }

    void ClimbLadder()
    {
        // Block climbing if level is loading
        if (LevelExit.IsLoading) { return; }
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            myRigidbody.gravityScale = gravityScaleAtStart;
            myAnimator.SetBool("isClimbing", false);
            return;
        }
        myRigidbody.gravityScale = 0f;
        Vector2 climbVelocity = new Vector2(myRigidbody.linearVelocity.x, moveInput.y * climbSpeed);
        myRigidbody.linearVelocity = climbVelocity;

        bool hasVerticalSpeed = Mathf.Abs(myRigidbody.linearVelocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("isClimbing", hasVerticalSpeed);
    }

    void OnAttack(InputValue value)
    {
        if (!isAlive) { return; }
        // Block input if level is loading
        if (LevelExit.IsLoading) { return; }
        Instantiate(bullet, gun.position, transform.rotation);
    }

    void Die()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            isAlive = false;
            myAnimator.SetTrigger("Dying");
            myRigidbody.linearVelocity = deathKick;
            
            // Try to find GameSession - use FindFirstObjectByType for better reliability
            var gameSession = FindFirstObjectByType<GameSession>();
            if (gameSession != null)
            {
                gameSession.ProcessPlayerDeath();
            }
            else
            {
                // Try FindAnyObjectByType as fallback
                gameSession = FindAnyObjectByType<GameSession>();
                if (gameSession != null)
                {
                    gameSession.ProcessPlayerDeath();
                }
                else
                {
                    Debug.LogWarning("[PlayerMovement] GameSession not found, cannot process death. GameSession may have been destroyed or not yet created. Waiting and retrying...");
                    // Wait a frame and retry - GameSession might still be initializing
                    StartCoroutine(RetryFindGameSessionAndProcessDeath());
                }
            }
        }
    }
    
    private System.Collections.IEnumerator RetryFindGameSessionAndProcessDeath()
    {
        // Wait longer for GameSession to be created/initialized
        // GameSession might be created in OnSceneLoaded or Start(), which can take a few frames
        int maxRetries = 10;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            yield return null; // Wait one frame
            
            var gameSession = FindFirstObjectByType<GameSession>();
            if (gameSession != null)
            {
                Debug.Log($"[PlayerMovement] Found GameSession after {retryCount + 1} retries, processing death");
                gameSession.ProcessPlayerDeath();
                yield break; // Exit coroutine
            }
            
            retryCount++;
        }
        
        // If still not found after all retries, try one more time with FindAnyObjectByType
        var gameSessionFallback = FindAnyObjectByType<GameSession>();
        if (gameSessionFallback != null)
        {
            Debug.Log("[PlayerMovement] Found GameSession with FindAnyObjectByType after all retries, processing death");
            gameSessionFallback.ProcessPlayerDeath();
        }
        else
        {
            Debug.LogError("[PlayerMovement] GameSession still not found after all retries. Reloading scene as fallback.");
            // If GameSession is still not found, reload the current scene (basic death handling)
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
        }
    }

}
