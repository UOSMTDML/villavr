using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;


public class XRStarter : MonoBehaviour
{

    [SerializeField] private bool startAtAwake;
    
    // Singleton pattern
    // (Comparing against initially created static instance in Awake) 
    private static XRStarter _singleton;
    public static XRStarter Singleton
    {
        get { return _singleton; }
        private set { _singleton = value; }
    }
    private void Awake()
    {
        if (_singleton != null && _singleton != this)
        {
            Destroy(this.gameObject);
        } else {
            _singleton = this;
        }
        
        DontDestroyOnLoad(this.gameObject);




        if (startAtAwake)
        {
            StartXR();
        }
    }

    
    

    void Update()
    {
        
    }

    public void StartXR()
    {
        StartCoroutine("StartXRCoroutine");
    }
    
    
    private IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }

    public void StopXR()
    {
        Debug.Log("Stopping XR...");

        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }

    private void OnApplicationQuit()
    {
        StopXR();
    }

}
