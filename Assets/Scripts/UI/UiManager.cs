using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] private Transform serverUi;
    
    private void Awake()
    {
        
    }

    private void Start()
    {
        
        // Activate Server UI
        if (ExperienceManager.Singleton.connectionRole == ExperienceManager.ClientOrHostOrServer.Host ||
            ExperienceManager.Singleton.connectionRole == ExperienceManager.ClientOrHostOrServer.Server)
        {
            serverUi.gameObject.SetActive(true);
        }
    }
    }
