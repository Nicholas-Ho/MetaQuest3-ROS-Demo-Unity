using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.SpringBoxes;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

enum SystemStatus
{
    Inactive,
    Activating,
    Active
}

public class SystemScript : MonoBehaviour
{
    ROSConnection ros;

    public Transform box1, box2;
    public float box1Mass = 1, box2Mass = 1;
    public float springConstant = 50, damperConstant = 0.5f, equilibriumSpringLength = 0.3f;

    private string subTopicName = "spring_system_state";
    private string pubTopicName = "unity_updates";
    private Vector3 box1Curr, box2Curr;
    private SystemStatus systemStatus = SystemStatus.Inactive;

    // Start is called before the first frame update
    void Start()
    {
        // Start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<SpringSystemStateMsg>(subTopicName, SubscribeCallback);
        ros.RegisterPublisher<UnityUpdateMsg>(pubTopicName);
    }

    // Update is called once per frame
    void Update()
    {
        if (systemStatus == SystemStatus.Inactive) { Initialise(); }
        if (systemStatus != SystemStatus.Active) return;  // Wait for initialisation

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
        UnityUpdateMsg msg = new UnityUpdateMsg();
        msg.simState = 1;  // Running
        msg.timeDelta = Time.deltaTime; // Placeholder, Should not matter
        msg.systemParams.spring_constant = springConstant;
        msg.systemParams.damper_constant = damperConstant;
        msg.systemParams.equil_spring_length = equilibriumSpringLength;
        msg.box1data.mass = box1Mass;
        msg.box1data.update = !box1grabbed;  // If currently grabbed, don't update in solver
        msg.box1data.position = Vector3ToPointMsg(box1.position);
        msg.box2data.mass = box2Mass;
        msg.box2data.update = !box2grabbed;  // If currently grabbed, don't update in solver
        msg.box2data.position = Vector3ToPointMsg(box2.position);

        ros.Publish(pubTopicName, msg);
    }

    void OnDestroy()
    {
        SendTerminate();
    }

    void OnApplicationQuit()
    {
        SendTerminate();
    }

    void Initialise()
    {
        UnityUpdateMsg msg = new UnityUpdateMsg();
        msg.simState = 0;  // Reset
        msg.timeDelta = 1; // Placeholder, Should not matter
        msg.systemParams.spring_constant = springConstant;
        msg.systemParams.damper_constant = damperConstant;
        msg.systemParams.equil_spring_length = equilibriumSpringLength;
        msg.box1data.mass = box1Mass;
        msg.box1data.update = true;
        msg.box1data.position = Vector3ToPointMsg(box1.position);
        msg.box2data.mass = box2Mass;
        msg.box2data.update = true;
        msg.box2data.position = Vector3ToPointMsg(box2.position);

        ros.Publish(pubTopicName, msg);

        box1Curr = box1.position;
        box2Curr = box2.position;
    }

    void SendTerminate()
    {
        UnityUpdateMsg msg = new UnityUpdateMsg();
        msg.simState = 2;  // Terminate
        msg.timeDelta = 1; // Placeholder, Should not matter
        msg.systemParams.spring_constant = springConstant;
        msg.systemParams.damper_constant = damperConstant;
        msg.systemParams.equil_spring_length = equilibriumSpringLength;
        msg.box1data.mass = box1Mass;
        msg.box1data.update = false;
        msg.box1data.position = Vector3ToPointMsg(box1.position);
        msg.box2data.mass = box2Mass;
        msg.box2data.update = false;
        msg.box2data.position = Vector3ToPointMsg(box2.position);

        ros.Publish(pubTopicName, msg);
    }

    void SubscribeCallback(SpringSystemStateMsg response)
    {
        box1Curr = PointMsgToVector3(response.obj1position);
        box2Curr = PointMsgToVector3(response.obj2position);
        systemStatus = SystemStatus.Active;
    }

    Vector3 PointMsgToVector3(PointMsg msg) {
        return new Vector3((float) msg.x, (float) msg.y, (float) msg.z);
    }

    PointMsg Vector3ToPointMsg(Vector3 vec) {
        return new PointMsg(vec.x, vec.y, vec.z);
    }
}
