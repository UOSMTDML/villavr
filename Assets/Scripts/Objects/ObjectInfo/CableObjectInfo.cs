using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CableObjectInfo : ObjectInfo
{
    
    // Inherited from ObjectInfo 
    //[SerializeField] protected ObjectType objectType;
    //[SerializeField] protected string objectName;
    //public NetworkVariable<int> uniqueObjectId;
    //public NetworkVariable<AccessType> accessType;
    
    

    private InteractableObject interactableObject;
    
    
    // Store network list of client ids that can modify values, if access type is set to some   
    public NetworkList<int> especiallyAllowedClientIds;
    
    
    

   

    private void Awake()
    {
        // Get associated interactable Object 
        interactableObject = GetComponent<InteractableObject>();

        // Object will be created by server, this will store the Unique Object ID given at spawn
        uniqueObjectId = new NetworkVariable<int>(-1,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        // Object will be created by server, this will store the Access Type 
        accessType = new NetworkVariable<AccessType>(AccessType.EveryoneCanModify,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
        
        // Move init to awake to prevent compiler error
        especiallyAllowedClientIds = new NetworkList<int>();
        

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        
        // Add a listener for changed access type 
        accessType.OnValueChanged += (AccessType previousValue, AccessType newValue) => {
            UpdateInteractableModifiableStatus();
        };
        
        // Add a listener for changed especially allowed client ids 
        especiallyAllowedClientIds.OnListChanged += (NetworkListEvent<int> networkListEvent) =>
        {
            UpdateInteractableModifiableStatus();
        };
        
        // Add a listener for the value change of uniqueObjectId
        uniqueObjectId.OnValueChanged += (int previousValue, int newValue) => {
            
            // Add Object to Spawned Objects, do this in onvaluechanged to gurantee execution
            // Also add in Start()
            // (under different circumstances either OnNetworkSpawn or Start are executed first) 
            NetworkSpawner.Singleton.AddUniqueObjectIdAndCorrespondingGameObject(uniqueObjectId.Value, this.gameObject);

        };
        
        
        // Update Modifiable Status once, for clients that connect later
        UpdateInteractableModifiableStatus();

    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
        if (IsSpawned && (uniqueObjectId.Value > 0))
        {
            NetworkSpawner.Singleton.AddUniqueObjectIdAndCorrespondingGameObject(uniqueObjectId.Value, this.gameObject);
        }
        
    }
    
    
    

    // Used as RPC if update of id list was initiated by server 
    [ClientRpc]
    public void UpdateInteractableModifiableStatus_ClientRpc()
    {
        UpdateInteractableModifiableStatus();
    }

    private void UpdateInteractableModifiableStatus()
    {
        if (accessType.Value == AccessType.EveryoneCanModify)
        {
            GetComponent<InteractableObject>().UpdateInteractableModifiable(true);
            return;
        }
        else if(accessType.Value == AccessType.NobodyCanModify)
        {
            GetComponent<InteractableObject>().UpdateInteractableModifiable(false);
            return;
        }
        else if(accessType.Value == AccessType.SomeCanModify)
        {
            if (especiallyAllowedClientIds.Contains((int)NetworkManager.Singleton.LocalClientId))
            {
                GetComponent<InteractableObject>().UpdateInteractableModifiable(true);
            }
            else
            {
                GetComponent<InteractableObject>().UpdateInteractableModifiable(false);
            }
            return;
        }
        else
        {
            Debug.Log("[SubObjectInfo] UpdateInteractableModifiableStatus: Found non-allowed Access-Type!");
        }
            
    }


    

    // Update is called once per frame
    void Update()
    {
        
    }

    
    // Called by server 
    // Set the ClientIDs allowed to modify values, when access type is 'some'
    public override void SetEspeciallyAllowedClientIds(HashSet<int> allowedIds)
    {
        if (IsServer)
        {
            especiallyAllowedClientIds.Clear();
            foreach (int currentId in allowedIds)
            {
                especiallyAllowedClientIds.Add(currentId);
            }
            
            // Signal to clients that change appeared 
            UpdateInteractableModifiableStatus_ClientRpc();
        }
        else
        {
            Debug.Log("[SubObjectInfo] SetEspeciallyAllowedClientIds: Override is not server!");
        }
    }
    
    
    // Called by server
    // Set the access type of the object 
    public override void SetAccessType(AccessType type)
    {
        if (IsServer)
        {
            accessType.Value = type;
        }
        else
        {
            Debug.Log("[SubObjectInfo] SetAccessType: Override is not server!");
        }
    }
    
    
    
    
    // Set Unique Object ID
    // Should only be called by Server 
    public override void SetUniqueObjectId(int objectId)
    {
        if (!IsServer) // Should not be the reached  
        {
            Debug.Log("[ObjectInfo] SetUniqueObjectId: Cannot set, because not owner!");
            return;
        }
        uniqueObjectId.Value = objectId;
    }
    
    
    // Access Unique Object ID
    public override int GetUniqueObjectId()
    {
        return uniqueObjectId.Value;
    }



}

