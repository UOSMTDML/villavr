using System.Collections;
using System.Collections.Generic;
using Autohand;
using UnityEngine;

public class LocalButton : MonoBehaviour
{


    public PhysicsGadgetButton physicsGadgetButton;

    [SerializeField] private Material isUsableMaterial;
    [SerializeField] private Material isNotUsableMaterial;
    [SerializeField] private GameObject mainButtonObject;


    public void ToggleMaterial(bool isUsable)
    {
        if (isUsable)
        {
            mainButtonObject.GetComponent<Renderer>().material = isUsableMaterial;
        }
        else
        {
            mainButtonObject.GetComponent<Renderer>().material = isNotUsableMaterial;
        }
        
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
