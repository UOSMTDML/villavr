using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Autohand;
using NaughtyAttributes;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectorPlacement : NetworkBehaviour
{
    [SerializeField] private Connection connection;
    [SerializeField] private GameObject cablePrefab;
    [SerializeField] private Grabbable dummyAllowGrabbable;
    [SerializeField] private Material connectionNotPossibleMaterial;
    [SerializeField] private Material connectionPossibleMaterial;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private GameObject[] colorableConnectorParts;
    [SerializeField] private bool skipLookingForRigidbody;
    
    private PlacePoint placePoint;
    private HandTriggerAreaEvents handTriggerAreaEvents;
    //private GameObject spawnedCableGameObject;
    
    
    private bool LOG_VERBOSE = true;


    void Awake()
    {
        placePoint = GetComponent<PlacePoint>();
        handTriggerAreaEvents = GetComponent<HandTriggerAreaEvents>();
        
        
        // Get Rigidbody for joint and set 
        if (!skipLookingForRigidbody)
        {
            Transform currentTransform = transform.parent;
            while (currentTransform.GetComponent<UpdateSurrogateTransformFromTransform>() == null)
            {
                if (currentTransform.parent != null)
                {
                    currentTransform = currentTransform.parent;
                }
                else
                {
                    break;
                }
            }

            if (currentTransform.GetComponent<UpdateSurrogateTransformFromTransform>() != null)
            {
                placePoint.placedJointLink = currentTransform.GetComponent<UpdateSurrogateTransformFromTransform>()
                    .GetSourceRigidbody();
            }

        }
        
    }



    // Start is called before the first frame update
    void Start()
    {
        
        
        
        // Called whenever hand gets close 
        handTriggerAreaEvents.HandEnter.AddListener((Hand hand) =>
        {
            if (LOG_VERBOSE)
            {
                Debug.Log("[ConnectorPlacement] HandEnter | Hand: " + hand + "; HeldGrabbable: " 
                          + hand.GetHeldGrabbable() + "; PlacePoint placed Object: " 
                          + placePoint.GetPlacedObject());
            }
            
            // Check if hand is grabbing something 
            // If not: Spawn cable with two connectors  
            if (hand.GetHeldGrabbable() != null)
            {
                // Handled in OnHighlight, because of slightly different activation radius 
                if (LOG_VERBOSE)
                {
                    Debug.Log("[ConnectorPlacement] HandEnter - Held Grabbable | ");
                }
            }
            else // Hand is not grabbing anything 
            {
                if (LOG_VERBOSE)
                {
                    Debug.Log("[ConnectorPlacement] HandEnter - No Held Grabbable | ");
                }

                // make sure that nothing is already placed 
                if (placePoint.GetPlacedObject() == null)
                {
                    if (LOG_VERBOSE)
                    {
                        Debug.Log("[ConnectorPlacement] HandEnter - Held Grabbable | No placed Object | ");
                    }

                    StartCoroutine(DelayedTranslucentSpawner());
                    
                }
                else
                { // Something is already placed
                    if (LOG_VERBOSE)
                    {
                        Debug.Log("[ConnectorPlacement] HandEnter - Held Grabbable | Placed Object Exists ");
                    }
                }
                 
                
            }
            

        });


        // Called when hand distances  
        handTriggerAreaEvents.HandExit.AddListener((Hand hand) =>
        {
            if (LOG_VERBOSE)
            {
                Debug.Log("[ConnectorPlacement] HandExit | Hand: " + hand + "; HeldGrabbable: "
                          + hand.GetHeldGrabbable() + "; PlacePoint placed Object: "
                          + placePoint.GetPlacedObject());
            }


            // Check if hand is grabbing something 
            if (hand.GetHeldGrabbable() != null)
            {
                if (LOG_VERBOSE)
                {
                    Debug.Log("[ConnectorPlacement] HandExit - Held Grabbable | ");
                }

                Grabbable grabbedObject = hand.GetHeldGrabbable();
                
                
                Debug.Log("[][][][][]");
                Debug.Log(grabbedObject);

                // Check if grabbed object is connector 
                // It's possible, that held connector is not connected with the one spawned, in that case, delete the spawned one 
                if (grabbedObject.GetComponent<Connector>() != null)
                {
                    
                    if (LOG_VERBOSE)
                    {
                        Debug.Log("[ConnectorPlacement] HandExit - Held Grabbable | Is Connector | ");
                    }

                    if (placePoint.GetPlacedObject() != null)
                    {
                        if (LOG_VERBOSE)
                        {
                            Debug.Log("[ConnectorPlacement] HandExit - Held Grabbable | Placed Object");
                        }

                        if (placePoint.GetPlacedObject().GetComponent<Connector>() != null)
                        {
                            if (LOG_VERBOSE)
                            {
                                Debug.Log(
                                    "[ConnectorPlacement] HandExit - Held Grabbable | Placed Object | Is Connector ");
                            }
                            
                            
                            Debug.Log("][][][[][][][[][]]");
                            Debug.Log("][][][[][][][[][]]");
                            Debug.Log("][][][[][][][[][]]");
                            Debug.Log(placePoint.GetPlacedObject().GetComponent<Connector>()
                                .GetIsConnected());
                            Debug.Log("][][][[][][][[][]]");
                            
                            
                            if (!placePoint.GetPlacedObject().GetComponent<Connector>()
                                    .GetIsConnected()) // check if placed connector is connected: if newly spawned connector, then placed connector does not have is connected attribute set to true 
                            {
                                if (LOG_VERBOSE)
                                {
                                    Debug.Log(
                                        "[ConnectorPlacement] HandExit - Held Grabbable | Placed Object | Is Connector | Is Not Connected  ");
                                }
            
                                placePoint.GetPlacedObject().GetComponent<Connector>().DestroyCable();
                            }
                            // Else: is connected: Do not destroy 
                        }
                    }

                }
                else // Grabbed object is something else: Despawn what is placed on place point 
                {
                    if (LOG_VERBOSE)
                    {
                        Debug.Log("[ConnectorPlacement] HandExit - Held Grabbable | Is Not Connector | ");
                    }

                    if (placePoint.GetPlacedObject() != null)
                    {
                        Destroy(placePoint.GetPlacedObject().gameObject);
                    }
                }
            }
            else // Not grabbing anything: Despawn currently placed connector if it is not connected 
            {
                if (LOG_VERBOSE)
                {
                    Debug.Log("[ConnectorPlacement] HandExit - No Held Grabbable ");
                }

                if (placePoint.GetPlacedObject() != null)
                {
                    if (LOG_VERBOSE)
                    {
                        Debug.Log("[ConnectorPlacement] HandExit - No Held Grabbable | Placed Object");
                    }

                    if (placePoint.GetPlacedObject().GetComponent<Connector>() != null)
                    {
                        if (LOG_VERBOSE)
                        {
                            Debug.Log(
                                "[ConnectorPlacement] HandExit - No Held Grabbable | Placed Object | Is Connector ");
                        }
                        
                        
                        if (!placePoint.GetPlacedObject().GetComponent<Connector>().GetIsConnected()) // check if placed connector is connected: if newly spawned connector, then placed connector does not have is connected attribute set to true 
                        {
                            if (LOG_VERBOSE)
                            {
                                Debug.Log(
                                    "[ConnectorPlacement] HandExit - No Held Grabbable | Placed Object | Is Connector | Is Not Connected  ");
                            }
                            

                            placePoint.GetPlacedObject().GetComponent<Connector>().DestroyCable();   
                        }
                        // Else: is connected: Do not destroy 
                    }
                    else // No connector placed, destroy whatever else is placed 
                    {
                        if (LOG_VERBOSE)
                        {
                            Debug.Log(
                                "[ConnectorPlacement] HandExit - No Held Grabbable | Placed Object | Is Not Connector ");
                        }
                        
                        Destroy(placePoint.GetPlacedObject());
                    }
                    
                }
                    
            }
        
            
            
            


        });

        
        // Define behavior for getting close to socket 
        // Only called when hand is grabbing something 
        placePoint.OnHighlight.AddListener((PlacePoint pp, Grabbable grabbable) =>
        {
            if (LOG_VERBOSE)
            {
                Debug.Log("[ConnectorPlacement] OnHighlight | Grabbable: "
                          + grabbable + "; PlacePoint placed Object: "
                          + placePoint.GetPlacedObject());
            }



            // Check if grabbed object is connector & check if connection would be possible
            if (grabbable.GetComponent<Connector>() != null)
            {

                if (LOG_VERBOSE)
                {
                    Debug.Log("[ConnectorPlacement] OnHighlight - Held Grabbable | Is Connector |  ");
                }


                // Get Connector 
                Connector approachingConnector = grabbable.GetComponent<Connector>();
                   
                // Get ID of other end of cable 
                int connectedToId = approachingConnector.GetOtherEndConnectedToId();
                
                // Check if connection is possible 
                bool isPossible = connection.IsConnectionPossible(connectedToId,
                    approachingConnector.GetOtherEndIsConnected());
            
                // Check if something is placed already 
                bool somethingIsPlaced = placePoint.GetPlacedObject() != null;

                // Connection is possible and nothing is placed already
                if (isPossible & !somethingIsPlaced)
                {
                    if (LOG_VERBOSE)
                    {
                        Debug.Log(
                            "[ConnectorPlacement] OnHighlight - Held Grabbable | Is Connector | Connection is Possible & nothing is Placed ");
                    }

                    // Highlight 
                    HighlightConnectionPossible();
                }
                else if (!isPossible & !somethingIsPlaced) // not possible 
                {
                    if (LOG_VERBOSE)
                    {
                        Debug.Log(
                            "[ConnectorPlacement] OnHighlight - Held Grabbable | Is Connector | Connection is not Possible & nothing is Placed ");
                    }

                    // Highlight
                    HighlightConnectionNotPossible();
                
                    // Deactivate ability to place connector
                    placePoint.onlyAllows.Add(dummyAllowGrabbable);
                
                }
                // else: do not highlight when something is placed already 
            }
            else // grabbed object is not connector
            {
                if (LOG_VERBOSE)
                {
                    Debug.Log("[ConnectorPlacement] OnHighlight - Held Grabbable | Is Not Connector ");
                }

                // Deactivate ability to place object
                placePoint.onlyAllows.Add(dummyAllowGrabbable);
            }
            
        });
        


        // Define behavior for distancing from connection socket 
        // Only called when hand is grabbing something 
        placePoint.OnStopHighlight.AddListener((PlacePoint pp, Grabbable grabbable) =>
        {
            if (LOG_VERBOSE)
            {
                Debug.Log("[ConnectorPlacement] OnStopHighlight | Grabbable: "
                          + grabbable + "; PlacePoint placed Object: "
                          + placePoint.GetPlacedObject());
            }


            // Remove highlight of connector 
            SetDefaultColor();
            
            
            // Remove possible only allow blocks
            placePoint.onlyAllows.Clear();
            
        });
        
        
        // Define behavior for placing connector 
        // Only called when hand is grabbing something 
        this.placePoint.OnPlaceAugmentedEvent += (placePoint, grabbable, programmaticallyStarted) => 
        {

            
            if (LOG_VERBOSE)
            {
                Debug.Log("[ConnectorPlacement] OnPlace | Grabbable: "
                          + grabbable + "; PlacePoint placed Object: "
                          + placePoint.GetPlacedObject() 
                          + "; Programmatically Started: " 
                          + programmaticallyStarted);
            }
            
            
            
            
            // Try to set RigidBody of placed object non-kinematic, to make it move when jointed to RigidBody of PlacePoint 
            if (grabbable.GetComponent<Rigidbody>() != null)
            {
                grabbable.GetComponent<Rigidbody>().isKinematic = false;
            }
            
            
            
            if (programmaticallyStarted)
            {
                return; // Do not do anything besides placing, if placing was called via code; for remote machines 
            }
            
            

            // Check if held object is connector 
            if (grabbable.GetComponent<Connector>() == null)
            {
                if (LOG_VERBOSE)
                {
                    Debug.Log(
                        "[ConnectorPlacement] OnPlace: Tried to place Grabbable that is not connector. Not allowed!");
                }
            }
            else // Placed object is connector  
            {
                if (LOG_VERBOSE)
                {
                    Debug.Log("[ConnectorPlacement] OnPlace - Held Grabbable | Is Connector ");
                }

                // Get Connector 
                Connector placedConnector = grabbable.GetComponent<Connector>();
                
                // Get connected object id on other end 
                int otherEndObjectId = placedConnector.GetOtherEndConnectedToId();
                
                // Set connector's state (is connected & to id) 
                placedConnector.SetConnectorConnectionState(true, connection.GetUniqueObjectId());

                if (LOG_VERBOSE)
                {
                    Debug.Log("[ConnectorPlacement] OnPlace: Other end Object ID: " + otherEndObjectId +
                              "; Own local Connection Socket ID: " + connection.GetUniqueObjectId());
                }

                // Check if object id on other end is not own: Establish a connection 
                if (otherEndObjectId != connection.GetUniqueObjectId())
                {
                    if (LOG_VERBOSE)
                    {
                        Debug.Log(
                            "[ConnectorPlacement] OnPlace - Held Grabbable | Is Connector | Other End ID is different from local connection socket ID ");
                    }

                    // Check if other end is actually connected 
                    if (placedConnector.GetOtherEndIsConnected())
                    {
                        if (LOG_VERBOSE)
                        {
                            Debug.Log(
                                "[ConnectorPlacement] OnPlace - Held Grabbable | Is Connector | Other End ID is different from local connection socket ID | Other End is Connected ");
                        }
                        
                        
                        // Establish connection 
                        connection.EstablishOutgoingConnection(otherEndObjectId);
                    }
                    else // Other end is not connected 
                    {

                        if (LOG_VERBOSE)
                        {
                            Debug.Log(
                                "[ConnectorPlacement] OnPlace - Held Grabbable | Is Connector | Other End ID is different from local connection socket ID | Other End is not Connected ");
                        }
                    }
                    
                }
                 
                else
                {
                    // Is connected to own socket
                    if (LOG_VERBOSE)
                    {
                        Debug.Log(
                            "[ConnectorPlacement] OnPlace - Held Grabbable | Is Connector | Other End ID is the same as from local connection socket ID ");
                    }
                }
                
            }
        
         

        };
        
        
        
        // Define behavior for taking connector 
        //placePoint.OnRemove.AddListener((PlacePoint placePoint, Grabbable grabbable) =>
        this.placePoint.OnRemoveAugmentedEvent += (placePoint, grabbable, programmaticallyStarted) => 
        {
            
            if (LOG_VERBOSE)
            {
                Debug.Log("[ConnectorPlacement] OnRemove | Grabbable: "
                          + grabbable + "; PlacePoint placed Object: "
                          + placePoint.GetPlacedObject()               
                          + "; Programmatically Started: " 
                          + programmaticallyStarted);
            }
            
            
            // Same approach as with OnPlace is not feasible
            // because OnRemove is also called on remote machines via AutoHands code due to the objects having grabbable components etc
            // Thus: Check if grab was done by this machine or another 

            
            
            if (!IsOwner)
            {
                return;
            }
            
            
            // TODO 
            // TODO 
            // Seems not fast enough to be set already after RPC, delayed checking of value is correct 
            
            // Check if remove happened by person grabbing last 
            // If not, stop 
            /*
            if (grabbable.GetComponent<Connector>().GetLastGrabbedByPlayerId() !=
                (int)NetworkManager.Singleton.LocalClientId)
            {
                
                Debug.Log("*************");
                Debug.Log(grabbable.GetComponent<Connector>().GetLastGrabbedByPlayerId());
                Debug.Log("*************");
                Debug.Log("*************");
                Debug.Log("*************");
                
                return;
            }
            */
            
            //StartCoroutine(InfoPrint(grabbable));
            
            
            // Check if the taken connector is the one that spawned here, i.e. is not yet connected to anything 
            // First Connector is taken, second connector is placed in socket 
            if (!grabbable.GetComponent<Connector>().GetIsConnected())
            {
                
                if (LOG_VERBOSE)
                {
                    Debug.Log("[ConnectorPlacement] OnRemove - Held Grabbable | Is Connector | Spawned Here ");
                }


                // Change translucency of first connector
                ConnectionCable spawnedCable = grabbable.GetComponent<Connector>().GetConnectionCable();
                spawnedCable.ChangeConnectorTranslucency(FirstOrSecondConnector.First, false); 
                
                // Activate visibility of second connector and line renderer 
                spawnedCable.ChangeLinerendererVisibility(true);
                spawnedCable.ChangeConnectorVisibility(FirstOrSecondConnector.Second, true);
                
                // Update second connector state 
                // Placement of second connector is handled by cable via network variable update 
                spawnedCable.SetSecondConnectorConnectionState(true, connection.GetUniqueObjectId());

                

            }
            else // removed connector of established connection or 
            {

                if (LOG_VERBOSE)
                {
                    Debug.Log(
                        "[ConnectorPlacement] OnRemove - Held Grabbable | Is Connector | Removed established Connection ");
                }
                

                // Disconnect local from other end 
                connection.RemoveOutgoingConnection(grabbable.GetComponent<Connector>().GetOtherEndConnectedToId());
            
                // Update Connector state (not connected, invalid id)
                grabbable.GetComponent<Connector>().SetConnectorConnectionState(false,-999);
                
                
            }
            
            
        };
        


    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator PlaceConnectorCoroutine(int uniqueObjectId, FirstOrSecondConnector firstOrSecondConnector)
    {
        yield return new WaitForSeconds(0.1f);
        
        PlaceConnector_ServerRpc(uniqueObjectId, firstOrSecondConnector);
    }
    
    // Wrapper Method to broadcast visibility change to all clients 
    [ServerRpc(RequireOwnership = false)]
    private void PlaceConnector_ServerRpc(int uniqueObjectId, FirstOrSecondConnector firstOrSecondConnector)
    {
        PlaceConnector_ClientRpc(uniqueObjectId, firstOrSecondConnector);
    }

    [ClientRpc]
    private void PlaceConnector_ClientRpc(int uniqueObjectId, FirstOrSecondConnector firstOrSecondConnector)
    {
        
        GameObject spawnedObject = NetworkSpawner.Singleton.GetSpawnedObjectById(uniqueObjectId);
        
        if (spawnedObject != null)
        {
            ConnectionCable spawnedCable = spawnedObject.GetComponent<ConnectionCable>();

            if (spawnedCable != null)
            {
                Grabbable grabbable;
                
                if (firstOrSecondConnector == FirstOrSecondConnector.First)
                {
                    grabbable = spawnedCable.GetFirstConnector().GetComponent<Grabbable>();
                }
                else // second connector 
                {
                    grabbable = spawnedCable.GetSecondConnector().GetComponent<Grabbable>();
                }
                
                if (grabbable != null)
                {
                    placePoint.Place(grabbable, true); // make sure held place only is disabled in place point
                }
                
            }
            
        }
        
        
        
    }
    
    

    private IEnumerator DelayedTranslucentSpawner()
    {
        
       
        // Spawn a cable
        DateTime dateTime = DateTime.Now;
        long unixTime = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        string uniqueName = "cable_" + NetworkManager.Singleton.LocalClientId.ToString() + "_" + unixTime.ToString(); 
        NetworkSpawner.Singleton.SpawnObject(SpawnableObject.ConnectionCable, placePoint.placedOffset.position + new Vector3(0, -100, 0),
            new Vector3(0,0,0), new Vector3(1,1,1), uniqueName);
        
        
        // Find cable again 
        GameObject spawnedCableGameObject;
        while (true)
        {
            spawnedCableGameObject = GameObject.Find(uniqueName);
            if (spawnedCableGameObject == null)
            {
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                break;
            }
        }
        ConnectionCable spawnedCable = spawnedCableGameObject.GetComponent<ConnectionCable>();
        
        // Set ownership of cable 
        spawnedCable.GetComponent<OwnershipHelper>().SetLocalClientAsOwner();
        
        
        // Setup spawned cable (first connector will be in hand, second is not yet connected) 
        spawnedCable.SetSecondConnectorConnectionState(false, -999);
        spawnedCable.SetFirstConnectorConnectionState(false, -999);
        spawnedCable.ChangeConnectorVisibility(FirstOrSecondConnector.Second, false); // deactivate visibility of second connector
        spawnedCable.ChangeConnectorTranslucency(FirstOrSecondConnector.First, true); // make first translucent
        spawnedCable.ChangeLinerendererVisibility(false); // deactivate linerenderer 
        
        // Move first connector of cable to place point, delay to make sure id is set properly as network variable 
        // Broadcast to all Clients 
        //StartCoroutine(PlaceConnectorCoroutine(placePoint,
        //    spawnedCable.GetFirstConnector().GetComponent<Grabbable>()));
        StartCoroutine(PlaceConnectorCoroutine(spawnedCable.GetUniqueObjectId(), FirstOrSecondConnector.First));
    }
    
    
    private  IEnumerator InfoPrint(Grabbable grabbable)
    {
        yield return new WaitForSeconds(2);
        
        Debug.Log("*************");
        Debug.Log(NetworkManager.Singleton.LocalClientId);
        Debug.Log(grabbable.GetComponent<Connector>().GetLastGrabbedByPlayerId());
        Debug.Log("*************");
        Debug.Log("*************");
        Debug.Log("*************");
    }

    void HighlightConnectionNotPossible()
    {
        foreach (GameObject connectorPart in colorableConnectorParts)
        {
            connectorPart.GetComponent<Renderer>().material = connectionNotPossibleMaterial;
        }
    }
    
    void HighlightConnectionPossible()
    {
        foreach (GameObject connectorPart in colorableConnectorParts)
        {
            connectorPart.GetComponent<Renderer>().material = connectionPossibleMaterial;
        }
    }
    
    void SetDefaultColor()
    {
        foreach (GameObject connectorPart in colorableConnectorParts)
        {
            connectorPart.GetComponent<Renderer>().material = defaultMaterial;
        }
    }
    
    
    
}
