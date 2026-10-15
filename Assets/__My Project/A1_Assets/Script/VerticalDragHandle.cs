using UnityEngine;
using UnityEngine.InputSystem;

public class VerticalDragHandle : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public GameObject hintLabel;
    public GameObject glowShell;
    public Renderer handleRenderer;

    [Header("Pot Resize")]
    public Transform potModel;
    public float resizeSensitivity = 1.5f;
    public float minPotScaleY = 0.5f;
    public float maxPotScaleY = 2.0f;

    [Header("Materials")]
    public Material normalMaterial;
    public Material hoverMaterial;
    public Material grabbedMaterial;

    [Header("Handle Movement")]
    public float minY = 1.08f;
    public float maxY = 1.5f;

    [Header("Grab Feedback")]
    public float grabbedScaleMultiplier = 1.12f;

    private bool dragging = false;
    private bool hovering = false;
    private bool hasDragged = false;

    private Plane dragPlane;
    private Vector3 clickOffset;
    private Vector3 originalHandleScale;

    private float startHandleY;
    private Vector3 startPotScale;

    private float fixedPotBottomY;

    void Start()
    {
        originalHandleScale = transform.localScale;

        if (hintLabel != null)
            hintLabel.SetActive(false);

        if (potModel != null)
        {
            startHandleY = transform.position.y;
            startPotScale = potModel.localScale;

            fixedPotBottomY = GetPotBottomY();
        }

        SetNormalVisual();
    }

    void Update()
    {
        if (Mouse.current == null || playerCamera == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        hovering = false;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                hovering = true;
            }
        }

        // Start dragging
        if (hovering && Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragging = true;
            hasDragged = true;

            dragPlane = new Plane(
                playerCamera.transform.forward,
                transform.position
            );

            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                clickOffset = transform.position - hitPoint;
            }
        }

        // Dragging
        if (dragging && Mouse.current.leftButton.isPressed)
        {
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter) + clickOffset;

                Vector3 pos = transform.position;
                pos.y = Mathf.Clamp(hitPoint.y, minY, maxY);

                transform.position = pos;

                ResizePot();
            }
        }

        // Stop dragging
        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
        }

        // Visual states
        if (dragging)
        {
            SetGrabbedVisual();
        }
        else if (hovering)
        {
            SetHoverVisual();
        }
        else
        {
            SetNormalVisual();
        }
    }

    void ResizePot()
    {
        if (potModel == null)
            return;

        float handleDifference = transform.position.y - startHandleY;

        float newScaleY =
            startPotScale.y + handleDifference * resizeSensitivity;

        newScaleY = Mathf.Clamp(
            newScaleY,
            minPotScaleY,
            maxPotScaleY
        );

        Vector3 newScale = potModel.localScale;
        newScale.y = newScaleY;
        potModel.localScale = newScale;

        // Keep the bottom of the pot on the same surface
        float newBottomY = GetPotBottomY();
        float bottomDifference = fixedPotBottomY - newBottomY;

        potModel.position += Vector3.up * bottomDifference;
    }

    float GetPotBottomY()
    {
        Renderer[] renderers = potModel.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return potModel.position.y;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.min.y;
    }

    void SetNormalVisual()
    {
        if (handleRenderer != null && normalMaterial != null)
            handleRenderer.material = normalMaterial;

        if (hintLabel != null)
            hintLabel.SetActive(false);

        transform.localScale = originalHandleScale;

        if (glowShell != null)
            glowShell.SetActive(!hasDragged);
    }

    void SetHoverVisual()
    {
        if (handleRenderer != null && hoverMaterial != null)
            handleRenderer.material = hoverMaterial;

        if (hintLabel != null)
            hintLabel.SetActive(true);

        transform.localScale = originalHandleScale;

        if (glowShell != null)
            glowShell.SetActive(false);
    }

    void SetGrabbedVisual()
    {
        if (handleRenderer != null && grabbedMaterial != null)
            handleRenderer.material = grabbedMaterial;

        if (hintLabel != null)
            hintLabel.SetActive(true);

        transform.localScale =
            originalHandleScale * grabbedScaleMultiplier;

        if (glowShell != null)
            glowShell.SetActive(false);
    }
}