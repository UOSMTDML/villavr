using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SurrogateHands : NetworkBehaviour
{

    
    //Surrogate
    [SerializeField] private GameObject surrogateHandRootLeft;
    [SerializeField] private SkinnedMeshRenderer surrogateReferenceMeshRendererLeft; // there might be multiple renderers, so specify which one to use as reference
    [SerializeField] private GameObject surrogateHandRootRight;
    [SerializeField] private SkinnedMeshRenderer surrogateReferenceMeshRendererRight; // there might be multiple renderers, so specify which one to use as reference
    
    
    // Source
    [SerializeField] private GameObject sourceHandRootLeftVive;
    [SerializeField] private SkinnedMeshRenderer sourceReferenceMeshRendererLeftVive; // there might be multiple renderers, so specify which one to use as reference
    [SerializeField] private GameObject sourceHandRootRightVive;
    [SerializeField] private SkinnedMeshRenderer sourceReferenceMeshRendererRightVive; // there might be multiple renderers, so specify which one to use as reference
    
    // Source
    [SerializeField] private GameObject sourceHandRootLeftOculus;
    [SerializeField] private SkinnedMeshRenderer sourceReferenceMeshRendererLeftOculus; // there might be multiple renderers, so specify which one to use as reference
    [SerializeField] private GameObject sourceHandRootRightOculus;
    [SerializeField] private SkinnedMeshRenderer sourceReferenceMeshRendererRightOculus; // there might be multiple renderers, so specify which one to use as reference

    
    
    // Debug 
    [SerializeField] private bool debugShowSurrogatesAndOffset = false;
    
    // Start is called before the first frame update
    void Start()
    {
        // DeActivate Surrogate Hands Renderers (skinnedmeshrenderers) if spawned on local machine
        if (IsOwner)
        {
            if (!debugShowSurrogatesAndOffset)
            {

                foreach (Renderer rend in surrogateHandRootLeft.GetComponentsInChildren<Renderer>())
                {
                    rend.enabled = false;
                }

                foreach (Renderer rend in surrogateHandRootRight.GetComponentsInChildren<Renderer>())
                {
                    rend.enabled = false;
                }
            }
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        // Sync surrogate hand's position & rotation; update bone transforms 
        if (IsOwner)
        {
            
            // Check which type of input is used 
            if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerViveInput)
            {
                surrogateHandRootLeft.transform.position = sourceHandRootLeftVive.transform.position;
                surrogateHandRootLeft.transform.rotation = sourceHandRootLeftVive.transform.rotation;
                surrogateHandRootRight.transform.position = sourceHandRootRightVive.transform.position;
                surrogateHandRootRight.transform.rotation = sourceHandRootRightVive.transform.rotation;

            
                for(int idx = 0; idx < surrogateReferenceMeshRendererLeft.bones.Length; idx++)
                {
                    surrogateReferenceMeshRendererLeft.bones[idx].position =
                        sourceReferenceMeshRendererLeftVive.bones[idx].position;
                    surrogateReferenceMeshRendererLeft.bones[idx].rotation =
                        sourceReferenceMeshRendererLeftVive.bones[idx].rotation;
                
                
                    surrogateReferenceMeshRendererRight.bones[idx].position =
                        sourceReferenceMeshRendererRightVive.bones[idx].position;
                    surrogateReferenceMeshRendererRight.bones[idx].rotation =
                        sourceReferenceMeshRendererRightVive.bones[idx].rotation;


                    if (debugShowSurrogatesAndOffset)
                    {
                        surrogateReferenceMeshRendererLeft.bones[idx].position += new Vector3(0,0.2f,0);
                        surrogateReferenceMeshRendererRight.bones[idx].position += new Vector3(0,0.2f,0);
                    }
                
                };
            }
            
            else if (ExperienceManager.Singleton.playerType == ExperienceManager.PlayerType.PlayerOculusInput)
            {
                surrogateHandRootLeft.transform.position = sourceHandRootLeftOculus.transform.position;
                surrogateHandRootLeft.transform.rotation = sourceHandRootLeftOculus.transform.rotation;
                surrogateHandRootRight.transform.position = sourceHandRootRightOculus.transform.position;
                surrogateHandRootRight.transform.rotation = sourceHandRootRightOculus.transform.rotation;

            
                for(int idx = 0; idx < surrogateReferenceMeshRendererLeft.bones.Length; idx++)
                {
                    surrogateReferenceMeshRendererLeft.bones[idx].position =
                        sourceReferenceMeshRendererLeftOculus.bones[idx].position;
                    surrogateReferenceMeshRendererLeft.bones[idx].rotation =
                        sourceReferenceMeshRendererLeftOculus.bones[idx].rotation;
                
                
                    surrogateReferenceMeshRendererRight.bones[idx].position =
                        sourceReferenceMeshRendererRightOculus.bones[idx].position;
                    surrogateReferenceMeshRendererRight.bones[idx].rotation =
                        sourceReferenceMeshRendererRightOculus.bones[idx].rotation;


                    if (debugShowSurrogatesAndOffset)
                    {
                        surrogateReferenceMeshRendererLeft.bones[idx].position += new Vector3(0,0.2f,0);
                        surrogateReferenceMeshRendererRight.bones[idx].position += new Vector3(0,0.2f,0);
                    }
                
                };
            }
            
            // If input is other, move surrogate hands below map  
            else
            {
                surrogateHandRootLeft.transform.position = new Vector3(-1000, 1000, 1000);
                surrogateHandRootRight.transform.position = new Vector3(-1000, 1000, 1000);

            }
            
            
        }
    }

    
    
}
