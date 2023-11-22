using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Autohand;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


public class SoundStoreFloatArrayInteractable : FloatArrayInteractable
{

    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private DistributeAudio distributeAudio;
    [SerializeField] private RecordAudio recordAudio;
    [SerializeField] private PhysicsGadgetButton screenToggleButton;
    [SerializeField] private Canvas screenCanvas;
    [SerializeField] private Material physicalButtonOnMaterial;
    [SerializeField] private Material physicalButtonOffMaterial;


    [SerializeField] private SoundStoreScreenUi screenUi;

    private CustomAudioRenderer audioRenderer;
    private WavReader wavReader; 
    
    private bool screenIsActive = false;

    private int nChannels = 0;
    private int samplingFrequency = 0;
    private string audioIdentifier = "";
    
    

    // Inherited From FloatArrayInteractable 
    // protected float[] floatArray;
    
    // Inherited from Interactable 
    // public bool isModifiable; 
    
    
    
    // Order of Execution: On some Client record sound & confirm -> send to server -> send to clients 
    
    // Called after Awake
    public override void OnNetworkSpawn()
    {
        
        // Add a listener for when new audio was received on client 
        distributeAudio.newAudioOnClientEvent.AddListener(() =>
        {
            // Get Audio 
            floatArray = distributeAudio.GetAudioReceivedFromServer();
            
            // Get Identifier, Sampling Frequency, number of channels 
            audioIdentifier = distributeAudio.GetIdentifierReceivedFromServer();
            nChannels = distributeAudio.GetNumberChannelsReceivedFromServer();
            samplingFrequency = distributeAudio.GetSamplingFrequencyReceivedFromServer();
            
            // Apply
            GenerateAudioClipAndApply();
            
            // Write to disk 
            WriteAudioToFile(ExperienceManager.Singleton.storeNarrationsPath);
            
            // Switch to play view in display 
            screenUi.EnablePlayMenu();

        });

        
        // Add a listener on server for when new audio is received from client to distribute to other clients 
        if (IsServer)
        {
            distributeAudio.newAudioOnServerEvent.AddListener(() =>
            {
                // Get Audio 
                floatArray = distributeAudio.GetAudioReceivedFromClient();
                
                // Get Identifier, Sampling Frequency, number of channels 
                audioIdentifier = distributeAudio.GetIdentifierReceivedFromClient();
                nChannels = distributeAudio.GetNumberChannelsReceivedFromClient();
                samplingFrequency = distributeAudio.GetSamplingFrequencyReceivedFromClient();
                
                // Distribute to Clients 
                distributeAudio.SendAudioToClients(floatArray, samplingFrequency, nChannels, audioIdentifier);
                
                // As soon as audio has been set, nobody can change anymore 
                AccessManager.Singleton.SetAccessTypePerUniqueObjectId(AccessType.NobodyCanModify,new int[] {objectInfo.GetUniqueObjectId()});
                
            });
        }
        

    }

    void Start()
    {

        audioRenderer = GetComponent<CustomAudioRenderer>();


        // Add a listener for the screen toggle button 
        screenToggleButton.OnPressed.AddListener(() =>
            {
                // Toggle canvas 
                screenCanvas.enabled = !screenIsActive;
                screenIsActive = !screenIsActive;
                
                // Toggle button color
                if (screenIsActive)
                {
                    screenToggleButton.transform.GetChild(0).GetComponent<Renderer>().material = 
                        physicalButtonOnMaterial;
                }
                else
                {
                    screenToggleButton.transform.GetChild(0).GetComponent<Renderer>().material = 
                        physicalButtonOffMaterial;
                }
                
                
                
                // Check whether recording is still possible 
                if (!isModifiable)
                {
                    screenUi.EnablePlayMenu();
                }
                
            });
        
        // Add listeners for the screen 
        screenUi.startRecordAudioEvent.AddListener(StartRecordAudio);
        screenUi.stopRecordAudioEvent.AddListener(StopRecordAudio);
        screenUi.startPlayAudioEvent.AddListener(StartPlayAudio);
        screenUi.stopPlayAudioEvent.AddListener(StopPlayAudio);
        screenUi.confirmRecordedAudioEvent.AddListener(ConfirmRecordedAudio);
    }
    
    
    
    // Use to initialize NetworkVariable locally or at first occurence on server globally 
    void Awake()
    {
        
       base.Awake();
       
    }


    
    // Update whether player can modfiy value 
    public override void UpdateInteractableModifiable(bool canBeModified)
    {
        isModifiable = canBeModified;
    }
    
    

    protected override void UpdateFloatArrayState(float[] newFloatArray)
    {
        if (isModifiable)
        {
            // Send to Server so that server distributes to Clients; metadata is set beforehand locally  
            distributeAudio.SendAudioToServer(newFloatArray, samplingFrequency, nChannels, audioIdentifier);
        }
        else
        {
            // Do not update 
        }
    }
    
    
    public virtual void RestoreValue(float[] newFloatArray)
    {
        if (IsServer)
        {
             
            // Distribute audio to Clients 
            distributeAudio.SendAudioToClients(newFloatArray, samplingFrequency, nChannels, audioIdentifier);
            
            // As soon as audio has been set, nobody can change anymore 
            AccessManager.Singleton.SetAccessTypePerUniqueObjectId(AccessType.NobodyCanModify,new int[] {objectInfo.GetUniqueObjectId()});

        }
    }

    public void RestoreValueAndMetaData(float[] newFloatArray, int samplingFreq, int numberChannels, string identifier)
    {
        floatArray = newFloatArray;
        samplingFrequency = samplingFreq;
        nChannels = numberChannels;
        audioIdentifier = identifier;
        
        if (IsServer && (newFloatArray != null) && (newFloatArray.Length > 0))
        {
            // Distribute audio to Clients 
            distributeAudio.SendAudioToClients(floatArray, samplingFrequency, nChannels, audioIdentifier);
            
            // As soon as audio has been set, nobody can change anymore 
            AccessManager.Singleton.SetAccessTypePerUniqueObjectId(AccessType.NobodyCanModify,new int[] {objectInfo.GetUniqueObjectId()});

        }
    }

    
    public void RestoreMetaDataLocal(int samplingFreq, int numberChannels, string identifier)
    {
        samplingFrequency = samplingFreq;
        nChannels = numberChannels;
        audioIdentifier = identifier;
    }

    
    public void RestoreValueAndMetaDataFromAudioFile(string filePath)
    {
        if (filePath == "" || filePath == " ")
        {
            return;
        }

        string fileName = Path.GetFileNameWithoutExtension(filePath);
        if (fileName.ToLower().StartsWith("narrationaudio_"))
        {
            fileName = fileName.Remove(0, 15);
        }
        
        AudioClip audioClip = wavReader.ToAudioClip(filePath);
        float[] data = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(data, 0);
        RestoreValueAndMetaData(data, audioClip.frequency, audioClip.channels, fileName);
    }


    
    

    public int GetChannels()
    {
        return nChannels;
    }

    public int GetSamplingFreq()
    {
        return samplingFrequency;
    }
    
    public string GetAudioIdentifier()
    {
        return audioIdentifier;
    }


    public string WriteAudioToFile(string directoryPath)
    {
        
        
        // Make sure data exists 
        if (floatArray != null && floatArray.Length > 0)
        {
            audioRenderer.Clear();
            audioRenderer.SetSamplingFrequency(samplingFrequency);
            audioRenderer.SetChannels(nChannels);
            audioRenderer.Write(floatArray);

            string fileName = "NarrationAudio_" + audioIdentifier + ".wav";
            string filePath = directoryPath + "/" + fileName;
            
            // Audio might potentially already exist 
            // when server is requested to generate Scene setup
            if (!System.IO.File.Exists(filePath))
            {
                
                // Make sure folder exists
                Directory.CreateDirectory(directoryPath);
                
                audioRenderer.Save(filePath);
            }
            audioRenderer.Clear();

            return fileName;

        }
        else
        {
            return "";
        }
       
    }


    

    private void GenerateAudioClipAndApply()
    {
        // Generate AudioClip with locally stored data 
        AudioClip audioClip = AudioClip.Create("recordedAudio_" + audioIdentifier.ToString(), floatArray.Length / nChannels, nChannels, ExperienceManager.Singleton.micAudioSamplingFrequency, false);
        audioClip.SetData(floatArray, 0); 
        
        // Apply to audio source 
        audioSource.clip = audioClip;
    }

    private void StartRecordAudio()
    {
        recordAudio.StartRecordingAudio();
    }
    
    
    private void StopRecordAudio()
    {
        // Stop recording
        recordAudio.StopRecordingAudio();
        
        // Get Metadata 
        floatArray = recordAudio.GetAudio();
        nChannels = recordAudio.GetChannels();
        samplingFrequency = recordAudio.GetSamplingFrequency();
        audioIdentifier = "rec_clientId_" + NetworkManager.Singleton.LocalClientId.ToString() + "_date_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        
        GenerateAudioClipAndApply();
    }


    private void StartPlayAudio()
    {
        audioSource.Play();
    }

    private void StopPlayAudio()
    {
        audioSource.Stop();
    }

    private void ConfirmRecordedAudio()
    {
        // Send Float array to everyone 
        UpdateFloatArrayState(floatArray);

        // Switch menus and save to all clients 
        screenUi.EnablePlayMenu();
    }
    
    
    
   
}

