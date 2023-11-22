using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Connector : NetworkBehaviour
{
    
    /*
     *  For Network-Behavior, Connector is only interacted with via ConnectionCable.
     */
    
    

    // Reference to Connection Cable of this Connector
    [SerializeField] private ConnectionCable connectionCable;
    
    // Store connection point
    [SerializeField] private Transform cableConnectionPoint;
    
    // Store materials 
    [SerializeField] private Material opaqueMetalMaterial;
    [SerializeField] private Material translucentMetalMaterial;
    
    // Store whether this is the first or second connector of the cable
    [Header("Connector Identity")]
    [SerializeField] private bool isFirstConnector;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
        // Update last grabbed when anybody is grabbing 
        // Grab events only happen locally, i.e. set local client id 
        GetComponent<Grabbable>().OnBeforeGrabEvent += (Hand hand, Grabbable grabbable ) =>
        {
            
            SetConnectorLastGrabbedByPlayerId((int)NetworkManager.Singleton.LocalClientId);
            
        };
        
        // Update state of currently grabbing 
        GetComponent<Grabbable>().OnGrabEvent += (Hand Hand, Grabbable grabbable) =>
        {
            SetConnectorCurrentlyGrabbed(true);
        };
        GetComponent<Grabbable>().OnReleaseEvent += (Hand Hand, Grabbable grabbable) =>
        {
            SetConnectorCurrentlyGrabbed(false);
        };
        
        
        
        
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    
    
    
    public void SetTranslucency(bool translucent)
    {
        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            if (translucent)
            {
                childRenderer.material = translucentMetalMaterial;
            }
            else
            {
                childRenderer.material = opaqueMetalMaterial;
            }
            
        }
    }
    
    public void SetVisibility(bool visible)
    {
        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            if (visible)
            {
                childRenderer.enabled = true;
            }
            else
            {
                childRenderer.enabled = false;
            }
            
        }
    }

    


    public ConnectionCable GetConnectionCable()
    {
        return connectionCable;
    }
    
    
    
    public Transform GetCableConnectionPoint()
    {
        return cableConnectionPoint;
    }


    public void DestroyCable()
    {
        connectionCable.DestroyObject();
    }
    
    
    
    public bool GetOtherEndIsConnected()
    {
        if (isFirstConnector)
        {
            return connectionCable.GetIsSecondConnectorConnected();
        }
        else
        {
            return connectionCable.GetIsFirstConnectorConnected();
        }
        
    }

    public bool GetIsConnected()
    {
        if (isFirstConnector)
        {
            return connectionCable.GetIsFirstConnectorConnected();
        }
        else
        {
            return connectionCable.GetIsSecondConnectorConnected();
        }
        
    }
    
    public int GetOtherEndConnectedToId()
    {
        if (isFirstConnector)
        {
            return connectionCable.GetSecondConnectorConnectedToId();
        }
        else
        {
            return connectionCable.GetFirstConnectorConnectedToId();
        }
        
    }
    
    public int GetConnectedToId()
    {
        if (isFirstConnector)
        {
            return connectionCable.GetFirstConnectorConnectedToId();
        }
        else
        {
            return connectionCable.GetSecondConnectorConnectedToId();
        }
        
    }
    
    
    public int GetLastGrabbedByPlayerId()
    {
        if (isFirstConnector)
        {
            return connectionCable.GetFirstConnectorLastGrabbedByPlayerId();
        }
        else
        {
            return connectionCable.GetSecondConnectorLastGrabbedByPlayerId();
        }
        
    }
    
    
    
    
    
    public void SetConnectorConnectionState(bool isConnected, int connectedToId)
    {
        if (isFirstConnector)
        {
            connectionCable.SetFirstConnectorConnectionState(isConnected, connectedToId);
        }
        else
        {
            connectionCable.SetSecondConnectorConnectionState(isConnected, connectedToId);
        }
        
    }
    
  
    
    public void SetConnectorLastGrabbedByPlayerId(int id)
    {
        if (isFirstConnector)
        {
            connectionCable.SetFirstConnectorLastGrabbedByPlayerId(id);
        }
        else
        {
            connectionCable.SetSecondConnectorLastGrabbedByPlayerId(id);
        }
        
    }
    
    public void SetConnectorCurrentlyGrabbed(bool grabbed)
    {
        if (isFirstConnector)
        {
            connectionCable.SetFirstConnectorCurrentlyGrabbed(grabbed);
        }
        else
        {
            connectionCable.SetSecondConnectorCurrentlyGrabbed(grabbed);
        }
        
    }

}


public enum FirstOrSecondConnector
{
    First,
    Second
}