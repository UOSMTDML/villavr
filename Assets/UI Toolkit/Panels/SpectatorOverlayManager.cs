using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SpectatorOverlayManager : MonoBehaviour
{
    
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("Exit").clicked += () => Debug.Log("Exit button clicked");
        root.Q<Button>("Exit").clicked += () => SceneManager.LoadScene(ExperienceManager.Singleton.mainMenuScene);
        root.Q<Button>("Exit").clicked += () => ExperienceManager.Singleton.spectatorOverlay.SetActive(false);

    }
    
}
