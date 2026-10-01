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

    private InputAction moveAction;
    private InputAction jumpAction;

    private float coyoteCounter;
    private float jumpBufferCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
        bool grounded = IsGrounded();

        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (jumpAction != null && jumpAction.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (animator != null)
            animator.SetBool("IsGrounded", grounded);
    }

    void FixedUpdate()
    {
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
            KnifeNinjaAudio.Instance?.PlayJump();
        }

        if (jumpAction != null &&
            jumpAction.WasReleasedThisFrame() &&
            rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * 0.5f
            );
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
