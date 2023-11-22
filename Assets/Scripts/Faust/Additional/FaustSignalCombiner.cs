using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autohand;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class FaustSignalCombiner : FaustObject
{

    // Inherited
    //protected FaustObject[] connectedSoundElements;
    //protected bool isReady
    

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    private IEnumerator LogConnectedElements()
    {
        while (true)
        {
            if (connectedSoundElements != null)
            {
                Debug.Log(connectedSoundElements.Length);
            }
                yield return new WaitForSeconds(1);
            
            
        }
    }
    

    public override void ProcessBuffer(float[] buffer, int numChannels)
    {

        if (connectedSoundElements != null)

        {
            // Calculate for each connected input the buffer 
            for (int i = 0; i < connectedSoundElements.Length; i++)
            {
                
                // Populate buffer with data from each input 
                float[] currentBuffer = new float[buffer.Length];
                connectedSoundElements[i].ProcessBuffer(currentBuffer, numChannels);

                
                // Add scaled (to fraction of number of connected inputs) input buffer to final buffer  
                for (int j = 0; j < currentBuffer.Length; j++)
                {
                    buffer[j] += currentBuffer[j] / connectedSoundElements.Length;
                }
                
                
            }

        }
        
    }
    
}
