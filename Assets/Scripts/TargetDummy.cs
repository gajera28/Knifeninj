using UnityEngine;

public class TargetDummy : MonoBehaviour
{
    private bool hit;

    void Start()
    {
        Level1GameManager.Instance?.RegisterTarget();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hit)
            return;

        Knife knife = other.GetComponent<Knife>();
        if (knife == null)
            return;

        hit = true;
        Level1GameManager.Instance?.TargetDestroyed();
        KnifeNinjaAudio.Instance?.PlayHit();
        VFXBurst.Spawn(transform.position, new Color(0.95f, 0.55f, 0.25f), 18);

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
