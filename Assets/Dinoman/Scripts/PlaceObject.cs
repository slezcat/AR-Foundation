using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using TMPro;

[RequireComponent(
    typeof(ARRaycastManager),
    typeof(ARPlaneManager)
)]

public class PlaceObject : MonoBehaviour
{
    [SerializeField]
    public GameObject existingObject;
    public TextMeshProUGUI relocateText;
    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager aRPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool showRelocateInfo = true;

    private bool objectPlaced = false; // Flag to track if the object has been placed

    public HumanSpawner humanSpawner;

    void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
       
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    void OnDisable()
    {
       
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        // if (objectPlaced || finger.index != 0) return; // Return if object is already placed

        PlacePrefab(finger.currentTouch.screenPosition);
    }

    private void Update()
    {
        // Check for mouse button down (left mouse button)
        if (objectPlaced) return; // Return if object is already placed

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlacePrefab(Mouse.current.position.ReadValue());
        }
    }

    private void PlacePrefab(Vector2 screenPosition)
    {
        if (aRRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in hits)
            {
                Pose pose = hit.pose;
                Vector3 offsetPosition = pose.position + new Vector3(0, 0.05f, 0); // Adjust the Y offset value (0.05f) as needed

                if (existingObject != null)
                {
                    // Reposition the existing object
                    existingObject.transform.position = offsetPosition;
                    objectPlaced = true; // Set the flag to true after repositioning the object

                    if (aRPlaneManager.GetPlane(hit.trackableId).alignment == PlaneAlignment.HorizontalUp)
                    {
                        Vector3 position = existingObject.transform.position;
                        Vector3 cameraPosition = Camera.main.transform.position;
                        Vector3 direction = cameraPosition - position;
                        Vector3 targetRotationEuler = Quaternion.LookRotation(direction).eulerAngles;
                        Vector3 scaledEuler = Vector3.Scale(targetRotationEuler, existingObject.transform.up.normalized);
                        Quaternion targetRotation = Quaternion.Euler(scaledEuler);
                        existingObject.transform.rotation = existingObject.transform.rotation * targetRotation;
                    }

                    humanSpawner.spawnInitial = true;
                    // Disable input handling here
                    DisableInput();
                    break; // Exit the loop after repositioning the object
                }
            }
        }
    }

    private void DisableInput()
    {
         relocateText.gameObject.SetActive(false);
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown; // Disable touch input

    }

    public void Relocate()
    {
         relocateText.gameObject.SetActive(true);
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }
}
