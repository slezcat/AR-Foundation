using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DinoSelection : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    public List<GameObject> Dinosaurs;
    public GameObject inGameUI;
    public GameObject selectionUI;
    public GameObject ARscene;

    public PlaceObject placeObjectScript;


    int pickedEntryIndex = 0;
    public void GetDropdownValue()
    {
        pickedEntryIndex = dropdown.value;
        string selectedOption = dropdown.options[pickedEntryIndex].text;
        Debug.Log(selectedOption);
    }

    public void SubmitSelection()
    {
        inGameUI.SetActive(true);
        selectionUI.SetActive(false);
        ARscene.SetActive(true);


        for (int i = 0; i < Dinosaurs.Count; i++)
        {
           if (i != pickedEntryIndex){
            Dinosaurs[i].SetActive(false);
           }
        }

        placeObjectScript.existingObject = Dinosaurs[pickedEntryIndex];
    }
}
