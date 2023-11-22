using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


public class ButtonBooleanInteractable : BooleanInteractable
{

    // Header Settings inherited from BooleanInteractable
    [SerializeField] private Boolean onOnlyWhenActivelyPushing; // choose whether button should be active only when pushed or keep state after push 
    [SerializeField] private bool updateFaustParam;
    [SerializeField] private int faustParamIdx;
    [SerializeField] private FaustObject processingFaustObject;

    [Header("Internals")]
    [SerializeField] private ConfigurableJoint mainConfigurableJoint;
    [SerializeField] private GameObject button;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;
    [SerializeField] private GameObject lockSymbol;

    
    private Boolean updateButtonColor;
    

    // Inherited From BooleanInteractable 
    // protected NetworkVariable<Boolean> stateValue
    // [SerializeField] protected bool initialValueIsTrue;
    // Inherited from Interactable 
    // public bool isModifiable; 
    
    
    // Called after Awake
    public override void OnNetworkSpawn()
    {
        
        // Update Visualisation to match actual value 
        updateButtonColor = true;
        
        // Update local Faust once at start to overcome initialization values
        if (updateFaustParam)
        {
            if (stateValue.Value)
            {
                processingFaustObject.setParameter(faustParamIdx, 1);
            }
            else
            {
                processingFaustObject.setParameter(faustParamIdx, 0);
            }
        }  
        
        
        // Add a listener for the value change of the network variable 
        stateValue.OnValueChanged += (Boolean previousValue, Boolean newValue) => {
            
            updateButtonColor = true;
            
            // If Faust parameter should be updated, do so
            if (updateFaustParam)
            {
                if (newValue)
                {
                    processingFaustObject.setParameter(faustParamIdx, 1);
                }
                else
                {
                    processingFaustObject.setParameter(faustParamIdx, 0);
                }
                
            }  
            
        };
        
        // Add a listener for the local button push 
        mainConfigurableJoint.GetComponent<PhysicsGadgetButton>().OnPressed.AddListener(() =>
        {
            UpdateBooleanState(!stateValue.Value, "");
        });
        
        // Add a listener for the local button push release
        // Use only when button is of type: Fire only when actively pushing 
        if (onOnlyWhenActivelyPushing)
        {
            mainConfigurableJoint.GetComponent<PhysicsGadgetButton>().OnUnpressed.AddListener(() =>
            {
                UpdateBooleanState(!stateValue.Value, "");
            });
        }
        
        
        
        
        
    }

    // Use to initialize NetworkVariable locally or at first occurence on server globally 
    void Awake()
    {
        base.Awake();
        
        // Set initial network variable and value, this will only work on Server  
       stateValue = new NetworkVariable<Boolean>(initialValueIsTrue, 
           NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
       
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
        
        
        // Update angle from remote value change 
        // Update dial rotation only when hand is not attached, otherwise hand updates position 
        if (updateButtonColor)
        {
            if (stateValue.Value)
            {
                
                button.GetComponent<Renderer>().material = onMaterial;
            }
            else
            {
                button.GetComponent<Renderer>().material = offMaterial;
            }
            updateButtonColor = false;
        }
        

    }
    
   
}

