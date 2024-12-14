using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestInputSystem : MonoBehaviour
{
    private Rigidbody sphereRigidBody;
    private PlayerInput playerInput;
    private NewControls playerInputActions;

    void Awake()
    {
        sphereRigidBody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        //without player input comps
        playerInputActions = new NewControls();
        playerInputActions.player.Enable();
        playerInputActions.player.jump.performed += Jump;
        playerInputActions.ui.submit.performed += Submit;

        // playerInputActions.player.Disable();
        // playerInputActions.player.jump.PerformInteractiveRebinding()
        // .WithControlsExcluding("Mouse")
        // .OnComplete(callback =>{
        //     Debug.Log(callback.action.bindings[0].overridePath);
        //     callback.Dispose();
        //     playerInputActions.player.Enable();
        // }).
        // Start();
    }

    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("swtrichh t");
            playerInput.SwitchCurrentActionMap("ui");
            playerInputActions.player.Disable();
            playerInputActions.ui.Enable();
        }
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
             Debug.Log("swtrichh y");
            playerInput.SwitchCurrentActionMap("player");
        }
    }

    void FixedUpdate()
    {
        Vector2 inputVector = playerInputActions.player.movement.ReadValue<Vector2>();
        float speed = 1f;
        sphereRigidBody.AddForce(new Vector3(inputVector.x, 0, inputVector.y)* speed, ForceMode.Force);
    }


    public void Jump(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        if (context.performed)
        {
            Debug.Log("jump" + context.phase);
            sphereRigidBody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }
    public void Submit(InputAction.CallbackContext context)
    {
        Debug.Log("SUBMITTT");
       
    }
}
