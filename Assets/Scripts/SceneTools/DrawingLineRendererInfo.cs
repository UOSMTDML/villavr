using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingLineRendererInfo : MonoBehaviour
{

    //ExperienceManager.Singleton.drawingMaterials
    public int colorIdx;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void GenerateLineRendererColliders()
    {

        StartCoroutine(GenerateColliders());
    }
    
    
    
    // Produce little boxes of colliders along side the linerenderer to enable finding if trigger collision 
    private IEnumerator GenerateColliders()
    {

        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        Transform lineRendererTransform = lineRenderer.transform;
        
        // Get linerenderer positions 
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);

        for(int posIdx = 0; posIdx < positions.Length - 1; posIdx++) // let posIdx run until 1 idx earlier to make sure next idx is always available 
        {
            Vector3 firstPos = positions[posIdx];
            Vector3 secondPos = positions[posIdx + 1];

            // Create cube at position of first line renderer position
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = ExperienceManager.Singleton.drawingColliderPrefix + posIdx.ToString();
            cube.transform.SetParent(lineRendererTransform);
            cube.transform.position = new Vector3(firstPos.x, firstPos.y, firstPos.z);
            
            // Make cubes triggers & disable rendering 
            cube.GetComponent<Collider>().isTrigger = true;
            cube.GetComponent<Renderer>().enabled = false;
            
            // Calculate distance between linerenderer points & make cube that length 
            float length = Vector3.Distance(firstPos, secondPos);
            cube.transform.localScale = new Vector3(ExperienceManager.Singleton.drawingToolWidth * 3, ExperienceManager.Singleton.drawingToolWidth * 3, length);
            
            // Make cube face next point and shift by half the distance since pivot is in center of cube 
            cube.transform.LookAt(secondPos);
            cube.transform.position += cube.transform.forward * length / 2;
            
        }
        
        yield break;
    }
    
    
    
}
