using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SampleAnalyzer : FaustObject
{

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject noSignalText;
    [SerializeField] private float lineRendererPlotSampleFraction = 0.5f;

    [Header("Internals")] 
    [SerializeField] private bool tryPlotWaveStanding = true; // Apply all the calculations and try to plot consecutive waves on top pf each other  
    [SerializeField] private int numberOfStoreBuffers = 10;
    [SerializeField] private int smoothOverLastCalculatedFreqs;
    [SerializeField] private int calculateFrequencyEveryNthUpdate;
    [SerializeField] private int plotEveryNthUpdate;
    private int updateIdxCalculate; 
    private int updateIdxPlot;
    

    [Header("Pyin")]
    [SerializeField] private int blockSize = 2048; // original 2048 
    [SerializeField] private int stepSize = 256; // original 512;
    
    // Components 
    private Pyin pyin;

    // Store last buffer information 
    private float[] currentBuffer; // bufferLength 1024, currentBuffer size 2048, plotted is only half i.e. 512 data points 
    private List<float[]> lastBufferStore  = new List<float[]>(); // List of buffers; last 
    
    private int lastBufferStoreIdx = -1;
    private bool storeBufferIsReady = false;
    
    // Values gotten from DSP 
    private int bufferLength; // OnAudioFilterRead data length is (channel count) * bufferLength; in interleaved format (left-right-left-right...)
    private int numBuffers; // number of buffers 
    private int sampleRate; // typcially 48k Hz
    private double sampleNumber; // number of the current sample 

    // Store last plot information 
    private double lastSamplePlotted; // number of the last plotted sample 
    private int lastOffset; // offset of the last plotted sample 

    // Keep track of main frequency 
    private List<float> lastMainFrequencies = new List<float>();


    private void Awake()
    {
        
    }


    // Start is called before the first frame update
    void Start()
    {
       
        // Get DSP data and components 
        AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
        sampleRate = AudioSettings.outputSampleRate;
        pyin = GetComponent<Pyin>();

        // Init 
        lineRenderer.positionCount = (int) (bufferLength * lineRendererPlotSampleFraction); // Always only plot half of the signal to not show offset at the end of signal  
        currentBuffer = new float[bufferLength];
        lastSamplePlotted = 0;
        lastOffset = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        // If all buffers have been filled once 
        if (storeBufferIsReady && (updateIdxCalculate >= calculateFrequencyEveryNthUpdate))
        {
            // Create a copy of the buffer list to make thread-safe 
            List<float[]> storedBufferCopy = new List<float[]>(lastBufferStore);

            // Concatenate multiple previously stored buffers 
            int idx = 0;
            List<float> bufferList = storedBufferCopy[0].ToList();
            
            foreach(float[] buffer in storedBufferCopy)
            {
                if (idx != 0)
                {
                    bufferList.AddRange(buffer);
                }
                idx += 1;
            }
            
            // Remove right channel 
            float[] samples = new float[bufferList.Count / 2];
            for (int i = 0; i < bufferList.Count / 2; i++)
            {
                samples[i] = bufferList[i * 2];
            }
            
            
            int sampleCount = samples.Length;
            float[] res = pyin.CalculateMainPitches(samples, sampleRate, sampleCount, blockSize, stepSize);
        
            // Calculate main frequency 
            float mainFreq = res.Sum() / res.Length; 
            //Debug.Log("Calculated Main Frequency: " + mainFreq);
            
            // Keep track of last main frequencies
            lastMainFrequencies.Add(mainFreq);
            if (lastMainFrequencies.Count > smoothOverLastCalculatedFreqs)
            {
                lastMainFrequencies.RemoveAt(0);
            }
            
            // Reset update idx 
            updateIdxCalculate = 0;

        }

        // Smooth last few calculated frequencies 
        float processFreq = lastMainFrequencies.Sum() / lastMainFrequencies.Count;
        //Debug.Log("Calculated Smoothed Frequency: " + processFreq);

        if (updateIdxPlot >= plotEveryNthUpdate)
        {
            ProcessSignal((double)processFreq, sampleNumber);
            updateIdxPlot = -1;
        }
        

        updateIdxCalculate += 1;
        updateIdxPlot += 1;
        
        
        // Check if plot should be reset
        if (!isReady)
        {
            ResetPlot();
        }
        else
        {
            noSignalText.SetActive(false);
        }
    }

    private void ProcessSignal(double mainFrequency, double currentSampleNumber)
    {
        /*
         * Findings:
         * Audio Buffer is non-overlapping, i.e. next buffer immediately follows previous one.
         * 
         * The number of samples per period of sampled signal is sample rate / sampled signal rate, e.g. 48000 / 300 = 160
         * For 300Hz signal and 48000Hz sampling, data points 64 to 224 is one period, 160 period length
         *
         * Find offset between signals in the two buffers by calculating modulo of sample number difference % period length number of samples
         * The resulting number is an offset, that the second signal is further in the future.
         * Offset this with period sample number - offset (i.e. the "other direction offset") 
         */
        
        
        if (currentSampleNumber == lastSamplePlotted)
        {
            // No new sample submitted by OnAudioFilterRead()
            return;
        }
        
        // Minimal sanity check, that frequency is valid (e.g. not -1) 
        bool frequencyValid = mainFrequency > 0;
        
        
        // Calculate number of samples per signal period length
        double periodLengthNumberOfSamples = sampleRate / mainFrequency;
        
        // Number of samples between current and last plotted sample, should be bufferlength times the amount of store buffers 
        int sampleNumberDifference =  (int) (currentSampleNumber - lastSamplePlotted);  

        // Calculate offsets between previous signal phase and current phase, take into account previous offset; generate offset in opposite direction 
        int multiplePeriodOffset = (int) ((sampleNumberDifference - lastOffset)  % periodLengthNumberOfSamples); // Offset necessary (+) to align to period
        int multiplePeriodOffsetOtherDirection = (int) (periodLengthNumberOfSamples - multiplePeriodOffset); // Offset necessary (-)

        if (false)
        {
            Debug.Log("****");
            Debug.Log("Processing Sample Number" + sampleNumber);
            Debug.Log("Difference" + (sampleNumber - lastSamplePlotted));
            Debug.Log("periodLengthNumberOfSamples: " + periodLengthNumberOfSamples);
            Debug.Log("sampleNumberDifference: " + sampleNumberDifference);
            Debug.Log("lastOffset: " + lastOffset);
            Debug.Log("multiplePeriodOffset: " + multiplePeriodOffset);
            Debug.Log("multiplePeriodOffsetOtherDirection: " + multiplePeriodOffsetOtherDirection);
            Debug.Log("*****");
        }
        
        

        // Try to plot standing wave, make sure that calculated frequency is valid 
        // And make sure that plotting offset does not reach into visible area 
        if (tryPlotWaveStanding && frequencyValid && (multiplePeriodOffsetOtherDirection < (bufferLength * (1 - lineRendererPlotSampleFraction)) ))
        {
            // Update last offset 
            lastOffset = multiplePeriodOffsetOtherDirection;
            
            // Calculate y scaling; prev div by 4.0f; max line renderer y value: -0.25 and 0.25
            float bufferMaxValue = currentBuffer.Max();
            float bufferMinValue = currentBuffer.Min();
            float bufferScaling = Mathf.Max(Mathf.Max(Mathf.Abs(bufferMaxValue), Mathf.Abs(bufferMinValue)), 0.2f);
            
            
            // Plot, nothing plotted before 
            if (lastSamplePlotted == 0)
            {
                Vector3[] positionsAudioData = new Vector3[bufferLength];
                
                for (int i = 1; i < bufferLength; i += 1)
                {
                    positionsAudioData[i - 1] = new Vector3(i / 400.0f, currentBuffer[i*2] / bufferScaling * 0.25f, 0); 
                }
                lineRenderer.SetPositions(positionsAudioData); // Sample positions extending beyond length of line renderer will be omitted 
            }
            else // plot, something plotted before 
            {
                Vector3[] positionsAudioData = new Vector3[bufferLength];
                for (int i = 1; i < bufferLength; i += 1)
                {
                    if (i < bufferLength - multiplePeriodOffsetOtherDirection)
                    {
                        // Shift values by offset 
                        positionsAudioData[i - 1] = new Vector3(i / 400.0f, currentBuffer[i*2 + multiplePeriodOffsetOtherDirection * 2] / bufferScaling * 0.25f, 0);
                    }
                    else
                    {
                        // Values beyond not available, hence set to zero 
                        positionsAudioData[i - 1] = new Vector3(i / 400.0f, 0, 0);
                    }
                }
                lineRenderer.SetPositions(positionsAudioData); // Sample positions extending beyond length of line renderer will be omitted 
            }
        }
        else // do not plot standing wave 
        {
            // Update last offset 
            lastOffset = 0;
            
            // Calculate y scaling; prev div by 4.0f; max line renderer y value: -0.25 and 0.25
            float bufferMaxValue = currentBuffer.Max();
            float bufferMinValue = currentBuffer.Min();
            float bufferScaling = Mathf.Max(Mathf.Max(Mathf.Abs(bufferMaxValue), Mathf.Abs(bufferMinValue)), 0.2f);
            
            Vector3[] positionsAudioData = new Vector3[bufferLength];
            for (int i = 1; i < bufferLength; i += 1)
            {
                positionsAudioData[i - 1] = new Vector3(i / 400.0f, currentBuffer[i*2] / bufferScaling * 0.25f, 0);
            }
            lineRenderer.SetPositions(positionsAudioData); // Sample positions extending beyond length of line renderer will be omitted 
        }
        
        // Update plotted sample number 
        lastSamplePlotted = currentSampleNumber;

    }

    private void ResetPlot()
    {
        Vector3[] positionsAudioData = new Vector3[(int) (bufferLength * lineRendererPlotSampleFraction)];
        for (int i = 0; i < (int) (bufferLength * lineRendererPlotSampleFraction); i += 1)
        {
            positionsAudioData[i] = new Vector3(0, 0, 0);
        }
        positionsAudioData[((int) (bufferLength * lineRendererPlotSampleFraction)) - 1] = new Vector3(1.26f,0,0);

        lineRenderer.SetPositions(positionsAudioData);
        
        noSignalText.SetActive(true);
    }
    
    
    public override void ProcessBuffer(float[] buffer, int numchannels)
    {
        // Make sure connected sound elements are available 
        if (!isReady)
        {
            return;
        }
        
        // Only process buffer using connected previous element, do not process further 
        connectedSoundElements[0].ProcessBuffer(buffer,numchannels);
        
        // Local processing 
        currentBuffer = buffer;
        sampleNumber = AudioSettings.dspTime * sampleRate;

        if (lastBufferStore != null)
        {
            lastBufferStore.Add(buffer);
            if (lastBufferStore.Count > numberOfStoreBuffers)
            {
                lastBufferStore.RemoveAt(0);
                storeBufferIsReady = true;
            }
        }
        
		
    }
   
    /*

    private void OnAudioFilterRead(float[] data, int channels)
    {
        currentBuffer = data;
        sampleNumber = AudioSettings.dspTime * sampleRate;

        if (lastBufferStore != null)
        {
            lastBufferStore.Add(data);
            if (lastBufferStore.Count > numberOfStoreBuffers)
            {
                lastBufferStore.RemoveAt(0);
                storeBufferIsReady = true;
            }
        }
        
    }
    */ 
}
