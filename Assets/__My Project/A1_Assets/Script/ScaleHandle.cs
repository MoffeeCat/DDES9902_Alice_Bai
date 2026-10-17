using UnityEngine;
using UnityEngine.InputSystem;

public class ScaleHandle : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform targetObject;

    public GameObject hintLabel;
    public GameObject glowShell;
    public Renderer handleRenderer;

    [Header("First Drag Complete")]
    public GameObject showAfterFirstDrag;
    public bool showOnlyOnce = true;

    [Header("Materials")]
    public Material normalMaterial;
    public Material hoverMaterial;
    public Material grabbedMaterial;

    [Header("Handle Movement Axes")]
    public bool moveX = false;
    public bool moveY = true;
    public bool moveZ = true;

    [Header("Object Scale Axes")]
    public bool scaleX = true;
    public bool scaleY = true;
    public bool scaleZ = true;

    [Header("Position Lock")]
    public bool keepBottomFixed = true;

    [Header("Handle Follow")]
    public bool followTargetEdge = true;

    [Header("Scale Range")]
    public float maxScaleMultiplier = 5f;

    [Tooltip("How far the mouse needs to drag to reach maximum scale.")]
    public float maxDragDistance = 0.8f;

    [Header("Grab Feedback")]
    public float grabbedScaleMultiplier = 1.12f;

    private bool dragging = false;
    private bool hovering = false;
    private bool hasDragged = false;
    private bool firstDragActionTriggered = false;

    private Plane dragPlane;

    private Vector3 startHandlePosition;
    private Vector3 startObjectScale;
    private Vector3 startObjectPosition;

    private Vector3 originalVisualScale;

    private Vector3 dragDirection;

    private float fixedBottomY;

    private float handleXOffset;
    private float handleYOffset;
    private float handleZOffset;

    private bool usePositiveX;
    private bool usePositiveY;
    private bool usePositiveZ;


    void Start()
    {
        SaveInitialState();
        BuildDragDirection();

        if (hintLabel != null)
            hintLabel.SetActive(false);

        if (showAfterFirstDrag != null)
            showAfterFirstDrag.SetActive(false);

        SetNormalVisual();
    }


    void Update()
    {
        if (Mouse.current == null ||
            playerCamera == null)
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
        }

        // DRAGGING
        if (dragging &&
            Mouse.current.leftButton.isPressed)
        {
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 pointerPosition =
                    ray.GetPoint(enter);

                Vector3 fromStart =
                    pointerPosition -
                    startHandlePosition;

                float dragDistance =
                    Vector3.Dot(
                        fromStart,
                        dragDirection
                    );

                dragDistance =
                    Mathf.Clamp(
                        dragDistance,
                        0f,
                        maxDragDistance
                    );

                ResizeTarget(dragDistance);
            }
        }

        // STOP DRAG
        if (dragging &&
            Mouse.current.leftButton.wasReleasedThisFrame)
        {
            dragging = false;

            if (showAfterFirstDrag != null)
            {
                if (!showOnlyOnce ||
                    !firstDragActionTriggered)
                {
                    showAfterFirstDrag.SetActive(true);
                    firstDragActionTriggered = true;
                }
            }
        }

        // VISUAL STATES
        if (dragging)
            SetGrabbedVisual();
        else if (hovering)
            SetHoverVisual();
        else
            SetNormalVisual();
    }


    void SaveInitialState()
    {
        startHandlePosition = transform.position;

        if (targetObject != null)
        {
            startObjectScale = targetObject.localScale;
            startObjectPosition = targetObject.position;

            Bounds bounds = GetTargetBounds();

            if (keepBottomFixed)
            {
                fixedBottomY = bounds.min.y;
            }

            if (followTargetEdge)
            {
                usePositiveX =
                    transform.position.x >= bounds.center.x;

                usePositiveY =
                    transform.position.y >= bounds.center.y;

                usePositiveZ =
                    transform.position.z >= bounds.center.z;

                float edgeX =
                    usePositiveX ? bounds.max.x : bounds.min.x;

                float edgeY =
                    usePositiveY ? bounds.max.y : bounds.min.y;

                float edgeZ =
                    usePositiveZ ? bounds.max.z : bounds.min.z;

                handleXOffset =
                    transform.position.x - edgeX;

                handleYOffset =
                    transform.position.y - edgeY;

                handleZOffset =
                    transform.position.z - edgeZ;
            }
        }

        if (handleRenderer != null)
        {
            originalVisualScale =
                handleRenderer.transform.localScale;
        }
    }


    void BuildDragDirection()
    {
        Vector3 direction =
            Vector3.zero;

        if (moveX)
            direction += Vector3.right;

        if (moveY)
            direction += Vector3.up;

        if (moveZ)
            direction += Vector3.forward;

        if (direction == Vector3.zero)
        {
            direction = Vector3.up;
        }

        dragDirection =
            direction.normalized;
    }


    void ResizeTarget(float dragDistance)
    {
        if (targetObject == null)
            return;

        float t =
            Mathf.Clamp01(
                dragDistance /
                maxDragDistance
            );

        float multiplier =
            Mathf.Lerp(
                1f,
                maxScaleMultiplier,
                t
            );

    // IMPORTANT:
    // Start from the object's CURRENT scale,
    // so other handles' changes are preserved.
    Vector3 newScale =
        targetObject.localScale;

        if (scaleX)
        {
            newScale.x =
                startObjectScale.x *
                multiplier;
        }

        if (scaleY)
        {
            newScale.y =
                startObjectScale.y *
                multiplier;
        }

        if (scaleZ)
        {
            newScale.z =
                startObjectScale.z *
                multiplier;
        }

        targetObject.localScale =
            newScale;

        if (keepBottomFixed)
        {
            Bounds newBounds =
                GetTargetBounds();

            float bottomDifference =
                fixedBottomY -
                newBounds.min.y;

            targetObject.position +=
                Vector3.up *
                bottomDifference;
        }

        UpdateHandlePosition();
    }


    void UpdateHandlePosition()
    {
        if (!followTargetEdge ||
            targetObject == null)
            return;

        Bounds bounds =
            GetTargetBounds();

        float edgeX =
            usePositiveX ?
            bounds.max.x :
            bounds.min.x;

        float edgeY =
            usePositiveY ?
            bounds.max.y :
            bounds.min.y;

        float edgeZ =
            usePositiveZ ?
            bounds.max.z :
            bounds.min.z;

        Vector3 newPosition =
            transform.position;

        if (moveX)
        {
            newPosition.x =
                edgeX +
                handleXOffset;
        }

        if (moveY)
        {
            newPosition.y =
                edgeY +
                handleYOffset;
        }

        if (moveZ)
        {
            newPosition.z =
                edgeZ +
                handleZOffset;
        }

        transform.position =
            newPosition;
    }


    Bounds GetTargetBounds()
    {
        Renderer[] renderers =
            targetObject
            .GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            return new Bounds(
                targetObject.position,
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


    public void ResetInteraction()
    {
        dragging = false;
        hovering = false;
        hasDragged = false;
        firstDragActionTriggered = false;

        if (targetObject != null)
        {
            targetObject.localScale =
                startObjectScale;

            targetObject.position =
                startObjectPosition;
        }

        transform.position =
            startHandlePosition;

        if (showAfterFirstDrag != null)
        {
            showAfterFirstDrag.SetActive(false);
        }

        if (hintLabel != null)
        {
            hintLabel.SetActive(false);
        }

        SetNormalVisual();
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
        {
            handleRenderer.transform.localScale =
                originalVisualScale;
        }

        if (glowShell != null)
        {
            glowShell.SetActive(
                !hasDragged
            );
        }
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
        {
            handleRenderer.transform.localScale =
                originalVisualScale;
        }

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