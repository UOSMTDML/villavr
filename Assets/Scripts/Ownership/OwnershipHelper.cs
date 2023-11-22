using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OwnershipHelper : NetworkBehaviour
{

    public int currentOwner = -9;
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("UpdateCurrentOwnerDisplay");
    }


    public void SetLocalClientAsOwner()
    {
        UpdateOwnership_ServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    public void SetOwnership(ulong newOwnerClientId)
    {
        UpdateOwnership_ServerRpc(newOwnerClientId);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateOwnership_ServerRpc(ulong newOwnerClientId)
    {
        Debug.Log("[OwnershipHelper] For " + name + " New Owner ClientID: " + newOwnerClientId);
        GetComponent<NetworkObject>().ChangeOwnership(newOwnerClientId);        
    }


    private IEnumerator UpdateCurrentOwnerDisplay()
    {
        while (true)
        {
            currentOwner = (int)GetComponent<NetworkObject>().OwnerClientId;
            yield return new WaitForSeconds(2);
        }
    }
    
    
    
}
