using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionChecker : MonoBehaviour
{

    public int currentNumberOfTriggers; 
    public int currentNumberOfCollisions; 
    
    public UnityEvent<Collider> onTriggerEnter;
    public UnityEvent<Collider> onTriggerExit;
    public UnityEvent<Collider> onTriggerStay;
    public UnityEvent<Collision> onCollisionEnter;
    public UnityEvent<Collision> onCollisionExit;
    public UnityEvent<Collision> onCollisionStay;
 
    void OnTriggerEnter(Collider other)
    {
        currentNumberOfTriggers += 1;
        
        if(onTriggerEnter != null) onTriggerEnter.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        currentNumberOfTriggers -= 1;
        
        if(onTriggerExit != null) onTriggerExit.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if(onTriggerStay != null) onTriggerStay.Invoke(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        currentNumberOfCollisions += 1;
        
        if(onCollisionEnter != null) onCollisionEnter.Invoke(collision);
    }

    private void OnCollisionExit(Collision other)
    {
        currentNumberOfCollisions -= 1; 
        
        if(onCollisionExit != null) onCollisionExit.Invoke(other);
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        if(onCollisionStay != null) onCollisionStay.Invoke(collisionInfo);
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
