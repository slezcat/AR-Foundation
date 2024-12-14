using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//WTF THIS SCRIPT IS PERFECT
public class ChasingScript : MonoBehaviour
{
    private Transform target; // The target the human will run towards
    public float speed = 5f; // Speed at which the human will move
    public float stopDistance = 3f; // Distance from the target to stop running
    public float attackDistance = 1.5f; // Distance at which the human starts attacking
    public float attackCooldown = 1f; // Time between attacks

    private Animator animator; // Reference to the Animator component
    private bool isAttacking = false; // Whether the human is attacking
    private float attackTimer = 0f; // Timer to handle attack cooldown

    private void Start()
    {
        GameObject creatureObject = GameObject.FindGameObjectWithTag("Creature");
        target = creatureObject.transform;
        animator = GetComponent<Animator>(); // Get the Animator attached to the human object
    }


    private void Update()
    {
        // Calculate the distance to the target
        float distance = Vector3.Distance(transform.position, target.position);

        // If human is far enough, run towards the target
        if (distance > attackDistance)
        {
            isAttacking = false; // Not attacking when far away
            animator.SetBool("isRunning", true);

            // Move towards the target
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);

            // Rotate towards the target on the Y axis only
            if (flatDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
                transform.rotation = targetRotation; // Directly set the rotation
            }
        }
        // If within attacking distance, continue attacking
        else if (distance <= attackDistance)
        {
            animator.SetBool("isRunning", false);
            HandleAttacking();
        }
    }

    private void HandleAttacking()
    {
        // Handle attack cooldown
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            // Randomly set the attack animation (0, 1, or 2)
            int randomAttackIndex = Random.Range(0, 3); // 0 to 2
            Debug.Log(randomAttackIndex);
            animator.SetInteger("Attack", randomAttackIndex);
            attackTimer = 0f; // Reset cooldown timer
        }
    }
}
