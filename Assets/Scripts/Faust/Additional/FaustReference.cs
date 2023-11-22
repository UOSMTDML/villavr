using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// DEPRECATED
/// </summary>


public class FaustReference : MonoBehaviour
{
    
    public FaustObject signalGen;
    public FaustObject filterGen; 
    
    
    // Start is called before the first frame update
    void Start()
    {
        
        int bufferLength,
            numBuffers;
        AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);

        int numChannels = 1;

        int numFrames = bufferLength / numChannels;

        int sampleRate = AudioSettings.outputSampleRate;
        
        AudioClip myClip = AudioClip.Create("Filtered", numFrames, numChannels, sampleRate, true);
        AudioSource aud = GetComponent<AudioSource>();
        aud.clip = myClip;
        aud.Play();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    private void OnAudioFilterRead(float[] buffer, int numchannels) {
		
        signalGen.ProcessBuffer(buffer, numchannels);
        
		
        filterGen.ProcessBuffer(buffer, numchannels);
        
    }
}
