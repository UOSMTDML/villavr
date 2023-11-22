using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using Unity.Netcode;
using UnityEngine;

public class ObjectHandle : MonoBehaviour
{
    private bool alignHorizontallyOnRelease;
    private float adjustmentSpeed; // amount added per Update() until 1 is reached 
    
    
    private Grabbable grabbable;
    private Transform mainObjectTransform;
    
    private bool updatePositionRotation;
    private Quaternion initialRotation;
    private float percentageDone;
    
    

    // Start is called before the first frame update
    void Start()
    {
        grabbable = GetComponent<Grabbable>();
        mainObjectTransform = grabbable.body.transform;
        
        
        
        // Get from Global settings 
        alignHorizontallyOnRelease = ExperienceManager.Singleton.interactablesRotateBackIntoHorizontalPlaneOnRelease;
        adjustmentSpeed = ExperienceManager.Singleton.rotationAmountPerFrame;

        // Add listener for grab event 
        grabbable.OnGrabEvent += (Hand hand, Grabbable grabbable1) =>
        {
            RunOnGrab();
        };
        
        // Add listener for release event 
        grabbable.OnReleaseEvent += (Hand hand, Grabbable grabbable1) =>
        {
            RunOnRelease();
        };
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (updatePositionRotation & alignHorizontallyOnRelease)
        {
           // Slerp towards target 
           Quaternion newRotation = Quaternion.Slerp(initialRotation, Quaternion.Euler(new Vector3(0, initialRotation.eulerAngles.y, 0)), percentageDone);

           // Update rot and keep track of amount 
           mainObjectTransform.rotation = newRotation;
           percentageDone += adjustmentSpeed;

           // Stop if needed 
           if (percentageDone >= 1)
           {
               updatePositionRotation = false;
               percentageDone = 0;

           }
        }
    }
    
    private void RunOnGrab()
    {
        // Toggle kinematic 
        mainObjectTransform.GetComponent<Rigidbody>().isKinematic = false;
        
       
        
    }

    private void RunOnRelease()
    {
        // Toggle kinematic 
        mainObjectTransform.GetComponent<Rigidbody>().isKinematic = true;
        
        // Indicate update of rot
        initialRotation = mainObjectTransform.rotation;
        updatePositionRotation = true;
    }
    
    
}
