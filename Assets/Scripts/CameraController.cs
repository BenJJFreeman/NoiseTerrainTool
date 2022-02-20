using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float speed,moveSpeed;
    public float zoomSpeed;

    public float minZoomDist;
    public float maxZoomDist;

    private Camera cam;

    private Vector3 oldMousePos;

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        Vector3 dir = transform.forward * zInput + transform.right * xInput;
        transform.position += dir * moveSpeed * Time.deltaTime;


        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        cam.transform.position += cam.transform.forward * scrollInput * zoomSpeed;



        if (Input.GetMouseButtonDown(2)) {
            oldMousePos = Input.mousePosition;
        }


        if (!Input.GetMouseButton(2))
            return;

        Ray ray = Camera.main.ScreenPointToRay(oldMousePos);
        RaycastHit hit;
        

        if (Physics.Raycast(ray, out hit))
            transform.RotateAround(hit.point, Vector3.up, Input.GetAxis("Mouse X") * speed * Time.deltaTime);

    }
}
