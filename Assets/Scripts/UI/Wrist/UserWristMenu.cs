using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Management;

public class UserWristMenu : MonoBehaviour
{
    
    [SerializeField] private Button mainMenuQuitButton;
    [SerializeField] private Toggle mainMenuMuteToggle;
    [SerializeField] private Slider mainMenuVolumeSlider;
    
    [SerializeField] private Button exitMenuYesButton;
    [SerializeField] private Button exitMenuNoButton;
    
    [SerializeField] private GameObject exitMenuElements;
    [SerializeField] private GameObject mainMenuElements;
    
    
    
    private float audioListenerVolume; 
    
    // Start is called before the first frame update
    void Start()
    {
        // Init Audio Listener Volume with 1 (slider min 0.0001, max 1)
        audioListenerVolume = 1;
        mainMenuVolumeSlider.value = 1;
        
        // Main Menu 
        mainMenuQuitButton.onClick.AddListener(() => ClickedMainMenuExitButton());
        mainMenuMuteToggle.onValueChanged.AddListener((bool newValue) => ClickedMainMenuMuteToggle(newValue));
        mainMenuVolumeSlider.onValueChanged.AddListener((float newValue) => ChangedMainMenuVolumeSlider(newValue));
        
        
        // Exit Menu 
        exitMenuYesButton.onClick.AddListener(() => ClickedExitMenuYesButton());
        exitMenuNoButton.onClick.AddListener(() => ClickedExitMenuNoButton());
        
        
    }

   

    private void ClickedMainMenuExitButton()
    {
        mainMenuElements.SetActive(false);
        exitMenuElements.SetActive(true);
    }

    private void ClickedMainMenuMuteToggle(bool newValue)
    {
        // Change AudioListener volume for muting 
        if (!newValue)
        {
            AudioListener.volume = audioListenerVolume; // reset to last value set by slider 
        }
        else // mute selected, i.e. newValue == True 
        {
            AudioListener.volume = 0;
        }
    }


    private void ChangedMainMenuVolumeSlider(float newValue)
    {
        // Store and update 
        audioListenerVolume = newValue;
        if (!mainMenuMuteToggle.isOn)
        {
            AudioListener.volume = newValue;
        }
    }
    


    private void ClickedExitMenuYesButton()
    {
        XRStarter.Singleton.StopXR();
        SceneManager.LoadScene(ExperienceManager.Singleton.mainMenuScene);
    }

    private void ClickedExitMenuNoButton()
    {
        mainMenuElements.SetActive(true);
        exitMenuElements.SetActive(false);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
