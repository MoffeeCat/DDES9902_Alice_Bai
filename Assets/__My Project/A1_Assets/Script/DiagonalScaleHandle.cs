using UnityEngine;
using UnityEngine.InputSystem;

public class DiagonalScaleHandle : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public GameObject hintLabel;
    public GameObject glowShell;
    public Renderer handleRenderer;
    public GameObject lockEstimateButton;

    [Header("Pot References")]
    public Transform potModel;
    public Transform reference28cmPot;

    [Header("Materials")]
    public Material normalMaterial;
    public Material hoverMaterial;
    public Material grabbedMaterial;

    [Header("Resize Range")]
    public float maxReferenceMultiplier = 2f;

    [Tooltip("How far the mouse needs to drag along the diagonal to reach maximum size.")]
    public float maxDragDistance = 0.8f;

    [Header("Grab Feedback")]
    public float grabbedScaleMultiplier = 1.12f;

    private bool dragging = false;
    private bool hovering = false;
    private bool hasDragged = false;

    private Plane dragPlane;
    private Vector3 clickOffset;

    private Vector3 originalVisualScale;

    private Vector3 startHandlePosition;
    private Vector3 startPotScale;

    private float maxScaleFactor;

    private float fixedPotBottomY;

    private float handleYOffset;
    private float handleZOffset;
    private float fixedHandleX;

    private bool usePositiveZ;
    private Vector3 dragDirection;

    void Start()
    {
        startHandlePosition = transform.position;

        if (handleRenderer != null)
            originalVisualScale = handleRenderer.transform.localScale;

        if (potModel != null)
        {
            startPotScale = potModel.localScale;

            Bounds startBounds = GetPotBounds();

            fixedPotBottomY = startBounds.min.y;

            fixedHandleX = transform.position.x;

            // Keep the same distance above the pot
            handleYOffset =
                transform.position.y - startBounds.max.y;

            // Work out which Z side the handle starts on
            usePositiveZ =
                transform.position.z >= startBounds.center.z;

            float startingZEdge =
                usePositiveZ ? startBounds.max.z : startBounds.min.z;

            handleZOffset =
                transform.position.z - startingZEdge;

            // Choose diagonal direction automatically
            dragDirection =
                (Vector3.up +
                (usePositiveZ ? Vector3.forward : -Vector3.forward))
                .normalized;
        }

        if (potModel != null && reference28cmPot != null)
        {
            // Example:
            // interact pot = 0.2
            // 28cm pot = 1
            // maximum = 1 × 2 = 2
            //
            // Therefore maximum factor from interact pot:
            // 2 / 0.2 = 10 times its starting size

            float targetMaxScale =
                reference28cmPot.localScale.x *
                maxReferenceMultiplier;

            maxScaleFactor =
                targetMaxScale / startPotScale.x;
        }

        if (hintLabel != null)
            hintLabel.SetActive(false);

        if (lockEstimateButton != null)
            lockEstimateButton.SetActive(false);

SetNormalVisual();

        SetNormalVisual();
    }

    void Update()
    {
        if (Mouse.current == null || playerCamera == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            playerCamera.ScreenPointToRay(mousePosition);

        hovering = false;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                hovering = true;
            }
        }

        // START DRAG
        if (hovering &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragging = true;
            hasDragged = true;

            dragPlane = new Plane(
                playerCamera.transform.forward,
                transform.position
            );

            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint =
                    ray.GetPoint(enter);

                clickOffset =
                    transform.position - hitPoint;
            }
        }

        // DRAGGING
        if (dragging &&
            Mouse.current.leftButton.isPressed)
        {
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 pointerPosition =
                    ray.GetPoint(enter) + clickOffset;

                Vector3 fromStart =
                    pointerPosition - startHandlePosition;

                float dragDistance =
                    Vector3.Dot(
                        fromStart,
                        dragDirection
                    );

                // Cannot move inward past the starting point
                dragDistance =
                    Mathf.Clamp(
                        dragDistance,
                        0f,
                        maxDragDistance
                    );

                ResizePot(dragDistance);
            }
        }

        // STOP DRAG
        if (dragging &&
            Mouse.current.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
            
            if (lockEstimateButton != null)
                lockEstimateButton.SetActive(true);
        }

        // VISUAL STATES
        if (dragging)
            SetGrabbedVisual();
        else if (hovering)
            SetHoverVisual();
        else
            SetNormalVisual();
    }

    void ResizePot(float dragDistance)
    {
        if (potModel == null ||
            reference28cmPot == null)
            return;

        float t =
            Mathf.Clamp01(
                dragDistance / maxDragDistance
            );

        // Start = interact pot original size
        // End = 2x the 28cm reference pot
        float scaleFactor =
            Mathf.Lerp(
                1f,
                maxScaleFactor,
                t
            );

        potModel.localScale =
            startPotScale * scaleFactor;

        // Keep pot sitting on the bench
        Bounds resizedBounds =
            GetPotBounds();

        float bottomDifference =
            fixedPotBottomY -
            resizedBounds.min.y;

        potModel.position +=
            Vector3.up * bottomDifference;

        // Recalculate after correcting pot position
        resizedBounds =
            GetPotBounds();

        UpdateHandlePosition(resizedBounds);
    }

    void UpdateHandlePosition(Bounds potBounds)
    {
        float zEdge =
            usePositiveZ ?
            potBounds.max.z :
            potBounds.min.z;

        Vector3 newPosition =
            transform.position;

        newPosition.x =
            fixedHandleX;

        newPosition.y =
            potBounds.max.y +
            handleYOffset;

        newPosition.z =
            zEdge +
            handleZOffset;

        transform.position =
            newPosition;
    }

    Bounds GetPotBounds()
    {
        Renderer[] renderers =
            potModel.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            return new Bounds(
                potModel.position,
                Vector3.zero
            );
        }

        Bounds bounds =
            renderers[0].bounds;

        for (int i = 1;
             i < renderers.Length;
             i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }

        return bounds;
    }

    void SetNormalVisual()
    {
        if (handleRenderer != null &&
            normalMaterial != null)
        {
            handleRenderer.sharedMaterial =
                normalMaterial;
        }

        if (hintLabel != null)
            hintLabel.SetActive(false);

        if (handleRenderer != null)
            handleRenderer.transform.localScale =
                originalVisualScale;

        if (glowShell != null)
            glowShell.SetActive(!hasDragged);
    }

    void SetHoverVisual()
    {
        if (handleRenderer != null &&
            hoverMaterial != null)
        {
            handleRenderer.sharedMaterial =
                hoverMaterial;
        }

        if (hintLabel != null)
            hintLabel.SetActive(true);

        if (handleRenderer != null)
            handleRenderer.transform.localScale =
                originalVisualScale;

        if (glowShell != null)
            glowShell.SetActive(false);
    }

    void SetGrabbedVisual()
    {
        if (handleRenderer != null &&
            grabbedMaterial != null)
        {
            handleRenderer.sharedMaterial =
                grabbedMaterial;
        }

        if (hintLabel != null)
            hintLabel.SetActive(true);

        if (handleRenderer != null)
        {
            handleRenderer.transform.localScale =
                originalVisualScale *
                grabbedScaleMultiplier;
        }

        if (glowShell != null)
            glowShell.SetActive(false);
    }
}