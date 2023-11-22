using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using Unity.Netcode;
using UnityEngine;


public class InputConnection : Connection
{
    
    // Inherited From Connection 
    //protected ConnectionType connectionType; 
    //protected int numberOfAllowedConnections;
    //protected FaustObject processingFaustObject;
    //protected Transform connectionPointTransform;
    //public NetworkList<int> connectedWithObjectIds;
    
    // Inherited from InteractableObject 
    //protected ObjectInfo objectInfo;
    //protected bool isModifiable; 
    
    
    [SerializeField] private GameObject lockSymbol;
    [SerializeField] private PlacePoint associatedPlacePoint;

    void Awake()
    {
        base.Awake();
    }

    public override PlacePoint GetPlacePoint()
    {
        return associatedPlacePoint;
    }
    
    public override void OnNetworkSpawn()
    {
        // Add a listener for the value change of connected object ids
        // To update processing faust element 
        connectedWithObjectIds.OnListChanged += (NetworkListEvent<int> networkListEvent) =>
        {
            // Update connected sound elements of processing faust element 
            List<int> objectIds = new List<int>();
            foreach (int elem in connectedWithObjectIds)
            {
                objectIds.Add(elem);
            }

            if (processingFaustObject != null)
            {
                processingFaustObject.UpdateConnectedSoundElements(objectIds, objectInfo.GetUniqueObjectId());
            }
            
        };
        
    }

    
    // Method to call from outside 
    // to try outgoing connection to other object 
    public override bool EstablishOutgoingConnection(int uniqueObjectId)
    {
        bool success = AddConnectedId(uniqueObjectId);

        if (success)
        {
            // Add own object id to connected object 
            NetworkSpawner.Singleton.GetSpawnedObjectsDictionary()[uniqueObjectId].GetComponent<Connection>()
                .ReceiveIncomingConnection(GetComponent<ObjectInfo>().GetUniqueObjectId());
            Debug.Log("YEAH try connect");
        }
        
        
        // Line rendering always happens from output side, i.e. output takes care of creating line 
        return success; 
    }

    
    // Method to call from outside
    // to try remove outgoing connection with other object 
    public override bool RemoveOutgoingConnection(int uniqueObjectId)
    {
        bool success = RemoveConnectedId(uniqueObjectId);

        if (success)
        {
            // Remove own object id from connected object 
            NetworkSpawner.Singleton.GetSpawnedObjectsDictionary()[uniqueObjectId].GetComponent<Connection>()
                .RemoveIncomingConnection(GetComponent<ObjectInfo>().GetUniqueObjectId());
            Debug.Log("YEAH try remove ");
        }
        
        
        // Line rendering always happens from output side, i.e. output takes care of deleting line 
        return success; 
    }
    

    
    // Method to be called from other object to initiate connection 
    public override bool ReceiveIncomingConnection(int uniqueObjectId)
    {
        bool success = AddConnectedId(uniqueObjectId);
        
        // Line rendering always happens from output side, i.e. output takes care of creating line 
        return success;
    }
    
    
    // Method to be called from other object to remove connection 
    public override bool RemoveIncomingConnection(int uniqueObjectId)
    {
        bool success = RemoveConnectedId(uniqueObjectId);
        
        // Line rendering always happens from output side, i.e. output takes care of deleting line 
        return success;
    }
    
    


    // Visualize interactability 
    public override void UpdateInteractableModifiable(bool canBeModified)
    {
        isModifiable = canBeModified;
        if (canBeModified)
        {
            lockSymbol.SetActive(false);
        }
        else
        {
            lockSymbol.SetActive(true);
        }
    }

    void Update()
    {
        
    }
}
