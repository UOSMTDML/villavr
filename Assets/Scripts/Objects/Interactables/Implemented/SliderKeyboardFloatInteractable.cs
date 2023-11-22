using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms;


public class SliderKeyboardFloatInteractable : FloatInteractable
{

    [SerializeField] private GameObject knob;
    [SerializeField] private GameObject leftVisualBound;
    [SerializeField] private GameObject rightVisualBound;
    [SerializeField] private TMP_Text leftValueBoundText;
    [SerializeField] private TMP_Text rightValueBoundText;
    
    [SerializeField] private GameObject lockSymbol;

    [SerializeField] private float incrementRate; 

    [SerializeField] private bool updateFaustParam;
    [SerializeField] private int faustParamIdx;
    [SerializeField] private FaustObject processingFaustObject;
    
    private Boolean updatePosition;
    private Vector3 currentLocalTarget;
    
    
    // Inherited From FloatInteractable 
    // [SerializeField] private float lowerBound;
    // [SerializeField] private float upperBound;
    // [SerializeField] protected float initialValue;
    // protected NetworkVariable<float> stateValue
    // Inherited from Interactable 
    // public bool isModifiable; 
    
    
    // Called after Awake, Network related info is available
    public override void OnNetworkSpawn()
    {
        
        // Update Visualisation to match actual value 
        CalculateNewPosition(stateValue.Value);
        updatePosition = true;
        
        // Update local Faust once at start to overcome initialization values
        if (updateFaustParam)
        {
            processingFaustObject.setParameter(faustParamIdx, stateValue.Value);
        }  
        
        
        // Add a listener for the value change
        stateValue.OnValueChanged += (float previousValue, float newValue) => {
            
            CalculateNewPosition(newValue);
            
            // If Faust parameter should be updated, do so
            if (updateFaustParam)
            {
                processingFaustObject.setParameter(faustParamIdx, newValue);
            }
            
        };
        
    }

    // Use to initialize NetworkVariable locally or at first occurence on server globally 
    void Awake()
    {
        base.Awake();
        
        // Set initial network variable and value, this will only work on Server  
        stateValue = new NetworkVariable<float>(Mathf.Clamp(initialValue, lowerBound, upperBound), 
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
       
       // Set value of lower and upper visual bound to actual values 
       leftValueBoundText.text = lowerBound.ToString();
       rightValueBoundText.text = upperBound.ToString();
    }
    
    

    
    // Update whether player can modfiy value 
    public override void UpdateInteractableModifiable(bool canBeModified)
    {
        isModifiable = canBeModified;
        if (canBeModified)
        {
            lockSymbol.SetActive(false);
        }
        else
        {
            lockSymbol.SetActive(true);
        }
        
    }
    
    
    
    


    void CalculateNewPosition(float stateValue)
    {
        // Calculate distance vector between bounds
        Vector3 boundsVector = rightVisualBound.transform.localPosition - leftVisualBound.transform.localPosition;
        
        // Scale distance by new stateValue 
        Vector3 scaledTargetVector = boundsVector * (stateValue - lowerBound) / (upperBound - lowerBound);
        
        // Set target 
        currentLocalTarget = leftVisualBound.transform.localPosition + scaledTargetVector;
        
        // Indicate update local position
        updatePosition = true;
        
    }
    
    
    
 

    void Update()
    {
        // Change network variable value 
        if (Input.GetKey(KeyCode.L))
        {
            Debug.Log("[SliderFloatInteractable] Pressed L");
            float updateVal = stateValue.Value + incrementRate * Time.deltaTime;
            updateVal = Mathf.Clamp(updateVal , lowerBound, upperBound);
            UpdateFloatState(updateVal, "testFloat");
        }
        else if (Input.GetKey(KeyCode.K))
        {
            Debug.Log("[SliderFloatInteractable] Pressed K");
            float updateVal = stateValue.Value - incrementRate * Time.deltaTime;
            updateVal = Mathf.Clamp(updateVal , lowerBound, upperBound);
            UpdateFloatState(updateVal, "testFloat");
        }
        
        
        
        if (updatePosition)
        {

            var step = 6f * Time.deltaTime;
            knob.transform.localPosition = Vector3.MoveTowards(knob.transform.localPosition, currentLocalTarget, step);

            // Check if position reached
            if (Vector3.Distance(knob.transform.localPosition, currentLocalTarget) < 0.001f)
            {
                updatePosition = false;
            }


        }

    }
    
    
}
