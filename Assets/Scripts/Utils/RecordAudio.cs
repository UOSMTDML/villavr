using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RecordAudio : MonoBehaviour
{

    private float[] audioData;
    private int channels;
    private int samplingFrequency; 

    private string currentlyRecordingMicName;
    private AudioSource audioSource;


    [SerializeField] private AudioSource dummySource;



    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // For Microphones, always 1
        channels = 1;
        
        // From ExperienceManager get Sampling Frequency
        samplingFrequency = ExperienceManager.Singleton.micAudioSamplingFrequency;
    }
    

    public void StartRecordingAudio()
    {
        audioSource.clip = Microphone.Start(ExperienceManager.Singleton.selectedMicName, false, ExperienceManager.Singleton.micAudioMaxRecordTimeSeconds, samplingFrequency);
        currentlyRecordingMicName = ExperienceManager.Singleton.selectedMicName;
    }

    public void StopRecordingAudio()
    {
        // Stop Recording 
        Microphone.End(currentlyRecordingMicName);
        
        // Store Audio Data 
        audioData = new float[audioSource.clip.samples * audioSource.clip.channels];
        audioSource.clip.GetData(audioData, 0);
        channels = audioSource.clip.channels;
    }


    public float[] GetAudio()
    {
        
        
        return audioData;
        
        
        /*
        // Using a dummy 
        float[] audioDummy = new float[dummySource.clip.samples * dummySource.clip.channels];
        dummySource.clip.GetData(audioDummy, 0);
        return audioDummy;
        */ 

        
        
    }
    

    public int GetChannels()
    {
        return channels;
    }

    public int GetSamplingFrequency()
    {
        return samplingFrequency;
    }
    
    
}
