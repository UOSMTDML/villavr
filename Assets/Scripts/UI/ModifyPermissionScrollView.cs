using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModifyPermissionScrollView : MonoBehaviour
{

    [SerializeField] private int lineOffset = 90;
    [SerializeField] private int minContentHeight = 300;
    [SerializeField] private bool inputFromSelectionScreen = false;
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private GameObject referenceText;
    [SerializeField] private GameObject referenceToggleFirst;
    [SerializeField] private GameObject referenceToggleSecond;
    [SerializeField] private GameObject referenceToggleThird;
    [SerializeField] private GameObject referenceInputField;
    

    private int numberOfActiveLines = 0;
    private List<int> lineIdentifiers = new List<int>();
    private List<GameObject> texts = new List<GameObject>();
    private List<GameObject> firstToggles = new List<GameObject>();
    private List<GameObject> secondToggles = new List<GameObject>();
    private List<GameObject> thirdToggles = new List<GameObject>();
    private List<GameObject> inputFields = new List<GameObject>();

    private PermissionUi permissionUi; 
    
    
    
    // Start is called before the first frame update
    void Start()
    {

        permissionUi = GetComponent<PermissionUi>();

        

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // Add a line, buttons are optional
    public void AddLineWithTextAndToggles(int lineIdentifier, string text, string firstToggle = "", string secondToggle = "", string thirdToggle = "", int currentlyActive = 1, int inputFieldBelongsTo = 3)
    {  
        // Add new line GameObjects 
        Transform newText = Instantiate(referenceText.transform, scrollViewContent.transform);
        newText.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, -lineOffset * numberOfActiveLines, 0);
        newText.GetComponent<TextMeshProUGUI>().text = text;
        newText.GameObject().SetActive(true);
       
        // Create Toggles & Input Field 
        Transform newToggleFirst = Instantiate(referenceToggleFirst.transform, scrollViewContent.transform);
        newToggleFirst.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, -lineOffset * numberOfActiveLines, 0);
        newToggleFirst.GetChild(1).GetComponent<TextMeshProUGUI>().text = firstToggle;
        
        Transform newToggleSecond = Instantiate(referenceToggleSecond.transform, scrollViewContent.transform);
        newToggleSecond.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, -lineOffset * numberOfActiveLines, 0);
        newToggleSecond.GetChild(1).GetComponent<TextMeshProUGUI>().text = secondToggle;
        
        Transform newToggleThird = Instantiate(referenceToggleThird.transform, scrollViewContent.transform);
        newToggleThird.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, -lineOffset * numberOfActiveLines, 0);
        newToggleThird.GetChild(1).GetComponent<TextMeshProUGUI>().text = thirdToggle;
        
        Transform newInputField = Instantiate(referenceInputField.transform, scrollViewContent.transform);
        newInputField.GetComponent<RectTransform>().anchoredPosition3D += new Vector3(0, -lineOffset * numberOfActiveLines, 0);
        newInputField.GetComponent<TMP_InputField>().interactable = false;
        newInputField.GameObject().SetActive(true);

        // Activate first toggle 
        if (currentlyActive == 1)
        {
            newToggleFirst.GetComponent<Toggle>().isOn = true;
            newToggleSecond.GetComponent<Toggle>().isOn = false;
            newToggleThird.GetComponent<Toggle>().isOn = false;
            
            newToggleFirst.GetComponent<Toggle>().interactable = false;
            newToggleSecond.GetComponent<Toggle>().interactable = true;
            newToggleThird.GetComponent<Toggle>().interactable = true;

            // Activate Input Field
            if (inputFieldBelongsTo == 1)
            {
                newInputField.GetComponent<TMP_InputField>().interactable = true;
            }
            
            
        }
        else if (currentlyActive == 2)
        {
            newToggleFirst.GetComponent<Toggle>().isOn = false;
            newToggleSecond.GetComponent<Toggle>().isOn = true;
            newToggleThird.GetComponent<Toggle>().isOn = false;
            
            newToggleFirst.GetComponent<Toggle>().interactable =  true;
            newToggleSecond.GetComponent<Toggle>().interactable = false;
            newToggleThird.GetComponent<Toggle>().interactable = true;
            
            // Activate Input Field
            if (inputFieldBelongsTo == 2)
            {
                newInputField.GetComponent<TMP_InputField>().interactable = true;
            }
            
        }
        else if (currentlyActive == 3)
        {
            newToggleFirst.GetComponent<Toggle>().isOn = false;
            newToggleSecond.GetComponent<Toggle>().isOn = false;
            newToggleThird.GetComponent<Toggle>().isOn = true;
            
            newToggleFirst.GetComponent<Toggle>().interactable = true;
            newToggleSecond.GetComponent<Toggle>().interactable = true;
            newToggleThird.GetComponent<Toggle>().interactable = false;
            
            // Activate Input Field
            if (inputFieldBelongsTo == 3)
            {
                newInputField.GetComponent<TMP_InputField>().interactable = true;
            }
            
            
        }
        
        
        
        
        // Add listeners 
        if (firstToggle != "")
        {
            newToggleFirst.GameObject().SetActive(true);
            newToggleFirst.GameObject().GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn)
                {
                    permissionUi.ProcessToggleChangeSpawnedObjects(lineIdentifier, firstToggle); 
                    
                    // Disable other toggles 
                    newToggleSecond.GetComponent<Toggle>().isOn = false;
                    newToggleThird.GetComponent<Toggle>().isOn = false;
                    
                    // Change interactability
                    newToggleFirst.GetComponent<Toggle>().interactable = false;
                    newToggleSecond.GetComponent<Toggle>().interactable = true;
                    newToggleThird.GetComponent<Toggle>().interactable = true;
                    
                    // Activate Input Field or clear content & deactivate
                    if (inputFieldBelongsTo == 1)
                    {
                        newInputField.GetComponent<TMP_InputField>().interactable = true;
                    }
                    else
                    {
                        newInputField.GetComponent<TMP_InputField>().text = "";
                        newInputField.GetComponent<TMP_InputField>().interactable = false;
                    }
                    
                }
                else // Toggle off 
                {
                    // Deactivate Input Field 
                    if (inputFieldBelongsTo == 1)
                    {
                        newInputField.GetComponent<TMP_InputField>().interactable = false;
                    }
                }
            });
        }
        
        if (secondToggle != "")
        {
            newToggleSecond.GameObject().SetActive(true);
            newToggleSecond.GameObject().GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn)
                {
                    permissionUi.ProcessToggleChangeSpawnedObjects(lineIdentifier, secondToggle);
                    
                    // Disable other toggles 
                    newToggleFirst.GetComponent<Toggle>().isOn = false;
                    newToggleThird.GetComponent<Toggle>().isOn = false;
                    
                    // Change interactability
                    newToggleFirst.GetComponent<Toggle>().interactable = true;
                    newToggleSecond.GetComponent<Toggle>().interactable = false;
                    newToggleThird.GetComponent<Toggle>().interactable = true;
                    
                    // Activate Input Field or clear content & deactivate 
                    if (inputFieldBelongsTo == 2)
                    {
                        newInputField.GetComponent<TMP_InputField>().interactable = true;
                    }
                    else
                    {
                        newInputField.GetComponent<TMP_InputField>().text = "";
                        newInputField.GetComponent<TMP_InputField>().interactable = false;
                    }
                    
                }
                else // Toggle off 
                {
                    // Deactivate Input Field 
                    if (inputFieldBelongsTo == 2)
                    {
                        newInputField.GetComponent<TMP_InputField>().interactable = false;
                    }
                }
                
            });
        }
        
        if (thirdToggle != "")
        {
            newToggleThird.GameObject().SetActive(true);
            newToggleThird.GameObject().GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn)
                {
                    permissionUi.ProcessToggleChangeSpawnedObjects(lineIdentifier, thirdToggle);
                    
                    // Disable other toggles 
                    newToggleFirst.GetComponent<Toggle>().isOn = false;
                    newToggleSecond.GetComponent<Toggle>().isOn = false;
                    
                    // Change interactability
                    newToggleFirst.GetComponent<Toggle>().interactable = true;
                    newToggleSecond.GetComponent<Toggle>().interactable = true;
                    newToggleThird.GetComponent<Toggle>().interactable = false;
                    
                    
                    // Activate Input Field or clear content & deactivate 
                    if (inputFieldBelongsTo == 3)
                    {
                        newInputField.GetComponent<TMP_InputField>().interactable = true;
                    }
                    else 
                    {
                        newInputField.GetComponent<TMP_InputField>().text = "";
                        newInputField.GetComponent<TMP_InputField>().interactable = false;
                    }
                    
                }
                else // Toggle off 
                {
                    // Deactivate Input Field 
                    if (inputFieldBelongsTo == 3)
                    {
                        newInputField.GetComponent<TMP_InputField>().interactable = false;
                    }
                }
                
            });
        }
        
        // Add input field listener depending on whether input comes from vr or mouse & Keyboard
        if (inputFromSelectionScreen)
        {
            newInputField.GetComponent<TMP_InputField>().onSelect.AddListener((string content) =>
            {
                // Activate overlay 
                permissionUi.ProcessInputFieldPressedForSelectionInput(lineIdentifier);
            });
        }
        
        // Add input field listener 
        newInputField.GetComponent<TMP_InputField>().onValueChanged.AddListener((string content) =>
        {
            // Send to serverui for further processing 
            permissionUi.ProcessInputFieldChangedSpawnedObjects(lineIdentifier, newInputField.GetComponent<TMP_InputField>().text);
            
        });
        
        

        // Keep track of line contents 
        numberOfActiveLines += 1;
        lineIdentifiers.Add(lineIdentifier);
        texts.Add(newText.GameObject());
        firstToggles.Add(newToggleFirst.GameObject());
        secondToggles.Add(newToggleSecond.GameObject());
        thirdToggles.Add(newToggleThird.GameObject());
        inputFields.Add(newInputField.GameObject());
        scrollViewContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0,
            Mathf.Max((numberOfActiveLines) * lineOffset, minContentHeight));

    }

    public void RemoveLineByIdentifier(int removeIdentifier)
    {
        int removeIndex = lineIdentifiers.IndexOf(removeIdentifier);
        if (removeIndex == -1)
        {
            Debug.Log("[ModifyScrolLViewContent] RemoveLineByIdentifier: Identifier not found in list!");
        }
        else
        {
            RemoveLineByIndex(removeIndex);
        }
        
    }
    
    private void RemoveLineByIndex(int removeIndex)
    {
        // Remove GameObjects 
        Destroy(texts[removeIndex]);
        Destroy(firstToggles[removeIndex]);
        Destroy(secondToggles[removeIndex]);
        Destroy(thirdToggles[removeIndex]);
        
        // Remove line 
        lineIdentifiers.RemoveAt(removeIndex);
        texts.RemoveAt(removeIndex);
        firstToggles.RemoveAt(removeIndex);
        secondToggles.RemoveAt(removeIndex);
        thirdToggles.RemoveAt(removeIndex);
        
        // Move lines up 
        for (int i = 0; i < texts.Count; i++)
        {
            if (i >= removeIndex)
            {
                texts[i].GetComponent<RectTransform>().position += new Vector3(0, +lineOffset,0);
                firstToggles[i].GetComponent<RectTransform>().position += new Vector3(0, +lineOffset,0);
                secondToggles[i].GetComponent<RectTransform>().position += new Vector3(0, +lineOffset,0);
                thirdToggles[i].GetComponent<RectTransform>().position += new Vector3(0, +lineOffset,0);
            }
        }

        numberOfActiveLines -= 1;
        
        // Update scroll view size 
        scrollViewContent.GetComponent<RectTransform>().sizeDelta = new Vector2( 0, 
            Mathf.Max((numberOfActiveLines + 1) * lineOffset, minContentHeight));
        
    }


    public void SetTextFieldContentsByLineId(int lineIdentifier, string textContent)
    {
        // Get idx for identifier 
        int lineIdx = lineIdentifiers.IndexOf(lineIdentifier);

        // Proper format: id1,id2,id3,...,idn;
        inputFields[lineIdx].GetComponent<TMP_InputField>().text = textContent;
    }
    

}
