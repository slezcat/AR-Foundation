using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterHit : StateMachineBehaviour
{

    public float delay = 0.5f;
    private MyDinoManager myDino;


  private void Awake()
    {
        GameObject creature = GameObject.FindWithTag("Creature");
        if (creature != null)
        {
            myDino = creature.GetComponent<MyDinoManager>();
        }
        else
        {
            Debug.LogError("No GameObject with the 'Creature' tag found.");
            
        }
    }
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(LogAfterDelay(delay));
    }

    private IEnumerator LogAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        Debug.Log("The human hits the dino");
        // Check if myDino is not null before accessing its health
        myDino.OnHit();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    // override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {

    // }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    // override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {

    // }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
