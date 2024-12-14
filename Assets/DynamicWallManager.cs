using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DynamicWallManager : MonoBehaviour
{
    public GameObject wallPrefab; // Assign your wall prefab in the Inspector
    public ARPlaneManager arPlaneManager;

    private List<GameObject> walls = new List<GameObject>();

    private void Start()
    {
        arPlaneManager.planesChanged += OnPlanesChanged; // Subscribe to the planesChanged event
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs)
    {
        // Handle added planes
        foreach (var addedPlane in eventArgs.added)
        {
            CreateWalls(addedPlane);
        }

        // Handle updated planes (optional)
        foreach (var updatedPlane in eventArgs.updated)
        {
            UpdateWalls(updatedPlane);
        }

        // Handle removed planes (optional)
        foreach (var removedPlane in eventArgs.removed)
        {
            RemoveWalls(removedPlane);
        }
    }

    private void CreateWalls(ARPlane plane)
    {
        // Clear existing walls for this plane
        RemoveWalls(plane);

        // Get the boundary points of the AR plane as a NativeArray
        NativeArray<Vector2> boundaryNative = plane.boundary;

        // Convert NativeArray<Vector2> to Vector3[]
        Vector3[] boundary = new Vector3[boundaryNative.Length];
        for (int i = 0; i < boundaryNative.Length; i++)
        {
            boundary[i] = new Vector3(boundaryNative[i].x, 0, boundaryNative[i].y); // Convert Vector2 to Vector3
        }

        // Create walls based on the boundary points
        for (int i = 0; i < boundary.Length; i++)
        {
            Vector3 start = boundary[i];
            Vector3 end = boundary[(i + 1) % boundary.Length];

            // Calculate wall position and rotation
            Vector3 wallPosition = new Vector3((start.x + end.x) / 2, 0, (start.z + end.z) / 2);
            Quaternion wallRotation = Quaternion.LookRotation(new Vector3(end.x - start.x, 0, end.z - start.z));

            // Instantiate the wall
            GameObject wall = Instantiate(wallPrefab, wallPosition, wallRotation);
            float distance = Vector3.Distance(start, end);
            wall.transform.localScale = new Vector3(distance, 2, 0.1f); // Adjust height and width as needed
            wall.transform.parent = transform; // Optional: Parent walls to this GameObject
            walls.Add(wall);
        }
    }

    private void UpdateWalls(ARPlane plane)
    {
        // Optional: Logic to update walls based on plane updates if needed
    }

    private void RemoveWalls(ARPlane plane)
    {
        // Destroy walls associated with this plane
        for (int i = walls.Count - 1; i >= 0; i--)
        {
            Destroy(walls[i]);
        }
        walls.Clear();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        arPlaneManager.planesChanged -= OnPlanesChanged;
    }
}
