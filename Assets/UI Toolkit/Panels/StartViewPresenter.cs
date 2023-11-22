using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Management;

public class StartViewPresenter : MonoBehaviour
{
    private VisualElement _settingsView;
    private VisualElement _startView;

    
    void Awake()
    {
        
    }
    
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        _startView = root.Q("MainMenu");
        _settingsView = root.Q("SettingsMenu");

        SetupStartMenu();
        SetupSettingsMenu();

    }

    private void SetupStartMenu()
    {
        MainMenuPresenter menuPresenter = new MainMenuPresenter(_startView);
        menuPresenter.OpenSettings = () => ToggleSettingsMenu(true);
    }

    private void SetupSettingsMenu()
    {
        SettingsMenuPresenter settingsPresenter = new SettingsMenuPresenter(_settingsView);
        settingsPresenter.BackFromSettingsAction = () => ToggleSettingsMenu(false);
    }

    private void ToggleSettingsMenu(bool enable)
    {
        _startView.Display(!enable);
        _settingsView.Display(enable);
    }
    
   

}
