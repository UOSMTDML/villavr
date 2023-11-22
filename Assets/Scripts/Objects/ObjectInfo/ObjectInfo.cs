using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectInfo : NetworkBehaviour
{

    // Store Info about Object
    [Header("Set Manually")]
    [SerializeField] public string objectName; // Set in inspector
    [SerializeField] protected ObjectType objectType; // Set in inspector
    [Header("Set During Runtime")] 
    public NetworkVariable<FixedString64Bytes> uniqueSpawningName;
    public NetworkVariable<int> uniqueObjectId;
    public NetworkVariable<AccessType> accessType;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        
        // Set unique name (only set during spawn by server) 
        uniqueSpawningName.OnValueChanged += (FixedString64Bytes prevValue, FixedString64Bytes newValue) =>
        {
            if (newValue != "")
            {
                gameObject.name = newValue.ToString();
            }
        };


    }

    // Access Object Type 
    public ObjectType GetObjectType()
    {
        return objectType;
    }
    
    
    // Set Unique Object Id, overridden in Sub and Main Object Info 
    public virtual void SetUniqueObjectId(int baseObjectId){}
    
    // Access Unique Object Id, overridden in Sub and Main Object Info 
    public virtual int GetUniqueObjectId()
    {
        return -1;
    }
    
    // Update Access Type 
    public virtual void SetAccessType(AccessType accessType) { }
    
    
    // Should only be called by server 
    // Update Client IDs allowed to modify values, if specified access type is to allow some 
    public virtual void SetEspeciallyAllowedClientIds(HashSet<int> allowedIds) { }

    
}


public enum ObjectType
{
    FullComponent, // Entire generation component, consisting of separate Inputs
    BooleanInteractionInput, // Interaction Input
    IntegerInteractionInput, // Interaction Input
    FloatInteractionInput, // Interaction Input
    FloatArrayInteractionInput, // Interaction Input 
    ConnectionInput, // (Cable) Connection Input
    ConnectionOutput, // (Cable) Connection Output 
    ConnectionCable, // Cable (with two connectors)  
    NarrationObject, // Object that acts as a speech bubble  
    AnnotationTool, // Object that draws or erases 
    StaticObject // Other object without functionality 
}

public enum AccessType
{
    EveryoneCanModify,
    SomeCanModify, // Ids specified in additional field 
    NobodyCanModify
}
