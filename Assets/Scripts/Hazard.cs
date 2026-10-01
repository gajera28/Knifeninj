using UnityEngine;

public class Hazard : MonoBehaviour
{
    public bool instantKill;
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
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
