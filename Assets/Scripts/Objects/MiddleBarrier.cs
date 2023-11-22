using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiddleBarrier : MonoBehaviour
{

    [SerializeField] private bool isPrespawnedSide;
    
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
        
        
        
        // Find all audio sources 
        AudioSource[] audioSources = GameObject.FindSceneObjectsOfType(typeof(AudioSource)) as AudioSource[];
        
        foreach(AudioSource audioSource in audioSources)
        {
            
            // Depending on which side (pos or neg z values) switch on or off audio sources 
            if (isPrespawnedSide)
            {
                if (audioSource.transform.position.z > 0)
                {
                    audioSource.volume = 1;
                }
                else
                {
                    audioSource.volume = 0;
                }
            }
            else // dynamic side 
            {
                if (audioSource.transform.position.z < 0)
                {
                    audioSource.volume = 1;
                }
                else
                {
                    audioSource.volume = 0;
                }
            }
            
        }
        
    }
}
