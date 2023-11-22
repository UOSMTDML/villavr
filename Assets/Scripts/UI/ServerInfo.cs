using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ServerInfo : MonoBehaviour
{
    [SerializeField] private GameObject serverInfoText;

    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        // Move to OnEnable to guarantee coroutine start after gameobject had been disabled 
        StartCoroutine(UpdateInformationDisplay(0.5f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    // Coroutine to update information 
    private IEnumerator UpdateInformationDisplay(float waitTime)
    {
        while (true)
        {
            //
            // --- Update Server Info 
            // 
            
            string infoText = "\n\n\nIP: " +
                              NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address
                              + "\nPort: " +
                              NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port
                              + "\nServer Client ID: " +
                              NetworkManager.Singleton.LocalClientId
                              + "\n\nConnected Client IDs: ";
            foreach (var idKvp in NetworkManager.Singleton.ConnectedClients)
            {
                if (idKvp.Key == NetworkManager.Singleton.ConnectedClients.Last().Key)
                {
                    infoText += idKvp.Key.ToString();
                }
                else
                {
                    infoText += idKvp.Key.ToString() + ", ";
                }
                
            }
            
            serverInfoText.GetComponent<TextMeshProUGUI>().text = infoText;
            
            yield return new WaitForSeconds(waitTime);
        }
        
    }
    
    
    
    
    
}
