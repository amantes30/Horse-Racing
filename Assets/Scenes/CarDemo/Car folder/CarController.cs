using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CarController : MonoBehaviour
{
    
    
    [TextArea(0, 1)]
    [SerializeField] string Name;
    [Space(5)]

    [Header("The WheelColliders of the car")]
    [Tooltip("The WheelColliders of the car")]
    
    [SerializeField] private List<WheelCollider> wheelColliders;
    [SerializeField] private List<Transform> wheelObjs;
    [Space(5)]
    [Header("Adjust Car Speed")]
    [SerializeField] private int speed = 400;
    [Space(5)]
    
    [Header("From tire turning limit")]
    [SerializeField] private float TurnAngle = 50;
    
    private float horizontalInput, verticalInput;
    [Space(5)]
    [Header("The Camera for the Car")]
    
    public GameObject camera;

    

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.Instantiate(camera);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameObject.name = Name;
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        Steer();
        Acc();
        UpdatePos();
        //camera.Rotate(transform.rotation.eulerAngles*horizontalInput);
        camera.transform.position = transform.GetChild(2).position;
        
        camera.transform.rotation = new Quaternion(  transform.localRotation.x, camera.transform.rotation.y, camera.transform.rotation.z, transform.localRotation.w) ;
        //camera.LookAt(transform.GetChild(2).position);
    }


    
    public void Acc()
    {
        wheelColliders[0].motorTorque = verticalInput * speed;
        wheelColliders[1].motorTorque = verticalInput * speed;
    }
    void Steer()
    {
        wheelColliders[0].steerAngle = horizontalInput * TurnAngle;
        wheelColliders[1].steerAngle = horizontalInput * TurnAngle;
    }
    private void UpdatePos()
    {
        for(int i=0; i < wheelColliders.Count; i++)
        {
            UpdateWheelTransform(wheelObjs[i],wheelColliders[i]);
        }
    }

    void UpdateWheelTransform(Transform _t, WheelCollider _w)
    {
        Vector3 pos = _t.position;
        Quaternion rot = _t.rotation;

        _w.GetWorldPose(out pos, out rot);

        _t.position = pos;
        _t.rotation = rot;
    }
}
