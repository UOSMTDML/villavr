using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UIElements.Button;

public class SettingsMenuPresenter
{
    
    // Define Action for Back from Settings Button 
     public Action BackFromSettingsAction { set => _backButton.clicked += value; }
    
    
    private List<string> _devices = new List<string>()
    {
        "HTC Vive", // idx 0; used below 
        "Oculus Rift"  // idx 1; used below 
    };
    
    private List<string> _clientOrHost = new List<string>()
    {
        "Host", // idx 0; used below 
        "Client",  // idx 1; used below 
        //"Server"   // idx 2; used below; Server is not available, Server will always be Client, too, i.e. Host 
    };
    
    private List<string> _roles = new List<string>()
    {
        "User", // idx 0; used below 
        "Moderator"  // idx 1; used below 
    };
    
    
    
    private DropdownField _connectionRoleSelection;
    private DropdownField _userRoleSelection;
    private Button _SpectatorStartButton;
    private Button _VrStartButton;
    private Button _backButton;
    private DropdownField _deviceSelection;
    private TextField _ipAdressTextField;
    private TextField _userNameTextField;
    private string _ipAddress;
    
    
    public SettingsMenuPresenter(VisualElement root)
    {
        // Get buttons & fields 
        _connectionRoleSelection = root.Q<DropdownField>("ConnectionRoleSelection");
        _userRoleSelection = root.Q<DropdownField>("UserRoleSelection");
        _VrStartButton = root.Q<Button>("StartVR");
        _SpectatorStartButton = root.Q<Button>("StartSpectator");
        _backButton = root.Q<Button>("BackButton");
        _deviceSelection = root.Q<DropdownField>("DeviceSelection");
        _ipAdressTextField = root.Q<TextField>("IpAddressInput");
        _userNameTextField = root.Q<TextField>("UserNameInput");
        
        // Set up dropdowns
        _deviceSelection.choices = _devices;
        _deviceSelection.index = 0;
        _connectionRoleSelection.choices = _clientOrHost;
        _connectionRoleSelection.index = 0;
        _userRoleSelection.choices = _roles;
        _userRoleSelection.index = 0;
        
        // Set up text fields
        _ipAdressTextField.value = GetLocalIPAddress(); // default to local ip
        _userNameTextField.value = "";
        
        // Start Buttons 
        _VrStartButton.clicked += () => ClickedStartVr();
        _SpectatorStartButton.clicked += () => ClickedStartSpectator();
        
    }

    private void ClickedStartVr()
    {
        // Save 
        SaveSettings();
        
        // Start VR
        XRStarter.Singleton.StartXR();
        
        // No need to adapt player type, change scene 
        SceneManager.LoadScene(ExperienceManager.Singleton.mainCollaborationScene);

    }

    private void ClickedStartSpectator()
    {
        // Save
        SaveSettings();
        
        // Change player type and then change scene 
        ExperienceManager.Singleton.playerType = ExperienceManager.PlayerType.PlayerSpectator;
        SceneManager.LoadScene(ExperienceManager.Singleton.mainCollaborationScene);
    }
    
    // Save Settings to Experience Manager 
    private void SaveSettings()
    {
        // Save input type 
        if (_deviceSelection.index == 0) // HTC Vive 
        {
            ExperienceManager.Singleton.playerType = ExperienceManager.PlayerType.PlayerViveInput;
        }
        if (_deviceSelection.index == 1) // Oculus Rift
        {
            ExperienceManager.Singleton.playerType = ExperienceManager.PlayerType.PlayerOculusInput;
        }

        // Save IP Adress 
        if (string.Concat(_ipAdressTextField.text.Where(c => !char.IsWhiteSpace(c))) != "")
        {
            ExperienceManager.Singleton.ipAddress = _ipAdressTextField.text;
        }
        
        // Save User Name 
        if (string.Concat(_userNameTextField.text.Where(c => !char.IsWhiteSpace(c))) != "")
        {
            ExperienceManager.Singleton.playerName = _userNameTextField.text;
        }
        
        // Save connection role 
        if (_connectionRoleSelection.index == 0) // Host
        {
            ExperienceManager.Singleton.connectionRole = ExperienceManager.ClientOrHostOrServer.Host;
        }
        if (_connectionRoleSelection.index == 1) // Client
        {
            ExperienceManager.Singleton.connectionRole = ExperienceManager.ClientOrHostOrServer.Client;
        }
        if (_connectionRoleSelection.index == 2) // Server
        {
            ExperienceManager.Singleton.connectionRole = ExperienceManager.ClientOrHostOrServer.Server;
        }
        
        // Save User Role 
        if (_userRoleSelection.index == 0) // User
        {
            ExperienceManager.Singleton.playerRole = ExperienceManager.PlayerRole.User;
        }
        if (_userRoleSelection.index == 1) // Moderator
        {
            ExperienceManager.Singleton.playerRole = ExperienceManager.PlayerRole.Moderator;
        }
        
        

    }
    
    
    private string GetLocalIPAddress()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "";
    }
    
    
}
