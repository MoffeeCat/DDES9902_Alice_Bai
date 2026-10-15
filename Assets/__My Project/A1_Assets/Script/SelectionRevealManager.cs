using UnityEngine;

public class SelectionRevealManager : MonoBehaviour
{
    [Header("Actual Objects")]
    public GameObject[] actualObjects;

    [Header("Optional")]
    public GameObject objectToHideOnReveal;

    private int selectedIndex = -1;


    void Start()
    {
        // Hide all actual objects at the beginning
        foreach (GameObject obj in actualObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }


    public void SelectOption(int index)
    {
        if (index < 0 || index >= actualObjects.Length)
            return;

        selectedIndex = index;

        Debug.Log("Selected option: " + index);
    }


    public void RevealSelected()
    {
        if (selectedIndex < 0 || selectedIndex >= actualObjects.Length)
        {
            Debug.LogWarning("No option has been selected yet.");
            return;
        }

        // Hide all actual objects first
        HideAllActualObjects();

        // Reveal selected object
        actualObjects[selectedIndex].SetActive(true);

        // Optional: hide something after reveal
        if (objectToHideOnReveal != null)
            objectToHideOnReveal.SetActive(false);
    }


    public void HideAllActualObjects()
    {
        foreach (GameObject obj in actualObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}