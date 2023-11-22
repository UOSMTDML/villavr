using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Pyin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (false)
        {
        
            int SAMPLE_RATE = 44100;
            int SAMPLE_COUNT = 10000;
            int BLOCK_SIZE = 2048;
            int STEP_SIZE = 512;

            //float* dataPtr = stackalloc float[SAMPLE_COUNT];
            // Generate a 440 herz sine wave
            float[] samples = new float[SAMPLE_COUNT];
            double freq = 440;
            double angle_speed = 2 * Math.PI * (freq / SAMPLE_RATE);
            for (int i = 0; i < SAMPLE_COUNT; i++)
            {
                samples[i] = (float)Math.Sin(angle_speed * i);
            }
                

            float[] res = CalculateMainPitches(samples, SAMPLE_RATE, SAMPLE_COUNT, BLOCK_SIZE, STEP_SIZE);

            foreach (float elem in res)
            {
                Debug.Log(elem);
            }
                
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    // Calculate Main Pitches 
    public float[] CalculateMainPitches(float[] samples, int sampleRate = 48000, int sampleCount = 10000, int blockSize = 2048,
        int stepSize = 512)
    {
        unsafe
        {
            fixed ( float* dataPtr = & samples[0])
            {
                float[] res = Pylin.use(dataPtr, sampleRate, sampleCount, blockSize, stepSize);
                return res;
            }
        }
    }

}




public class Pylin  {
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct pyinc_pitch_range
    {
        public float* begin;
        public float* end;
    }

    [DllImport("LibPyin")]
    static extern void pyinc_init(int sample_rate, int block_size, int step_size);

    [DllImport("LibPyin")]
    unsafe static extern pyinc_pitch_range pyinc_feed(float * data, int size);

    [DllImport("LibPyin")]
    unsafe static extern void pyinc_clear();

    
    unsafe static public float[] use (
        float* dataPtr, 
        int SAMPLE_RATE = 48000,
        int SAMPLE_COUNT = 10000,
        int BLOCK_SIZE = 2048,
        int STEP_SIZE = 512
    ) 
    {
        
        // Prepare objects
        pyinc_init(SAMPLE_RATE, BLOCK_SIZE, STEP_SIZE);
        
        // Mine pitches
        pyinc_pitch_range pitches = pyinc_feed(dataPtr, SAMPLE_COUNT);

        // Go through and store pitches
        float* res_ptr = pitches.begin;
        
        float[] resPitches = new float[(int) (pitches.end - pitches.begin)];
        while (res_ptr != pitches.end)
        {
            resPitches[(int) (res_ptr - pitches.begin)] = *res_ptr;
            res_ptr++;
        }
        
        
        // Release the data
        pyinc_clear();

        // Return pitches 
        return (resPitches);
        
    }
}

/*
  float* dataPtr = stackalloc float[SAMPLE_COUNT];
  // Generate a 440 herz sine wave
  double freq = 440;
  double angle_speed = 2 * Math.PI * (freq / SAMPLE_RATE);
  for (int i = 0; i < SAMPLE_COUNT; i++)
      dataPtr[i] = (float) Math.Sin(angle_speed * i);
 */
