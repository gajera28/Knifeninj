using UnityEngine;

// Flags are created at runtime so existing scenes and editor rebuilds both work.
public class CheckpointFlag : MonoBehaviour
{
    private static Sprite flagSprite;
    private static CheckpointFlag activeFlag;
    private SpriteRenderer banner;
    private Vector3 respawnPosition;
    private bool activated;

    public static void CreateLevelFlags(Transform parent)
    {
        if (FindAnyObjectByType<CheckpointFlag>() != null)
            return;

        activeFlag = null;
        Create(parent, new Vector3(18f, 1f, 0f));
        Create(parent, new Vector3(25f, 0f, 0f));
    }

    private static void Create(Transform parent, Vector3 groundPosition)
    {
        if (flagSprite == null)
            flagSprite = Sprite.Create(Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f), Texture2D.whiteTexture.width);

        GameObject root = new GameObject("Checkpoint Flag");
        root.transform.SetParent(parent);
        root.transform.position = groundPosition;
        CheckpointFlag flag = root.AddComponent<CheckpointFlag>();
        flag.respawnPosition = groundPosition + Vector3.up * 0.85f;
        AddPart(root.transform, "Pole", new Vector2(0f, 0.75f),
            new Vector2(0.08f, 1.5f), Color.white);
        flag.banner = AddPart(root.transform, "Banner", new Vector2(0.35f, 1.25f),
            new Vector2(0.65f, 0.4f), Color.yellow);

        BoxCollider2D trigger = root.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.offset = new Vector2(0f, 0.8f);
        trigger.size = new Vector2(1f, 1.6f);
    }

    private static SpriteRenderer AddPart(Transform parent, string name,
        Vector2 position, Vector2 size, Color color)
    {
        GameObject part = new GameObject(name);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = flagSprite;
        renderer.color = color;
        renderer.sortingOrder = 4;
        return renderer;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || activated)
            return;

        activated = true;
        if (activeFlag != null && activeFlag.banner != null)
            activeFlag.banner.color = Color.gray;
        activeFlag = this;
        banner.color = Color.green;
        health.SetCheckpoint(respawnPosition);
        health.GetComponent<PlayerThrow>()?.RefillKnives();
        KnifeNinjaAudio.Instance?.PlayCoin();
        Level1GameManager.Instance?.HUD?.ShowMessage(
            "Checkpoint saved! Knives refilled.", 2f);
    }
}
