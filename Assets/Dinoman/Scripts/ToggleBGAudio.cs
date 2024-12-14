using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleBGAudio : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource
    public Toggle audioToggle; // Reference to the UI Toggle

    void Start()
    {
        // Set the toggle's value based on the audio source's mute state
        audioToggle.isOn = !audioSource.mute;

        // Add a listener to the toggle to call the ToggleAudio method when it is changed
        audioToggle.onValueChanged.AddListener(ToggleAudio);
    }

    // Method to handle the toggle change
    private void ToggleAudio(bool isOn)
    {
        // Mute or unmute the audio based on the toggle's value
        audioSource.mute = !isOn;
    }

    private void OnDestroy()
    {
        // Remove the listener when this object is destroyed
        audioToggle.onValueChanged.RemoveListener(ToggleAudio);
    }
}
