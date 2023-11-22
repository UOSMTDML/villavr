using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpawnTool : MonoBehaviour
{
    // Start is called before the first frame update
    
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private Canvas spawnInfoCanvas;


    [SerializeField] private TMP_Text componentNameText; // objectNameText
    [SerializeField] private Button componentsLeftButton; //canvasButtonLeft;
    [SerializeField] private Button componentsRightButton; //canvasButtonRight;
    [SerializeField] private Button componentsSpawnButton; //canvasButtonSpawn;
    [SerializeField] private Button componentsBackButton;
    
    [SerializeField] private TMP_Text toolNameText;
    [SerializeField] private Button toolsLeftButton; 
    [SerializeField] private Button toolsRightButton; 
    [SerializeField] private Button toolsSpawnButton;
    [SerializeField] private Button toolsBackButton;

    [SerializeField] private Button mainMenuToolsButton;
    [SerializeField] private Button mainMenuComponentsButton;
    
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject componentsMenu;
    [SerializeField] private GameObject toolsMenu;
    
    [SerializeField] private Collider proximityCollider;


    [Header("Spawnable Components")] [SerializeField]
    private List<GameObject> spawnableComponents;
    
    [Header("Spawnable Tools")] [SerializeField]
    private List<GameObject> spawnableTools;
    
    

    private int currentComponentViewIdx = 0;
    private int currentToolViewIdx = 0;
    private int lastElemIdx;
    private bool spawnAllowed;



    private void Start()
    {

        /*
        // Get end of list idx, either specified to exclude, or full length  
        lastElemIdx = Mathf.Min(Enum.GetNames(typeof(SpawnableObject)).Length - 1,
            ExperienceManager.Singleton.excludeSpawnableIdsAfterAndIncluding - 1);
        */ 
        
        // Show Main Menu 
        mainMenu.SetActive(true);
        componentsMenu.SetActive(false);
        toolsMenu.SetActive(false);
        
        // Selecting Other Menus
        mainMenuComponentsButton.onClick.AddListener(() =>
        {
            // Show Components Menu 
            mainMenu.SetActive(false);
            componentsMenu.SetActive(true);
            toolsMenu.SetActive(false);
            UpdateComponentView(0);
        });
        mainMenuToolsButton.onClick.AddListener(() =>
        {
            // Show Tools Menu 
            mainMenu.SetActive(false);
            componentsMenu.SetActive(false);
            toolsMenu.SetActive(true);
            UpdateToolView(0);
        });
        componentsBackButton.onClick.AddListener(() =>
        {
            // Show Main Menu 
            mainMenu.SetActive(true);
            componentsMenu.SetActive(false);
            toolsMenu.SetActive(false);
        });
        toolsBackButton.onClick.AddListener(() =>
        {
            // Show Main Menu 
            mainMenu.SetActive(true);
            componentsMenu.SetActive(false);
            toolsMenu.SetActive(false);
        });
        
        
        
        // Clicked buttons in Components view 
        componentsLeftButton.onClick.AddListener(() =>
        {
            // Try update view 
            UpdateComponentView(currentComponentViewIdx - 1);
        });
        componentsRightButton.onClick.AddListener(() =>
        {
            // Try update view 
            UpdateComponentView(currentComponentViewIdx + 1);
        });
        componentsSpawnButton.onClick.AddListener(() =>
        {
            // Spawn selected Component 
            if (spawnAllowed)
            {
                SpawnComponentObject();
            }
        });
        
        
        // Clicked buttons in Tools view 
        toolsLeftButton.onClick.AddListener(() =>
        {
            // Try update view 
            UpdateToolView(currentToolViewIdx - 1);
        });
        toolsRightButton.onClick.AddListener(() =>
        {
            // Try update view 
            UpdateToolView(currentToolViewIdx + 1);
        });
        toolsSpawnButton.onClick.AddListener(() =>
        {
            // Spawn selected Component 
            if (spawnAllowed)
            {
                SpawnToolObject();
            }
        });
        
        
        
        

        proximityCollider.GetComponent<CheckSpawnedCollision>().spawnNotPossible.AddListener(() =>
        {
            // Deactivate spawn buttons, if something is blocking 
            componentsSpawnButton.interactable = false;
            toolsSpawnButton.interactable = false;
            spawnAllowed = false;
        });



        proximityCollider.GetComponent<CheckSpawnedCollision>().spawnPossible.AddListener(() =>
        {
            // Activate spawn buttons, if no longer blocking 
            componentsSpawnButton.interactable = true;
            toolsSpawnButton.interactable = true;
            spawnAllowed = true;
        });

        
        
        
        // Init spawn allowed 
        spawnAllowed = true;

    }





    private void SpawnComponentObject()
    {

        SpawnableObject objectId = NetworkSpawner.Singleton.GetSpawnableObjectForObjectName(spawnableComponents[currentComponentViewIdx]
            .GetComponent<ObjectInfo>().objectName);

        NetworkSpawner.Singleton.SpawnObject(objectId, spawnPoint.transform.position, new Vector3(0,90,0), new Vector3(1,1,1));
    }

    private void SpawnToolObject()
    {
        SpawnableObject objectId = NetworkSpawner.Singleton.GetSpawnableObjectForObjectName(spawnableTools[currentToolViewIdx]
            .GetComponent<ObjectInfo>().objectName);

        NetworkSpawner.Singleton.SpawnObject(objectId, spawnPoint.transform.position, new Vector3(0,90,0), new Vector3(1,1,1));
    }


    private void UpdateToolView(int newObjectIdx)
    {
        
        // End of list already reached, no update 
        if ((newObjectIdx < 0) || newObjectIdx >= spawnableTools.Count)
        {
            return; 
        }

        
        // Update idx 
        currentToolViewIdx = newObjectIdx;
        
        // Display 
        SpawnableObject objectId =
            NetworkSpawner.Singleton.GetSpawnableObjectForObjectName(spawnableTools[currentToolViewIdx]
                .GetComponent<ObjectInfo>().objectName);
        toolNameText.text = objectId.ToString();

        
        // Check if left or right border are reached and toggle button material 
        if (newObjectIdx == 0)
        {
            toolsLeftButton.interactable = false;
        }
        else
        { 
            toolsLeftButton.interactable = true;
        }
        
        if (newObjectIdx == spawnableTools.Count - 1)
        {
            toolsRightButton.interactable = false;
        }
        else
        {
            toolsRightButton.interactable = true;
        }
        
    }
    

    private void UpdateComponentView(int newObjectIdx)
    {
        
        // End of list already reached, no update 
        if ((newObjectIdx < 0) || newObjectIdx >= spawnableComponents.Count)
        {
            return; 
        }

        
        // Update idx 
        currentComponentViewIdx = newObjectIdx;
        
        // Display 
        SpawnableObject objectId = NetworkSpawner.Singleton.GetSpawnableObjectForObjectName(spawnableComponents[currentComponentViewIdx]
            .GetComponent<ObjectInfo>().objectName);
        componentNameText.text = objectId.ToString();

        
        // Check if left or right border are reached and toggle button material 
        if (newObjectIdx == 0)
        {
            componentsLeftButton.interactable = false;
        }
        else
        { 
            componentsLeftButton.interactable = true;
        }
        
        if (newObjectIdx == spawnableComponents.Count - 1)
        {
            componentsRightButton.interactable = false;
        }
        else
        {
            componentsRightButton.interactable = true;
        }
        
    }


    private void Update()
    {
        
        
    }
    
    
}
