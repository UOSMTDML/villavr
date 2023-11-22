using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCollision : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        
        try
        {
            other.gameObject.transform.root.GetComponent<MainObjectInfo>().DestroyObject(true);
        }
        
        catch (Exception e)
        {
        }

    }

   
    
    
    
    
    
}
