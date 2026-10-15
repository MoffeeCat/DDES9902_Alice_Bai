using UnityEngine;

public class PulseGlow : MonoBehaviour
{
    public float minScale = 1.05f;
    public float maxScale = 1.20f;
    public float speed = 1.5f;

    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        float scaleMultiplier = Mathf.Lerp(minScale, maxScale, t);

        transform.localScale = baseScale * scaleMultiplier;
    }
}