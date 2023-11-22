using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MainObjectInfo : ObjectInfo
{
    
    // Inherited from ObjectInfo 
    //[SerializeField] protected ObjectType objectType;
    //[SerializeField] protected string objectName;
    //public NetworkVariable<int> uniqueObjectId;

   
    // List of SubObjects 
    [SerializeField] private List<SubObjectInfo> associatedSubObjectsAndConnections;

    public NetworkVariable<int> spawnableObjectId; // Store which spawnable object id used for instantiating the corresponding prefab this object has 

    


    private void Awake()
    {

        // Object will be created by server, this will store the Unique Object ID given at spawn
        uniqueObjectId = new NetworkVariable<int>(-1,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
    }
    
    
    public override void OnNetworkSpawn()
    {
        
        base.OnNetworkSpawn();
        
        // Add a listener for the value change of uniqueObjectId
        uniqueObjectId.OnValueChanged += (int previousValue, int newValue) => {
            
            // Add Object to Spawned Objects, do this in OnValueChanged to guarantee execution
            // Also add in Start()
            // (under different circumstances either OnNetworkSpawn or Start are executed first) 
            NetworkSpawner.Singleton.AddUniqueObjectIdAndCorrespondingGameObject(uniqueObjectId.Value, this.gameObject);
            
            // On the server side, set sub object ids
            if (IsServer)
            {
                UpdateSubObjectUniqueObjectIds(newValue);
            }
            
        };
        
        
    }
    
    


    // Start is called before the first frame update
    void Start()
    {
        if (IsSpawned && (uniqueObjectId.Value > 0))
        {
            NetworkSpawner.Singleton.AddUniqueObjectIdAndCorrespondingGameObject(uniqueObjectId.Value, this.gameObject);
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    // Get list of associated Sub Objects 
    public List<SubObjectInfo> GetSubObjectAndConnectionsInfoList()
    {
        return associatedSubObjectsAndConnections;
    }
    
    
    
    
    // Update SubObject's creator Ids
    // Called by listener when own object id is changed 
    private void UpdateSubObjectUniqueObjectIds(int baseObjectId)
    {
        int counter = 1;
        
        // Update the Unique Object ID for all sub units 
        foreach (SubObjectInfo subObjectInfo in associatedSubObjectsAndConnections)
        {
            subObjectInfo.SetUniqueObjectId(baseObjectId + counter);
            counter++; 
        }

    }

    
    // Access Unique Object ID, overridden in Sub and Main Object Info 
    public override int GetUniqueObjectId()
    {
        return uniqueObjectId.Value;
    }

    

    // Set Unique Object ID for main object, used at Spawn of Object 
    // Effectively used like Server RPC, since it's only called from Server 
    // after Spawning, but it sets a network variable 
    // Change of network variable initiates update of sub object's IDs on all Clients 
    public override void SetUniqueObjectId(int baseObjectId)
    {
        if (!IsServer) // Should not be the reached  
        {
            Debug.Log("[ObjectInfo] SetUniqueObjectId: Cannot set, because not Server!");
            return;
        }
        
        // Set ID, will be updated on sub objects through listener for value change of network variable 
        uniqueObjectId.Value = baseObjectId;
        
    }


    // Store which spawnable object id instantiating the corresponding prefab has 
    public void SetSpawnableObjectId(int spawnableId)
    {
        if (!IsServer) // Should not be the reached  
        {
            Debug.Log("[ObjectInfo] SetSpawnableObjectId: Cannot set, because not Server!");
            return;
        }
        
        // Set ID
        spawnableObjectId.Value = spawnableId;
    }
    
    public int GetSpawnableObjectId()
    {
        return spawnableObjectId.Value;
    }
    
    
    public void DestroyObject(bool animateDissolve = true)
    {
        if (animateDissolve)
        {
            // Animate Dissolve on all CLients 
            AnimateDissolve_ClientRPC();
            
            // Start Coroutine to Destroy object 
            StartCoroutine(DestroyDelayed());
        }
        else
        {
            // Call ServerRPC to make sure it gets destroied on all clients 
            Destroy_ServerRPC();
        }
        
    }


    [ClientRpc]
    private void AnimateDissolve_ClientRPC(ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(AnimateDissolve(ExperienceManager.Singleton.dissolveMaterial));
    }
    
    
    private IEnumerator AnimateDissolve(Material dissolveMaterial)
    {
        // Get all renderers of object 
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        // Set init val of shader 
        float inputSineValue = 0f;
        
        // Set material 
        foreach(Renderer renderer in renderers)
        {
            renderer.materials = new[] { dissolveMaterial};
            //renderer.material = dissolveMaterial;
        }
        
        while (true)
        {
            if (inputSineValue >= 1)
            {
                break;
            }
                
            // Apply new value 
            foreach(Renderer renderer in renderers)
            {
                renderer.material.SetFloat("_Input_Sine_Value", inputSineValue); 
            }
            
            // Step input sine value 
            inputSineValue += 0.008f;

            yield return new WaitForSeconds(0.01f);
            
        }
        
        // Stop animation 
        foreach(Renderer renderer in renderers)
        {
            renderer.material.SetFloat("_Stop_Animation", 1); 
        }
        
    }


    private IEnumerator DestroyDelayed()
    {
        yield return new WaitForSeconds(3);
        
        // Call ServerRPC to make sure it gets destroied on all clients 
        Destroy_ServerRPC();
    }
    

    [ServerRpc(RequireOwnership = false)]
    private void Destroy_ServerRPC()
    {
        // Remove main ID and sub ids in network spawner dict 
        NetworkSpawner.Singleton.RemoveDespawnedId(uniqueObjectId.Value);

        foreach (SubObjectInfo info in associatedSubObjectsAndConnections)
        {
            NetworkSpawner.Singleton.RemoveDespawnedId(info.uniqueObjectId.Value);
        }
        
        // Destroy this gameobject with all associated sub objects 
        Destroy(this.gameObject);

    }
    
    
    
    
    
}

