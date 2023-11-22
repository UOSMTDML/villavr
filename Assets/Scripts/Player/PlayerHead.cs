using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHead : NetworkBehaviour
{
    [SerializeField] private Camera hmdCameraVive;
    [SerializeField] private Camera hmdCameraOculus;
    [SerializeField] private Camera spectatorCamera;
    [SerializeField] private Transform headGeometry;
    
    // Debug 
    [SerializeField] private bool debugShowHead = false;
    
    // Start is called before the first frame update
    void Start()
    {
        // DeActivate renderers on local machine 
        if (IsOwner)
        {
            if (!debugShowHead)
            {
                foreach (Renderer rend in headGeometry.GetComponentsInChildren<Renderer>())
                {
                    rend.enabled = false;
                }
            }
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        // Sync head's position 
        if (IsOwner)
        {
            // Check which type of input is used 
            if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerViveInput)
            {
                transform.position = hmdCameraVive.transform.position;
                transform.rotation = hmdCameraVive.transform.rotation;
            }
            
            else if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerOculusInput)
            {
                transform.position = hmdCameraOculus.transform.position;
                transform.rotation = hmdCameraOculus.transform.rotation;
            }
            
            // If input is other, move surrogate hands below map  
            else if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerSpectator)
            {
                transform.position = spectatorCamera.transform.position;
                transform.rotation = spectatorCamera.transform.rotation;
            }
            
            
        }
    }

    
    
}
