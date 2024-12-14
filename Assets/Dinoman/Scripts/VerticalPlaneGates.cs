using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class VerticalPlaneGates : MonoBehaviour
{
    public ARPlaneManager arPlaneManager;
    public GameObject verticalPlanePrefab; // Prefab for the vertical plane

    void OnEnable()
    {
        arPlaneManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        arPlaneManager.planesChanged -= OnPlanesChanged;
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (var plane in args.added)
        {
            CreateGates(plane);
        }
    }

    private void CreateGates(ARPlane plane)
    {
        // Calculate the center and size of the plane
        Vector3 center = plane.transform.position;
        Vector3 size = plane.size; // This gives the width and length of the plane

        // Define the corners of the horizontal plane
        Vector3 topLeft = center + new Vector3(-size.x / 2, 0, size.y / 2);
        Vector3 topRight = center + new Vector3(size.x / 2, 0, size.y / 2);
        Vector3 bottomLeft = center + new Vector3(-size.x / 2, 0, -size.y / 2);
        Vector3 bottomRight = center + new Vector3(size.x / 2, 0, -size.y / 2);

        // Create vertical planes at each corner
        CreateVerticalGate(topLeft);
        CreateVerticalGate(topRight);
        CreateVerticalGate(bottomLeft);
        CreateVerticalGate(bottomRight);
    }

    private void CreateVerticalGate(Vector3 position)
    {
        // Instantiate a vertical plane gate
        GameObject verticalGate = Instantiate(verticalPlanePrefab, position, Quaternion.Euler(0, 0, 0));
        
        // Adjust the size of the vertical gate as necessary
        verticalGate.transform.localScale = new Vector3(0.1f, 2f, 2f); // Example size, adjust as needed
    }
}
