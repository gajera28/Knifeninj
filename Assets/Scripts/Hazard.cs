using UnityEngine;

public class Hazard : MonoBehaviour
{
    public bool instantKill;
    public int damage = 1;

    // Stay (not Enter) so a player still standing on spikes is hurt again
    // once the invulnerability blink ends.
    void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
            return;

        if (instantKill)
            health.Kill();
        else
            health.TakeDamage(damage, transform.position);
    }
}
