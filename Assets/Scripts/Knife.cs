using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Knife : MonoBehaviour
{
    public float lifetime = 3f;
    public float spinSpeed = 540f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed)
    {
        Vector2 dir = direction.normalized;
        rb.linearVelocity = dir * speed;
        rb.angularVelocity = -spinSpeed * Mathf.Sign(dir.x);
        Destroy(gameObject, lifetime);
    }
}
