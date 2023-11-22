using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DialFloatInteractable : FloatInteractable
{

    // Header Settings inherited from FloatInteractable
    [SerializeField] private bool updateFaustParam;
    [SerializeField] private int faustParamIdx;
    [SerializeField] private FaustObject processingFaustObject;
    
    [Header("Internals")]
    [SerializeField] private HingeJoint mainHingeJoint;
    [SerializeField] private TMP_Text leftValueBoundText;
    [SerializeField] private TMP_Text rightValueBoundText;
    [SerializeField] private TMP_Text currentValueBoundText;
    [SerializeField] private GameObject lockSymbol;

    
    private Boolean updateDialPosition;
    private Boolean handIsAttached;
    private float previousAngle = -999;
    private float angleMin;
    private float angleMax; 

    
    
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
        updateDialPosition = true;
        
        // Update local Faust once at start to overcome initialization values
        if (updateFaustParam)
        {
            processingFaustObject.setParameter(faustParamIdx, stateValue.Value);
        }  
        
        
        // Add a listener for the value change
        stateValue.OnValueChanged += (float previousValue, float newValue) =>
        {

            updateDialPosition = true;
            
            // If Faust parameter should be updated, do so
            if (updateFaustParam)
            {
                processingFaustObject.setParameter(faustParamIdx, newValue);
            }
            
        };
        
        // Add a listener for the hand attachment 
        mainHingeJoint.GetComponent<Grabbable>().OnGrabEvent += (Hand hand, Grabbable grabbable) =>
        {
            handIsAttached = true;
        };
        
        
        // Add a listener for the hand release
        mainHingeJoint.GetComponent<Grabbable>().OnReleaseEvent += (Hand hand, Grabbable grabbable) =>
        {
            handIsAttached = false;
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
       
       // Init hinge angle range 
       angleMin = mainHingeJoint.limits.min;
       angleMax = mainHingeJoint.limits.max;

    }

    private float FormatAngle180(float angle)
    {
        angle = (angle % 360);
        if (angle >= 180)
        {
            angle = angle - 360;
        }
        return angle;
    }

    private float AngleToValue(float hingeAngle)
    {
        // map from [a,b] to [c,d], where f(a) = c, f(b) = d, 
        // f(t) = c + ((d-c) / (b-a)) * (t-a) 
        //
        // [a,b] is hinge angle, [c,d] is value range 
        
        // Format angle to fit [-180,180] interval 
        hingeAngle = FormatAngle180(hingeAngle);
        
        return lowerBound + ((upperBound - lowerBound) / (angleMax - angleMin) * (hingeAngle - angleMin));

    }
    
    private float ValueToAngle(float val)
    {
        // map from [a,b] to [c,d], where f(a) = c, f(b) = d, 
        // f(t) = c + ((d-c) / (b-a)) * (t-a) 
        //
        // [a,b] is value range, [c,d] is hinge angle  
        
        float calculatedHingeAngle = angleMin + ((angleMax - angleMin) / (upperBound - lowerBound)) * (val - lowerBound);
        calculatedHingeAngle = (calculatedHingeAngle % 360);
        
        return calculatedHingeAngle;

    }

    
    // Update whether player can modfiy value 
    public override void UpdateInteractableModifiable(bool canBeModified)
    {
        isModifiable = canBeModified;
        if (canBeModified)
        {
            lockSymbol.SetActive(false);
            mainHingeJoint.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None; // Activate grabbing 
        }
        else
        {
            lockSymbol.SetActive(true);
            mainHingeJoint.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation; // Deactivate grabbing 
        }
        
    }
    
 

    void Update()
    {

        // Update current value 
        currentValueBoundText.text = stateValue.Value.ToString("F2");
        
        // Update angle from remote value change 
        // Update dial rotation only when hand is not attached, otherwise hand updates position 
        if (updateDialPosition && !handIsAttached)
        {
            mainHingeJoint.transform.localEulerAngles = new Vector3(0,ValueToAngle(stateValue.Value),0);
            updateDialPosition = false;
        }
        
        
        // Update rotation (and value) from local change 
        // Check whether position has changed and update 
        if (Mathf.Abs(FormatAngle180(mainHingeJoint.transform.localEulerAngles.y) - previousAngle) > 0.00001f && IsOwner)
        {
            
            UpdateFloatState(AngleToValue(mainHingeJoint.transform.localEulerAngles.y), "");
        }
        
        
        // Store previous x position
        previousAngle = FormatAngle180(mainHingeJoint.transform.localEulerAngles.y);


    }
    
   
}
