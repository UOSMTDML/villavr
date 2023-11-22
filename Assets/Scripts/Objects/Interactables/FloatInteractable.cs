using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class FloatInteractable : InteractableObject
{
    [Header("Settings")]
    [SerializeField] protected float lowerBound;
    [SerializeField] protected float upperBound;
    [SerializeField] protected float initialValue;

    protected NetworkVariable<float> stateValue;


    protected void Awake()
    {
        base.Awake();
    }
    
    public float GetState()
    {
        return stateValue.Value;
    }
    
    
    // Wrapper method to update Float State for ServerRPC
    // Can make use of locally available isModifiable 
    protected void UpdateFloatState(float newValue, string message)
    {
        if (isModifiable)
        {
            UpdateFloatState_ServerRpc(newValue, message);
        }
        else
        {
            // Do not update 
        }
    }
    
    
    // Method to request value update on server 
    [ServerRpc(RequireOwnership = false)] 
    private void UpdateFloatState_ServerRpc(float newValue, string message, ServerRpcParams serverRpcParams = default) {
        Debug.Log("[UpdateFloatState] " + objectInfo.objectName + ": Update Value to " + newValue.ToString() + "; Message: " + message + "; Initiated from ClientID: " + serverRpcParams.Receive.SenderClientId + "; Local Client ID " + NetworkManager.Singleton.LocalClientId);
        Debug.Log(IsServer);
        
        // Verify that target value is within range
        if (Mathf.Clamp(newValue, lowerBound, upperBound) == newValue)
        {
            stateValue.Value = newValue;
        }
        else
        {
            Debug.Log("[UpdateFloatState] Value not within range [" + lowerBound.ToString() + " to " + upperBound.ToString() + "]! Not updating!");
        }
        
    }
    
    // Restore Value after scene loading
    public void RestoreValue(float newValue)
    {
        if (IsServer)
        {
            stateValue.Value = newValue;
        }
    }
    
}
