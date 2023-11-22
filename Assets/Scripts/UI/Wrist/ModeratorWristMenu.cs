using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModeratorWristMenu : MonoBehaviour
{

    [SerializeField] private Button mainMenuPermissionsButton;
    [SerializeField] private Button mainMenuSaveLoadButton;
    [SerializeField] private Button mainMenuServerInfoButton;
    [SerializeField] private Button saveLoadMenuBackButton;
    [SerializeField] private Button permissionsMenuBackButton;
    [SerializeField] private Button serverInfoMenuBackButton;

    [SerializeField] private GameObject saveLoadMenuElements;
    [SerializeField] private GameObject permissionsMenuElements;
    [SerializeField] private GameObject serverInfoMenuElements;
    [SerializeField] private GameObject mainMenuElements;
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
      
        // Main Menu 
        mainMenuPermissionsButton.onClick.AddListener(() => ClickedMainMenuPermissionsButton());
        mainMenuSaveLoadButton.onClick.AddListener(() => ClickedMainMenuSaveLoadButton());
        mainMenuServerInfoButton.onClick.AddListener(() => ClickedMainMenuServerInfoButton());
        
        // Back to main Menu 
        saveLoadMenuBackButton.onClick.AddListener(() => ClickedBackToMenu());
        permissionsMenuBackButton.onClick.AddListener(() => ClickedBackToMenu());
        serverInfoMenuBackButton.onClick.AddListener(() => ClickedBackToMenu());
    }

    private void ClickedMainMenuPermissionsButton()
    {
        mainMenuElements.SetActive(false);
        permissionsMenuElements.SetActive(true);
    }
    
    private void ClickedMainMenuSaveLoadButton()
    {
        mainMenuElements.SetActive(false);
        saveLoadMenuElements.SetActive(true);
    }
    
    private void ClickedMainMenuServerInfoButton()
    {
        mainMenuElements.SetActive(false);
        serverInfoMenuElements.SetActive(true);
    }
    
    private void ClickedBackToMenu()
    {
        serverInfoMenuElements.SetActive(false);
        permissionsMenuElements.SetActive(false);
        saveLoadMenuElements.SetActive(false);
        mainMenuElements.SetActive(true);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
