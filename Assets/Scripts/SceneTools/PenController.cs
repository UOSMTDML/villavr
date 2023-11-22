
using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;

public class PenController : NetworkBehaviour
{
    
    [Header("Pen Properties")]
    public Transform tip;
    public Material tipMaterial;
    public GameObject colorIndicator;
    

    [Header("Hands & Grabbable")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string oculusInputActionsName;
    [SerializeField] private string htcViveInputActionsName;
    [SerializeField] private string drawActionName;
    [SerializeField] private string changeColorActionName;
    private InputAction _drawInputAction;
    private InputAction _changeColorInputAction;
    private LineRenderer _currentDrawing;
    private LineRenderer _lastDrawing;
    private int _drawIndex;
    private int _currentColorIndex;
    private bool _drawButtonPressed = false;
    private bool externalDrawing = false;
    private bool isGrabbed = false;
    
    

    private void Start()
    {
        //set start color to first color listed in the inspector
        _currentColorIndex = 0;
        tipMaterial.color = ExperienceManager.Singleton.drawingMaterials[_currentColorIndex].color;
        
        //load controller mappings for oculus and htc vive
        if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerOculusInput)
        {
            // Draw 
            _drawInputAction = inputActions.FindActionMap(oculusInputActionsName).FindAction(drawActionName);
            _drawInputAction.Enable(); // Make action listen to callbacks 
            //draw when trigger is pressed
            _drawInputAction.started += ToggleDraw;
            _drawInputAction.canceled += ToggleDraw;
            
            // Change Color
            _changeColorInputAction = inputActions.FindActionMap(oculusInputActionsName).FindAction(changeColorActionName);
            _changeColorInputAction.Enable();
            _changeColorInputAction.started += ToggleDraw;
        }

        if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerViveInput)
        {
            // Draw 
            _drawInputAction = inputActions.FindActionMap(htcViveInputActionsName).FindAction(drawActionName);
            _drawInputAction.Enable(); // Make action listen to callbacks 
            //draw when trigger is pressed
            _drawInputAction.started += ToggleDraw;
            _drawInputAction.canceled += ToggleDraw;
            
            // Change Color 
            _changeColorInputAction = inputActions.FindActionMap(htcViveInputActionsName).FindAction(changeColorActionName);
            _changeColorInputAction.Enable(); 
            _changeColorInputAction.started += SwitchColor;
        }
        
    }
    
    public override void OnDestroy()
    {
        // Remove action callback
        _drawInputAction.started -= ToggleDraw;
        _drawInputAction.canceled -= ToggleDraw;
        _changeColorInputAction.started -= SwitchColor;
            
        // invoke the base 
        base.OnDestroy();
    }
    
    

    private void Update()
    {
        
        // Draw 
        if ((_drawButtonPressed && isGrabbed) || externalDrawing)
        {
            Draw();
        }
        
        // Update last drawing 
        else if (_currentDrawing != null)
        {
            _lastDrawing = _currentDrawing;
            _currentDrawing = null;
            
            // Generate colliders on linerenderer 
            _lastDrawing.GetComponent<DrawingLineRendererInfo>().GenerateLineRendererColliders();
            //StartCoroutine(GenerateColliders(_lastDrawing));
        }

        
        
        
        
    }

    private void Draw()
    {
        
        // Init 
        if (_currentDrawing == null)
        {
            _drawIndex = 0;
            _currentDrawing = new GameObject().AddComponent<LineRenderer>();
            _currentDrawing.name = "DrawingLineRenderer";
            _currentDrawing.tag = ExperienceManager.Singleton.drawingGameObjectTag;
            _currentDrawing.AddComponent<DrawingLineRendererInfo>();
            _currentDrawing.GetComponent<DrawingLineRendererInfo>().colorIdx = _currentColorIndex;
            _currentDrawing.material = ExperienceManager.Singleton.drawingMaterials[_currentColorIndex];
            _currentDrawing.startWidth = _currentDrawing.endWidth = ExperienceManager.Singleton.drawingToolWidth;
            _currentDrawing.positionCount = 1;
            _currentDrawing.SetPosition(0, tip.transform.position);
            
        }
        else // Add line renderer position, if far enough away from pen tip 
        {
            var currentPos = _currentDrawing.GetPosition(_drawIndex);
            if (Vector3.Distance(currentPos, tip.position) > 0.01f)
            {
                _drawIndex++;
                _currentDrawing.positionCount = _drawIndex + 1;
                _currentDrawing.SetPosition(_drawIndex, tip.position);
            }
        }
        
    }
    
    
    private void ToggleDraw(InputAction.CallbackContext context) 
    {
        _drawButtonPressed = !_drawButtonPressed;
        
        // Signal other Clients that drawing is engaged/ not engaged
        if (isGrabbed)
        {
            SignalEngaged_ServerRpc(_drawButtonPressed);
        }
        
        
    }
    
    
    // Called when object is grabbed; by Grabbable in Inspector 
    public void ToggleGrabbed(bool grabbed)
    {
        isGrabbed = grabbed;
    }


    private void SetColor()
    {
        tipMaterial.color = ExperienceManager.Singleton.drawingMaterials[_currentColorIndex].color;
        colorIndicator.GetComponent<MeshRenderer>().material = ExperienceManager.Singleton.drawingMaterials[_currentColorIndex];
    }
    
    
    public void SwitchColor(InputAction.CallbackContext context)
    {

        if (isGrabbed)
        {
            // Go back to start of list 
            if (_currentColorIndex == ExperienceManager.Singleton.drawingMaterials.Length - 1)
            {
                _currentColorIndex = 0;
            }
            else // increase idx 
            {
                _currentColorIndex++;
            }
        
            // Update Color 
            SetColor();

            // Signal color change to other clients 
            SignalColorChange_ServerRpc(_currentColorIndex);
        }
        
    }
    
    
    
    
    [ServerRpc(RequireOwnership = false)]
    private void SignalColorChange_ServerRpc(int colorIdx, ServerRpcParams serverRpcParams = default)
    {
        ReceiveColorIdx_ClientRPC(colorIdx, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ReceiveColorIdx_ClientRPC(int colorIdx, ulong initiallySentFromId, ClientRpcParams clientRpcParams = default)
    {
        
        // This client indicated color change
        if (initiallySentFromId == NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        // Some other client indicated   
        else
        {
            _currentColorIndex = colorIdx;
            SetColor();
        }
        
    }



    [ServerRpc(RequireOwnership = false)]
    private void SignalEngaged_ServerRpc(bool isEngaged, ServerRpcParams serverRpcParams = default)
    {
        ReceiveIsEngaged_ClientRPC(isEngaged, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ReceiveIsEngaged_ClientRPC(bool isEngaged, ulong initiallySentFromId, ClientRpcParams clientRpcParams = default)
    {
        
        // This client indicated drawing/ not drawing 
        if (initiallySentFromId == NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        // Some other client is drawing/ not drawing  
        else
        {
            externalDrawing = isEngaged;
        }
        
    }
    
     
    
}