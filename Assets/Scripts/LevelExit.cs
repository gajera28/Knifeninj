using UnityEngine;

public class LevelExit : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null)
            return;

        Level1GameManager.Instance?.TryCompleteLevel();
    }
}
