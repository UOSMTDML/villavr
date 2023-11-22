using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms;


public class SliderFloatInteractable : FloatInteractable
{
    // Header Settings inherited from FloatInteractable    
    [SerializeField] private bool updateFaustParam;
    [SerializeField] private int faustParamIdx;
    [SerializeField] private FaustObject processingFaustObject;

    [Header("Internals")]
    [SerializeField] private GameObject knob;
    [SerializeField] private GameObject leftRangeEnd;
    [SerializeField] private GameObject rightRangeEnd;
    [SerializeField] private TMP_Text leftValueBoundText;
    [SerializeField] private TMP_Text rightValueBoundText;
    [SerializeField] private TMP_Text currentValueText;

    [SerializeField] private GameObject lockSymbol;

    
    private Boolean updateKnobPosition;
    private Boolean handIsAttached;
    private float xPrevious = -999; 
    private float xMin;
    private float xMax;
    
    
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
        updateKnobPosition = true;
        
        // Update local Faust once at start to overcome initialization values
        if (updateFaustParam)
        {
            processingFaustObject.setParameter(faustParamIdx, stateValue.Value);
        }  
        
        // Add a listener for the value change
        stateValue.OnValueChanged += (float previousValue, float newValue) =>
        {

            updateKnobPosition = true;
            
            // If Faust parameter should be updated, do so
            if (updateFaustParam)
            {
                processingFaustObject.setParameter(faustParamIdx, newValue);
            }
            
        };
        
        
        // Add a listener for the hand attachment 
        knob.GetComponent<Grabbable>().OnGrabEvent += (Hand hand, Grabbable grabbable) =>
        {
            handIsAttached = true;
            if (isModifiable)
            {
                knob.GetComponent<Rigidbody>().isKinematic = false;
            }
            else
            {
                knob.GetComponent<Rigidbody>().isKinematic = true;
            }
        };
        
        
        // Add a listener for the hand release
        knob.GetComponent<Grabbable>().OnReleaseEvent += (Hand hand, Grabbable grabbable) =>
        {
            handIsAttached = false;
            knob.GetComponent<Rigidbody>().isKinematic = true;
        };

        
        
        
    }

    // Use to initialize NetworkVariable locally or at first occurence on server globally 
    void Awake()
    {
        base.Awake();
        
        // Set initial network variable and value, this will only work on Server  
        stateValue = new NetworkVariable<float>(Mathf.Clamp(initialValue, lowerBound, upperBound), 
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        // Get min and max positions of knob 
        xMin = leftRangeEnd.transform.localPosition.x;
        xMax = rightRangeEnd.transform.localPosition.x;
        
       
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
    
    private float PositionToValue(float xPosition)
    {
        // map from [a,b] to [c,d], where f(a) = c, f(b) = d, 
        // f(t) = c + ((d-c) / (b-a)) * (t-a) 
        //
        // [a,b] is x value, [c,d] is value range 
        
        float calculatedVal = lowerBound + ((upperBound - lowerBound) / (xMax - xMin) * (xPosition - xMin));
        return calculatedVal;
    }
    
    
    private float ValueToPosition(float val)
    {
        // map from [a,b] to [c,d], where f(a) = c, f(b) = d, 
        // f(t) = c + ((d-c) / (b-a)) * (t-a) 
        //
        // [a,b] is value range, [c,d] is position 
        
        float calculatedPos = xMin + ((xMax - xMin) / (upperBound - lowerBound) * (val - lowerBound));
        return calculatedPos;

    }

    
    
    
    
 

    void Update()
    {
        
        // Update current value 
        currentValueText.text = stateValue.Value.ToString("F2");
        
        // Update position from remote value change 
        // Update knob position only when hand is not attached, otherwise hand updates position 
        if (updateKnobPosition && !handIsAttached)
        {
            knob.transform.localPosition = new Vector3(ValueToPosition(stateValue.Value), knob.transform.localPosition.y, knob.transform.localPosition.z);
            updateKnobPosition = false;
        }
        
        
        // Update position (and value) from local change 
        // Check whether position has changed and update 
        if (Mathf.Abs(knob.transform.localPosition.x - xPrevious) > 0.00001f && IsOwner)
        {
            // Clamp actual position to min/ max, since collision might be slightly off 
            float position = knob.transform.localPosition.x;
            position = Mathf.Clamp(position, xMin, xMax);
            
            UpdateFloatState(PositionToValue(position), "");
        }
        
        
        // Store previous x position
        xPrevious = knob.transform.localPosition.x;


    }
    
    
}
