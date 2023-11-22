using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autohand;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FaustOutput : FaustObject
{

    // Inherited
    //protected FaustObject[] connectedSoundElements;
    //protected bool isReady


    [SerializeField] private PhysicsGadgetButton recordButton;
    [SerializeField] private GameObject recordButtonGameObject;
    [SerializeField] private Material defaultRecordButtonMaterial;
    [SerializeField] private Material recordingRecordButtonMaterial;
    [SerializeField] private string storeAudioRecordingsDirectory;
    private CustomAudioRenderer audioRenderer;
    private bool record = false;
    

    // Start is called before the first frame update
    void Start()
    {
        
        // Set up Audio Renderer 
        audioRenderer = GetComponent<CustomAudioRenderer>();
        audioRenderer.SetSamplingFrequency(AudioSettings.outputSampleRate);
        audioRenderer.SetChannels(2);
        audioRenderer.Clear();
        
        // Add listener for recording button 
        recordButton.OnPressed.AddListener(() =>
        {
            ToggleRecord(record);
        });

    }

    // Update is called once per frame
    void Update()
    {
        
        
    }


    private void ToggleRecord(bool doRecord)
    {
        if (!doRecord)
        {
            recordButtonGameObject.GetComponent<Renderer>().material = recordingRecordButtonMaterial;
            record = true;
        }
        else
        {
            recordButtonGameObject.GetComponent<Renderer>().material = defaultRecordButtonMaterial;
            record = false;
                
            
            // Save Recording 
            string time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            System.IO.Directory.CreateDirectory(storeAudioRecordingsDirectory);
            string fileName = storeAudioRecordingsDirectory + "/audio_" + time + ".wav";
            audioRenderer.Save(fileName);
            audioRenderer.Clear();
                
        }
        
        
    }
    
    
    
    

    [ServerRpc(RequireOwnership = false)]
    private void SignalEngaged_ServerRpc(bool isEngaged, ServerRpcParams serverRpcParams = default)
    {
        ReceiveIsEngaged_ClientRPC(isEngaged, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ReceiveIsEngaged_ClientRPC(bool isEngaged, ulong initiallySentFromId, ClientRpcParams clientRpcParams = default)
    {
        
        // This client indicated drawing/ not drawing 
        if (initiallySentFromId == NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        // Some other client is erasing/ not erasing  
        else
        {
            ToggleRecord(isEngaged);
        }
        
    }


    
    
    
    
    
    

    public override void ProcessBuffer(float[] buffer, int numChannels)
    {
        // Output node, do nothing 
        return;
    }
  
    
    
    private void OnAudioFilterRead(float[] buffer, int numChannels) {
        
        
        // Incoming buffer is empty, i.e. all 0s 

        if (isReady)
        {
            
            // Compute buffer of connected elements
            connectedSoundElements[0].ProcessBuffer(buffer, numChannels);
            

            if (record)
            {
                audioRenderer.Write(buffer);
            }
            
        }
        
        
    }
}
