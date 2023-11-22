using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EraserController : NetworkBehaviour
{
    
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string oculusInputActionsName;
    [SerializeField] private string htcViveInputActionsName;
    [SerializeField] private string drawActionName;
    [SerializeField] private string colliderNamePrefix;
    private InputAction drawInputAction;

    private bool eraseIsEngaged = false;
    private bool externalErasing = false;
    private bool isGrabbed = false;
    
    
    void Start()
    {
        //load controller mappings for oculus and htc vive
        if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerOculusInput)
        {
            drawInputAction = inputActions.FindActionMap(oculusInputActionsName).FindAction(drawActionName);
            drawInputAction.Enable();
            
            // Erase when trigger is pressed
            drawInputAction.started += ToggleErase;
            drawInputAction.canceled += ToggleErase;
        }

        if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerViveInput)
        {
            drawInputAction = inputActions.FindActionMap(htcViveInputActionsName).FindAction(drawActionName);
            drawInputAction.Enable(); 
            
            // Erase when trigger is pressed
            drawInputAction.started += ToggleErase;
            drawInputAction.canceled += ToggleErase;
        }
        
    }

    public override void OnDestroy()
    {
        // Remove action callback
        drawInputAction.started -= ToggleErase;
        drawInputAction.canceled -= ToggleErase;
            
        // invoke the base 
        base.OnDestroy();
    }


    private void ToggleErase(InputAction.CallbackContext context)
    {
        eraseIsEngaged = !eraseIsEngaged;
        
        // Signal other Clients that erasing is engaged/ not engaged
        SignalEngaged_ServerRpc(eraseIsEngaged);
    }
    
    // Called when object is grabbed; by Grabbable in Inspector 
    public void ToggleGrabbed(bool grabbed)
    {
        isGrabbed = grabbed;
    }
    
    
    

    [ServerRpc(RequireOwnership = false)]
    private void SignalEngaged_ServerRpc(bool isEngaged, ServerRpcParams serverRpcParams = default)
    {
        
        ReceiveIsEngaged_ClientRPC(isEngaged, serverRpcParams.Receive.SenderClientId);
        
        
        try
        {
            
        }
        catch (Exception e)
        {
            Debug.Log("[EraserController] Could not signal engaged: " + e.ToString());
        }

    }

    [ClientRpc]
    private void ReceiveIsEngaged_ClientRPC(bool isEngaged, ulong initiallySentFromId, ClientRpcParams clientRpcParams = default)
    {
        
        // This client indicated drawing/ not drawing 
        if (initiallySentFromId == NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        // Some other client is erasing/ not erasing  
        else
        {
            externalErasing = isEngaged;
        }
        
    }

    
    
    
    
    

    
    private void OnTriggerEnter(Collider other)
    {
       
        // If erasing is activated and hit object is part of line renderer, destroy 
        if (((eraseIsEngaged && isGrabbed) || externalErasing) && other.name.StartsWith(colliderNamePrefix))
        {
            Destroy(other.transform.parent.GameObject());
        }
    }
}
