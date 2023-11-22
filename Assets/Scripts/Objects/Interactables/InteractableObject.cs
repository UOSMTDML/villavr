using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;



// Parent class for interactable objects 
public class InteractableObject : NetworkBehaviour
{
    protected ObjectInfo objectInfo;
    
    // Keep track of whether state Value is modifiable by current player
    protected bool isModifiable; 
    public virtual void UpdateInteractableModifiable(bool canBeModified) { }
    
    protected void Awake()
    {
        objectInfo = GetComponent<ObjectInfo>();
    }

    


}



