using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public enum FollowMode
    {
        Pivot,
        BoundsTop
    }

    [Header("Target")]
    public Transform target;

    [Header("Follow Mode")]
    public FollowMode followMode = FollowMode.Pivot;

    [Header("Follow Axes")]
    public bool followX = false;
    public bool followY = true;
    public bool followZ = false;

    [Header("Offset")]
    public Vector3 offset = Vector3.zero;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (target == null)
            return;

        Vector3 targetPosition = GetTargetPosition();

        Vector3 newPosition = transform.position;

        newPosition.x = followX
            ? targetPosition.x + offset.x
            : startPosition.x;

        newPosition.y = followY
            ? targetPosition.y + offset.y
            : startPosition.y;

        newPosition.z = followZ
            ? targetPosition.z + offset.z
            : startPosition.z;

        transform.position = newPosition;
    }

    Vector3 GetTargetPosition()
    {
        if (followMode == FollowMode.Pivot)
        {
            return target.position;
        }

        Renderer[] renderers =
            target.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            return target.position;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        if (followMode == FollowMode.BoundsTop)
        {
            return new Vector3(
                bounds.center.x,
                bounds.max.y,
                bounds.center.z
            );
        }

        return target.position;
    }
}