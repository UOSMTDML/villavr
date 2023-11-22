using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class SpectrumAnalyzer : FaustObject
{

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject noSignalText;
    [SerializeField] private bool plotLogRatherThanLinear;
    [SerializeField] private int plotEveryNthUpdate;
    [SerializeField] private int useBufferFractionPowerOfTwo; // calculating FFT of entire buffer takes too long 
    
    private float[] currentBuffer;
    private int spectrumBins;
    private int numberOfUsedSamples; 
    private bool bufferReady;
    
    // Values gotten from DSP 
    private int bufferLength; // OnAudioFilterRead data length is (channel count) * bufferLength; in interleaved format (left-right-left-right...)
    private int numBuffers; // number of buffers 
    private int sampleRate; // typcially 48k Hz
    private double sampleNumber; // number of the current sample 

    // Store last plot information & update idx 
    private double lastSamplePlotted = 0; // number of the last plotted sample 
    private int updateIdxCalculate = 0;

    


    private void Awake()
    {
        
    }


    // Start is called before the first frame update
    void Start()
    {
        // Get DSP data and components 
        AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
        sampleRate = AudioSettings.outputSampleRate;
        
        // Init 
        numberOfUsedSamples = (int) ((1.0f / Math.Pow(2,useBufferFractionPowerOfTwo)) * bufferLength);
        spectrumBins = numberOfUsedSamples / 2 + 1; // buffer length refers to 1 channel; apply fraction 
        lineRenderer.positionCount = spectrumBins;
        
        Debug.Log(bufferLength);
        Debug.Log("number used samples" + numberOfUsedSamples + " spectrum bins" + spectrumBins);
    }

    // Update is called once per frame
    void Update()
    {
        
        if ((updateIdxCalculate >= plotEveryNthUpdate) && bufferReady)
        {
            // Remove right channel, only use specified number of samples 
            float[] samples = new float[numberOfUsedSamples]; 
            for (int i = 0; i < numberOfUsedSamples; i++)
            {
                samples[i] = currentBuffer[i * 2];
            }
            double[] signal = Array.ConvertAll(samples, x => (double)x);

            // Shape the signal using a Hanning window
            var window = new FftSharp.Windows.Hanning();
            window.ApplyInPlace(signal);

            // Calculate the FFT as an array of complex numbers
            //Complex[] fftRaw = FftSharp.Transform.FFT(signal);

            // Generates a mirrored plot, since fft is performed on real values
            // Hence, use n/2 + 1 data points, first component is dc 
            // Get the magnitude (units²) or power (dB) as real numbers
            double[] fftMag = FftSharp.Transform.FFTmagnitude(signal);
            //double[] fftPwr = FftSharp.Transform.FFTpower(signal);
            
            // Get associated frequencies 
            double[] associatedFrequencies = FftSharp.Transform.FFTfreq(sampleRate, fftMag.Length);
            
            // Update plotted sample number 
            lastSamplePlotted = sampleNumber;
            
            // Plot Spectrum 
            ProcessSignal(fftMag, associatedFrequencies);
            
            // Reset update idx 
            updateIdxCalculate = -1;
        }

        // Update update idx 
        updateIdxCalculate += 1;
        
        
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

    
    private void ResetPlot()
    {
        Vector3[] positionsAudioData = new Vector3[spectrumBins];
        for (int i = 0; i < spectrumBins; i += 1)
        {
            positionsAudioData[i] = new Vector3(0, 0, 0);
        }
        positionsAudioData[spectrumBins - 1] = new Vector3(1.26f,0,0);

        lineRenderer.SetPositions(positionsAudioData);
        
        noSignalText.SetActive(true);
    }
    
    
    private void ProcessSignal(double[] spectrum, double[] associatedFrequencies)
    {
        
        // Calculate y scaling; prev div by 2.0f; max line renderer y value: -0.25 and 0.25
        double spectrumMaxValue = spectrum.Max();
        double spectrumMinValue = spectrum.Min();
        float spectrumScaling = Mathf.Max(Mathf.Max(Mathf.Abs((float)spectrumMaxValue), Mathf.Abs((float)spectrumMinValue)), 0.1f);
        
        
        if (plotLogRatherThanLinear)
        {
                
            Vector3[] positionsSpectrumData = new Vector3[spectrum.Length - 1]; // Skip dc part 
            for (var i = 0; i < spectrum.Length - 1; i++)
            {
                float xPos = ( (float)Math.Log(associatedFrequencies[i + 1]) - (float)Math.Log(associatedFrequencies[1]) )/ 5f;
                
                positionsSpectrumData[i] = new Vector3(xPos , (float) (spectrum[i + 1] / spectrumScaling * 0.25f), 0); // spectrum & freq + 1 offset to omit dc part 
                Debug.Log("i: " + i);
                Debug.Log("ass freq: " + associatedFrequencies[i + 1]);
                Debug.Log("scaled log:" + Math.Log(associatedFrequencies[i  +1]) / 5f);
                
            }
            
            lineRenderer.SetPositions(positionsSpectrumData); // Sample positions extending beyond length of line renderer will be omitted 

            
        }
        else
        {
            Vector3[] positionsSpectrumData = new Vector3[spectrum.Length - 1];
            for (int i = 0; i < spectrum.Length - 1; i += 1)
            {
                positionsSpectrumData[i] = new Vector3(i / 400.0f, (float) (spectrum[i+1] / spectrumScaling * 0.25f), 0); // spectrum +1 offset to omit dc part 
            }
            
            lineRenderer.SetPositions(positionsSpectrumData); // Sample positions extending beyond length of line renderer will be omitted 

            
        }
         

    }


    public override void ProcessBuffer(float[] buffer, int numchannels)
    {
        // Make sure connected sound elements are available 
        if (!isReady)
        {
            return;
        }

        // Only process buffer using connected previous element, do not process further 
        connectedSoundElements[0].ProcessBuffer(buffer, numchannels);

        // Local processing 
        currentBuffer = buffer;
        sampleNumber = AudioSettings.dspTime * sampleRate;

        // Indicate first buffer loaded 
        bufferReady = true;
    }


    /*

    private void OnAudioFilterRead(float[] data, int channels)
    {
        currentBuffer = data;
        sampleNumber = AudioSettings.dspTime * sampleRate;

    }
    
    */ 
}

