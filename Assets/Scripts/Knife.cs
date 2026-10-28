using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Knife : MonoBehaviour
{
    public float lifetime = 3f;
    public float spinSpeed = 540f;

    private Rigidbody2D rb;
    private int groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    // The knife collider is a trigger, so walls and floors must stop it here
    // or it flies through them and can hit targets on the other side.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger || other.gameObject.layer != groundLayer)
            return;

        VFXBurst.Spawn(transform.position, new Color(0.8f, 0.8f, 0.85f), 6);
        Destroy(gameObject);
    }

    public void Launch(Vector2 direction, float speed)
    {
        Vector2 dir = direction.normalized;
        rb.linearVelocity = dir * speed;
        rb.angularVelocity = -spinSpeed * Mathf.Sign(dir.x);
        Destroy(gameObject, lifetime);
    }
}
