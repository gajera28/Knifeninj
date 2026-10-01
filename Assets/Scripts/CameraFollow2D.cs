using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector2 offset = new Vector2(1.5f, 1.25f);
    public float smoothTime = 0.22f;
    public Vector2 minBounds = new Vector2(-2f, 1.5f);
    public Vector2 maxBounds = new Vector2(44f, 5.5f);

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
        desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref velocity,
            smoothTime
        );
    }
}
