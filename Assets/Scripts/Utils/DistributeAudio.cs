using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class DistributeAudio : NetworkBehaviour
{

    // Events 
    public UnityEvent newAudioOnServerEvent;
    public UnityEvent newAudioOnClientEvent; 
    
    
    
    
    // Sending to Clients
    private string currentAudioSendToClientsIdentifier;
    private float[] currentAudioSendToClients;
    private List<float[]> currentAudioSendToClientsChunks = new List<float[]>();
    private int currentAudioSendToClientsSamplingFrequency; 
    private int currentAudioSendToClientNumberChannels;
    private Dictionary<int, float[]> fetchedChunksFromServer = new Dictionary<int, float[]>(); // chunk number : chunk data 
    private bool currentlyFetchingFromServer = false;
    private string currentAudioReceivedFromServerIdentifier;
    private int currentAudioReceivedFromServerSamplingFrequency;
    private int currentAudioReceivedFromServerNumberChannels;
    private float[] currentAudioReceivedFromServer;
    private List<float[]> currentAudioReceivedFromServerChunks = new List<float[]>();
   
    
    
    
    // Sending to Server 
    private string currentAudioSendToServerIdentifier;
    private float[] currentAudioSendToServer;
    private List<float[]> currentAudioSendToServerChunks = new List<float[]>();
    private int currentAudioSendToServerSamplingFrequency;
    private int currentAudioSendToServerNumberChannels;
    private Dictionary<int, float[]> fetchedChunksFromClient = new Dictionary<int, float[]>();
    private bool currentlyFetchingFromClient= false;
    private string currentAudioReceivedFromClientIdentifier;
    private int currentAudioReceivedFromClientSamplingFrequency;
    private int currentAudioReceivedFromClientNumberChannels;
    private float[] currentAudioReceivedFromClient;
    private List<float[]> currentAudioReceivedFromClientChunks = new List<float[]>();
    
  
    
    
    
    
   
    
    

    

    public void StartSendAudio()
    {
        
    }



    public void SendAudioToServer(float[] audioData, int samplingFrequency, int nChannels, string audioIdentifier)
    {
        // Store Data locally 
        currentAudioSendToServer = audioData;
        currentAudioSendToServerSamplingFrequency = samplingFrequency;
        currentAudioSendToServerNumberChannels = nChannels;
        currentAudioSendToServerIdentifier = audioIdentifier;
        currentAudioSendToServerChunks = DataSerializer.Singleton.SplitFloatArrayIntoChunks(audioData, ExperienceManager.Singleton.maxFloatsSendPerRpc);
        
        // Start sending 
        SignalFetchAudioToServer_ServerRpc(currentAudioSendToServerIdentifier, currentAudioSendToServerChunks.Count, currentAudioSendToServerSamplingFrequency, currentAudioSendToServerNumberChannels); 
        
    }


    public void SendAudioToClients(float[] audioData, int samplingFrequency, int nChannels, string audioIdentifier)
    {
        // Store Data locally 
        currentAudioSendToClients = audioData;
        currentAudioSendToClientsSamplingFrequency = samplingFrequency;
        currentAudioSendToClientNumberChannels = nChannels;
        currentAudioSendToClientsIdentifier = audioIdentifier;
        currentAudioSendToClientsChunks = DataSerializer.Singleton.SplitFloatArrayIntoChunks(audioData, ExperienceManager.Singleton.maxFloatsSendPerRpc);
        
        // Start sending 
        SignalFetchAudioToClient_ClientRpc(currentAudioSendToClientsIdentifier, currentAudioSendToClientsChunks.Count, currentAudioSendToClientsSamplingFrequency, currentAudioSendToClientNumberChannels);
    }



    public float[] GetAudioReceivedFromServer()
    {
        return currentAudioReceivedFromServer;
    }

    public string GetIdentifierReceivedFromServer()
    {
        return currentAudioReceivedFromServerIdentifier;
    }
    
    public int GetSamplingFrequencyReceivedFromServer()
    {
        return currentAudioReceivedFromServerSamplingFrequency;
    }

    public int GetNumberChannelsReceivedFromServer()
    {
        return currentAudioReceivedFromServerNumberChannels;
    }
    
    
    public float[] GetAudioReceivedFromClient()
    {
        return currentAudioReceivedFromClient;
    }
    
    public string GetIdentifierReceivedFromClient()
    {
        return currentAudioReceivedFromClientIdentifier;
    }
    
    public int GetSamplingFrequencyReceivedFromClient()
    {
        return currentAudioReceivedFromClientSamplingFrequency;
    }

    public int GetNumberChannelsReceivedFromClient()
    {
        return currentAudioReceivedFromClientNumberChannels;
    }
    
    


    
    
    
    //
    /////
    //////// Client to Server 
    /////
    //
    
    // Client signals to Server that recorded audio is available 
    [ServerRpc (RequireOwnership = false)]
    private void SignalFetchAudioToServer_ServerRpc(string audioIdentifier, int chunksToFetch, int samplingFreq, int nChannels, ServerRpcParams serverRpcParams = default)
    {
        // Reset fetched objects 
        fetchedChunksFromClient = new Dictionary<int, float[]>();
        
        // Start fetching 
        StartCoroutine(FetchAudioFromClient(audioIdentifier, chunksToFetch, samplingFreq, nChannels, serverRpcParams));
    }
    
    // Main Fetcher for Server
    private IEnumerator FetchAudioFromClient(string audioIdentifier, int chunksToFetch, int samplingFreq, int nChannels, ServerRpcParams serverRpcParams)
    {
        Debug.Log("[DistributeAudio] Fetching Audio from Client for Audio " + audioIdentifier.ToString());
        
        currentlyFetchingFromClient = true;
        
        
        // Get Client that invoked ServerRPC 
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        
        // Construct ClientRpcParams for sending Client 
        ClientRpcParams clientRpcParams =  new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { senderClientId }
            }
        };
        
        
        // Issue Fetching requests 
        for (int chunk = 0; chunk < chunksToFetch; chunk++)
        {
            FetchChunkFromClient_ClientRpc(audioIdentifier, chunk, clientRpcParams); // Ask for chunks
            yield return new WaitForSeconds(0.0001f);
        }
        
        // Wait until all data is transferred 
        while (fetchedChunksFromClient.Keys.Count < chunksToFetch)
        {
            yield return new WaitForSeconds(2.0f);
        }
        
        // Fetched everything, combine audio 
        
        // Sort chunk idxs 
        int[] sortedKeys = fetchedChunksFromClient.Keys.ToArray();
        Array.Sort(sortedKeys);
        int chunkSize = fetchedChunksFromClient[sortedKeys[0]].Length;
        int chunkIdx = 0;
        
        // Combine Audio 
        float[] combinedAudio = new float[(fetchedChunksFromClient.Keys.Count - 1) * fetchedChunksFromClient[sortedKeys[0]].Length + fetchedChunksFromClient[sortedKeys[sortedKeys.Length -1]].Length];
        foreach (int key in sortedKeys)
        {
            Array.Copy(fetchedChunksFromClient[key], 0 , combinedAudio, chunkIdx * chunkSize, fetchedChunksFromClient[key].Length);
            chunkIdx += 1;
        }
        
        // Update Combined Audio 
        currentAudioReceivedFromClient = combinedAudio;
        currentAudioReceivedFromClientIdentifier = audioIdentifier;
        currentAudioReceivedFromClientSamplingFrequency = samplingFreq;
        currentAudioReceivedFromClientNumberChannels = nChannels;
        currentAudioReceivedFromClientChunks = DataSerializer.Singleton.SplitFloatArrayIntoChunks(currentAudioReceivedFromClient, ExperienceManager.Singleton.maxFloatsSendPerRpc);

        
        // Done 
        Debug.Log("[DistributeAudio] Done fetching Audio from Client for Audio " + audioIdentifier.ToString());
        currentlyFetchingFromClient = false;
        newAudioOnServerEvent.Invoke();
        
    }
    
    
    
    // Server asks Client for new Audio 
    [ClientRpc]
    private void FetchChunkFromClient_ClientRpc(string audioIdentifier, int chunkToFetch, ClientRpcParams clientRpcParams = default)
    {
        
        // Send Data
        SendChunkToServer_ServerRpc(audioIdentifier, chunkToFetch, currentAudioSendToServerChunks[chunkToFetch]);


    }
    
    
    // Client sends new audio to Client 
    [ServerRpc (RequireOwnership = false)]
    private void SendChunkToServer_ServerRpc(string audioIdentifier, int chunkNumber, float[] chunkData, ServerRpcParams serverRpcParams = default)
    {
        // Store chunks 
        fetchedChunksFromClient[chunkNumber] = chunkData;
      
    }


    
    
    
    
    
    //
    /////
    //////// Server to Client 
    /////
    //
    
    

    

    // Server signals to Client that new audio is available 
    [ClientRpc]
    private void SignalFetchAudioToClient_ClientRpc(string audioIdentifier, int chunksToFetch, int samplingFreq, int nChannels, ClientRpcParams clientRpcParams = default)
    {
        // Reset fetched objects 
        fetchedChunksFromServer = new Dictionary<int, float[]>();
        
        // Start fetching 
        StartCoroutine(FetchAudioFromServer(audioIdentifier, chunksToFetch, samplingFreq, nChannels));
    }
    
    // Main Fetcher for Client
    private IEnumerator FetchAudioFromServer(string audioIdentifier, int chunksToFetch, int samplingFreq, int nChannels)
    {
        Debug.Log("[DistributeAudio] Fetching Audio from Server for Audio " + audioIdentifier.ToString());
        
        currentlyFetchingFromServer = true;
        
        // Issue Fetching requests 
        for (int chunk = 0; chunk < chunksToFetch; chunk++)
        {
            FetchChunkFromServer_ServerRpc(audioIdentifier, chunk); // Ask for chunks
            yield return new WaitForSeconds(0.0001f);
        }
        
        // Wait unitl all data is transferred 
        while (fetchedChunksFromServer.Keys.Count < chunksToFetch)
        {
            Debug.Log("Fetched so far " + fetchedChunksFromServer.Keys);
            yield return new WaitForSeconds(2.0f);
        }
        
        // Fetched everything, combine audio 
        
        // Sort chunk idxs 
        int[] sortedKeys = fetchedChunksFromServer.Keys.ToArray();
        Array.Sort(sortedKeys);
        int chunkSize = fetchedChunksFromServer[sortedKeys[0]].Length;
        int chunkIdx = 0;
        
        // Combine Audio 
        float[] combinedAudio = new float[(fetchedChunksFromServer.Keys.Count - 1) * fetchedChunksFromServer[sortedKeys[0]].Length + fetchedChunksFromServer[sortedKeys[sortedKeys.Length -1]].Length];
        foreach (int key in sortedKeys)
        {
            Array.Copy(fetchedChunksFromServer[key], 0 , combinedAudio, chunkIdx * chunkSize, fetchedChunksFromServer[key].Length);
            chunkIdx += 1;
        }
        
        // Update Combined Audio 
        currentAudioReceivedFromServer = combinedAudio;
        currentAudioReceivedFromServerIdentifier = audioIdentifier;
        currentAudioReceivedFromServerSamplingFrequency = samplingFreq;
        currentAudioReceivedFromServerNumberChannels = nChannels;
        currentAudioReceivedFromServerChunks = DataSerializer.Singleton.SplitFloatArrayIntoChunks(currentAudioReceivedFromServer, ExperienceManager.Singleton.maxFloatsSendPerRpc);

        
        // Done 
        currentlyFetchingFromServer = false;
        newAudioOnClientEvent.Invoke();
        Debug.Log("[DistributeAudio] Done fetching Audio from Server for Audio " + audioIdentifier.ToString());

        
    }

    
    
    
    // Client asks Server for new Audio 
    [ServerRpc(RequireOwnership = false)]
    private void FetchChunkFromServer_ServerRpc(string audioIdentifier, int chunkToFetch, ServerRpcParams serverRpcParams = default)
    {
        // Get Client that invoked ServerRPC 
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        
        // Construct ClientRpcParams for receiving Client 
        ClientRpcParams clientRpcParams =  new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { senderClientId }
            }
        };
        
        // Send Data
        SendChunkToClient_ClientRpc(audioIdentifier, chunkToFetch, currentAudioSendToClientsChunks[chunkToFetch], clientRpcParams); //currentAudioReceivedFromServerChunks[chunkToFetch], clientRpcParams);


    }
    
    
    // Server sends new audio to Client 
    [ClientRpc]
    private void SendChunkToClient_ClientRpc(string audioIdentifier, int chunkNumber, float[] chunkData, ClientRpcParams clientRpcParams = default)
    {
        // Store chunks 
        fetchedChunksFromServer[chunkNumber] = chunkData;
      
    }


}
