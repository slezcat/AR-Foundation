using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSpawner : MonoBehaviour
{
    // Reference to the prefab you want to spawn
    public GameObject prefabToSpawn;

    // Reference to the player object
    public Transform player;

    // The range within which prefabs will spawn near the player
    public float spawnRadius = 10f;

    // How often to spawn the prefab (in seconds)
    public float spawnInterval = 10f;

    // List to keep track of spawned instances
    private List<GameObject> spawnedInstances = new List<GameObject>();

    // Timer to track spawn interval
    private float timer;

    // Flag to control whether to spawn or not
    public bool shouldSpawn = true;
    public bool spawnInitial = false;

    void Start()
    {
        // Initialize the timer
        timer = spawnInterval;
        DestroyAllSpawnedInstances();
    }

    void Update()
    {
        // If spawning is disabled, return early and don't spawn
        if (!shouldSpawn)
            return;

        // Decrease timer by time passed since last frame
        timer -= Time.deltaTime;

        // Check if it's time to spawn
        if (timer <= 0)
        {

            if (spawnInitial)
            {
                // Spawn the prefab
                SpawnPrefabNearPlayer();
            }


            // Reset the timer
            timer = spawnInterval;
        }
    }

    void SpawnPrefabNearPlayer()
    {
        // Get a random point within the spawn radius around the player, but lock the Y-axis (horizontal)
        Vector3 randomPosition = GetRandomPositionNearPlayer();

        // Lock the Y-axis to the player's Y-axis position (horizontal spawn only)
        randomPosition.y = player.position.y + 0.2f;

        // Instantiate the prefab at the random horizontal position
        GameObject spawnedObject = Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);

        // Add to list of spawned instances
        spawnedInstances.Add(spawnedObject);
    }

    Vector3 GetRandomPositionNearPlayer()
    {
        // Generate random direction and distance
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(0f, spawnRadius);

        // Get the random position near the player
        Vector3 spawnPosition = player.position + new Vector3(randomDirection.x, 0, randomDirection.y) * randomDistance;

        return spawnPosition;
    }

    public void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            // Stop spawning and destroy all spawned instances
            shouldSpawn = false;
            DestroyAllSpawnedInstances();
            Debug.Log("Toggle is checked! All humans disappeared.");
        }
        else
        {
            // Resume spawning
            shouldSpawn = true;
            Debug.Log("Toggle is unchecked! Humans will spawn again.");
        }
    }

    void DestroyAllSpawnedInstances()
    {
        // Loop through each spawned instance and destroy it
        foreach (GameObject spawnedObject in spawnedInstances)
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }
        }

        // Clear the list of spawned instances
        spawnedInstances.Clear();
    }
}
