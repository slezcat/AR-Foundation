using UnityEngine;

public class DespawnPrefab : MonoBehaviour
{
    // Time in seconds before the object is despawned
    public float despawnTime = 5f;


    // Start is called before the first frame update
    void Start()
    {
        // Call the Despawn method after the specified time
        Destroy(gameObject, despawnTime);
    }
}
