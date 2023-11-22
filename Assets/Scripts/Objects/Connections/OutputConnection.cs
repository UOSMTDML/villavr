using System.Collections;
using System.Collections.Generic;
using Autohand;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class OutputConnection : Connection
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

    // Store list of rendered connection lines 
    private Dictionary<int, GameObject> connectionWires = new Dictionary<int, GameObject>();

    void Awake()
    {
        base.Awake();
    }
    
    
    
    public override void OnNetworkSpawn()
    {
        // Add a listener for the value change 
        connectedWithObjectIds.OnListChanged += (NetworkListEvent<int> networkListEvent) =>
        {
            // No need to update processing faust object, since only incoming outputs are relevant 
            
            // Handle line rendering 
            List<int> objectIds = new List<int>();
            foreach (int elem in connectedWithObjectIds)
            {
                objectIds.Add(elem);
            }
            UpdateWires(objectIds);
        };
    }

    public override PlacePoint GetPlacePoint()
    {
        return associatedPlacePoint;
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


    // Update LineRenderers that represent connections
    private void UpdateWires(List<int> connectedIds)
    {
        
        // Check pre-existing connections 
        foreach (int id in connectionWires.Keys)
        {
            // Check if previously stored connection still exists, if not, remove 
            if (!connectedIds.Contains(id))
            {
                GameObject.Destroy(connectionWires[id]);
                connectionWires.Remove(id);
            }

            // Remove inspected id from to be inspected ids 
            connectedIds.Remove(id);
        }

        // Inspect left-over ids, i.e. new connections 
        foreach (int id in connectedIds)
        {
            Transform endTransform = NetworkSpawner.Singleton.GetSpawnedObjectById(id).GetComponent<Connection>()
                .GetConnectionPointTransform(); 

            // Create new GameObject with LineRenderer 
            //GameObject newWire = Instantiate(wire, new Vector3(0, 0, 0), Quaternion.identity);
            // newWire.GetComponent<ConnectionCable>().SetStartEndTransform(connectionPointTransform, endTransform);

            // TODO Cable 
            
            
            // Add new Wire to List 
            //connectionWires.Add(id, newWire);
        }
        
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
        }
        
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
        }
        
        return success; 
    }
    

    // Method to be called from other object to initiate connection 
    public override bool ReceiveIncomingConnection(int uniqueObjectId)
    {
        bool success = AddConnectedId(uniqueObjectId);
        
        return success;
    }
    
    
    
    // Method to be called from other object to remove connection 
    public override bool RemoveIncomingConnection(int uniqueObjectId)
    {
        bool success = RemoveConnectedId(uniqueObjectId);
        
        return success;
    }
    
    

    void Update()
    {
        
    }
}
