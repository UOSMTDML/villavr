using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class IntegerInteractable : InteractableObject
{
    [Header("Settings")]
    [SerializeField] protected int lowerBound;
    [SerializeField] protected int upperBound;
    [SerializeField] protected int initialValue;
    
    protected NetworkVariable<int> stateValue;


    protected void Awake()
    {
        base.Awake();
    }
    

    public int GetState()
    {
        return stateValue.Value;
    }
    
    
    
    // Wrapper method to update Integer State for ServerRPC
    // Can make use of locally available isModifiable 
    protected void UpdateIntegerState(int newValue, string message)
    {
        if (isModifiable)
        {
            UpdateIntegerState_ServerRpc(newValue, message);
        }
        else
        {
            // Do not update 
        }
    }
    
    
    // Method to request value update on server 
    [ServerRpc(RequireOwnership = false)] 
    private void UpdateIntegerState_ServerRpc(int newValue, string message, ServerRpcParams serverRpcParams = default) {
        Debug.Log("[UpdateIntegerState] " + objectInfo.objectName + ": Update Value to " + newValue.ToString() + "; Message: " + message + "; Initiated from ClientID: " + serverRpcParams.Receive.SenderClientId + "; OwnerID " + OwnerClientId);
        
        // Verify that target value is within range
        if (Mathf.Clamp(newValue, lowerBound, upperBound) == newValue)
        {
            stateValue.Value = newValue;
        }
        else
        {
            Debug.Log("[UpdateIntegerState] Value not within range [" + lowerBound.ToString() + " to " + upperBound.ToString() + "]! Not updating!");
        }
        
        
    }
    
    
    // Restore Value after scene loading
    public void RestoreValue(int newValue)
    {
        if (IsServer)
        {
            stateValue.Value = newValue;
        }
    }
    
    
}
