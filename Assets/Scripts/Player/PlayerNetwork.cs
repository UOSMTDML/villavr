using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class PlayerNetwork : NetworkBehaviour
{
    
    [SerializeField] private GameObject vrComponentsVive;
    [SerializeField] private GameObject vrComponentsOculus;
    [SerializeField] private GameObject spectatorComponents;
    [SerializeField] private GameObject vrCameraVive;
    [SerializeField] private GameObject vrCameraOculus;
    [SerializeField] private GameObject spectatorCamera;
    [SerializeField] private SurrogateHands surrogateHands;
    [SerializeField] private GameObject nameTag;
    
    // Store name tag 
    private NetworkVariable<FixedString128Bytes> nameTagString = new NetworkVariable<FixedString128Bytes>();
    

    public override void OnNetworkDespawn()
    {
        // Reactivate main camera 
        if (IsOwner)
        {
            ExperienceManager.Singleton.ActivateMainSceneCamera();
        }
        
    }

    

    // Player is dynamically spawned object, i.e. Start is called last 
    // Start is called before the first frame update
    void Start()
    {
        
        
        
        // Activate VR Components depending on chosen Player Type 
        // Set name tag 
        if (IsOwner)
        {
            
            // Register ID to access manager 
            // Do in Start() to make sure everything is already setup (especially with client) 
            AccessManager.Singleton.AddClientIdName((int)NetworkManager.Singleton.LocalClientId, ExperienceManager.Singleton.playerName);


            // VR Components 
            if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerViveInput)
            {
                vrComponentsVive.SetActive(true);
                ExperienceManager.Singleton.SetVrCamera(vrCameraVive);
            }
            else if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerOculusInput)
            {
                vrComponentsOculus.SetActive(true);
                ExperienceManager.Singleton.SetVrCamera(vrCameraOculus);
            }
            else if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerSpectator)
            {
                spectatorComponents.SetActive(true);
                ExperienceManager.Singleton.SetVrCamera(spectatorCamera);
                ExperienceManager.Singleton.spectatorOverlay.SetActive(true);
                EventSystem.current.GameObject().SetActive(false);
            }
            ExperienceManager.Singleton.DeactivateMainSceneCamera();
            
            
            // Name Tag 
            SetNameTag_ServerRpc(ExperienceManager.Singleton.playerName);

        }
        
        // Update name tag when value is changed and show for first time 
        nameTag.GetComponent<TMP_Text>().text = nameTagString.Value.ToString();
        nameTagString.OnValueChanged += (value, newValue) =>
        {
            nameTag.GetComponent<TMP_Text>().text = nameTagString.Value.ToString();
        };





    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    [ServerRpc(RequireOwnership = false)] 
    private void SetNameTag_ServerRpc(string nameTag, ServerRpcParams serverRpcParams = default)
    {
        nameTagString.Value = nameTag;

    }
    

    
}
