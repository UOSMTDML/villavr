using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BezierCurver : MonoBehaviour
{
    private Transform startTransform;
    private Transform endTransform;
    private LineRenderer lineRenderer;
    [SerializeField] private int vertexCount = 12;
    [SerializeField] private float middleUpFraction = 0.3f;

    private bool isReady; 

    // Use this for initialization
    void Start()
    {
        if (vertexCount < 1)
        {
            Debug.Log("[BezierCurver] Vertex count must be at least 1. Setting to 1.");
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isReady)
        {
            // Calculate a middle point 
            Vector3 startEndDirection = endTransform.position - startTransform.position;
            Vector3 middlePosition = startTransform.position + 0.5f * startEndDirection + startTransform.up * middleUpFraction ; 
            
            
            List<Vector3> pointList = new List<Vector3>();
            for (float ratio = 0; ratio <= 1; ratio += 1.0f / vertexCount)
            {
                Vector3 tangentLineVertex1 = Vector3.Lerp(startTransform.position, middlePosition, ratio);
                Vector3 tangentLineVertex2 = Vector3.Lerp(middlePosition, endTransform.position, ratio);
                Vector3 bezierPoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
                pointList.Add(bezierPoint);
            }
            lineRenderer.positionCount = pointList.Count;
            lineRenderer.SetPositions(pointList.ToArray());
        }
        
    }
    
    
    public void Setup(Transform start, Transform end, LineRenderer lr)
    {
        startTransform = start;
        endTransform = end;
        lineRenderer = lr;

        isReady = true;
    }
    
    
    
}