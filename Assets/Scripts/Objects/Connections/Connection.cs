using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using Unity.Netcode;
using UnityEngine;



// Parent class for interactable objects 
public class Connection : InteractableObject
{
    
    // Inherited from InteractableObject 
    //protected ObjectInfo objectInfo;
    //protected bool isModifiable; 
    
    // Set connection type in inspector 
    [SerializeField] protected ConnectionType connectionType; 
    
    // Deprecated: Each connector connects with only one cable 
    // For multiple inputs/ outputs, use dedicated summation/ distribution piece
    // Set amount of allowed connections in inspector
    protected int numberOfAllowedConnections = 1;
    
    // Store processing Faust element
    [SerializeField] protected FaustObject processingFaustObject;
    
    // Store connection point for connection position 
    [SerializeField] protected Transform connectionPointTransform;

    // Keep track of connections 
    public NetworkList<int> connectedWithObjectIds;


    protected void Awake()
    {
        base.Awake();
        
        // Move init to awake to prevent compiler error
        connectedWithObjectIds = new NetworkList<int>();
        
    }

    public int GetUniqueObjectId()
    {
        return objectInfo.GetUniqueObjectId();
    }


    public Transform GetConnectionPointTransform()
    {
        return connectionPointTransform;
    }

    public FaustObject GetProcessingFaustObject()
    {
        return processingFaustObject;
    }

    public ConnectionType GetConnectionType()
    {
        return connectionType;
    }

    public List<int> GetConnectedObjectIds()
    {
        List<int> ids = new List<int>();
        foreach (int id in connectedWithObjectIds)
        {
            ids.Add(id);
        }

        return ids;
    }


    public virtual PlacePoint GetPlacePoint()
    {
        return null;
    }
    
    
    // Receive an incoming connection 
    // Initiated by other object 
    public virtual bool ReceiveIncomingConnection(int uniqueObjectId)
    {
        return false;
        
    } 
    
    // Remove an incoming connection 
    // Initiated by other object 
    public virtual bool RemoveIncomingConnection(int uniqueObjectId)
    {
        return false;
        
    }
    
    
    // Establish an outgoing connection
    // Initiated by same object 
    public virtual bool EstablishOutgoingConnection(int uniqueObjectId)
    {
        return false;
    }
    
    
    // Method to call from outside
    // to try remove outgoing connection with other object 
    public virtual bool RemoveOutgoingConnection(int uniqueObjectId)
    {
        return false; 
    }
    
    
    // Check whether connection is possible
    public bool IsConnectionPossible(int idToConnectTo, bool otherEndIsAlreadyConnected = false)
    {

        if (connectedWithObjectIds.Count >= numberOfAllowedConnections)
        {
            Debug.Log("[Connection] IsConnectionPossible from " + objectInfo.uniqueObjectId + " to " +  idToConnectTo +  ": Not possible, limit of " + numberOfAllowedConnections.ToString() + " connections already reached.");
            return false;
        }

        if (otherEndIsAlreadyConnected) // other end of cable is already connected
        {
            if (NetworkSpawner.Singleton.GetSpawnedObjectsDictionary().ContainsKey(idToConnectTo))
            {
                if (NetworkSpawner.Singleton.GetSpawnedObjectsDictionary()[idToConnectTo].GetComponent<Connection>() != null)
                {
                    if (NetworkSpawner.Singleton.GetSpawnedObjectsDictionary()[idToConnectTo].GetComponent<Connection>()
                            .GetConnectionType() == connectionType)
                    {
                        Debug.Log("[Connection] IsConnectionPossible from " + objectInfo.uniqueObjectId.Value + " to " +
                                  idToConnectTo +
                                  ": Not possible, other connector is of same Connection type.");
                        return false;
                    }
                }
            }
            
        }
        
        
        Debug.Log("[Connection] IsConnectionPossible from " + idToConnectTo + " to " + objectInfo.uniqueObjectId.Value + ": Possible.");
        return true;
        
        
    }
    
    
    
    //
    // --- Update Connected IDs 
    // --- (protected methods) 
    //

    
    // Wrapper method to update Connected Object Ids
    // Can make use of locally available isModifiable 
    protected bool AddConnectedId(int newId)
    {
        if (isModifiable)
        {
            return UpdateConnectedObjects(newId, "add");
        }
        else
        {
            // Do not update 
            return false;
        }
    }
    
    // Wrapper method to update Connected Object Ids
    // Can make use of locally available isModifiable 
    protected bool RemoveConnectedId(int removeId)
    {
        if (isModifiable)
        {
            return UpdateConnectedObjects(removeId, "remove");
        }
        else
        {
            // Do not update 
            return false;
        }
    }
    
    // Wrapper method to update Connected Object Ids for ServerRPC
    // Can make use of locally available isModifiable 
    protected bool SetConnectedIds(int[] connectedIds)
    {
        if (isModifiable)
        {
            return UpdateConnectedObjects(-1, "set", connectedIds);
        }
        else
        {
            // Do not update 
            return false;
        }
    }
    
    
    
    
    
    private bool UpdateConnectedObjects(int objectId, string mode, int[] objectIds = null) {
        
        // Make sure that object ID exists 
        Dictionary<int, GameObject> spawnedObjects = NetworkSpawner.Singleton.GetSpawnedObjectsDictionary();
        if (!spawnedObjects.ContainsKey(objectId))
        {
            Debug.Log("[Connection] Cannot perform '" + mode + "' because specified ID '" + objectId.ToString() +"' does not exist in spawned IDs. LocalClientId " + NetworkManager.Singleton.LocalClientId);
            return false;
        };
        
        // Make sure that intended connection is actually from input to output or from output to input 
        if (connectionType == spawnedObjects[objectId].GetComponent<Connection>().GetConnectionType())
        {
            Debug.Log("[Connection] Cannot perform '" + mode + "' because specified ID is not of opposite connection type. LocalClientId " + NetworkManager.Singleton.LocalClientId);
            return false;
        }

        if (objectIds != null)
        {
           foreach (int id in objectIds)
           {
               if (connectionType == spawnedObjects[id].GetComponent<Connection>().GetConnectionType())
               {
                   Debug.Log("[Connection] Cannot perform '" + mode + "' because specified IDs are not of opposite connection type. LocalClientId " + NetworkManager.Singleton.LocalClientId);
                   return false;
               } 
           } 
        }
        
        
        
        
        if (mode == "add")
        {
            Debug.Log("[Connection] Add connected Object ID " + objectId.ToString() + "; LocalClientId " + NetworkManager.Singleton.LocalClientId);
            
            // Check if Max Connections is already reached
            if (connectedWithObjectIds.Count >= numberOfAllowedConnections)
            {
                Debug.Log("[Connection] Cannot add ID, limit of " + numberOfAllowedConnections.ToString() + " connections already reached.");
                return false;
            }
            else
            {
                // Check if already included, else add 
                if (!connectedWithObjectIds.Contains(objectId))
                {
                    AddToConnectedWithObjectIds_ServerRpc(objectId);
                    //connectedWithObjectIds.Add(objectId);
                    return true;
                }

                Debug.Log("[Connection] Add not successful, ID already included.");
                return false;

            }
            
            
        }
        else if (mode == "remove")
        {
            Debug.Log("[Connection] Remove connected Object ID " + objectId.ToString() + "; LocalClientId " + NetworkManager.Singleton.LocalClientId);
    
            // Check if already included, else add 
            if (connectedWithObjectIds.Contains(objectId))
            {
                RemoveFromConnectedWithObjectIds_ServerRpc(objectId);
                //connectedWithObjectIds.Remove(objectId);
                return true;
            }
            
            Debug.Log("[Connection] Remove not successful, ID not included.");
            return false; 

        }
        
        else if (mode == "set")
        {
            string objectIdsString = String.Join(", ", objectIds);
            Debug.Log("[Connection] Set connected Object IDs " + objectIdsString + "; LocalClientId " + NetworkManager.Singleton.LocalClientId);
    
            ClearConnectedWithObjectIds_ServerRpc();
            //connectedWithObjectIds.Clear();
            foreach (int elem in objectIds)
            {
                AddToConnectedWithObjectIds_ServerRpc(elem);
                //connectedWithObjectIds.Add(elem);
            }

            return true;
        }

        return false;

    }


    [ServerRpc(RequireOwnership = false)]
    private void AddToConnectedWithObjectIds_ServerRpc(int addId)
    {
        connectedWithObjectIds.Add(addId);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RemoveFromConnectedWithObjectIds_ServerRpc(int removeId)
    {
        connectedWithObjectIds.Remove(removeId);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ClearConnectedWithObjectIds_ServerRpc()
    {
        connectedWithObjectIds.Clear();
    }
    
    

}

public enum ConnectionType
{
    InputConnection, 
    OutputConnection
}



