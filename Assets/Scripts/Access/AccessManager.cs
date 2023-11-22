using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class AccessManager : NetworkBehaviour
{
    // Singleton pattern
    // (Comparing against initially created static instance in Awake) 
    private static AccessManager _singleton;
    public static AccessManager Singleton
    {
        get { return _singleton; }
        private set { _singleton = value; }
    }
    private void Awake()
    {
        if (_singleton != null && _singleton != this)
        {
            Destroy(this.gameObject);
        } else {
            _singleton = this;
        }
        

        // Init list 
        allClientIdsNames_InFullOnServer = new Dictionary<int, string>();
    }
  

    
    // Data available in full only on server 
    private Dictionary<int, int> uniqueObjectIdCreatorIdMap_InFullOnServer = new Dictionary<int, int>();
    private Dictionary<int, HashSet<int>> especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer = new Dictionary<int, HashSet<int>>(); // use HashSet as collection with unique elements 
    private Dictionary<int, AccessType> accessTypePerUniqueObjectId_InFullOnServer = new Dictionary<int, AccessType>(); 
    private Dictionary<int, string> allClientIdsNames_InFullOnServer;
    
    // Data synched to clients
    private List<ulong> connectedClients; 


    public override void OnNetworkSpawn()
    {
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SyncConnectedClients());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public List<ulong> GetConnectedClients()
    {
        return connectedClients; 
    }

    private IEnumerator SyncConnectedClients()
    {
        while (true)
        {
            UpdateConnectedClients_ServerRpc();
            yield return new WaitForSeconds(2);
        }
    }

    // Update Connected Client IDs from Server to local Clients 
    [ServerRpc(RequireOwnership = false)]
    private void UpdateConnectedClients_ServerRpc(ServerRpcParams serverRpcParams = default)
    {
        List<ulong> newList = new List<ulong>(); 
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            newList.Add(kvp.Key); 
        }
        connectedClients = newList;

    }
    
    
    
    //
    // ------- Update access privileges on actual GameObjects
    // ------- (Run only on server)
    //

    private void UpdateObjectAccessPrivileges(int[] uniqueObjectIds)
    {
        if (IsServer)
        {
            // Get GameObject dictionary
            Dictionary<int, GameObject> gameObjects = NetworkSpawner.Singleton.GetSpawnedObjectsDictionary();


            // Check privileges in dictionary and apply to objects
            foreach (int objectId in uniqueObjectIds)
            {

                // Update Access Type 
                if (gameObjects.Keys.Contains(objectId) & accessTypePerUniqueObjectId_InFullOnServer.Keys.Contains(objectId))
                {
                    gameObjects[objectId].GetComponent<ObjectInfo>()
                        .SetAccessType(accessTypePerUniqueObjectId_InFullOnServer[objectId]);
                }
                else
                {
                    Debug.Log("[AccessManager] UpdateObjectAccessPrivileges: Unknown unique object id " +
                              objectId.ToString());
                }

                // Known object 
                if (especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer.ContainsKey(objectId))
                {
                    gameObjects[objectId].GetComponent<ObjectInfo>()
                        .SetEspeciallyAllowedClientIds(especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[objectId]);
                }
                else // Not yet known 
                {
                    gameObjects[objectId].GetComponent<ObjectInfo>()
                        .SetEspeciallyAllowedClientIds(new HashSet<int>());
                }

            }
        }
        else
        {
            Debug.Log("[AccessManager] UpdateObjectAccessPrivileges: Not running as Server!");
        }

    }
    
    
    
    
    
    //
    // ---------- Modify especially allowed player ids 
    //
    
    
    
    
    // Runs only on server 
    // Set especially allowed player ids for specified unique object id
    // Wrapper to allow running on server directly 
    public void SetEspeciallyAllowed(int uniqueObjectId, int[] allowPlayerIds)
    {
        if (IsServer)
        {
            // Construct debug information 
            string allowedPlayersString = "";
            foreach (int id in allowPlayerIds)
            {
                if (id == allowPlayerIds.Last())
                {
                    allowedPlayersString += id.ToString();
                }
                else
                {
                    allowedPlayersString += id.ToString() + ", ";
                }
            }
            Debug.Log("[AccessManager] SetEspeciallyAllowed_ServerRpc: Unique Object ID " + uniqueObjectId.ToString() + ", Allow Player IDs: " + allowedPlayersString);
        

            // Clear access for unique object id before adding back client ids 
            especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uniqueObjectId] = new HashSet<int>();
        
            // Allow only specified player IDs 
            foreach (int playerId in allowPlayerIds)
            {
                especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uniqueObjectId].Add(playerId);
            }

            // Apply changes to actual game objects 
            UpdateObjectAccessPrivileges(new [] {uniqueObjectId});
        }
        else
        {
            SetEspeciallyAllowed_ServerRpc(uniqueObjectId, allowPlayerIds);
        }
        
    }
    
    
    
    // Runs only on server 
    // Set especially allowed player ids for specified unique object id
    [ServerRpc(RequireOwnership = false)]
    private void SetEspeciallyAllowed_ServerRpc(int uniqueObjectId, int[] allowPlayerIds, ServerRpcParams serverRpcParams = default)
    {
        
        // Construct debug information 
        string allowedPlayersString = "";
        foreach (int id in allowPlayerIds)
        {
            if (id == allowPlayerIds.Last())
            {
                allowedPlayersString += id.ToString();
            }
            else
            {
                allowedPlayersString += id.ToString() + ", ";
            }
        }
        Debug.Log("[AccessManager] SetEspeciallyAllowed_ServerRpc: Unique Object ID " + uniqueObjectId.ToString() + ", Allow Player IDs: " + allowedPlayersString);
        

        // Clear access for unique object id before adding back client ids 
        especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uniqueObjectId] = new HashSet<int>();
        
        // Allow only specified player IDs 
        foreach (int playerId in allowPlayerIds)
        {
            especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uniqueObjectId].Add(playerId);
        }

        // Apply changes to actual game objects 
        UpdateObjectAccessPrivileges(new [] {uniqueObjectId});
    }
    
    
   
    
    
    //
    // ------------- accessTypePerUniqueObjectId
    // 
    
    // Only server can modify accessTypePerUniqueObjectId ; wrapper to also be callable on server directly 
    public void SetAccessTypePerUniqueObjectId(AccessType accessType, int[] uniqueObjectIds)
    {
        
        // Run On Server 
        if (IsServer)
        {
            // Construct debug information 
            string uois = "";
            foreach (int id in uniqueObjectIds)
            {
                if (id == uniqueObjectIds.Last())
                {
                    uois += id.ToString();
                }
                else
                {
                    uois += id.ToString() + ", ";
                }
            }
            Debug.Log("[AccessManager] SetAccessTypePerUniqueObjectId_ServerRpc: Unique Object IDs " + uois + ", Access Type: " + accessType);

            
            foreach (int uniqueObjectId in uniqueObjectIds)
            {
                // Update dictionary  
                accessTypePerUniqueObjectId_InFullOnServer[uniqueObjectId] = accessType;
                
                // Reset especially allowed ids if access type is changed 
                if (accessType == AccessType.EveryoneCanModify | accessType == AccessType.NobodyCanModify)
                {
                    especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uniqueObjectId] = new HashSet<int>();
                }
            }
        
            // Apply changes to actual game objects 
            UpdateObjectAccessPrivileges(uniqueObjectIds);
        }

        
        // Or RPC to run on server 
        else
        {
            SetAccessTypePerUniqueObjectId_ServerRpc(accessType,uniqueObjectIds);
        }

    }
    
    // Only server can modify accessTypePerUniqueObjectId
    [ServerRpc(RequireOwnership = false)]
    private void SetAccessTypePerUniqueObjectId_ServerRpc(AccessType accessType, int[] uniqueObjectIds, ServerRpcParams serverRpcParams = default)
    {
        
        // Construct debug information 
        string uois = "";
        foreach (int id in uniqueObjectIds)
        {
            if (id == uniqueObjectIds.Last())
            {
                uois += id.ToString();
            }
            else
            {
                uois += id.ToString() + ", ";
            }
        }
        Debug.Log("[AccessManager] SetAccessTypePerUniqueObjectId_ServerRpc: Unique Object IDs " + uois + ", Access Type: " + accessType);

        
         
        foreach (int uniqueObjectId in uniqueObjectIds)
        {
            // Update dictionary  
            accessTypePerUniqueObjectId_InFullOnServer[uniqueObjectId] = accessType;
                
            // Reset especially allowed ids if access type is changed 
            if (accessType == AccessType.EveryoneCanModify | accessType == AccessType.NobodyCanModify)
            {
                especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uniqueObjectId] = new HashSet<int>();
            }
        }
        
        // Apply changes to actual game objects 
        UpdateObjectAccessPrivileges(uniqueObjectIds);

    }
    
   
    
    
    //
    // ------------- uniqueObjectIdCreatorIdMap
    // 
    
    // Only server can modify uniqueObjectIdCreatorIdMap; wrapper to also be callable on server directly 
    public void AddToUniqueObjectIdCreatorIdMap(int uniqueObjectId, int creatorId)
    {
        if (IsServer)
        {
            uniqueObjectIdCreatorIdMap_InFullOnServer.Add(uniqueObjectId,creatorId);
        }
        else
        {
            AddToUniqueObjectIdCreatorIdMap_ServerRpc(uniqueObjectId, creatorId);
        }
    }
    
    // Only server can modify uniqueObjectIdCreatorIdMap
    [ServerRpc(RequireOwnership = true)]
    private void AddToUniqueObjectIdCreatorIdMap_ServerRpc(int uniqueObjectId, int creatorId, ServerRpcParams serverRpcParams = default)
    {
        uniqueObjectIdCreatorIdMap_InFullOnServer.Add(uniqueObjectId,creatorId);
    }
    
    // Wrapper 
    public void RemoveFromUniqueObjectIdCreatorIdMap(int uniqueObjectId)
    {
        if (IsServer)
        {
            uniqueObjectIdCreatorIdMap_InFullOnServer.Remove(uniqueObjectId);
        }
        else
        {
            RemoveFromUniqueObjectIdCreatorIdMap_ServerRpc(uniqueObjectId);
        }
        
    }
    
    [ServerRpc(RequireOwnership = true)]
    private void RemoveFromUniqueObjectIdCreatorIdMap_ServerRpc(int uniqueObjectId, ServerRpcParams serverRpcParams = default)
    {
        uniqueObjectIdCreatorIdMap_InFullOnServer.Remove(uniqueObjectId);
        
    }
    
    
    
    
    //
    // ---------- Client IDs 
    //
    
    
    // Add Client ID to List of all client ids; wrapper to be able to run on server directly   
    public void AddClientIdName(int addId, string addName) {

        if (IsServer)
        {
            Debug.Log("[AddClientIdName] Initiated from Server. Added ID: " + addId + " with player name: " + addName);
            allClientIdsNames_InFullOnServer.Add(addId, addName);
        }
        else
        {
            AddClientIdName_ServerRpc(addId, addName);
        }
    }
    
    // Add Client ID to List of all client ids  
    [ServerRpc(RequireOwnership = false)] 
    private void AddClientIdName_ServerRpc(int addId, string addName, ServerRpcParams serverRpcParams = default) {
        
        Debug.Log("[AddClientIdName_ServerRpc] Initiated from ClientID: " + serverRpcParams.Receive.SenderClientId + "; Local Client ID " + NetworkManager.Singleton.LocalClientId + "; Added ID: " + addId + " with player name: " + addName);
        
        allClientIdsNames_InFullOnServer.Add(addId, addName);
    }
    
    
    // Remove Client ID (and name) e.g. when disconnected  
    public void RemoveClientIdName(int removeId) {
    
        // Make sure id exists in dict  
        if (!allClientIdsNames_InFullOnServer.ContainsKey(removeId))
        {
            return;
        }
        
        if (IsServer)
        {
            Debug.Log("[RemoveClientIdName] Initiated from Server. Removed ID: " + removeId + " with player name: " + allClientIdsNames_InFullOnServer[removeId]);
            allClientIdsNames_InFullOnServer.Remove(removeId);
        }
        else
        {
            RemoveClientIdName_ServerRpc(removeId);
        }
    }
    
    [ServerRpc(RequireOwnership = false)] 
    private void RemoveClientIdName_ServerRpc(int removeId, ServerRpcParams serverRpcParams = default) {
        
        Debug.Log("[RemoveClientIdName_ServerRpc] Initiated from ClientID: " + serverRpcParams.Receive.SenderClientId + "; Local Client ID " + NetworkManager.Singleton.LocalClientId + "; Removed ID: " + removeId + " with player name: " + allClientIdsNames_InFullOnServer[removeId]);
        
        allClientIdsNames_InFullOnServer.Remove(removeId);
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    


    
    
    ////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////
    /// Deprecated Code
    ///
    
    
    // Block modification for uniqueObjectsIds for players in blockPlayerIds
    // If blockEveryone is specified, nobody can modify and blockPlayerIds is ignored 
    [ServerRpc(RequireOwnership = false)]
    public void DEPRECATED_AddObjectBlock_ServerRpc(int[] uniqueObjectIds, int[] blockPlayerIds, bool blockEveryone = false, ServerRpcParams serverRpcParams = default)
    {
        //TODO: Check if Modification sent by player with accurate rights 

        // If modification is allowed, make change for all clients & server 
        DEPRECATED_AddObjectBlock_ClientRpc(uniqueObjectIds, blockPlayerIds, blockEveryone);
        
        // Apply changes to actual game objects 
        UpdateObjectAccessPrivileges(uniqueObjectIds);
    }
   
    // If change of access privileges was granted by server, run client rpc on all clients & Server and update  
    [ClientRpc]
    public void DEPRECATED_AddObjectBlock_ClientRpc(int[] uniqueObjectIds, int[] blockPlayerIds, bool blockEveryone = false)
    {
        //Run on clients & server! 
        
        foreach (int uoi in uniqueObjectIds)
        {
            // Block every known player ID, i.e. no allowed player per unique object  
            if (blockEveryone)
            {
                foreach (var key in especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer.Keys)
                {
                    especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[key].Clear();
                }
            }
            
            // Block only specified player IDs
            else
            {
                // Check for all known players if they are allowed 
                foreach (int playerId in allClientIdsNames_InFullOnServer.Keys)
                {
                    if (!blockPlayerIds.Contains(playerId))
                    {
                        // Check if key exists 
                        if (!especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer.ContainsKey(uoi))
                        {
                            especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uoi] = new HashSet<int>();
                        }

                        especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uoi].Add(playerId);
                    }
                    
                }
            }
        }
        
        
        
    }
    
    
    
     
    // Allow modification for uniqueObjectsIds for players in allowPlayerIds
    // If allowEveryone is specified, everyone can modify and allowPlayerIds is ignored 
    [ServerRpc(RequireOwnership = false)]
    public void DEPRECATED_RemoveObjectBlock_ServerRpc(int[] uniqueObjectIds, int[] allowPlayerIds, bool allowEveryone = false, ServerRpcParams serverRpcParams = default)
    {
        //TODO: Check if Modification sent by player with accurate rights 

        // If modification is allowed, make change for all clients & server  
        DEPRECATED_RemoveObjectBlock_ClientRpc(uniqueObjectIds, allowPlayerIds, allowEveryone);
        
        // Apply changes to actual game objects 
        UpdateObjectAccessPrivileges(uniqueObjectIds);

    }
    
    // If change of access privileges was granted by server, run client rpc on all clients & Server and update  
    [ClientRpc]
    public void DEPRECATED_RemoveObjectBlock_ClientRpc(int[] uniqueObjectIds, int[] allowPlayerIds, bool allowEveryone = false)
    {
        //Run on clients & server! 
        
        foreach (int uoi in uniqueObjectIds)
        {
            // Allow every known player ID 
            if (allowEveryone)
            {
                foreach (int playerId in allClientIdsNames_InFullOnServer.Keys)
                {
                    // Check if key exists 
                    if (!especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer.ContainsKey(uoi))
                    {
                        especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uoi] = new HashSet<int>();
                    }
                    especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uoi].Add(playerId);
                }
                
            }
            
            // Allow only specified player IDs
            else
            {
                foreach (int playerId in allowPlayerIds)
                {
                    // Check if key exists 
                    if (!especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer.ContainsKey(uoi))
                    {
                        especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uoi] = new HashSet<int>();
                    }
                    especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer[uoi].Add(playerId);
                    
                }
            }
        }
    }
    
    
    // Request full copy of playerIdsBlockedPerUniqueObjectId 
    [ServerRpc(RequireOwnership = false)] 
    private void DEPRECATED_RequestObjectAccessDict_ServerRpc(ServerRpcParams serverRpcParams = default) {
        
        Debug.Log("[RequestObjectAccessDict_ServerRpc] Initiated from ClientID: " + serverRpcParams.Receive.SenderClientId + "; Local Client ID " + NetworkManager.Singleton.LocalClientId);

        SerializedIntHashSetIntDict serialized = DataSerializer.Singleton.SerializeIntHashSetIntDict(especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer);
        DEPRECATED_CopyObjectAccessDict_ClientRpc(serialized, (int)serverRpcParams.Receive.SenderClientId);
        
    }
    
    
    // Copy entire Dictionary playerIdsBlockedPerUniqueObjectId to the clients on Client's connect  
    [ClientRpc]
    private void DEPRECATED_CopyObjectAccessDict_ClientRpc(SerializedIntHashSetIntDict serializedAccessDict, int applyToClientId, ClientRpcParams clientRpcParams = default)
    {
        // On Server do nothing, since already up-to-date
        if (IsServer)
        {
            return; 
        }

        // On Clients copy data 
        if ((int)NetworkManager.Singleton.LocalClientId == applyToClientId)
        {
            Debug.Log("[CopyObjectAccessDict_ClientRpc] Local Client ID " + NetworkManager.Singleton.LocalClientId);
            
            // DeSerialize and store 
            especiallyAllowedPlayerIdsPerUniqueObjectId_InFullOnServer =
                DataSerializer.Singleton.ReconstructIntHashSetIntDict(serializedAccessDict);
        }
        
    }
    
    
    // Method to request accessTypePerUniqueObjectId
    [ServerRpc(RequireOwnership = false)] 
    private void DEPRECATED_RequestAccessTypePerUniqueObjectId_ServerRpc(ServerRpcParams serverRpcParams = default) {
        
        Debug.Log("[RequestAccessTypePerUniqueObjectId_ServerRpc] Initiated from ClientID: " + serverRpcParams.Receive.SenderClientId + "; Local Client ID " + NetworkManager.Singleton.LocalClientId);

        SerializedIntIntDict serialized = DataSerializer.Singleton.SerializeIntAccessTypeDict(accessTypePerUniqueObjectId_InFullOnServer);
        DEPRECATED_UpdateAccessTypePerUniqueObjectId_ClientRpc(serialized, (int)serverRpcParams.Receive.SenderClientId);
        
    }
    
    // Copy accessTypePerUniqueObjectId to Client 
    [ClientRpc]
    private void DEPRECATED_UpdateAccessTypePerUniqueObjectId_ClientRpc(SerializedIntIntDict serializedDict, int receivingClientId,
        ClientRpcParams clientRpcParams = default)
    {
        
        if ((int)NetworkManager.Singleton.LocalClientId == receivingClientId) 
        {
            accessTypePerUniqueObjectId_InFullOnServer = DataSerializer.Singleton.ReconstructIntAccessTypeDict(serializedDict);
            
            Debug.Log("[UpdateAccessTypePerUniqueObjectId_ServerRpc] Local Client ID " + NetworkManager.Singleton.LocalClientId);


        }
        else
        {
            
        }

    }

    
    // Method to request UniqueObjectID CreatorID Map 
    [ServerRpc(RequireOwnership = false)] 
    private void DEPRECATED_RequestObjectCreatorIdMap_ServerRpc(ServerRpcParams serverRpcParams = default) {
        
        Debug.Log("[RequestObjectCreatorIdMap_ServerRpc] Initiated from ClientID: " + serverRpcParams.Receive.SenderClientId + "; Local Client ID " + NetworkManager.Singleton.LocalClientId);

        SerializedIntIntDict serialized = DataSerializer.Singleton.SerializeIntIntDict(uniqueObjectIdCreatorIdMap_InFullOnServer);
        DEPRECATED_UpdateObjectCreatorIdMap_ClientRpc(serialized, (int)serverRpcParams.Receive.SenderClientId);
        
    }
    
    // Copy UniqueObjectID CreatorID Map to Client 
    [ClientRpc]
    private void DEPRECATED_UpdateObjectCreatorIdMap_ClientRpc(SerializedIntIntDict serializedDict, int receivingClientId,
        ClientRpcParams clientRpcParams = default)
    {
        
        if ((int)NetworkManager.Singleton.LocalClientId == receivingClientId) 
        {
      
            uniqueObjectIdCreatorIdMap_InFullOnServer = DataSerializer.Singleton.ReconstructIntIntDict(serializedDict);
            
            Debug.Log("[ReceiveObjectCreatorIdMap_ClientRpc] Local Client ID " + NetworkManager.Singleton.LocalClientId);


        }
        else
        {
            
        }

    }
    
    

}
