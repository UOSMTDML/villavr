using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SoundStoreScreenUi : MonoBehaviour
{
    // Events 
    public UnityEvent startPlayAudioEvent;
    public UnityEvent stopPlayAudioEvent;
    public UnityEvent confirmRecordedAudioEvent;
    public UnityEvent startRecordAudioEvent;
    public UnityEvent stopRecordAudioEvent;
    

    [SerializeField] private GameObject screenGameObject;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject playMemoMenu;
    [SerializeField] private GameObject recordMemoMenu;
    
    // Play Menu 
    [SerializeField] private Button playMemoMenuPlayButton;
    [SerializeField] private Button playMemoMenuStopButton;

    // Record Menu 
    [SerializeField] private Button recordMemoMenuRecordButton;
    [SerializeField] private Button recordMemoMenuStopButton;
    [SerializeField] private Button recordMemoMenuPlayButton;
    [SerializeField] private Button recordMemoMenuConfirmButton;
    [SerializeField] private TMP_Text recordMemoMenuConfirmText;
    [SerializeField] private TMP_Dropdown recordMemoMenuDeviceDropdown;


    [SerializeField] private Material whiteColor;
    [SerializeField] private Material greyColor;
    [SerializeField] private Material redColor;


    private bool audioIsPlaying = false;
    
    
    // Start is called before the first frame update
    void Start()
    {
        SetupInit();
        
        // Register button presses, play menu  
        playMemoMenuPlayButton.onClick.AddListener(ClickedPlayMemoMenuPlayButton);
        playMemoMenuStopButton.onClick.AddListener(ClickedPlayMemoMenuStopButton);
        
        // Register button presses & drop down change, record menu 
        recordMemoMenuRecordButton.onClick.AddListener(ClickedRecordMemoMenuRecordButton);
        recordMemoMenuStopButton.onClick.AddListener(ClickedRecordMemoMenuStopButton);
        recordMemoMenuPlayButton.onClick.AddListener(ClickedRecordMemoMenuPlayButton);
        recordMemoMenuConfirmButton.onClick.AddListener(ClickedRecordMemoMenuConfirmButton);
        recordMemoMenuDeviceDropdown.onValueChanged.AddListener((int val) =>
        {
            // Store selected mic name 
            ExperienceManager.Singleton.selectedMicName = recordMemoMenuDeviceDropdown.options[recordMemoMenuDeviceDropdown.value].text;
        });

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // Setup Init state of sound store 
    private void SetupInit()
    {
        // Activate record menu 
        playMemoMenu.SetActive(false);
        recordMemoMenu.SetActive(true);
        
        // In play menu activate only play button 
        playMemoMenuPlayButton.interactable = true;
        playMemoMenuStopButton.interactable = false;
        
        // In record menu Activate only record button 
        ToggleRecordMenuButtonsAvailable(true, false, false); 
        
        // Deactivate confirm options  
        ToggleConfirmAvailable(false);
        
        // Coroutine to update available Microphones 
        StartCoroutine(UpdateMicDevices());
    }


    private IEnumerator UpdateMicDevices()
    {
        while (true)
        {
            if (ExperienceManager.Singleton.micAvailable)
            {
                recordMemoMenuDeviceDropdown.ClearOptions();
                recordMemoMenuDeviceDropdown.AddOptions(ExperienceManager.Singleton.availableMicNames);
                recordMemoMenuDeviceDropdown.value =
                    ExperienceManager.Singleton.availableMicNames.IndexOf(ExperienceManager.Singleton
                        .selectedMicName);
                recordMemoMenuDeviceDropdown.interactable = true;
            }
            else
            {
                recordMemoMenuDeviceDropdown.ClearOptions();
                recordMemoMenuDeviceDropdown.AddOptions(new List<string>{"No Device Connected"});
                recordMemoMenuDeviceDropdown.interactable = false;
            }

            yield return new WaitForSeconds(3);
        }
    }
    
    

    public void EnablePlayMenu()
    {
        playMemoMenu.SetActive(true);
        recordMemoMenu.SetActive(false);
    }


    private void ToggleRecordMenuButtonsAvailable(bool recordIsAvailable, bool stopIsAvailable, bool playIsAvailable)
    {
        recordMemoMenuRecordButton.interactable = recordIsAvailable;
        recordMemoMenuStopButton.interactable = stopIsAvailable;
        recordMemoMenuPlayButton.interactable = playIsAvailable;

    }

    
    // Change look of confirm 
    private void ToggleConfirmAvailable(bool isAvailable)
    {
        if (isAvailable)
        {
            recordMemoMenuConfirmButton.interactable = true;
        }
        else
        {
            recordMemoMenuConfirmButton.interactable = false;
        }
    }


    private void ClickedPlayMemoMenuPlayButton()
    {
        
        startPlayAudioEvent.Invoke();
        audioIsPlaying = true;

        playMemoMenuPlayButton.interactable = false;
        playMemoMenuStopButton.interactable = true;
    }

    private void ClickedPlayMemoMenuStopButton()
    {
        stopPlayAudioEvent.Invoke();
        audioIsPlaying = false;

        playMemoMenuPlayButton.interactable = true;
        playMemoMenuStopButton.interactable = false;
    }
    
    private void ClickedRecordMemoMenuRecordButton()
    {
        startRecordAudioEvent.Invoke();
        
        // Activate stop record button 
        ToggleRecordMenuButtonsAvailable(true, true, false);
        
        // Start Coroutine to check max record time
        StartCoroutine(CheckMaxRecordTime());

    }
    
    private IEnumerator CheckMaxRecordTime()
    {
        // Wait Max amount of seconds 
        int seconds = 0;
        while (seconds <= ExperienceManager.Singleton.micAudioMaxRecordTimeSeconds)
        {
            seconds += 1;
            yield return new WaitForSeconds(1);
        }
        
        // Then Stop Recording 
        ClickedRecordMemoMenuStopButton();
    }
    
    
    private void ClickedRecordMemoMenuStopButton()
    {
        if (audioIsPlaying) // playing audio 
        {
            stopPlayAudioEvent.Invoke();
            audioIsPlaying = false;
            
            // Activate play button and deactivate stop button 
            ToggleRecordMenuButtonsAvailable(true, false, true);
            
            
        }
        else // recording 
        {
            stopRecordAudioEvent.Invoke();
            StopCoroutine(CheckMaxRecordTime());
        
            // Activate play button and deactivate stop button 
            ToggleRecordMenuButtonsAvailable(true, false, true);
            
            // Activate Confirm options 
            ToggleConfirmAvailable(true);
        }
        
    }
    
    
    private void ClickedRecordMemoMenuPlayButton()
    {
        
        startPlayAudioEvent.Invoke();
        audioIsPlaying = true;
        
        // Activate stop record button, deactivate record button 
        ToggleRecordMenuButtonsAvailable(false, true, true);
    }
    
    
    private void ClickedRecordMemoMenuConfirmButton()
    {
        if (audioIsPlaying)
        {
            stopPlayAudioEvent.Invoke();
            audioIsPlaying = false;
        }
        
        confirmRecordedAudioEvent.Invoke();
        
        // Switch to play menu 
        EnablePlayMenu();
    }
    
    
    
    
    
}
