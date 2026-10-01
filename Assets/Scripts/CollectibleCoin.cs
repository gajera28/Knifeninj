using UnityEngine;

public class CollectibleCoin : MonoBehaviour
{
    public float bobHeight = 0.14f;
    public float bobSpeed = 2.4f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position = startPosition +
            Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.Rotate(0f, 0f, 90f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null)
            return;

        Level1GameManager.Instance?.AddCoin();
        KnifeNinjaAudio.Instance?.PlayCoin();
        VFXBurst.Spawn(transform.position, new Color(1f, 0.82f, 0.25f), 12);
        Destroy(gameObject);
    }
}
