using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class BooleanInteractable : InteractableObject
{
    
    [Header("Settings")]
    [SerializeField] protected bool initialValueIsTrue;

    protected NetworkVariable<Boolean> stateValue;

    protected void Awake()
    {
        base.Awake();
    }
    
    public Boolean GetState()
    {
        return stateValue.Value;
    }

    
    // Wrapper method to update Boolean State for ServerRPC
    // Can make use of locally available isModifiable 
    protected void UpdateBooleanState(Boolean newValue, string message)
    {
        if (isModifiable)
        {
            UpdateBooleanState_ServerRpc(newValue, message);
        }
        else
        {
            // Do not update 
        }
    }
    
    
    // Method to request value update on server 
    [ServerRpc(RequireOwnership = false)] 
    private void UpdateBooleanState_ServerRpc(Boolean newValue, string message, ServerRpcParams serverRpcParams = default) {
        
    
        Debug.Log("[UpdateBooleanState] Update Value to " + newValue.ToString() + "; Message: " + message + "; Initiated from ClientID: " + serverRpcParams.Receive.SenderClientId + "; OwnerID " + OwnerClientId);
    
        // Update state value 
        stateValue.Value = newValue;
    

    }
    
    
    
    // Restore Value after scene loading
    public void RestoreValue(bool newValue)
    {
        if (IsServer)
        {
            stateValue.Value = newValue;
        }
    }




}
