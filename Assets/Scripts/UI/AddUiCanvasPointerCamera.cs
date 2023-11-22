using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AddUiCanvasPointerCamera : MonoBehaviour
{

    private Canvas canvas;
    private GameObject cameraGameObject;
    
    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();

        StartCoroutine(AddCamera());
    }

    private IEnumerator AddCamera()
    {
        while (true)
        {

            cameraGameObject = GameObject.Find(ExperienceManager.Singleton.uiCanvasPointerCameraGameObjectName);

            if (cameraGameObject == null)
            {
                yield return new WaitForSeconds(2);
            }

            else
            {
                canvas.worldCamera = cameraGameObject.GetComponent<Camera>();
                yield break;
            }
            
            
        }
    }
    
}
