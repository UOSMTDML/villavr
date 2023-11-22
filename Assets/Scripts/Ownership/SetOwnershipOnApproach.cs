using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using UnityEngine;

public class SetOwnershipOnApproach : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        // When Grabbed set ownership
        GetComponent<Grabbable>().onHighlight.AddListener((arg0, grabbable) =>
            {
                try
                {
                    transform.root.GetComponent<OwnershipHelper>().SetLocalClientAsOwner();

                }
                catch (Exception e)
                {
                    Debug.Log("[SetOwnershipOnApproach] Could not find and update with OwnershipHelper for GameObject " +
                              this.name + ".");
                }
            });

        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
