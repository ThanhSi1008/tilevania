using System.Collections;
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

    [SerializeField] float dashSpeed = 30f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float dashCooldown = 0.5f;

    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    float gravityScaleAtStart;

    bool isAlive = true;
    bool isDashing;
    float lastDashTime;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        gravityScaleAtStart = myRigidbody.gravityScale;
    }

    void Update()
    {
        if (!isAlive) return;
        if (LevelExit.IsLoading) return;

        Run();
        FlipSprite();
        ClimbLadder();
        Die();
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) return;

        if (LevelExit.IsLoading)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (LevelExit.IsLoading) return;
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) return;

        if (value.isPressed)
        {
            myRigidbody.linearVelocity += new Vector2(0f, jumpSpeed);
        }
    }

    void OnDash(InputValue value)
    {
        if (!isAlive) return;
        if (LevelExit.IsLoading) return;
        if (isDashing) return;
        if (Time.time < lastDashTime + dashCooldown) return;

        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        float dashDir = transform.localScale.x;
        myRigidbody.gravityScale = 0f;
        myRigidbody.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        myRigidbody.gravityScale = gravityScaleAtStart;
        isDashing = false;
    }

    void Run()
    {
        if (LevelExit.IsLoading)
        {
            myRigidbody.linearVelocity = new Vector2(0f, myRigidbody.linearVelocity.y);
            myAnimator.SetBool("isRunning", false);
            return;
        }

        if (isDashing) return;

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
        if (LevelExit.IsLoading) return;

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
        if (!isAlive) return;
        if (LevelExit.IsLoading) return;

        Instantiate(bullet, gun.position, transform.rotation);
    }

    void Die()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            isAlive = false;
            myAnimator.SetTrigger("Dying");
            myRigidbody.linearVelocity = deathKick;

            var gameSession = FindFirstObjectByType<GameSession>();
            if (gameSession != null)
            {
                gameSession.ProcessPlayerDeath();
            }
            else
            {
                StartCoroutine(RetryFindGameSessionAndProcessDeath());
            }
        }
    }

    IEnumerator RetryFindGameSessionAndProcessDeath()
    {
        int retries = 10;

        while (retries-- > 0)
        {
            yield return null;
            var gameSession = FindFirstObjectByType<GameSession>();
            if (gameSession != null)
            {
                gameSession.ProcessPlayerDeath();
                yield break;
            }
        }

        var fallback = FindAnyObjectByType<GameSession>();
        if (fallback != null)
        {
            fallback.ProcessPlayerDeath();
        }
        else
        {
            int index = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene(index);
        }
    }
}
