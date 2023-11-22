using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

public class MainMenuPresenter
{
    
    // Define Action for opening Settings 
    public Action OpenSettings { set => _settingsButton.clicked += value; }

    
    
    private Button _settingsButton;
    private Button _quitButton;
    
    public MainMenuPresenter(VisualElement root)
    {
        // Get Buttons 
        
        _settingsButton = root.Q<Button>("Start");
        _quitButton = root.Q<Button>("Quit");

        // Define button Callbacks
        _quitButton.clicked += () => Application.Quit();
    }
    
    
    
}
