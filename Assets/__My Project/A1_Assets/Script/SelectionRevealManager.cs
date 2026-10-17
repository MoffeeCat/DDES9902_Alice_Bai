using UnityEngine;

public class SelectionRevealManager : MonoBehaviour
{
    public enum AlignmentMode
    {
        None,
        Center,
        CenterXZ_BottomY
    }

    [Header("Normal Actual Objects")]
    public GameObject[] actualObjects;

    [Header("Transparent Actual Objects")]
    public GameObject[] actualTransparentObjects;

    [Header("Estimate Objects")]
    public Transform estimateObject;
    public Transform estimateTransparentObject;

    [Header("Optional")]
    public GameObject objectToHideOnReveal;

    [Header("Alignment")]
    public bool alignOnReveal = false;
    public AlignmentMode alignmentMode = AlignmentMode.CenterXZ_BottomY;

    private int selectedIndex = -1;


    void Start()
    {
        HideAllActualObjects();

        if (estimateTransparentObject != null)
        {
            estimateTransparentObject.gameObject.SetActive(false);
        }
    }


    public void SelectOption(int index)
    {
        if (index < 0 || index >= actualObjects.Length)
        {
            Debug.LogWarning("Selected index is out of range.");
            return;
        }

        selectedIndex = index;

        Debug.Log("Selected option: " + index);
    }


    public void RevealSelected()
    {
        if (!HasValidSelection())
        {
            Debug.LogWarning("No option has been selected yet.");
            return;
        }

        ShowBoth();

        if (objectToHideOnReveal != null)
        {
            objectToHideOnReveal.SetActive(false);
        }
    }


    public void ShowEstimateOnly()
    {
        // Show normal estimate
        if (estimateObject != null)
        {
            estimateObject.gameObject.SetActive(true);
        }

        // Hide transparent estimate
        if (estimateTransparentObject != null)
        {
            estimateTransparentObject.gameObject.SetActive(false);
        }

        HideAllActualObjects();
    }


    public void ShowActualOnly()
    {
        if (!HasValidSelection())
            return;

        // Hide both estimate versions
        if (estimateObject != null)
        {
            estimateObject.gameObject.SetActive(false);
        }

        if (estimateTransparentObject != null)
        {
            estimateTransparentObject.gameObject.SetActive(false);
        }

        HideAllActualObjects();

        GameObject selectedActual =
            actualObjects[selectedIndex];

        if (selectedActual != null)
        {
            selectedActual.SetActive(true);

            if (alignOnReveal)
            {
                AlignActualObject(selectedActual);
            }
        }
    }


    public void ShowBoth()
    {
        if (!HasValidSelection())
            return;

        // Hide normal estimate
        if (estimateObject != null)
        {
            estimateObject.gameObject.SetActive(false);
        }

        // Copy current estimate transform
        // to transparent estimate
        if (estimateTransparentObject != null &&
            estimateObject != null)
        {
            estimateTransparentObject.position =
                estimateObject.position;

            estimateTransparentObject.rotation =
                estimateObject.rotation;

            estimateTransparentObject.localScale =
                estimateObject.localScale;

            estimateTransparentObject.gameObject.SetActive(true);
        }

        HideAllActualObjects();

        // Show selected transparent actual
        if (actualTransparentObjects != null &&
            selectedIndex < actualTransparentObjects.Length)
        {
            GameObject transparentActual =
                actualTransparentObjects[selectedIndex];

            if (transparentActual != null)
            {
                transparentActual.SetActive(true);

                if (alignOnReveal)
                {
                    AlignActualObject(
                        transparentActual
                    );
                }
            }
        }
    }


    public void HideAllActualObjects()
    {
        if (actualObjects != null)
        {
            foreach (GameObject obj in actualObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        if (actualTransparentObjects != null)
        {
            foreach (GameObject obj in actualTransparentObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }


    bool HasValidSelection()
    {
        return selectedIndex >= 0 &&
               selectedIndex < actualObjects.Length &&
               actualObjects[selectedIndex] != null;
    }


    void AlignActualObject(GameObject actualObject)
    {
        if (estimateObject == null ||
            actualObject == null ||
            alignmentMode == AlignmentMode.None)
        {
            return;
        }

        Bounds estimateBounds =
            GetObjectBounds(
                estimateObject.gameObject
            );

        Bounds actualBounds =
            GetObjectBounds(
                actualObject
            );


        if (alignmentMode == AlignmentMode.Center)
        {
            Vector3 difference =
                estimateBounds.center -
                actualBounds.center;

            actualObject.transform.position +=
                difference;
        }


        else if (
            alignmentMode ==
            AlignmentMode.CenterXZ_BottomY
        )
        {
            Vector3 difference =
                new Vector3(
                    estimateBounds.center.x -
                    actualBounds.center.x,

                    estimateBounds.min.y -
                    actualBounds.min.y,

                    estimateBounds.center.z -
                    actualBounds.center.z
                );

            actualObject.transform.position +=
                difference;
        }
    }


    Bounds GetObjectBounds(GameObject obj)
    {
        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            return new Bounds(
                obj.transform.position,
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
}