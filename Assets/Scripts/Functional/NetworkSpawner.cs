using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using UnityEngine;
using Unity.Netcode;

public class NetworkSpawner : NetworkBehaviour
{
    
    // Singleton pattern
    // (Comparing against initially created static instance in Awake) 
    private static NetworkSpawner _singleton;
    public static NetworkSpawner Singleton
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
        
        
        // Initialize variables 
        currentlyUsedLowestBaseUniqueObjectId_InFullOnServer = 10000;
        
    }
    
    
    // Dictionary of objects spawned by Network Owner 
    // Clients manage this dictionary themselves, i.e. fully available everywhere 
    private Dictionary<int, GameObject> spawnedObjects = new Dictionary<int, GameObject>();
    
    
    // List of available objects for spawning
    [SerializeField] private List<GameObject> spawnableObjectsList;
    
    
    // Currently used lowest base Unique Object ID
    // Available in full only on server! And used only on server. 
    // Increase per main component in 10000s 
    // Increase per sub component by 1
    [SerializeField] private int currentlyUsedLowestBaseUniqueObjectId_InFullOnServer;

    
    // Access Spawned Object Dictionary 
    public Dictionary<int, GameObject> GetSpawnedObjectsDictionary()
    {
        /*
        if (!IsServer)
        {
            Debug.Log("[NetworkSpawner] GetSpawnedObjectsDictionary: Needs to be called on Server!");
            return null; 
        }
        */

        //Debug.Log("accessing spawned objects dict, by " + NetworkManager.Singleton.LocalClientId); 
        return spawnedObjects; 
        
        
    }
    
    
    
    // Access Spawned Object Dictionary by ID
    public GameObject GetSpawnedObjectById(int uniqueObjectId)
    {
        if (spawnedObjects.ContainsKey(uniqueObjectId))
        {
            return spawnedObjects[uniqueObjectId];
        }
        else
        {
            return null;
        }
        
    }
    
    
    
    // Add Spawned Object ID and GameObject to Dictionary for local access
    public void AddUniqueObjectIdAndCorrespondingGameObject(int uniqueObjectId, GameObject obj)
    {
        Debug.Log("[NetworkSpawner] AddUniqueObjectIdAndCorrespondingGameObject: Adding ID " + uniqueObjectId.ToString() + " , GameObject " + obj.GetComponent<ObjectInfo>().objectName);
        
            // If not already present in dictionary, add 
            if (!spawnedObjects.ContainsKey(uniqueObjectId))
            {
                spawnedObjects.Add(uniqueObjectId, obj);
            }
            else
            {
                Debug.Log("[NetworkSpawner] AddUniqueObjectIdAndCorrespondingGameObject: Object already exists.");
            }
        

    }


    // Despawn everything, can only be used on Server
    public void DespawnEverything()
    {
        if (!IsServer)
        {
            Debug.Log("[NetworkSpawner] DespawnEverything: Not running as Server!");
            return;
        }

        foreach (KeyValuePair<int,GameObject> kvp in spawnedObjects)
        {
            Destroy(kvp.Value);
        }
        
        // Clear locally and on all clients 
        spawnedObjects.Clear();
        RemoveSpawnedIds_ClientRPC(0, true);
        
        
        // Remove all drawings locally and on clients
        GameObject[] drawings = GameObject.FindGameObjectsWithTag(ExperienceManager.Singleton.drawingGameObjectTag);
        foreach (GameObject drawing in drawings)
        {
            Destroy(drawing);
        }
        RemoveAllDrawings_ClientRPC();
        

        // Reset ID offset 
        currentlyUsedLowestBaseUniqueObjectId_InFullOnServer = 10000; 
    }


    public void RemoveDespawnedId(int uniqueObjectId)
    {
        // Delete locally 
        if (spawnedObjects.ContainsKey(uniqueObjectId))
        {
            spawnedObjects.Remove(uniqueObjectId);
        }
        
        // And on all clients 
        RemoveSpawnedIds_ClientRPC(uniqueObjectId, false);
        
    }


    [ClientRpc]
    private void RemoveAllDrawings_ClientRPC(ClientRpcParams clientRpcParams = default)
    {
        GameObject[] drawings = GameObject.FindGameObjectsWithTag(ExperienceManager.Singleton.drawingGameObjectTag);

        foreach (GameObject drawing in drawings)
        {
            Destroy(drawing);
        }
        
    }
    
    

    [ClientRpc]
    private void RemoveSpawnedIds_ClientRPC(int removeId, bool clearAll, ClientRpcParams clientRpcParams = default)
    {

        if (clearAll)
        {
            spawnedObjects.Clear();
        }
        else
        {
            if (spawnedObjects.ContainsKey(removeId))
            {
                spawnedObjects.Remove(removeId);
            }
        }
        
    }
    
    
    

    // Spawn Object wrapper for RPC 
    public void SpawnObject(SpawnableObject spawnObject, Vector3 spawnPosition, Vector3 spawnRotation, Vector3 spawnSize, string uniqueName = "", bool overrideObjectId = false, int objectId = -1)
    {
        SpawnOnServer_ServerRpc(spawnObject, spawnPosition, spawnRotation, spawnSize, uniqueName, overrideObjectId, objectId);
    }
    
    
    
    
    
    // Spawn Objects RPC 
    [ServerRpc(RequireOwnership = false)] 
    private void SpawnOnServer_ServerRpc(SpawnableObject spawnObject, Vector3 spawnPosition, Vector3 spawnRotation, Vector3 spawnSize, string uniqueName = "", bool overrideObjectId = false, int objectId = -1, ServerRpcParams serverRpcParams = default) {
        
        
        Debug.Log("[NetworkSpawner] SpawnOnServer: Object " + spawnObject.ToString() + "; Local Client ID " + NetworkManager.Singleton.LocalClientId);
        
        // Spawn & move to position & apply rotation and size
        GameObject spawnedGameObject = Instantiate(spawnableObjectsList[((int)spawnObject)]);
        spawnedGameObject.GetComponent<NetworkObject>().Spawn(true);
        
        
        spawnedGameObject.transform.position = spawnPosition;
        spawnedGameObject.transform.rotation = Quaternion.Euler(spawnRotation);
        spawnedGameObject.transform.localScale = spawnSize;


        ClientNetworkTransform tr = new ClientNetworkTransform();


        // Set unique object id
        if (!overrideObjectId)
        {
            spawnedGameObject.GetComponent<ObjectInfo>().SetUniqueObjectId(currentlyUsedLowestBaseUniqueObjectId_InFullOnServer);
            currentlyUsedLowestBaseUniqueObjectId_InFullOnServer += 10000;
        }
        else
        {
            spawnedGameObject.GetComponent<ObjectInfo>().SetUniqueObjectId(objectId);
            currentlyUsedLowestBaseUniqueObjectId_InFullOnServer = ((int) (objectId / 10000)) * 10000 + 10000;
        }
        
        
        // Set unique name if specified 
        spawnedGameObject.GetComponent<ObjectInfo>().uniqueSpawningName.Value = new FixedString64Bytes(uniqueName);
        
        
        
        // Add access type to access manager 
        // Clients will add the objects to the spawned objects dictionary themselves at start of objects 
        // Server adds here already 
        
        AddUniqueObjectIdAndCorrespondingGameObject(
           spawnedGameObject.GetComponent<ObjectInfo>().GetUniqueObjectId(), spawnedGameObject);
        
        AccessManager.Singleton.SetAccessTypePerUniqueObjectId(AccessType.EveryoneCanModify,
            new int[] {spawnedGameObject.GetComponent<ObjectInfo>().GetUniqueObjectId()});

        // Check if main object with sub objects 
        if (spawnedGameObject.GetComponent<MainObjectInfo>() != null)
        {
            // Add spawnable object id 
            spawnedGameObject.GetComponent<MainObjectInfo>().SetSpawnableObjectId((int)spawnObject);
            
            // Update sub objects 
            foreach (var subObjectInfo in spawnedGameObject.GetComponent<MainObjectInfo>().GetSubObjectAndConnectionsInfoList())
            {
                AddUniqueObjectIdAndCorrespondingGameObject(subObjectInfo.GetUniqueObjectId(), subObjectInfo.gameObject);
                AccessManager.Singleton.SetAccessTypePerUniqueObjectId(AccessType.EveryoneCanModify,
                    new int[]{subObjectInfo.GetUniqueObjectId()});
            }
        }
        
        
        
        
    }




    public SpawnableObject GetSpawnableObjectForObjectName(string objectName)
    {
        switch (objectName.ToLower())
        {
            case "bandpassfilter":
                return SpawnableObject.BandPassFilter;
            case "highpassfilter":
                return SpawnableObject.HighPassFilter;
            case "lowpassfilter":
                return SpawnableObject.LowPassFilter;
            case "noisegenerator":
                return SpawnableObject.NoiseGenerator;
            case "outputspeaker":
                return SpawnableObject.OutputSpeaker;
            case "samvirtualanalog":
                return SpawnableObject.SAMVirtualAnalog;
            case "sampleanalyzer":
                return SpawnableObject.SampleAnalyzer;
            case "sawtoothgenerator":
                return SpawnableObject.SawtoothGenerator;
            case "signalcombiner":
                return SpawnableObject.SignalCombiner;
            case "sinewavegenerator":
                return SpawnableObject.SineWaveGenerator;
            case "spectrumanalyzer":
                return SpawnableObject.SpectrumAnalyzer;
            case "speechbubble":
                return SpawnableObject.SpeechBubble;
            case "squarewavegenerator":
                return SpawnableObject.SquareWaveGenerator;
            case "erasertool":
                return SpawnableObject.EraserTool;
            case "pentool":
                return SpawnableObject.PenTool;
            case "connectioncable":
                return SpawnableObject.ConnectionCable;
            
            default:
                Debug.Log("[NetworkSpawner] Could not find SpawnableObject for name" + objectName + "!");
                return SpawnableObject.DefaultNull;
        }
    }


}


// Keep track of spawnable Objects 
public enum SpawnableObject
{
    BandPassFilter, // Starts at 0 
    HighPassFilter,
    LowPassFilter,
    NoiseGenerator,
    OutputSpeaker, 
    SAMVirtualAnalog,
    SampleAnalyzer,
    SawtoothGenerator,
    SignalCombiner,
    SineWaveGenerator,
    SpectrumAnalyzer,
    SpeechBubble,
    SquareWaveGenerator,
    EraserTool,
    PenTool,
    ConnectionCable,
    DefaultNull
}

