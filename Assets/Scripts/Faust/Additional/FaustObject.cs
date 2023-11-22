using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FaustObject : MonoBehaviour
{

    public virtual void ProcessBuffer(float[] buffer, int numChannels) {}
    
    //public virtual void SetParameter(int param, float x) {}

    // Implemented by default in the Faust Scripts
    public virtual void setParameter(int param, float x) {}

    // Keep track of connected sound elements
    protected FaustObject[] connectedSoundElements;
    
    // Keep track of connected sound elements by input object id 
    protected Dictionary<int, List<FaustObject>> connectedSoundElementsByInputObjectId = new Dictionary<int, List<FaustObject>>();


    // Keep track of ready state 
    protected bool isReady = false; 
    
    
    // Update Connected Elements 
    public void UpdateConnectedSoundElements(List<int> connectedObjectIds, int fromInputObjectId)
    {
        List<FaustObject> faustList = new List<FaustObject>();

        // Get GameObject for ID and get Faust Component
        Dictionary<int, GameObject> spawnedObjects = NetworkSpawner.Singleton.GetSpawnedObjectsDictionary();
        foreach (int id in connectedObjectIds)
        {
            faustList.Add( spawnedObjects[id].GetComponent<Connection>().GetProcessingFaustObject());
        }
        
        connectedSoundElementsByInputObjectId[fromInputObjectId] = faustList;

        if (faustList.Count > 0)
        {
            isReady = true;
        }
        else
        {
            isReady = false;
        }
        
        // Generate full array of connected elements 
        List<FaustObject> faustObjects = new List<FaustObject>(); 
        foreach (var kvp in connectedSoundElementsByInputObjectId)
        {
            foreach (FaustObject faustObj in kvp.Value)
            {
                faustObjects.Add(faustObj);
            }
        }

        if (faustObjects.Count > 0)
        {
            connectedSoundElements = faustObjects.ToArray();
        }
        else
        {
            connectedSoundElements = null;
        }
        
        
        
        Debug.Log("[FaustObject] Updated Connected Sound Elements.");
        
    }
    
    
    
    
    
}
