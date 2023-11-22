using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperienceManager : MonoBehaviour
{
    [Header("Player Settings (partly set at runtime")] 
    public PlayerType playerType;
    public PlayerRole playerRole;
    public string playerName;

    [Header("Network Settings")] 
    public string ipAddress;
    public ClientOrHostOrServer connectionRole;
    public int maxFloatsSendPerRpc;
    
    [Header("Interactable Settings")]
    public bool interactablesRotateBackIntoHorizontalPlaneOnRelease;
    public float rotationAmountPerFrame;

    [Header("Dissolve Settings")]
    public Material dissolveMaterial;

    [Header("Annotation Tool Settings")] 
    public Material[] drawingMaterials;
    public float drawingToolWidth;
    public string drawingColliderPrefix;
    public string drawingGameObjectTag;
    
    [Header("Menu Overlay Settings")] 
    public GameObject spectatorOverlay;
    
    [Header("Scene Settings")] 
    public string mainCollaborationScene;
    public string mainMenuScene;
    
    [Header("Save & Load Settings")]
    public string storeSceneSetupsPath;
    public string storeNarrationsPath;
    
    [Header("Audio Settings (partly assigned at runtime)")] 
    public int micAudioSamplingFrequency;
    public int micAudioMaxRecordTimeSeconds;
    public string selectedMicName;
    public List<string> availableMicNames;
    public bool micAvailable;

    [Header("Cameras (partly assigned at runtime")]
    public string uiCanvasPointerCameraGameObjectName;
    public GameObject mainSceneCamera;
    public GameObject vrCamera;
    
    
    // Singleton pattern
    // (Comparing against initially created static instance in Awake) 
    private static ExperienceManager _singleton;
    public static ExperienceManager Singleton
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
        
        DontDestroyOnLoad(this.gameObject);
        
    }
    
    
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Start updating connected mics 
        StartCoroutine(UpdateAvailableMicrophones());

    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[ExperienceManager] OnSceneLoaded");
        
        // Check if main collab scene and set values appropriately 
        // Spawn big synthesizer delayed 
        if (scene.name == ExperienceManager.Singleton.mainCollaborationScene)
        {
            
            // Find main camera
            mainSceneCamera = GameObject.FindWithTag("MainCamera");
            
            // Setup ip adress 
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = ExperienceManager.Singleton.ipAddress;

            
            // Make sure to return to main menu when connection fails 
            NetworkManager.Singleton.OnTransportFailure += () =>
            {
                Debug.Log("[ExperienceManager] Starting Server failed.");
                
                try
                {
                    XRStarter.Singleton.StopXR();
                }
                catch
                {
                }

                SceneManager.LoadScene(ExperienceManager.Singleton.mainMenuScene);SceneManager.LoadScene(ExperienceManager.Singleton.mainMenuScene);
            }; 
            
            
            // Connect to server
            if (connectionRole == ClientOrHostOrServer.Client)
            {
                NetworkManager.Singleton.StartClient();
            }
            else if (connectionRole == ClientOrHostOrServer.Host)
            {
                NetworkManager.Singleton.StartHost();
            }
            else if (connectionRole == ClientOrHostOrServer.Server)
            {
                NetworkManager.Singleton.StartServer();
            }
            
            
            
            // Spawn objects delayed
            if (NetworkManager.Singleton.IsServer)
            {
                StartCoroutine("SpawnLocalObjects");
            }

        }
        
        
        // Check if main menu, try to disconnect network manager after already connected 
        if (scene.name == ExperienceManager.Singleton.mainMenuScene)
        {
            try
            {
                NetworkManager.Singleton.Shutdown();
                Destroy(NetworkManager.Singleton.GameObject());
            }
            catch
            {
                Debug.Log("[ExperienceManager] Did not find NetworkManager to destroy.");
            }
        }
        
        
    }
    
    
    
    private IEnumerator UpdateAvailableMicrophones()
    {
        while (true)
        {
            micAvailable = Microphone.devices.Length > 0;
            availableMicNames = Microphone.devices.ToList();
            yield return new WaitForSeconds(5);
        }
    }


    private IEnumerator SpawnLocalObjects()
    {
        yield return new WaitForSeconds(1.0f);
        
        // Spawn SAM virtual analogue
        NetworkSpawner.Singleton.SpawnObject(SpawnableObject.SAMVirtualAnalog, new Vector3(0, 1.4f, 19.2f), new Vector3(0,0,0), new Vector3(1,1,1), "SAM_virtual_analogue_in_room", false);
        
        // Spawn Speaker 
        NetworkSpawner.Singleton.SpawnObject(SpawnableObject.OutputSpeaker, new Vector3(4.6f, 1.9f, 17.5f), new Vector3(0,90,0), new Vector3(1,1,1), "speaker_SAM_virtual_analogue_in_room", false);

        
        // Spawn cable 
        NetworkSpawner.Singleton.SpawnObject(SpawnableObject.ConnectionCable, new Vector3(0, -100, 0),
            new Vector3(0,0,0), new Vector3(1,1,1), "cable_sam_virtual_analogue_speaker_in_room", false);
            
        
        // Find synth again 
        GameObject synthInRoomGameObject; 
        while (true)
        {
            synthInRoomGameObject = GameObject.Find("SAM_virtual_analogue_in_room");
            if (synthInRoomGameObject == null)
            {
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                break;
            }
        }
        
        // Find speaker again 
        GameObject speakerInRoomGameObject; 
        while (true)
        {
            speakerInRoomGameObject = GameObject.Find("speaker_SAM_virtual_analogue_in_room");
            if (speakerInRoomGameObject == null)
            {
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                break;
            }
        }
        
        
        // Find cable again 
        GameObject spawnedCableGameObject;
        while (true)
        {
            spawnedCableGameObject = GameObject.Find("cable_sam_virtual_analogue_speaker_in_room");
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
        
        
        // Find Synth output connection 
        List<SubObjectInfo> subObjectsSynth = synthInRoomGameObject.GetComponent<MainObjectInfo>().GetSubObjectAndConnectionsInfoList();
        int synthOutputObjectId = -1;
        
        foreach (SubObjectInfo info in subObjectsSynth)
        {
            if (info.GetObjectType() == ObjectType.ConnectionOutput)
            {
                synthOutputObjectId = info.GetUniqueObjectId();
                break;
            }
        }
        
        // Find Speaker input connection 
        List<SubObjectInfo> subObjectsSpeaker = speakerInRoomGameObject.GetComponent<MainObjectInfo>().GetSubObjectAndConnectionsInfoList();
        int speakerInputObjectId = -1;
        
        foreach (SubObjectInfo info in subObjectsSpeaker)
        {
            if (info.GetObjectType() == ObjectType.ConnectionInput)
            {
                speakerInputObjectId = info.GetUniqueObjectId();
                break;
            }
        }
        
        
        // Setup spawned cable
        spawnedCable.SetSecondConnectorConnectionState(true, speakerInputObjectId);
        spawnedCable.SetFirstConnectorConnectionState(true, synthOutputObjectId);
        spawnedCable.ChangeConnectorVisibility(FirstOrSecondConnector.Second, true); 
        spawnedCable.ChangeConnectorVisibility(FirstOrSecondConnector.First, true); 
        spawnedCable.ChangeLinerendererVisibility(true); 
        
        
        yield return null;
    }
    
    
    
    

    
    
    
    

    public void DeactivateMainSceneCamera()
    {
        mainSceneCamera.SetActive(false);
    }
    
    public void ActivateMainSceneCamera()
    {
        try
        {
            mainSceneCamera.SetActive(false);
        }
        catch
        {
            
        }
    }

    public void SetVrCamera(GameObject camera)
    {
        vrCamera = camera;
    }


    public enum PlayerType
    {
        PlayerViveInput,
        PlayerOculusInput,
        PlayerSpectator,
        PlayerFreeFly 
    }

    public enum PlayerRole
    {
        User,
        Moderator 
    }

    public enum ClientOrHostOrServer
    {
        Host, 
        Client,
        Server
    }
    
    
}
