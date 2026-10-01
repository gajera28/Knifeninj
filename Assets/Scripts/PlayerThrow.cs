using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrow : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset playerControls;

    [Header("Knife")]
    public Knife knifePrefab;
    public Transform throwPoint;
    public float knifeSpeed = 12f;
    public int knivesRemaining = 8;
    public int maxKnives = 8;
    public float throwCooldown = 0.28f;

    private InputAction moveAction;
    private InputAction throwAction;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float nextThrowTime;
    private int facing = 1;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerControls == null)
        {
            Debug.LogError("Assign PlayerControls!");
            enabled = false;
            return;
        }

        moveAction = playerControls.FindAction("Player/Move");
        throwAction = playerControls.FindAction("Player/Throw");

        if (maxKnives < knivesRemaining)
            maxKnives = knivesRemaining;
    }

    void OnEnable()
    {
        moveAction?.Enable();
        throwAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        throwAction?.Disable();
    }

    void Update()
    {
        if (spriteRenderer != null)
            facing = spriteRenderer.flipX ? -1 : 1;
        else if (moveAction != null)
        {
            float x = moveAction.ReadValue<Vector2>().x;
            if (x > 0.1f) facing = 1;
            if (x < -0.1f) facing = -1;
        }

        if (throwPoint != null)
        {
            Vector3 pos = throwPoint.localPosition;
            pos.x = Mathf.Abs(pos.x) * facing;
            throwPoint.localPosition = pos;
        }

        if (throwAction != null &&
            throwAction.WasPressedThisFrame())
        {
            ThrowKnife();
        }
    }

    void ThrowKnife()
    {
        if (Time.time < nextThrowTime)
            return;

        if (knivesRemaining <= 0)
        {
            Level1GameManager.Instance?.HUD?.ShowMessage(
                "Out of knives - find an ammo crate!",
                1.6f
            );
            return;
        }

        if (knifePrefab == null || throwPoint == null)
            return;

        if (animator != null)
            animator.SetTrigger("Throw");

        Knife knife = Instantiate(
            knifePrefab,
            throwPoint.position,
            Quaternion.identity
        );

        knife.Launch(Vector2.right * facing, knifeSpeed);

        knivesRemaining--;
        nextThrowTime = Time.time + throwCooldown;
        KnifeNinjaAudio.Instance?.PlayThrow();
    }

    public int AddKnives(int amount)
    {
        int before = knivesRemaining;
        knivesRemaining = Mathf.Clamp(
            knivesRemaining + amount,
            0,
            maxKnives
        );
        return knivesRemaining - before;
    }
}
