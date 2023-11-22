using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using TMPro;
using Unity.Netcode;
using UnityEngine;


public class ButtonIntegerInteractable : IntegerInteractable
{

    // Header Settings inherited from IntegerInteractable 
    [SerializeField] private bool updateFaustParam;
    [SerializeField] private int faustParamIdx;
    [SerializeField] private FaustObject processingFaustObject;

    [Header("Internals")]
    [SerializeField] private ConfigurableJoint mainConfigurableJoint;
    [SerializeField] private TMP_Text currentValueBoundText;
    [SerializeField] private TMP_Text leftValueBoundText;
    [SerializeField] private TMP_Text rightValueBoundText;
    [SerializeField] private GameObject lockSymbol;

    

    
    
    // Inherited From IntegerInteractable 
    // [SerializeField] protected int lowerBound;
    // [SerializeField] protected int upperBound;
    //[SerializeField] protected int initialValue;
    // protected NetworkVariable<int> stateValue
    
    // Inherited from Interactable 
    // public bool isModifiable; 
    
    
    // Called after Awake
    public override void OnNetworkSpawn()
    {
        
        // Update local Faust once at start to overcome initialization values
        if (updateFaustParam)
        {
            processingFaustObject.setParameter(faustParamIdx, stateValue.Value);
        }  
        
        
        // Add a listener for the value change
        stateValue.OnValueChanged += (int previousValue, int newValue) => {
            
            
            // If Faust parameter should be updated, do so
            if (updateFaustParam)
            {
                processingFaustObject.setParameter(faustParamIdx, newValue);
            }
            
        };
        
        // Add a listener for the local button push 
        mainConfigurableJoint.GetComponent<PhysicsGadgetButton>().OnPressed.AddListener(() =>
        {
            // Loop through all possible integer values 

            if (stateValue.Value == upperBound) // Reached upper bound start at lower Bound again
            {
                UpdateIntegerState(lowerBound, "");
                
            }
            else
            {
                UpdateIntegerState(stateValue.Value + 1, "");
            }
        });
        
    }

    // Use to initialize NetworkVariable locally or at first occurence on server globally 
    void Awake()
    {
        
       base.Awake();
        
       // Set initial network variable and value, this will only work on Server  
       stateValue = new NetworkVariable<int>(Mathf.Clamp(initialValue, lowerBound, upperBound), 
           NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
       
       // Set value of lower and upper visual bound to actual values 
       leftValueBoundText.text = "From:\n" +  lowerBound.ToString();
       rightValueBoundText.text = "To:\n" + upperBound.ToString();
       
    }


    
    // Update whether player can modfiy value 
    public override void UpdateInteractableModifiable(bool canBeModified)
    {
        isModifiable = canBeModified;
        if (canBeModified)
        {
            lockSymbol.SetActive(false);
            mainConfigurableJoint.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None; // Activate pushing 
            mainConfigurableJoint.GetComponent<Rigidbody>().isKinematic = false;
        }
        else
        {
            lockSymbol.SetActive(true);
            mainConfigurableJoint.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition; // Deactivate pushing
            mainConfigurableJoint.GetComponent<Rigidbody>().isKinematic = true;
            
        }
        
    }
    
 

    void Update()
    {
        
        // Update current value 
        currentValueBoundText.text = "Current Value:\n" + stateValue.Value.ToString("F0");
        

    }
    
   
}

