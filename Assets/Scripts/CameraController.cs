using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float speed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(2))
            return;


        transform.RotateAround(target.position, Vector3.up, Input.GetAxis("Mouse X") * speed* Time.deltaTime);
    }
}
