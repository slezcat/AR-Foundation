using UnityEngine;

public class ScreenNavigator : MonoBehaviour
{
    public GameObject[] screens; // Array to hold all the Canvas objects

    public void ShowScreen(int screenIndex)
    {
        // Deactivate all screens
        foreach (GameObject screen in screens)
        {
            screen.SetActive(false);
        }

        // Activate the selected screen
        screens[screenIndex].SetActive(true);
    }
}