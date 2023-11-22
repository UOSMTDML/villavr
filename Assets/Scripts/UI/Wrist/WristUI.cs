using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class WristUI : MonoBehaviour
{

    [SerializeField] InputActionAsset inputActions;
    [SerializeField] private string oculusInputActionsName;
    [SerializeField] private string htcViveInputActionsName;
    [SerializeField] private string toggleActionName;
    [SerializeField] private bool requiresModeratorStatus;
    
    private Canvas _wristUICanvas;
    private InputAction _menuInputAction;
    private float lastMenuToggleTime;
    
    
    private void Start()
    {
        
        _wristUICanvas = GetComponent<Canvas>();
        
            
            // Depending on input type, assign actions to menu opening 
            if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerOculusInput)
            {
                _menuInputAction = inputActions.FindActionMap(oculusInputActionsName).FindAction(toggleActionName);
                _menuInputAction.Enable(); // Make action listen to callbacks 
                _menuInputAction.started += ToggleMenu;
            }

            if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerViveInput)
            {
                _menuInputAction = inputActions.FindActionMap(htcViveInputActionsName).FindAction(toggleActionName);
                _menuInputAction.Enable(); // Make action listen to callbacks 
                _menuInputAction.started += ToggleMenu;
            }
            
        
        
    }


    private void ToggleMenu(InputAction.CallbackContext context)
    {
        // Check if Menu can only be opened for Moderator or is already opened and should be closed 
        if ((requiresModeratorStatus &&
             (ExperienceManager.Singleton.playerRole == ExperienceManager.PlayerRole.Moderator)) ||
            !requiresModeratorStatus || _wristUICanvas.enabled)
        {
            
            // Make sure that enough time passed since last button press, since there is a unity problem
            // with button press being registered twice 
            if (Time.time < lastMenuToggleTime + 0.2f)
            {
                return;
            }

            lastMenuToggleTime = Time.time;

            _wristUICanvas.enabled = !_wristUICanvas.enabled;
        }
    }
    
    

    private void OnDestroy()
    {
        _menuInputAction.performed -= ToggleMenu;
    }
    
    
    
    
    
    
}
