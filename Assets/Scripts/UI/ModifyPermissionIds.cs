using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ModifyPermissionIds : MonoBehaviour
{

    [SerializeField] private PermissionUi permissionUi;
    [SerializeField] private int lineOffset = 12;
    [SerializeField] private int minContentHeight = 30;
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private GameObject referenceToggleFirst;
    [SerializeField] private GameObject referenceToggleSecond;
    [SerializeField] private GameObject referenceToggleThird;
    [SerializeField] private Button saveButton;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private string infoTextDefault;
    
    
    //[SerializeField] private GameObject referenceText;
    //[SerializeField] private GameObject referenceInputField;
    //    [SerializeField] private bool inputFromSelectionScreen;


    //private int numberOfActiveLines = 0;
    //private List<int> lineIdentifiers = new List<int>();
    //private List<GameObject> texts = new List<GameObject>();
    //private List<GameObject> firstToggles = new List<GameObject>();
    //private List<GameObject> secondToggles = new List<GameObject>();
    //private List<GameObject> thirdToggles = new List<GameObject>();
    //private List<GameObject> inputFields = new List<GameObject>();

    private int startedForId; 
    private List<int> selectedIds = new List<int>();
    
    
    
    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        // Add Listener for save button, signal back to permission UI  
        saveButton.onClick.AddListener(() =>
        {
            permissionUi.SignalBackSelectionInputPerformed(startedForId,selectedIds);
        });
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    
    

    public void StartUp(int forId)
    {
        // Delete previously instantiated toggles 
        ClearToggles();

        // Set started for line id 
        startedForId = forId;
        
        // Update Info Text 
        infoText.text = infoTextDefault + "\n" + forId.ToString();
        
        // Get Client IDs and set toggles
        SetTogglesForIds(AccessManager.Singleton.GetConnectedClients().Select(item => (int)item).ToArray());

    }

    private void ClearToggles()
    {
        // Clear list 
        selectedIds.Clear();
        
        // Iterate through children 
        for(int i = 0; i < scrollViewContent.transform.childCount; i++)
        {
            // Delete child if is not reference toggle 
            if ((scrollViewContent.transform.GetChild(i) != referenceToggleFirst.transform) &&
                (scrollViewContent.transform.GetChild(i) != referenceToggleSecond.transform) && 
                (scrollViewContent.transform.GetChild(i) != referenceToggleThird.transform))
            {
                Destroy(scrollViewContent.transform.GetChild((i)).gameObject);
            }
        }
    }

    
    private void SetTogglesForIds(int[] ids)
    {
        
        if (ids.Length == 0)
        {
            return; 
        }

        int idCnt = ids.Length;
        int column = 0;
        int row = 0;

        // Go through all ids and create toggles
        for (int i = 0; i < idCnt; i++)
        {
            
            column = i % 3;
            row = (int) (i / 3);
            int idxCopy = i; 

            string label = i.ToString();
            if (i == (int)NetworkManager.Singleton.LocalClientId)
            {
                label = i.ToString() + " (me)";
            }
            
            // First column
            if (i % 3 == 0)
            {   
                // Instantiate & activate 
                Transform newToggleFirst = Instantiate(referenceToggleFirst.transform, scrollViewContent.transform);
                newToggleFirst.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, -lineOffset * row, 0);
                newToggleFirst.GetChild(1).GetComponent<TextMeshProUGUI>().text = label;
                newToggleFirst.GameObject().SetActive(true);
                
                // Add listener, store in list  
                newToggleFirst.GameObject().GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
                {
                    ToggleSwitchedListener(ids[idxCopy], isOn); // use idxCopy to prevent i from being modified for different listener events 
                });
            }

            // Second column 
            if (i % 3 == 1)
            {
                // Instantiate & activate 
                Transform newToggleSecond = Instantiate(referenceToggleSecond.transform, scrollViewContent.transform);
                newToggleSecond.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, -lineOffset * row, 0);
                newToggleSecond.GetChild(1).GetComponent<TextMeshProUGUI>().text = label;
                newToggleSecond.GameObject().SetActive(true);
                
                // Add listener, store in list  
                newToggleSecond.GameObject().GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
                {
                    ToggleSwitchedListener(ids[idxCopy], isOn); // use idxCopy to prevent i from being modified for different listener events 
                });
            }
            
            // Third column 
            if (i % 3 == 2)
            {
                // Instantiate & activate 
                Transform newToggleThird = Instantiate(referenceToggleThird.transform, scrollViewContent.transform);
                newToggleThird.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, -lineOffset * row, 0);
                newToggleThird.GetChild(1).GetComponent<TextMeshProUGUI>().text = label;
                newToggleThird.GameObject().SetActive(true);
                
                // Add listener, store in list  
                newToggleThird.GameObject().GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
                {
                    ToggleSwitchedListener(ids[idxCopy], isOn); // use idxCopy to prevent i from being modified for different listener events 
                });
            }
            
        }
        
        
        // Update content height 
        scrollViewContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0,
            Mathf.Max((row) * lineOffset, minContentHeight));
        
        
    }


    private void ToggleSwitchedListener(int id, bool isOn)
    {
        if (isOn)
        {
            if (!selectedIds.Contains(id))
            {
                selectedIds.Add(id);
            }
            else
            {
                // Already contained
            }
        }
        else
        {
            if (selectedIds.Contains(id))
            {
                selectedIds.Remove(id);
            }
            else
            {
                // Already not contained 
            }
        }
        
    }


    public List<int> GetSelectedIds()
    {
        return selectedIds;
    }
    
   

}
