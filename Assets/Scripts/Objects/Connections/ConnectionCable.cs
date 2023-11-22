using System.Collections;
using System.Collections.Generic;
using Autohand;
using Unity.Collections;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectionCable : InteractableObject 
{

    // Inherited from InteractableObject 
    //protected ObjectInfo objectInfo;
    //protected bool isModifiable; 
    
    [SerializeField] private Connector firstConnector;
    [SerializeField] private Connector secondConnector;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private bool useBezierCurver;

    public NetworkVariable<int> firstConnectorConnectedToId; 
    public NetworkVariable<bool> firstConnectorIsConnected; 
    public NetworkVariable<int> secondConnectorConnectedToId; 
    public NetworkVariable<bool> secondConnectorIsConnected;

    public NetworkVariable<int> firstConnectorLastGrabbedByPlayerId;
    public NetworkVariable<int> secondConnectorLastGrabbedByPlayerId;
    public NetworkVariable<bool> firstConnectorCurrentlyGrabbed;
    public NetworkVariable<bool> secondConnectorCurrentlyGrabbed;

    private PlacePoint firstConnectorPlacePoint;
    private PlacePoint secondConnectorPlacePoint;

    private BezierCurver bezierCurver; 
    private Transform startPositionTransform;
    private Transform endPositionTransform;
    private bool isReady = false;
    
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (useBezierCurver)
        {
            bezierCurver = GetComponent<BezierCurver>();
            StartBezierCurver();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
        
        // Update Position if not using BezierCurver 
        if (isReady)
        {
            lineRenderer.SetPosition(0, startPositionTransform.position);
            lineRenderer.SetPosition(1, endPositionTransform.position);
        }
    }

    public override void OnNetworkSpawn()
    {
        
        firstConnectorIsConnected.OnValueChanged += (bool prevVal, bool newVal) =>
        {
            string prev = prevVal ? "connected" : "disconnected";
            string upd = newVal ? "connected" : "disconnected";
            
            
            Debug.Log("[ConnectionCable] First Connector is now " + upd + ". Previously: " + prev + ".");
            
            
            // Check if gameobject still exists 
            if (isActiveAndEnabled)
            {
                // Update delayed to make sure values are updated (isConnected is updated after actual values)
                StartCoroutine(StartUpdateConnectorsDelayed(FirstOrSecondConnector.First, prevVal, newVal));
            }

        };
        secondConnectorIsConnected.OnValueChanged += (bool prevVal, bool newVal) =>
        {
            string prev = prevVal ? "connected" : "disconnected";
            string upd = newVal ? "connected" : "disconnected";
            Debug.Log("[ConnectionCable] Second Connector is now " + upd + ". Previously: " + prev + ".");
            
            // Check if gameobject still exists 
            if (isActiveAndEnabled)
            {
                // Update delayed to make sure values are updated (isConnected is updated after actual values)
                StartCoroutine(StartUpdateConnectorsDelayed(FirstOrSecondConnector.Second, prevVal, newVal));
            }
           
        };

        

    }


    

    private void UpdateConnectors(FirstOrSecondConnector connector, bool prevConnected, bool nowConnected)
    {
        
        // Sanity check, values changed 
        if (prevConnected == nowConnected)
        {
            Debug.Log("[ConnectionCable] UpdateConnectors: Previous and new Value of " + connector +  " are the same: " + prevConnected + ". Should be different.");
            return;
        }

        if (prevConnected)
        {
            DisconnectConnector(connector);
        }
        else // now connected
        {
           
            ConnectConnector(connector);

        }
        
    }

    private void DisconnectConnector(FirstOrSecondConnector connector)
    {
        // Uncritical when certain components run twice (once when actually removing and second time when value change is recognized)
        
        if (connector == FirstOrSecondConnector.First)
        {
            if (firstConnectorPlacePoint != null)
            {
               
                ToggleConnectorNetworkTransform(true, FirstOrSecondConnector.First);
                

                // Remove connector from place point 
                firstConnectorPlacePoint.Remove(firstConnector.GetComponent<Grabbable>(), true);

            }
        }
        else
        {
            if (secondConnectorPlacePoint != null)
            {
                
                ToggleConnectorNetworkTransform(true, FirstOrSecondConnector.Second);
                
                
                // Remove connector from place point 
                secondConnectorPlacePoint.Remove(firstConnector.GetComponent<Grabbable>(), true);

            }
        }
        
        
        // Update PlacePoint 
        firstConnectorPlacePoint = null; 

    }

    private void ConnectConnector(FirstOrSecondConnector connector)
    {
        // Uncritical when certain components run twice (once when actually placed and second time when value change is recognized)


        
        
        if (connector == FirstOrSecondConnector.First)
        {
            
            
            // KEEP DEACTIVATED 
            //ToggleConnectorNetworkTransform(false, FirstOrSecondConnector.First);
            
            
            // Get PlacePoint and Place connector 
            GameObject connectionSocket = NetworkSpawner.Singleton.GetSpawnedObjectById(firstConnectorConnectedToId.Value);
            PlacePoint connectionPlacePoint = connectionSocket.GetComponent<Connection>().GetPlacePoint();
            
            // Check that connector is not already placed 
            if (connectionPlacePoint != null)
            {
                if (connectionPlacePoint.GetPlacedObject() != firstConnector.GetComponent<Grabbable>())
                {
                    connectionPlacePoint.Place(firstConnector.GetComponent<Grabbable>(), true);
                }
            }
            

            // Update PlacePoint 
            firstConnectorPlacePoint = connectionPlacePoint;
        }

        
        else // Second connector 
        {
            
            
            // KEEP DEACTIVATED
            //ToggleConnectorNetworkTransform(false, FirstOrSecondConnector.Second);
           

            // Get PlacePoint and Place connector 
            GameObject connectionSocket =
                NetworkSpawner.Singleton.GetSpawnedObjectById(secondConnectorConnectedToId.Value);
            Debug.Log(connectionSocket);
            PlacePoint connectionPlacePoint = connectionSocket.GetComponent<Connection>().GetPlacePoint();

            // Check that connector is not already placed 
            if (connectionPlacePoint != null)
            { if (connectionPlacePoint.GetPlacedObject() != secondConnector.GetComponent<Grabbable>())
                {
                    connectionPlacePoint.Place(secondConnector.GetComponent<Grabbable>(), true);
                }
            }

            // Update PlacePoint 
            secondConnectorPlacePoint = connectionPlacePoint;
        }
        
    }

    
    IEnumerator StartUpdateConnectorsDelayed(FirstOrSecondConnector connector, bool prevConnected, bool nowConnected, float time = 0.1f)
    {
        yield return new WaitForSeconds(time);
        UpdateConnectors(connector, prevConnected, nowConnected);

    }
    
    
    // Calls Server RPC to broadcast to Clients 
    private void ToggleConnectorNetworkTransform(bool toggleOn, FirstOrSecondConnector connector)
    {
        ToggleConnectorNetworkTransform_ServerRpc(toggleOn, connector);
    }
    
    
    // Wrapper Method to broadcast visibility change to all clients 
    [ServerRpc(RequireOwnership = false)]
    private void ToggleConnectorNetworkTransform_ServerRpc(bool toggleOn, FirstOrSecondConnector connector)
    {
        ToggleConnectorNetworkTransform_ClientRpc(toggleOn, connector);
    }

    [ClientRpc]
    private void ToggleConnectorNetworkTransform_ClientRpc(bool toggleOn, FirstOrSecondConnector connector)
    {
        Debug.Log("[Connection Cable] Toggle Connector Network transform ClientRPC: Toggle on " + toggleOn + ", connector " + connector);
        
        if (toggleOn)
        {
            if (connector == FirstOrSecondConnector.First)
            {
                // Activate network transform of connector
                ClientNetworkTransform clientNetworkTransform = firstConnector.GetComponent<ClientNetworkTransform>();
                clientNetworkTransform.SyncPositionX = true;
                clientNetworkTransform.SyncPositionY = true;
                clientNetworkTransform.SyncPositionZ = true;
                clientNetworkTransform.SyncRotAngleX = true; 
                clientNetworkTransform.SyncRotAngleY = true;
                clientNetworkTransform.SyncRotAngleZ = true;
            }
            else // second 
            {
                // Activate network transform of connector
                ClientNetworkTransform clientNetworkTransform = secondConnector.GetComponent<ClientNetworkTransform>();
                clientNetworkTransform.SyncPositionX = true;
                clientNetworkTransform.SyncPositionY = true;
                clientNetworkTransform.SyncPositionZ = true;
                clientNetworkTransform.SyncRotAngleX = true;
                clientNetworkTransform.SyncRotAngleY = true;
                clientNetworkTransform.SyncRotAngleZ = true;
            }
            
        }
        else // toggle Off 
        {
            if (connector == FirstOrSecondConnector.First)
            {
                // Disable network transform 
                ClientNetworkTransform clientNetworkTransform = firstConnector.GetComponent<ClientNetworkTransform>();
                clientNetworkTransform.SyncPositionX = false;
                clientNetworkTransform.SyncPositionY = false;
                clientNetworkTransform.SyncPositionZ = false;
                clientNetworkTransform.SyncRotAngleX = false; 
                clientNetworkTransform.SyncRotAngleY = false;
                clientNetworkTransform.SyncRotAngleZ = false;
            }
            else // second 
            {
                // Disable network transform 
                ClientNetworkTransform clientNetworkTransform = secondConnector.GetComponent<ClientNetworkTransform>();
                clientNetworkTransform.SyncPositionX = false;
                clientNetworkTransform.SyncPositionY = false;
                clientNetworkTransform.SyncPositionZ = false;
                clientNetworkTransform.SyncRotAngleX = false; 
                clientNetworkTransform.SyncRotAngleY = false;
                clientNetworkTransform.SyncRotAngleZ = false;
            }
        }
        
    }


    
    
    // Visualize interactability 
    public override void UpdateInteractableModifiable(bool canBeModified)
    {
        // Cable is completely passive in terms of modifiability 
        return;
    }

    private void StartBezierCurver()
    {
        startPositionTransform = firstConnector.GetCableConnectionPoint();
        endPositionTransform = secondConnector.GetCableConnectionPoint();
        
        bezierCurver.Setup(startPositionTransform, endPositionTransform, lineRenderer);
    }
    
    
    private void SetStartEndTransform()
    { 
        startPositionTransform = firstConnector.GetCableConnectionPoint();
        endPositionTransform = secondConnector.GetCableConnectionPoint();
        
        isReady = true;
    }

    
    public void SetFirstConnectorLastGrabbedByPlayerId(int id)
    {
        SetFirstConnectorLastGrabbedByPlayerId_ServerRpc(id);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetFirstConnectorLastGrabbedByPlayerId_ServerRpc(int id)
    {
        firstConnectorLastGrabbedByPlayerId.Value = id;
    }
    
    
    public void SetSecondConnectorLastGrabbedByPlayerId(int id)
    {
        SetSecondConnectorLastGrabbedByPlayerId_ServerRpc(id);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetSecondConnectorLastGrabbedByPlayerId_ServerRpc(int id)
    {
        secondConnectorLastGrabbedByPlayerId.Value = id;
    }
    
    public void SetFirstConnectorCurrentlyGrabbed(bool grabbed)
    {
        SetFirstConnectorCurrentlyGrabbed_ServerRpc(grabbed);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetFirstConnectorCurrentlyGrabbed_ServerRpc(bool grabbed)
    {
        firstConnectorCurrentlyGrabbed.Value = grabbed;
    }
    
    public void SetSecondConnectorCurrentlyGrabbed(bool grabbed)
    {
        SetSecondConnectorCurrentlyGrabbed_ServerRpc(grabbed);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetSecondConnectorCurrentlyGrabbed_ServerRpc(bool grabbed)
    {
        secondConnectorCurrentlyGrabbed.Value = grabbed;
    }
    
    

    public void SetFirstConnectorConnectionState(bool isConnected, int connectedToId)
    {
        SetFirstConnectorConnectionState_ServerRpc(isConnected, connectedToId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetFirstConnectorConnectionState_ServerRpc(bool isConnected, int connectedToId)
    {
        firstConnectorConnectedToId.Value = connectedToId;
        firstConnectorIsConnected.Value = isConnected;
    }
    
    public void SetSecondConnectorConnectionState(bool isConnected, int connectedToId)
    {
        Debug.Log("███████ SetSecondConnectorConnectionState");
        
        SetSecondConnectorConnectionState_ServerRpc(isConnected, connectedToId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetSecondConnectorConnectionState_ServerRpc(bool isConnected, int connectedToId)
    {
        Debug.Log("███████ SetSecondConnectorConnectionState_ServerRpc");
        secondConnectorConnectedToId.Value = connectedToId;
        secondConnectorIsConnected.Value = isConnected;
    }
    

    public bool GetIsFirstConnectorConnected()
    {
        return firstConnectorIsConnected.Value;
    }

    public bool GetIsSecondConnectorConnected()
    {
        return secondConnectorIsConnected.Value;
    }

    public int GetFirstConnectorConnectedToId()
    {
        return firstConnectorConnectedToId.Value;
    }
    
    public int GetSecondConnectorConnectedToId()
    {
        return secondConnectorConnectedToId.Value;
    }
    
    public int GetFirstConnectorLastGrabbedByPlayerId()
    {
        return firstConnectorLastGrabbedByPlayerId.Value;
    }
    
    public int GetSecondConnectorLastGrabbedByPlayerId()
    {
        return secondConnectorLastGrabbedByPlayerId.Value;
    }
    


    // Calls Server Rpc that class Client RPC for broadcast 
    public void ChangeConnectorTranslucency(FirstOrSecondConnector connector, bool isTranslucentRatherThanOpaque)
    {
        // Translate to number 
        int connectorNumber;
        if (connector == FirstOrSecondConnector.First)
        {
            connectorNumber = 1;
        }
        else
        {
            connectorNumber = 2;
        }
        
        
        
        // Check if gameobject still exists 
        if (isActiveAndEnabled)
        {
            StartCoroutine(ChangeConnectorTranslucencyCoroutine(connectorNumber, isTranslucentRatherThanOpaque));
        }
    }

    private IEnumerator ChangeConnectorTranslucencyCoroutine(int connectorNumber, bool isTranslucentRatherThanOpaque)
    {
        while (!IsSpawned)
        {
            yield return 0.1f;
        }
        ChangeConnectorTranslucency_ServerRpc(connectorNumber, isTranslucentRatherThanOpaque);
    }
    
    
    // Wrapper Method to broadcast translucency change to all clients 
    [ServerRpc(RequireOwnership = false)]
    private void ChangeConnectorTranslucency_ServerRpc(int connectorNumber, bool isTranslucentRatherThanOpaque)
    {
        ChangeConnectorTranslucency_ClientRpc(connectorNumber, isTranslucentRatherThanOpaque);
    }
    
    [ClientRpc]
    private void ChangeConnectorTranslucency_ClientRpc(int connectorNumber, bool isTranslucentRatherThanOpaque)
    {
        
    // Translate back to connector 
    FirstOrSecondConnector connector;
    if (connectorNumber == 1)
    {
        connector = FirstOrSecondConnector.First;
    }
    else
    {
        connector = FirstOrSecondConnector.Second;
    }
    
        
    if (connector == FirstOrSecondConnector.First)
    {
        if (isTranslucentRatherThanOpaque)
        {
            firstConnector.SetTranslucency(true);
        }
        else
        {
            firstConnector.SetTranslucency(false);
        }
        
    }
    else // second 
    {
        if (isTranslucentRatherThanOpaque)
        {
            secondConnector.SetTranslucency(true);
        }
        else
        {
            secondConnector.SetTranslucency(false);
        }
    }
    }


    // Calls Server RPC to broadcast to Clients 
    public void ChangeConnectorVisibility(FirstOrSecondConnector connector, bool isVisible)
    {
        // Translate to number 
        int connectorNumber;
        if (connector == FirstOrSecondConnector.First)
        {
            connectorNumber = 1;
        }
        else
        {
            connectorNumber = 2;
        }

        // Check if gameobject still exists 
        if (isActiveAndEnabled)
        {
            StartCoroutine(ChangeConnectorVisibilityCoroutine(connectorNumber, isVisible));
        }
        
       

    }

    private IEnumerator ChangeConnectorVisibilityCoroutine(int connectorNumber, bool isVisible)
    {
        while (!IsSpawned)
        {
            yield return 0.1f;
        }
        ChangeConnectorVisibility_ServerRpc(connectorNumber, isVisible);
    }
    
    
    // Wrapper Method to broadcast visibility change to all clients 
    [ServerRpc(RequireOwnership = false)]
    private void ChangeConnectorVisibility_ServerRpc(int connectorNumber, bool isVisible)
    {
        
        ChangeConnectorVisibility_ClientRpc(connectorNumber, isVisible);

    }

    [ClientRpc]
    private void ChangeConnectorVisibility_ClientRpc(int connectorNumber, bool isVisible)
    {
        
        // Translate back to connector 
        FirstOrSecondConnector connector;
        if (connectorNumber == 1)
        {
            connector = FirstOrSecondConnector.First;
        }
        else
        {
            connector = FirstOrSecondConnector.Second;
        }
        
        
        Connector changeConnector;
        bool setKinematic;
        bool enableCollider;


        if (connector == FirstOrSecondConnector.First)
        {
            changeConnector = firstConnector;
        }
        else
        {
            changeConnector = secondConnector;
        }

        if (isVisible)
        {
            setKinematic = false;
            enableCollider = true;
        }
        else
        {
            setKinematic = true;
            enableCollider = false;
        }
        
        changeConnector.SetVisibility(isVisible);
        changeConnector.GetComponent<Rigidbody>().isKinematic = setKinematic;
        foreach (Collider coll in changeConnector.GetComponentsInChildren<Collider>())
        {
            coll.enabled = enableCollider;
        }

    }
    
    
    
    // Calls Server RPC to broadcast to Clients 
    public void ChangeLinerendererVisibility(bool isVisible)
    {
        // Check if gameobject still exists 
        if (isActiveAndEnabled)
        {
            StartCoroutine(ChangeLinerendererVisibilityCoroutine(isVisible));
        }
    }
    private IEnumerator ChangeLinerendererVisibilityCoroutine(bool isVisible)
    {
        while (!IsSpawned)
        {
            yield return 0.1f;
        }
        ChangeLinerendererVisibility_ServerRpc(isVisible);
    }
    
    
    // Wrapper Method to broadcast visibility change to all clients 
    [ServerRpc(RequireOwnership = false)]
    private void ChangeLinerendererVisibility_ServerRpc(bool isVisible)
    {
        ChangeLinerendererVisibility_ClientRpc(isVisible);
    }

    [ClientRpc]
    private void ChangeLinerendererVisibility_ClientRpc(bool isVisible)
    {
        lineRenderer.enabled = isVisible;
    }
    
    
    
    private void SetLinerendererMaterial(Material material)
    {
        lineRenderer.material = material;
    }

    private void SetLinerendererWidth(float width)
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    public Connector GetFirstConnector()
    {
        return firstConnector;
    }
    
    public Connector GetSecondConnector()
    {
        return secondConnector;
    }

    public int GetUniqueObjectId()
    {
        return objectInfo.uniqueObjectId.Value;
    }
    
    public void DestroyObject()
    {
        
        Debug.Log("[ConnectionCable] Destroy Object.");
        
        // Call ServerRPC to make sure it gets destroied on all clients 
        Destroy_ServerRPC();
        
        // Remove ID in network spawner dict 
        NetworkSpawner.Singleton.RemoveDespawnedId(objectInfo.uniqueObjectId.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Destroy_ServerRPC()
    {
        GetComponent<NetworkObject>().Despawn(true);
        //Destroy(this.gameObject);
        
        // Remove ID in network spawner dict 
        NetworkSpawner.Singleton.RemoveDespawnedId(objectInfo.uniqueObjectId.Value);
    }
    

}
