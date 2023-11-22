using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckSpawnedCollision : MonoBehaviour
{

    private int collidingObjectsWithObjectInfo = 0;
    public bool isSpawnPossible;
    public UnityEvent spawnPossible;
    public UnityEvent spawnNotPossible;
    
    // Start is called before the first frame update
    void Start()
    {
        // Init with true;
        isSpawnPossible = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        
        // Check if collision is from Object with Object Info 
        if (other.transform.root.GetComponent<ObjectInfo>() != null)
        {
            collidingObjectsWithObjectInfo += 1;

            if (collidingObjectsWithObjectInfo > 0)
            {
                isSpawnPossible = false;
                spawnNotPossible.Invoke();
            }
            else
            {
                isSpawnPossible = true;
                spawnPossible.Invoke();
            }
        }
    }
    
    
    private void OnTriggerExit(Collider other)
    {
        
        // Check if collision is from Object with Object Info 
        if (other.transform.root.GetComponent<ObjectInfo>() != null)
        {
            collidingObjectsWithObjectInfo -= 1;
            
            if (collidingObjectsWithObjectInfo > 0)
            {
                isSpawnPossible = false;
                spawnNotPossible.Invoke();
            }
            else
            {
                isSpawnPossible = true;
                spawnPossible.Invoke();
            }
        }
    }
    
    
}
