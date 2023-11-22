using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class FloatArrayInteractable : InteractableObject
{

    protected float[] floatArray; // this will be handled via rpcs rather than via network variables 
    
    
    protected void Awake()
    {
        base.Awake();
    }
    

    public float[] GetState()
    {
        return floatArray;
    }
    
    
    // Update Float Array State, implemented differently in each use case 
    protected virtual void UpdateFloatArrayState(float[] newFloatArray)
    { }
    
    
    // Restore Value after scene loading, implemented differently in each use case 
    public virtual void RestoreValue(float[] newFloatArray)
    { }
    
    
}
