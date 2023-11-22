using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransformHelper : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += obj =>
            {
                
                // Set positional offset threshold to zero to guarantee loading of initial position, when client joins, reset after some time 
                StartCoroutine("SetUpdateRate");
            };
        }
        
    }


    private IEnumerator SetUpdateRate()
    {
        
        float prevThresh = GetComponent<ClientNetworkTransform>().PositionThreshold;
        GetComponent<ClientNetworkTransform>().PositionThreshold = 0;
        yield return new WaitForSeconds(2);
        GetComponent<ClientNetworkTransform>().PositionThreshold = prevThresh;
    }
    
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
