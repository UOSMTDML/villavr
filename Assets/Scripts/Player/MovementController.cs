using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovementController : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 moveDir = new Vector3(0,0,0);
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
        float moveSpeed = 3f;
        
        // Multiply target direction with current view direction, project onto ground plane 
        transform.position += Vector3.ProjectOnPlane(transform.localRotation * moveDir, Vector3.up) * moveSpeed * Time.deltaTime; 
    
    }
}
