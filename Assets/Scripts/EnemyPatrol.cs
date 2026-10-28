using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 1.25f;

    private Vector2 target;
    private SpriteRenderer spriteRenderer;
    private bool defeated;

    void Start()
    {
        target = pointB;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector2 current = transform.position;
        Vector2 next = Vector2.MoveTowards(
            current,
            target,
            speed * Time.deltaTime
        );

        transform.position = next;

        if (spriteRenderer != null)
            spriteRenderer.flipX = target.x < current.x;

        if (Vector2.Distance(next, target) < 0.05f)
            target = target == pointA ? pointB : pointA;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (defeated)
            return;

        Knife knife = other.GetComponent<Knife>();
        if (knife != null)
        {
            // Destroy is delayed to the end of the frame, so a second knife
            // in the same frame would otherwise score this enemy twice.
            defeated = true;
            Level1GameManager.Instance?.EnemyDefeated();
            KnifeNinjaAudio.Instance?.PlayHit();
            VFXBurst.Spawn(transform.position, new Color(0.85f, 0.16f, 0.22f), 18);
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }

    // Stay so the player keeps taking damage while touching the enemy.
    void OnTriggerStay2D(Collider2D other)
    {
        if (defeated)
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(1, transform.position);
    }
}
