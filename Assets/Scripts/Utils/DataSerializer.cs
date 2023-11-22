using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using System.IO;

public class DataSerializer : MonoBehaviour
{
    // Singleton pattern
    // (Comparing against initially created static instance in Awake) 
    private static DataSerializer _singleton;
    public static DataSerializer Singleton
    {
        get { return _singleton; }
        private set { _singleton = value; }
    }
    private void Awake()
    {
        if (_singleton != null && _singleton != this)
        {
            Destroy(this.gameObject);
        } else {
            _singleton = this;
        }
    }

    
    
    public void WriteJsonToDisk(string path, string fileName, string jsonData)
    {
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        string fullPath = path + "/" + fileName;
        FileStream fileStream = new FileStream(fullPath, FileMode.Create);
        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(jsonData);
        }
    }
    
    
    public string ReadJsonFromDisk(string path, string fileName)
    {
        string fullPath = path + "/" + fileName;
        if (File.Exists(fullPath))
        {
            using (StreamReader reader = new StreamReader(fullPath))
            {
                string jsonData = reader.ReadToEnd();
                return jsonData;
            }
        }
        else
        {   
            Debug.Log("[DataSerializer] ReadJsonFromDisk: File not found at " + fullPath + ".");
            return "";
        }
    }

    
    public List<float[]> SplitFloatArrayIntoChunks(float[] inputArray, int chunkSize)
    {

        if (chunkSize <= 0)
        {
            Debug.Log("[DataSerializer] SplitFloatArrayIntoChunks: Specified chunkSize is too small, falling back to 12000.");
            chunkSize = 12000;
        }
        
        
        int fullArrayLength = inputArray.Length;
        List<float[]> arrayChunks = new List<float[]>();

        int chunkIdx = 0;
        while ((chunkIdx * chunkSize) < fullArrayLength)
        {
            float[] shorterSamples = inputArray.Skip(chunkIdx * chunkSize).Take(chunkSize).ToArray();
            arrayChunks.Add(shorterSamples);
            chunkIdx += 1;
        }
        
        return arrayChunks;
    }

    
    
    
    
    public SerializedIntIntDict SerializeIntIntDict(Dictionary<int,int> inputDict)
    {
        var listOfKeyValuePairs = inputDict.ToList();
        var keys = listOfKeyValuePairs.Select(kvp => kvp.Key);
        var values = listOfKeyValuePairs.Select(kvp => kvp.Value);
        int[] keys_array = keys.ToArray();
        int[] values_array = values.ToArray();
        
        return new SerializedIntIntDict{keys = keys_array, values = values_array};
    }

    public Dictionary<int, int> ReconstructIntIntDict(SerializedIntIntDict serializedInput)
    {
        Dictionary<int, int> reconstructedDict = new Dictionary<int, int>();
        for (int index = 0; index < serializedInput.keys.Length; index++)
        {
            reconstructedDict.Add(serializedInput.keys[index], serializedInput.values[index]);
        }
        
        return reconstructedDict;
    }


    public SerializedIntIntDict SerializeIntAccessTypeDict(Dictionary<int, AccessType> inputDict)
    {
        var listOfKeyValuePairs = inputDict.ToList();
        var keys = listOfKeyValuePairs.Select(kvp => kvp.Key);
        var values = listOfKeyValuePairs.Select(kvp => kvp.Value);
        List<int> values_int = new List<int>();
        foreach (var current_value in values)
        {
            values_int.Add((int) current_value);
        }
        int[] keys_array = keys.ToArray();
        int[] values_array = values_int.ToArray();
        
        return new SerializedIntIntDict{keys = keys_array, values = values_array};
    }
    
    
    public Dictionary<int, AccessType> ReconstructIntAccessTypeDict(SerializedIntIntDict serializedInput)
    {
        Dictionary<int,AccessType> reconstructedDict = new Dictionary<int, AccessType>();

        
        for (int index = 0; index < serializedInput.keys.Length; index++)
        {
            reconstructedDict.Add(serializedInput.keys[index], (AccessType) serializedInput.values[index]);
        }
        
        return reconstructedDict;
    }

    
    
    
    
    public SerializedIntHashSetIntDict SerializeIntHashSetIntDict(Dictionary<int,HashSet<int>> inputDict)
    {
        var listOfKeyValuePairs = inputDict.ToList();
        var keys = listOfKeyValuePairs.Select(kvp => kvp.Key);
        var values = listOfKeyValuePairs.Select(kvp => kvp.Value);
        int[] keys_array = keys.ToArray();
        List<SerializedHashSetInt> valuesList = new List<SerializedHashSetInt>();

        foreach (HashSet<int> hashSet in values)
        {
            int[] intArray = new int[hashSet.Count];
            hashSet.CopyTo(intArray);
            valuesList.Add(new SerializedHashSetInt{collection = intArray});
        }

        SerializedHashSetInt[] values_array = valuesList.ToArray();
        
        return new SerializedIntHashSetIntDict{keys = keys_array, values = values_array};
    }
    
    public Dictionary<int, HashSet<int>> ReconstructIntHashSetIntDict(SerializedIntHashSetIntDict serializedInput)
    {
        Dictionary<int,HashSet<int>> reconstructedDict = new Dictionary<int, HashSet<int>>();
        
        for (int index = 0; index < serializedInput.keys.Length; index++)
        {
            HashSet<int> value = new HashSet<int>(serializedInput.values[index].collection); 
            reconstructedDict.Add(serializedInput.keys[index], value);
        }
        
        return reconstructedDict;
    }
    
    
    
}


[Serializable]
public struct SerializedIntIntDict : INetworkSerializable
{
    public int[] keys;
    public int[] values;
    
    // INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref keys);
        serializer.SerializeValue(ref values);
    }
    // ~INetworkSerializable

}



[Serializable]
public struct SerializedIntHashSetIntDict : INetworkSerializable
{
    public int[] keys;
    public SerializedHashSetInt[] values;
    
    // INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref keys);
        serializer.SerializeValue(ref values);
    }
    // ~INetworkSerializable

}

[Serializable]
public struct SerializedHashSetInt : INetworkSerializable
{
    public int[] collection;
    
    // INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref collection);
    }
    // ~INetworkSerializable
}



