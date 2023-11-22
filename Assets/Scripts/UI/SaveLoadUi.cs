using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUi : MonoBehaviour
{

    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadMenuButton;

    [SerializeField] private Button backToSaveLoadMenuButton;
    [SerializeField] private Button loadButton;

    [SerializeField] private GameObject loadMenuElements;

    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private GameObject loadRefToggle;
    [SerializeField] private GameObject loadRefText;

    [SerializeField] private int rowOffset;
    [SerializeField] private int minContentHeight;

    private List<string> availablePaths = new List<string>();
    private List<GameObject> generatedToggles = new List<GameObject>();
    private int selectedLoadIdx = -1;
    


    // Start is called before the first frame update
    void Start()
    {
        saveButton.onClick.AddListener(() =>
        {
            // Save Scene
            SaveLoader.Singleton.SaveSceneSetup();
            
            // Disable Save Button until reentering menu 
            saveButton.interactable = false;
        });
        
        
        loadMenuButton.onClick.AddListener(() =>
        {
            // Enable load menu elements 
            loadMenuElements.SetActive(true);
            
            // Generate list of available files to select 
            GenerateSceneSetupList();
            
            // Make load button non-interactable until scene setup is selected
            loadButton.interactable = false;

        });
        
        
        backToSaveLoadMenuButton.onClick.AddListener(() =>
        {
            // Disable load menu elements 
            loadMenuElements.SetActive(false);
        });
        
        
        loadButton.onClick.AddListener(() =>
        {
            // Find toggle that is selected 
            foreach (GameObject toggle in generatedToggles)
            {
                if (toggle.GetComponent<Toggle>().isOn)
                {
                    selectedLoadIdx = generatedToggles.IndexOf(toggle);
                    break;
                }
            }
            
            // Load selected file 
            SaveLoader.Singleton.LoadSceneSetup(Path.GetFileName(availablePaths[selectedLoadIdx]));
            
            // Disable load button 
            loadButton.interactable = false;
        });
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        // Enable Save Button when reentering save load menu 
        saveButton.interactable = true;
        
        // Disable load menu 
        loadMenuElements.SetActive(false);
        
        // Reset load idx 
        selectedLoadIdx = -1;

    }


    private void GenerateSceneSetupList()
    {
        availablePaths = SaveLoader.Singleton.GetAvailableSceneSetups();
        generatedToggles.Clear();

        int rowIdx = 0;
        foreach (string path in availablePaths)
        {
            // Clone Ref Toggle and text 
            GameObject newToggle = Instantiate(loadRefToggle, loadRefToggle.transform.parent);
            GameObject newRef = Instantiate(loadRefText, loadRefText.transform.parent);
            generatedToggles.Add(newToggle);
            
            
            // Set active and change position and set text 
            newToggle.SetActive(true);
            newRef.SetActive(true);
            newToggle.transform.localPosition += new Vector3(0, rowOffset * -1 * rowIdx, 0);
            newRef.transform.localPosition += new Vector3(0, rowOffset * -1 * rowIdx, 0);
            newRef.GetComponent<TMP_Text>().text = Path.GetFileName(path);
            
            // Add listener to toggle all others off when toggled on 
            newToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool newVal) =>
            {
                
                // Toggle this toggle on 
                if (newVal)
                {
                    
                    // Toggle all others off
                    foreach (GameObject listToggle in generatedToggles)
                    {
                        // Skip self 
                        if (GameObject.ReferenceEquals(listToggle, newToggle))
                        {
                            continue;
                        }
                        
                        listToggle.GetComponent<Toggle>().isOn = false;
                    }
                    
                    
                    // Activate Load Button
                    loadButton.interactable = true;

                }

                else // toggled this toggle off, no others can be on at this point, i.e. deactivate load button 
                {
                    loadButton.interactable = false;
                }
            });

            rowIdx += 1;
        }
        
        // Update Conent Scroll View Size 
        scrollViewContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0,
            Mathf.Max((availablePaths.Count + 1) * rowOffset, minContentHeight));
        
        
        Debug.Log("update to " + (new Vector2(0,
            Mathf.Max((availablePaths.Count + 1) * rowOffset, minContentHeight))).ToString());
        
        
    }
    
    
    
}
