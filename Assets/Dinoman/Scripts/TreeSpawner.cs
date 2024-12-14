using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public GameObject objectPrefab; // The object to spawn
    public GameObject trackablesParent; // The "Trackables" GameObject
    public float spawnRadius = 2f; // Distance from the player to spawn
    public float maxDistance = 10f; // Maximum distance before respawning the object near the player

    private GameObject spawnedObject;

    void Update()
    {
        // Check if the object is missing or too far
        if (spawnedObject == null || Vector3.Distance(transform.position, spawnedObject.transform.position) > maxDistance)
        {
            SpawnObjectNearPlayer();
        }
    }

    void SpawnObjectNearPlayer()
    {
        // Generate a random position near the player within the spawn radius
        Vector3 randomOffset = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnRadius, spawnRadius), 0);
        Vector3 spawnPosition = transform.position + randomOffset;

        // Set the y-position of the spawn to match the player's y-position
        spawnPosition.y = transform.position.y;

        // If the object exists, relocate it; otherwise, instantiate a new one
        if (spawnedObject != null)
        {
            spawnedObject.transform.position = spawnPosition;
        }
        else
        {
            spawnedObject = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);

            // Set the spawned object as a child of "Trackables" if it exists
            if (trackablesParent != null)
            {
                spawnedObject.transform.SetParent(trackablesParent.transform);
            }
        }
    }
}
