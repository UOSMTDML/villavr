using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using UnityEngine;

public class SetOwnershipOnPress : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        // When Grabbed set ownership
        try
        {
            GetComponent<PhysicsGadgetButton>().OnPressed.AddListener(() =>
            {
                Debug.Log("SETTING OWNERSHIP ON GRAB");
                transform.root.GetComponent<OwnershipHelper>().SetLocalClientAsOwner();
            });

        }
        catch (Exception e)
        {
            Debug.Log("[SetOwnershipOnGrab] Could not find and update with OwnershipHelper for GameObject " + this.name + ".");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
