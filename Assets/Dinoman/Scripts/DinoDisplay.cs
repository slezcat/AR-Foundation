using UnityEngine;

public class DinoDisplay : MonoBehaviour
{
    [Header("Dinosaur Display Settings")]
    public GameObject displayArea; // The GameObject representing the display area (e.g., a UI container or 3D area).
    public float rotationSpeed = 50f; // Speed at which the dinosaur rotates.

    private GameObject displayedDinosaur; // Reference to the currently displayed dinosaur.

    /// <summary>
    /// Displays the selected dinosaur by instantiating it in the display area.
    /// </summary>
    /// <param name="dinosaurPrefab">The prefab of the selected dinosaur.</param>
    public void DisplayDinosaur(GameObject dinosaurPrefab)
    {
        // Clear the previous dinosaur
        ClearDisplay();

        // Instantiate the selected dinosaur as a child of the display area
        displayedDinosaur = Instantiate(dinosaurPrefab, displayArea.transform);
        displayedDinosaur.transform.localPosition = Vector3.zero; // Center it in the display area
        displayedDinosaur.transform.localRotation = Quaternion.identity; // Reset rotation
        displayedDinosaur.transform.localScale = Vector3.one; // Adjust scale if necessary
    }

    /// <summary>
    /// Clears the display area by removing any existing dinosaur.
    /// </summary>
    public void ClearDisplay()
    {
        if (displayedDinosaur != null)
        {
            Destroy(displayedDinosaur);
        }
    }

    void Update()
    {
        // Rotate the displayed dinosaur
        if (displayedDinosaur != null)
        {
            displayedDinosaur.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
