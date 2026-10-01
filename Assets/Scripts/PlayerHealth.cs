using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int CurrentHealth { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 respawnPosition;
    private bool invulnerable;
    private bool respawning;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        CurrentHealth = maxHealth;
        respawnPosition = transform.position;
    }

    public void SetCheckpoint(Vector3 position)
    {
        respawnPosition = position;
    }

    public void TakeDamage(int amount, Vector2 source)
    {
        if (invulnerable || respawning)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        KnifeNinjaAudio.Instance?.PlayHurt();
        VFXBurst.Spawn(transform.position, new Color(0.9f, 0.25f, 0.25f), 10);

        if (CurrentHealth <= 0)
        {
            StartCoroutine(RespawnRoutine());
            return;
        }

        float direction = Mathf.Sign(transform.position.x - source.x);
        if (Mathf.Approximately(direction, 0f))
            direction = 1f;

        rb.linearVelocity = new Vector2(direction * 4.5f, 5f);
        StartCoroutine(InvulnerabilityRoutine());
    }

    public void Kill()
    {
        if (!respawning)
        {
            CurrentHealth = 0;
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator InvulnerabilityRoutine()
    {
        invulnerable = true;

        for (int i = 0; i < 8; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(0.08f);
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        invulnerable = false;
    }

    IEnumerator RespawnRoutine()
    {
        respawning = true;
        invulnerable = true;

        Level1GameManager.Instance?.HUD?.ShowMessage(
            "The shadows take you... respawning",
            1.3f
        );

        yield return new WaitForSeconds(0.45f);

        transform.position = respawnPosition;
        rb.linearVelocity = Vector2.zero;
        CurrentHealth = maxHealth;

        yield return new WaitForSeconds(0.35f);

        invulnerable = false;
        respawning = false;
    }
}
