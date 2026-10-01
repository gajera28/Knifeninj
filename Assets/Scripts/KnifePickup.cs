using UnityEngine;

public class KnifePickup : MonoBehaviour
{
    public int amount = 4;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerThrow thrower = other.GetComponent<PlayerThrow>();
        if (thrower == null)
            return;

        int added = thrower.AddKnives(amount);

        if (added > 0)
        {
            Level1GameManager.Instance?.HUD?.ShowMessage(
                "+" + added + " knives",
                1.2f
            );
            KnifeNinjaAudio.Instance?.PlayCoin();
            VFXBurst.Spawn(transform.position, new Color(0.5f, 0.9f, 1f), 12);
            Destroy(gameObject);
        }
    }
}
