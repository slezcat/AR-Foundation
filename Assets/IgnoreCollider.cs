using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollider : MonoBehaviour
{
     // Called when something enters the trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the collider is tagged as "Enemy"
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy detected!");
            // Add logic here to damage or affect the enemy
        }
    }

    // Called every frame an object stays inside the trigger collider
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // You could trigger damage over time here, if desired
            Debug.Log("Enemy is within the hitbox!");
        }
    }

    // Called when the object exits the trigger collider
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy left the hitbox.");
        }
    }
}
