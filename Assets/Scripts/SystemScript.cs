using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.SpringBoxes;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

public class SystemScript : MonoBehaviour
{
    ROSConnection ros;

    public Transform box1, box2;
    public float simulationTimeDelta = 0.1f;
    public float box1Mass = 1, box2Mass = 1;
    public float springConstant = 50, damperConstant = 0.5f, equilibriumSpringLength = 0.3f;

    private string odeServiceName = "spring_ode_solver";
    private Vector3 box1Curr, box2Curr;
    private bool initialised = false;

    void Awake()
    {
        Time.fixedDeltaTime = simulationTimeDelta;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<SpringOdeSolverRequest, SpringOdeSolverResponse>(odeServiceName);

        // Initialise service
        SpringOdeSolverRequest request = new SpringOdeSolverRequest();
        request.simState = 0;  // Reset
        request.timeDelta = simulationTimeDelta;
        request.spring_constant = springConstant;
        request.damper_constant = damperConstant;
        request.equil_spring_length = equilibriumSpringLength;
        request.obj1mass = box1Mass;
        request.obj2mass = box2Mass;
        request.obj1update = true;
        request.obj2update = true;
        request.obj1initial = Vector3ToPointMsg(box1.position);
        request.obj2initial = Vector3ToPointMsg(box2.position);
        ros.SendServiceMessage<SpringOdeSolverResponse>(odeServiceName, request, (r) => { initialised = true; });

        box1Curr = box1.position;
        box2Curr = box2.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!initialised) return;  // Wait for initialisation

        // Check if grabbed
        bool box1grabbed = box1
            .GetChild(0)
            .gameObject
            .GetComponent<GrabFreeTransformerEventBool>()
            .isGrabbed();
        bool box2grabbed = box2
            .GetChild(0)
            .gameObject
            .GetComponent<GrabFreeTransformerEventBool>()
            .isGrabbed();

        if (box1grabbed) {
            box1Curr = box1.position;
        } else {
            box1.position = box1Curr;
        }
        if (box2grabbed) {
            box2Curr = box2.position;
        } else {
            box2.position = box2Curr;
        }

        // Prepare message
        SpringOdeSolverRequest request = new SpringOdeSolverRequest();
        request.timeDelta = simulationTimeDelta;
        request.simState = 1;
        request.spring_constant = springConstant;
        request.damper_constant = damperConstant;
        request.equil_spring_length = equilibriumSpringLength;
        request.obj1mass = box1Mass;
        request.obj2mass = box2Mass;
        request.obj1update = !box1grabbed;  // If currently grabbed, don't update in solver
        request.obj2update = !box2grabbed;
        request.obj1initial = Vector3ToPointMsg(box1Curr);
        request.obj2initial = Vector3ToPointMsg(box2Curr);

        // Service call
        ros.SendServiceMessage<SpringOdeSolverResponse>(odeServiceName, request, ServiceCallback);
    }

    void OnDestroy()
    {
        SendTerminate();
    }

    void OnApplicationQuit()
    {
        SendTerminate();
    }

    void SendTerminate()
    {
        SpringOdeSolverRequest request = new SpringOdeSolverRequest();
        request.simState = 2;  // Terminate
        request.timeDelta = simulationTimeDelta;
        request.spring_constant = springConstant;
        request.damper_constant = damperConstant;
        request.equil_spring_length = equilibriumSpringLength;
        request.obj1mass = box1Mass;
        request.obj2mass = box2Mass;
        request.obj1update = false;
        request.obj2update = false;
        request.obj1initial = Vector3ToPointMsg(box1.position);
        request.obj2initial = Vector3ToPointMsg(box2.position);
        ros.SendServiceMessage<SpringOdeSolverResponse>(odeServiceName, request, (r) => {});
    }

    void ServiceCallback(SpringOdeSolverResponse response)
    {
        box1Curr = PointMsgToVector3(response.obj1final);
        box2Curr = PointMsgToVector3(response.obj2final);
    }

    Vector3 PointMsgToVector3(PointMsg msg) {
        return new Vector3((float) msg.x, (float) msg.y, (float) msg.z);
    }

    PointMsg Vector3ToPointMsg(Vector3 vec) {
        return new PointMsg(vec.x, vec.y, vec.z);
    }
}
