using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset playerControls;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.15f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth health;

    // While this is above zero the player is being knocked back and
    // movement input is ignored, so the hit push is not overwritten.
    private float knockbackTimer;

    private InputAction moveAction;
    private InputAction jumpAction;

    private float coyoteCounter;
    private float jumpBufferCounter;

    // Jump release is caught in Update (every frame) and applied in the next
    // FixedUpdate, so a quick tap is never missed between physics steps.
    private bool jumpCutQueued;

    // Right after a jump the ground check still touches the floor for a few
    // frames (longer on a short hop). Ignore the ground until the player stops
    // rising, so spamming jump can't start a second jump. The time limit is
    // only a safety cap in case something keeps the player moving up.
    private const float groundIgnoreAfterJump = 0.5f;
    private float groundIgnoreTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<PlayerHealth>();

        if (playerControls == null)
        {
            Debug.LogError("PlayerControls is not assigned!");
            enabled = false;
            return;
        }

        moveAction = playerControls.FindAction("Player/Move");
        jumpAction = playerControls.FindAction("Player/Jump");

        if (moveAction == null)
            Debug.LogError("Could not find Player/Move action!");

        if (jumpAction == null)
            Debug.LogError("Could not find Player/Jump action!");
    }

    void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }

    void Update()
    {
        if (groundIgnoreTimer > 0f)
        {
            groundIgnoreTimer -= Time.deltaTime;

            // Stopped rising (top of the jump, or landed): ground counts again.
            if (rb.linearVelocity.y <= 0f)
                groundIgnoreTimer = 0f;
        }

        bool grounded = IsGrounded() && groundIgnoreTimer <= 0f;

        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (jumpAction != null && jumpAction.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpAction != null && jumpAction.WasReleasedThisFrame())
            jumpCutQueued = true;

        if (animator != null)
            animator.SetBool("IsGrounded", grounded);
    }

    public void StartKnockback(float duration)
    {
        knockbackTimer = duration;
        jumpBufferCounter = 0f;
    }

    void FixedUpdate()
    {
        // Knockback state: let the physics push play out without input.
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            jumpCutQueued = false;
            if (animator != null)
                animator.SetFloat("Speed", 0f);
            return;
        }

        // Respawning: no control, and stop sideways drift.
        if (health != null && health.IsRespawning)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            jumpBufferCounter = 0f;
            jumpCutQueued = false;
            if (animator != null)
                animator.SetFloat("Speed", 0f);
            return;
        }

        float horizontal = 0f;

        if (moveAction != null)
            horizontal = moveAction.ReadValue<Vector2>().x;

        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(horizontal));

        if (spriteRenderer != null && Mathf.Abs(horizontal) > 0.05f)
            spriteRenderer.flipX = horizontal < 0f;

        rb.linearVelocity = new Vector2(
            horizontal * moveSpeed,
            rb.linearVelocity.y
        );

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            groundIgnoreTimer = groundIgnoreAfterJump;
            KnifeNinjaAudio.Instance?.PlayJump();
        }

        if (jumpCutQueued)
        {
            jumpCutQueued = false;

            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y * 0.5f
                );
            }
        }
    }

    public bool IsGrounded()
    {
        if (groundCheck == null)
            return false;

        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
