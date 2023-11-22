using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class PermissionUi : MonoBehaviour
{
    private HashSet<int> displayedSpawnedObjectIds = new HashSet<int>();

    [SerializeField] private string everyoneCanModifyString = "Everyone";
    [SerializeField] private string nobodyCanModifyString = "Noone";
    [SerializeField] private string someCanModifyString = "Some";

    [SerializeField] private GameObject permissionIndividualSelectionPopUp;


    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(UpdateInformationDisplay(0.5f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        // Use OnEnable to prevent issue where disabled and reenabled game object won't run coroutine 
        StartCoroutine(UpdateInformationDisplay(0.5f));
    }


    // Process Spawned Objects Access Changes 
    public void ProcessToggleChangeSpawnedObjects(int dataId, string message)
    {
        
        if (message == everyoneCanModifyString)
        {
            AccessManager.Singleton.SetAccessTypePerUniqueObjectId(AccessType.EveryoneCanModify,new int[] {dataId});
        }
        else if (message == nobodyCanModifyString)
        {
            AccessManager.Singleton.SetAccessTypePerUniqueObjectId(AccessType.NobodyCanModify,new int[] {dataId});
        }
        else if (message == someCanModifyString)
        {
            AccessManager.Singleton.SetAccessTypePerUniqueObjectId(AccessType.SomeCanModify,new int[] {dataId});
        }
        else
        {
            Debug.Log("[ServerUi] ProcessToggleChange: Invalid message!");
        }
    }
    
    // Process Input Field Spawned Objects Changes 
    public void ProcessInputFieldChangedSpawnedObjects(int dataId, string message)
    {
        message = message.Replace(" ", "");

        
        // Check if message is empty 
        if (message == "")
        {
            AccessManager.Singleton.SetEspeciallyAllowed(dataId, new int[] {});
        }
        
        // Check if message has overall right format 
        else if (message.EndsWith(";"))
        {
            message = message.Remove(message.Length - 1);
            string[] splitMessage = message.Split(",");

            List<int> resultingIds = new List<int>();

            foreach (string elem in splitMessage)
            {
                // Parse item 
                int result;
                bool success = int.TryParse(elem, out result);
                if (success)
                {
                    resultingIds.Add(result);
                }
                else
                {
                    Debug.Log("[ServerUi] ProcessInputFieldChangedSpawnedObjects: Ids were not formatted correctly!");
                    return;
                }
            }
            
            // Update Access on objects 
            AccessManager.Singleton.SetEspeciallyAllowed(dataId, resultingIds.ToArray());

        }
        else
        {
            
        }
    }

    public void ProcessInputFieldPressedForSelectionInput(int lineIdentifier)
    {
        
        // Activate Popup 
        permissionIndividualSelectionPopUp.SetActive(true);
        permissionIndividualSelectionPopUp.GetComponent<ModifyPermissionIds>().StartUp(lineIdentifier);
        
        
    }


    public void SignalBackSelectionInputPerformed(int lineIdentifier, List<int> selectedIds)
    {
        // Deactivate PopUp 
        permissionIndividualSelectionPopUp.SetActive(false);
        
        // Generate string from selected ids 
        // Proper format: id1,id2,id3,...,idn;
        string textContent = "";
        for (int i = 0; i < selectedIds.Count; i ++)
        {
            textContent += selectedIds[i].ToString();

            if (i < selectedIds.Count - 1)
            {
                textContent += ",";
            }
        }
        textContent += ";";
        
        // Signal to modify scroll view 
        GetComponent<ModifyPermissionScrollView>().SetTextFieldContentsByLineId(lineIdentifier, textContent);



    }
    
    

    // Coroutine to update information 
    private IEnumerator UpdateInformationDisplay(float waitTime)
    {
        while (true)
        {
            
            // 
            // --- Update list of spawned objects 
            // 
            
            Dictionary<int, GameObject> spawnedObjects = NetworkSpawner.Singleton.GetSpawnedObjectsDictionary();
            List<int> newSpawnedObjectIds = spawnedObjects.Keys.ToList();

            List<int> removeIds = new List<int>();
            List<int> addIds = new List<int>();
            
            
            
            // Exclude Ids of Main Objects 
            List<int> removeMainIds = new List<int>();
            foreach (int spawnedId in newSpawnedObjectIds)
            {
                if (spawnedId % 10000 == 0)
                {
                    removeMainIds.Add(spawnedId);
                }
            }
            foreach (int removeId in removeMainIds)
            {
                newSpawnedObjectIds.Remove(removeId);
            }
            
            
            
            
            foreach (int displayedId in displayedSpawnedObjectIds)
            {
                // Check if displayed Ids are still in spawned objects 
                if (!newSpawnedObjectIds.Contains(displayedId))
                {
                    // If not, mark for removal 
                    removeIds.Add(displayedId);
                }
                
                // Remove displayedId from spawned ids, resulting rest is ids that need to be added 
                newSpawnedObjectIds.Remove(displayedId);
            }
            
            // Add rest to to-be-added Ids
            foreach (int leftOverIds in newSpawnedObjectIds)
            {
                addIds.Add(leftOverIds);
            }
            
            // Process Ids 
            foreach (int id in removeIds)
            {
                GetComponent<ModifyPermissionScrollView>().RemoveLineByIdentifier(id);
                displayedSpawnedObjectIds.Remove(id);
            }
            foreach (int id in addIds)
            {
                // Add name of main component 
                string lineName = id.ToString() + ": " + spawnedObjects[((int)(id / 10000)) * 10000].GetComponent<ObjectInfo>().objectName + " - " + spawnedObjects[id].GetComponent<ObjectInfo>().objectName;
                GetComponent<ModifyPermissionScrollView>().AddLineWithTextAndToggles(id,lineName,everyoneCanModifyString,nobodyCanModifyString, someCanModifyString);
                displayedSpawnedObjectIds.Add(id);
            }
            

            yield return new WaitForSeconds(waitTime);
        }
        
    }
}
