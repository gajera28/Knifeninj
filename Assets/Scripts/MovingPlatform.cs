using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 1.5f;

    private Rigidbody2D rb;
    private Vector2 target;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        target = pointB;
    }

    void FixedUpdate()
    {
        Vector2 next = Vector2.MoveTowards(
            rb.position,
            target,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(next);

        if (Vector2.Distance(next, target) < 0.03f)
            target = target == pointA ? pointB : pointA;
    }
}
