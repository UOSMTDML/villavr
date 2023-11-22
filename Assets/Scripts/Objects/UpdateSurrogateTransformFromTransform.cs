using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSurrogateTransformFromTransform : MonoBehaviour
{

    [SerializeField] private Transform sourceObject;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = sourceObject.position;
        transform.rotation = sourceObject.rotation;
    }



    public Rigidbody GetSourceRigidbody()
    {
        return sourceObject.GetComponent<Rigidbody>();
    }


}
