using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionProhibitor : MonoBehaviour
{

    [SerializeField] private int[] layersToIgnore;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    void OnCollisionEnter(Collision collision)
    {

        // Ignore collision 
        if (layersToIgnore.Contains(collision.gameObject.layer))
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
        
        
    }

}
