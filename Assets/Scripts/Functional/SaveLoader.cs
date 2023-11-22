using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;

public class SaveLoader : NetworkBehaviour
{
    
    
    // Singleton pattern
    // (Comparing against initially created static instance in Awake) 
    private static SaveLoader _singleton;
    public static SaveLoader Singleton
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
        
    }


    private void Update()
    {
        
        
    }

    private void Start()
    {
        GetAvailableSceneSetups();
    }


    public List<string> GetAvailableSceneSetups()
    {
        
        List<string> paths = new List<string>();
        
        // Make sure directory exists, create only if not existent 
        System.IO.Directory.CreateDirectory(ExperienceManager.Singleton.storeSceneSetupsPath);

        DirectoryInfo info = new DirectoryInfo(ExperienceManager.Singleton.storeSceneSetupsPath);
        FileInfo[] fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo)
        {
            
            if (file.Name.ToString().StartsWith("SceneSetup_") && file.Name.ToString().EndsWith(".json"))
            {
                paths.Add(file.ToString());
            }
            
        }
        
        return paths;

    }

    
    // Call from outside this script 
    public void SaveSceneSetup()
    {
        SerializeScene_ServerRPC();
    }

    // Server RPC that generates the Scene Setup and sends it back to the Client that requested the Setup 
    [ServerRpc(RequireOwnership = false)]
    private void SerializeScene_ServerRPC(ServerRpcParams serverRpcParams = default)
    {
        
        // Determine who requested serialization and who to send back to 
        /*
        ulong sendByClientId = serverRpcParams.Receive.SenderClientId;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{sendByClientId}
            }
        };
        */ 
        
        // Send saved scene to all clients
        

        // Generate lists
        List<int> mainObjectIds = new List<int>();
        //List<int> correspondingSpawnableObjects = new List<int>(); 
        List<string> mainObjectSpawnableNames = new List<string>();
        List<Vector3> mainObjectPositions = new List<Vector3>();
        List<Vector3> mainObjectRotations = new List<Vector3>();
        List<Vector3> mainObjectSizes = new List<Vector3>();
        List<string> mainObjectUniqueSpawningNames = new List<string>();
        
        List<int> subObjectIds = new List<int>();
        List<InteractableValueTypes> subObjectValueTypes = new List<InteractableValueTypes>();
        List<bool> subObjectBoolValues = new List<bool>(); // Will have false if sub object id has other interactable type 
        List<int> subObjectIntValues = new List<int>(); // Will have 0 if sub object id has other interactable type 
        List<float> subObjectFloatValues = new List<float>();  // Will have 0 if sub object id has other interactable type 
        List<string> subObjectNarrationFileNames = new List<string>(); // Will have "" if sub object id has other interactable type 

        
        List<int> connectionsFrom = new List<int>();
        List<int> connectionsTo = new List<int>();
        List<string> cableUniqueSpawningNames = new List<string>();
        
    
        // Store drawing line renderers
        List<int> drawingPositionsCount = new List<int>();
        List<Vector3> drawingLineRendererPositions = new List<Vector3>();
        List<int> drawingColorIdx = new List<int>();


        

        Dictionary<int, GameObject> objectsDict = NetworkSpawner.Singleton.GetSpawnedObjectsDictionary();
        foreach (KeyValuePair<int, GameObject> kvp in objectsDict)
        {
            
            // Deal with main object infos 
            if (kvp.Value.GetComponent<ObjectInfo>() is MainObjectInfo)
            {
                mainObjectIds.Add(kvp.Value.GetComponent<MainObjectInfo>().GetUniqueObjectId());
                mainObjectSpawnableNames.Add(kvp.Value.GetComponent<MainObjectInfo>().objectName);
                
                try
                {   // Get position and rotation from rigidbody, scale from object root 
                    mainObjectPositions.Add(kvp.Value.transform.Find("RigidBody").position);
                    mainObjectRotations.Add(kvp.Value.transform.Find("RigidBody").rotation.eulerAngles);
                    mainObjectSizes.Add(kvp.Value.transform.localScale);
                }
                catch
                {
                    // If rigidbody is not present
                    mainObjectPositions.Add(kvp.Value.transform.position);
                    mainObjectRotations.Add(kvp.Value.transform.rotation.eulerAngles);
                    mainObjectSizes.Add(kvp.Value.transform.localScale);
                }
                
                
                mainObjectUniqueSpawningNames.Add(kvp.Value.GetComponent<MainObjectInfo>().uniqueSpawningName.Value.ToString());
            }
            
            // Sub Object Infos 
            else if (kvp.Value.GetComponent<ObjectInfo>() is SubObjectInfo)
            {
                
                // Determine Interactable Value Type 
                InteractableValueTypes valueType;
                if (kvp.Value.GetComponent<SubObjectInfo>().GetObjectType() == ObjectType.BooleanInteractionInput)
                {
                    subObjectIds.Add(kvp.Value.GetComponent<SubObjectInfo>().GetUniqueObjectId());
                    valueType = InteractableValueTypes.Boolean;
                    subObjectBoolValues.Add(kvp.Value.GetComponent<BooleanInteractable>().GetState());
                    subObjectIntValues.Add(0);
                    subObjectFloatValues.Add(0);
                    subObjectValueTypes.Add(valueType);
                    subObjectNarrationFileNames.Add("");
                    
                }
                else if (kvp.Value.GetComponent<SubObjectInfo>().GetObjectType() == ObjectType.IntegerInteractionInput)
                {
                    subObjectIds.Add(kvp.Value.GetComponent<SubObjectInfo>().GetUniqueObjectId());
                    valueType = InteractableValueTypes.Integer;
                    subObjectBoolValues.Add(false);
                    subObjectIntValues.Add(kvp.Value.GetComponent<IntegerInteractable>().GetState());
                    subObjectFloatValues.Add(0);
                    subObjectValueTypes.Add(valueType);
                    subObjectNarrationFileNames.Add("");
                }
                else if (kvp.Value.GetComponent<SubObjectInfo>().GetObjectType() == ObjectType.FloatInteractionInput)
                {
                    subObjectIds.Add(kvp.Value.GetComponent<SubObjectInfo>().GetUniqueObjectId());
                    valueType = InteractableValueTypes.Float;
                    subObjectBoolValues.Add(false);
                    subObjectIntValues.Add(0);
                    subObjectFloatValues.Add(kvp.Value.GetComponent<FloatInteractable>().GetState());
                    subObjectValueTypes.Add(valueType);
                    subObjectNarrationFileNames.Add("");
                }
                
                else if (kvp.Value.GetComponent<SubObjectInfo>().GetObjectType() ==
                         ObjectType.FloatArrayInteractionInput)
                {
                    
                    // Deal with narration objects 
                    if (kvp.Value.GetComponent<SoundStoreFloatArrayInteractable>() != null)
                    {
                        string narrationFileName = kvp.Value.GetComponent<SoundStoreFloatArrayInteractable>()
                            .WriteAudioToFile(ExperienceManager.Singleton.storeNarrationsPath);
                        
                        subObjectIds.Add(kvp.Value.GetComponent<SubObjectInfo>().GetUniqueObjectId());
                        valueType = InteractableValueTypes.FloatArrayNarration;
                        subObjectBoolValues.Add(false);
                        subObjectIntValues.Add(0);
                        subObjectFloatValues.Add(0);
                        subObjectValueTypes.Add(valueType);
                        subObjectNarrationFileNames.Add(narrationFileName);
                        
                    }
                    
                    // Skip others (not defined) 
                    
                }
                
                else if (kvp.Value.GetComponent<SubObjectInfo>().GetObjectType() == ObjectType.ConnectionInput |
                         kvp.Value.GetComponent<SubObjectInfo>().GetObjectType() == ObjectType.ConnectionOutput)
                {
                    // skip inputs and outputs 
                }
                else
                {
                    valueType = InteractableValueTypes.Unknown;
                    Debug.Log("[SaveLoader] Serialize Scene: Unknown Interactable Type!");
                }
                
            }
            
            // Deal with cables 
            else if (kvp.Value.GetComponent<ObjectInfo>() is CableObjectInfo)
            {
                // Cable is actually connecting something 
                if (kvp.Value.GetComponent<ConnectionCable>().GetIsFirstConnectorConnected() &&
                    kvp.Value.GetComponent<ConnectionCable>().GetIsSecondConnectorConnected())
                {
                    connectionsFrom.Add(kvp.Value.GetComponent<ConnectionCable>().GetFirstConnectorConnectedToId());
                    connectionsTo.Add(kvp.Value.GetComponent<ConnectionCable>().GetSecondConnectorConnectedToId());
                    cableUniqueSpawningNames.Add(kvp.Value.GetComponent<CableObjectInfo>().uniqueSpawningName.Value.ToString());
                }
            }
            else
            {
                Debug.Log("[SaveLoader] Serialize Scene: Unknown ObjectInfo type!");
            }
            
        }
        
        // make sure that all string arrays contain at least one dummy element
        if (mainObjectUniqueSpawningNames.Count < 1)
        {
            mainObjectUniqueSpawningNames.Add("");
        }

        if (cableUniqueSpawningNames.Count < 1)
        {
            cableUniqueSpawningNames.Add("");
        }
        
        
        // deal with drawings 
        
        // Find all drawing game objects & construct lists 
        GameObject[] drawings = GameObject.FindGameObjectsWithTag(ExperienceManager.Singleton.drawingGameObjectTag);

        foreach (GameObject drawing in drawings)
        {
            LineRenderer currentLineRenderer = drawing.GetComponent<LineRenderer>();
            DrawingLineRendererInfo info = drawing.GetComponent<DrawingLineRendererInfo>();
            Vector3[] currentPositions = new Vector3[currentLineRenderer.positionCount];
            currentLineRenderer.GetPositions(currentPositions);
            
            drawingPositionsCount.Add(currentLineRenderer.positionCount);
            drawingColorIdx.Add(info.colorIdx);
            drawingLineRendererPositions.AddRange(currentPositions);
        }
        
        
       
        SceneSetup sceneSetup = new SceneSetup
        {
            mainObjectIds = mainObjectIds.ToArray(),
            mainObjectSpawnableNames = new SerializableStrings
            {
                stringArray = mainObjectSpawnableNames.ToArray()
            },
            mainObjectPositions = mainObjectPositions.ToArray(),
            mainObjectRotations = mainObjectRotations.ToArray(),
            mainObjectSizes = mainObjectSizes.ToArray(),
            mainObjectUniqueSpawningNames = new SerializableStrings
            {
                stringArray = mainObjectUniqueSpawningNames.ToArray()
            },

            subObjectIds = subObjectIds.ToArray(),
            subObjectValueTypes = subObjectValueTypes.ToArray(),
            subObjectBoolValues = subObjectBoolValues.ToArray(),
            subObjectIntValues = subObjectIntValues.ToArray(),
            subObjectFloatValues = subObjectFloatValues.ToArray(),
            subObjectNarrationFileNames = new SerializableStrings()
            {
                stringArray = subObjectNarrationFileNames.ToArray()
            },
            
            connectionsFrom = connectionsFrom.ToArray(),
            connectionsTo = connectionsTo.ToArray(),
            cableUniqueSpawningNames = new SerializableStrings
            {
                stringArray = cableUniqueSpawningNames.ToArray()
            },
            
            drawingColorIdx = drawingColorIdx.ToArray(),
            drawingPositionsCount = drawingPositionsCount.ToArray(),
            drawingLineRendererPositions = drawingLineRendererPositions.ToArray()
        };
        

        string fileName = "SceneSetup_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        fileName += ".json";
        ReceiveSceneSetup_ClientRPC(fileName, sceneSetup);
        
    }
    

    [ClientRpc]
    private void ReceiveSceneSetup_ClientRPC(string fileName, SceneSetup sceneSetup, ClientRpcParams clientRpcParams = default)
    {
        
        string jsonData = JsonUtility.ToJson(sceneSetup);
        DataSerializer.Singleton.WriteJsonToDisk(ExperienceManager.Singleton.storeSceneSetupsPath, fileName, jsonData);
        
        Debug.Log("[SaveLoader] Received Scene Setup on Client and wrote to disk.");
        
    }



    // Call from outside 
    public void LoadSceneSetup(string fileName)
    {
        string jsonData = DataSerializer.Singleton.ReadJsonFromDisk(ExperienceManager.Singleton.storeSceneSetupsPath, fileName);
        SceneSetup sceneSetup = JsonUtility.FromJson<SceneSetup>(jsonData);
     
        LoadSceneSetup_ServerRpc(sceneSetup, fileName);
        
    }
    
    
    

    // Server RPC called by Client to Load certain Scene 
    // Called by other local method that first loads from json file 
    [ServerRpc(RequireOwnership = false)]
    private void LoadSceneSetup_ServerRpc(SceneSetup sceneSetup, string fileName, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("[SaveLoader] LoadSceneSetup ServerRPC from file " + fileName);
        
        
        // Despawn everything 
        NetworkSpawner.Singleton.DespawnEverything();
        
        // Spawn main objects 
        for (int i = 0; i < sceneSetup.mainObjectIds.Length; i++)
        {
            
            SpawnableObject spawnableObject = NetworkSpawner.Singleton.GetSpawnableObjectForObjectName(sceneSetup.mainObjectSpawnableNames.stringArray[i]);
            NetworkSpawner.Singleton.SpawnObject(spawnableObject, sceneSetup.mainObjectPositions[i], sceneSetup.mainObjectRotations[i], sceneSetup.mainObjectSizes[i], sceneSetup.mainObjectUniqueSpawningNames.stringArray[i], true, sceneSetup.mainObjectIds[i]);
            
        }
        
        // Set sub object values 
        StartCoroutine(SetSubObjectValuesDelayed(sceneSetup));

        // Create cables delayed 
        StartCoroutine(CreateCablesDelayed(sceneSetup));
        
        // Create annotations 
        CreateAnnotations_ClientRpc(sceneSetup.drawingPositionsCount, sceneSetup.drawingColorIdx, sceneSetup.drawingLineRendererPositions);
        

    }


    private IEnumerator SetSubObjectValuesDelayed(SceneSetup sceneSetup)
    {
        yield return new WaitForSeconds(0.1f);

        // Update values depending on interactable type
        for (int i = 0; i < sceneSetup.subObjectIds.Length; i++)
        {
            if (sceneSetup.subObjectValueTypes[i] == InteractableValueTypes.Boolean)
            {
                NetworkSpawner.Singleton.GetSpawnedObjectById(sceneSetup.subObjectIds[i]).GetComponent<BooleanInteractable>().RestoreValue(sceneSetup.subObjectBoolValues[i]);
            }
            else if (sceneSetup.subObjectValueTypes[i] == InteractableValueTypes.Integer)
            {
                NetworkSpawner.Singleton.GetSpawnedObjectById(sceneSetup.subObjectIds[i]).GetComponent<IntegerInteractable>().RestoreValue(sceneSetup.subObjectIntValues[i]);
            }
            else if (sceneSetup.subObjectValueTypes[i] == InteractableValueTypes.Float)
            {
                NetworkSpawner.Singleton.GetSpawnedObjectById(sceneSetup.subObjectIds[i]).GetComponent<FloatInteractable>().RestoreValue(sceneSetup.subObjectFloatValues[i]);
            }
            else if (sceneSetup.subObjectValueTypes[i] == InteractableValueTypes.FloatArrayNarration)
            {
                NetworkSpawner.Singleton.GetSpawnedObjectById(sceneSetup.subObjectIds[i]).GetComponent<SoundStoreFloatArrayInteractable>().RestoreValueAndMetaDataFromAudioFile(sceneSetup.subObjectNarrationFileNames.stringArray[i]);
            }
            else
            {
                Debug.Log("[SaveLoader] SetSubObjectValuesDelayed: Unknown Interactable Type!");
            }
        }
        
    }

    private IEnumerator CreateCablesDelayed(SceneSetup sceneSetup)
    {
        yield return new WaitForSeconds(0.2f);
        
        for (int i = 0; i < sceneSetup.connectionsFrom.Length; i++)
        {
            // Spawn cable 
            NetworkSpawner.Singleton.SpawnObject(SpawnableObject.ConnectionCable, new Vector3(0, -100, 0),
                new Vector3(0,0,0), new Vector3(1,1,1), sceneSetup.cableUniqueSpawningNames.stringArray[i]);
            
            // Find cable again 
            GameObject spawnedCableGameObject;
            while (true)
            {
                spawnedCableGameObject = GameObject.Find(sceneSetup.cableUniqueSpawningNames.stringArray[i]);
                if (spawnedCableGameObject == null)
                {
                    yield return new WaitForSeconds(0.01f);
                }
                else
                {
                    break;
                }
            }
            ConnectionCable spawnedCable = spawnedCableGameObject.GetComponent<ConnectionCable>();
            
            // Setup spawned cable 
            spawnedCable.SetSecondConnectorConnectionState(true, sceneSetup.connectionsTo[i]);
            spawnedCable.SetFirstConnectorConnectionState(true, sceneSetup.connectionsFrom[i]);
            spawnedCable.ChangeConnectorVisibility(FirstOrSecondConnector.Second, true); 
            spawnedCable.ChangeConnectorVisibility(FirstOrSecondConnector.First, true); 
            spawnedCable.ChangeLinerendererVisibility(true); 
        
            
            
            // TODO 
            // Move first connector of cable to place point, delay to make sure id is set properly as network variable 
            // Broadcast to all Clients 
            
            //StartCoroutine(PlaceConnectorCoroutine(spawnedCable.GetUniqueObjectId(), FirstOrSecondConnector.First));
            
        }
        
    }


    [ClientRpc]
    private void CreateAnnotations_ClientRpc(int[] drawingPositionsCount, int[] drawingColorIdx, Vector3[] drawingLineRendererPositions)
    {
        
        
        // Create annotations 
        int lowerBound = 0;
        for (int idx = 0; idx < drawingPositionsCount.Length; idx++)
        {
            LineRenderer currentDrawing = new GameObject().AddComponent<LineRenderer>();
            currentDrawing.name = "DrawingLineRenderer";
            currentDrawing.tag = ExperienceManager.Singleton.drawingGameObjectTag;
            currentDrawing.startWidth = currentDrawing.endWidth = ExperienceManager.Singleton.drawingToolWidth;
            currentDrawing.AddComponent<DrawingLineRendererInfo>();
            
            currentDrawing.GetComponent<DrawingLineRendererInfo>().colorIdx = drawingColorIdx[idx];
            currentDrawing.material = ExperienceManager.Singleton.drawingMaterials[drawingColorIdx[idx]];
            currentDrawing.positionCount = drawingPositionsCount[idx];
            currentDrawing.SetPositions(drawingLineRendererPositions.Skip(lowerBound).Take(drawingPositionsCount[idx]).ToArray());
            currentDrawing.GetComponent<DrawingLineRendererInfo>().GenerateLineRendererColliders();
            lowerBound += drawingPositionsCount[idx];
        }
        
    }
    
    
    
    /*
    
    private IEnumerator PlaceConnectorCoroutine(int uniqueObjectId, FirstOrSecondConnector firstOrSecondConnector)
    {
        yield return new WaitForSeconds(0.1f);
        
        PlaceConnector_ServerRpc(uniqueObjectId, firstOrSecondConnector);
    }
    
    // Wrapper Method to broadcast visibility change to all clients 
    [ServerRpc(RequireOwnership = false)]
    private void PlaceConnector_ServerRpc(int uniqueObjectId, FirstOrSecondConnector firstOrSecondConnector)
    {
        PlaceConnector_ClientRpc(uniqueObjectId, firstOrSecondConnector);
    }

    [ClientRpc]
    private void PlaceConnector_ClientRpc(int uniqueObjectId, FirstOrSecondConnector firstOrSecondConnector)
    {
        
        GameObject spawnedObject = NetworkSpawner.Singleton.GetSpawnedObjectById(uniqueObjectId);
        
        if (spawnedObject != null)
        {
            ConnectionCable spawnedCable = spawnedObject.GetComponent<ConnectionCable>();

            if (spawnedCable != null)
            {
                Grabbable grabbable;
                
                if (firstOrSecondConnector == FirstOrSecondConnector.First)
                {
                    grabbable = spawnedCable.GetFirstConnector().GetComponent<Grabbable>();
                }
                else // second connector 
                {
                    grabbable = spawnedCable.GetSecondConnector().GetComponent<Grabbable>();
                }
                
                if (grabbable != null)
                {
                    placePoint.Place(grabbable, true); // make sure held place only is disabled in place point
                }
                
            }
            
        }
        
        
        
    }
    
    */




}





[Serializable]
public struct SceneSetup : INetworkSerializable
{
    // Store main Objects
    public int[] mainObjectIds;
    public SerializableStrings mainObjectSpawnableNames;
    public Vector3[] mainObjectPositions;
    public Vector3[] mainObjectRotations;
    public Vector3[] mainObjectSizes;
    public SerializableStrings mainObjectUniqueSpawningNames;
    

    // Store sub objects that can hold values, especially Narration Bubble 
    public int[] subObjectIds;
    public InteractableValueTypes[] subObjectValueTypes;
    public bool[] subObjectBoolValues; // same length as subObjectIds, i.e. if other value type, this is false/0 
    public int[] subObjectIntValues; // same length as subObjectIds, i.e. if other value type, this is false/0 
    public float[] subObjectFloatValues; // same length as subObjectIds, i.e. if other value type, this is false/0 
    public SerializableStrings subObjectNarrationFileNames; // same length as subObjectIds, stores file names of wavs of float[] 


    // Store connections 
    public int[] connectionsFrom;
    public int[] connectionsTo;
    public SerializableStrings cableUniqueSpawningNames; 
   
    
    // Store drawing line renderers
    public int[] drawingPositionsCount; // stores how many entries in drawingLineRendererPositions belong to the current idx line renderer 
    public Vector3[] drawingLineRendererPositions; // stores all positions of all line renderers in one big array, to be cut with info from drawingPositionsCount
    public int[] drawingColorIdx; // stores color idx for each line renderer 
    
    
   
    
    
    

    
    // INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref mainObjectIds);
        serializer.SerializeValue(ref mainObjectSpawnableNames);
        serializer.SerializeValue(ref mainObjectPositions);
        serializer.SerializeValue(ref mainObjectRotations);
        serializer.SerializeValue(ref mainObjectSizes);
        serializer.SerializeValue(ref mainObjectUniqueSpawningNames);
        serializer.SerializeValue(ref subObjectIds);
        serializer.SerializeValue(ref subObjectValueTypes);
        serializer.SerializeValue(ref subObjectBoolValues);
        serializer.SerializeValue(ref subObjectIntValues);
        serializer.SerializeValue(ref subObjectFloatValues);
        serializer.SerializeValue(ref subObjectNarrationFileNames);
        serializer.SerializeValue(ref connectionsFrom);
        serializer.SerializeValue(ref connectionsTo);
        serializer.SerializeValue(ref cableUniqueSpawningNames);
        serializer.SerializeValue(ref drawingPositionsCount);
        serializer.SerializeValue(ref drawingLineRendererPositions);
        serializer.SerializeValue(ref drawingColorIdx);
    }
    // ~INetworkSerializable

}


[Serializable]
public struct SerializableStrings : INetworkSerializable
{
    public string[] stringArray;
 
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var length = 0;
        if (!serializer.IsReader)
            length = stringArray.Length;
 
        serializer.SerializeValue(ref length);
 
        if (serializer.IsReader)
            stringArray = new string[length];
 
        for (var n = 0; n < length; ++n)
            serializer.SerializeValue(ref stringArray[n]);
    }
}




public enum InteractableValueTypes
{
    Boolean,
    Integer,
    Float, 
    FloatArrayNarration,
    Unknown 

}

