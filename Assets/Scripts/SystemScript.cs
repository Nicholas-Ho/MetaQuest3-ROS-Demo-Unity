using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.SpringBoxes;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Oculus.Platform;

enum SystemStatus
{
    Inactive,
    Active
}

// Utility class encapsulating data about boxes
[System.Serializable]
public class BoxParams {
    public Transform transform;
    public float mass = 1;
    private string rosId;
    private string rosTopic;
    private Vector3 position;

    // Getters and Setters
    public string id { get=>rosId; }
    public string subTopicName { get=>rosTopic; }
    public Vector3 lastUpdatedPos {get=>position; set=>position=value;}

    // Constructor
    public BoxParams(string id, string topic) { rosId=id; rosTopic=topic; }

    // Utility
    public bool isGrabbed() {
        return transform
            .GetChild(0)
            .gameObject
            .GetComponent<GrabFreeTransformerEventBool>()
            .isGrabbed();
    }
}

public class SystemScript : MonoBehaviour
{
    ROSConnection ros;

    // Physics
    public BoxParams box1 = new BoxParams("box1", "box1/box_state");
    public BoxParams box2 = new BoxParams("box2", "box2/box_state");
    public float springConstant = 50, damperConstant = 0.5f, equilibriumSpringLength = 0.3f;

    // Mapping IDs to BoxParams
    private Dictionary<string, BoxParams> idBoxMap = new Dictionary<string, BoxParams>();

    // ROS setup
    private string pubTopicName = "unity_updates";
    private SystemStatus systemStatus = SystemStatus.Inactive;

    // Start is called before the first frame update
    void Start()
    {
        // Start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<ObjectStateMsg>(box1.subTopicName, SubscribeCallback);
        ros.Subscribe<ObjectStateMsg>(box2.subTopicName, SubscribeCallback);
        ros.RegisterPublisher<UnityUpdateMsg>(pubTopicName);
    }

    // Update is called once per frame
    void Update()
    {
        if (systemStatus == SystemStatus.Inactive) { Initialise(); }
        if (systemStatus != SystemStatus.Active) return;  // Wait for initialisation

        // Check if grabbed and update states accordingly
        if (box1.isGrabbed()) {
            box1.lastUpdatedPos = box1.transform.position;
        } else {
            box1.transform.position = box1.lastUpdatedPos;
        }
        if (box2.isGrabbed()) {
            box2.lastUpdatedPos = box2.transform.position;
        } else {
            box2.transform.position = box2.lastUpdatedPos;
        }

        // Prepare message
         UnityUpdateMsg msg = new UnityUpdateMsg(
            1,  // Running
            new UnityObjDataMsg[2] {
                new UnityObjDataMsg(box1.id,
                                    !box1.isGrabbed(),  // If currently grabbed, don't update in solver
                                    Vector3ToPointMsg(box1.transform.position),
                                    box1.mass),
                new UnityObjDataMsg(box2.id,
                                    !box2.isGrabbed(),
                                    Vector3ToPointMsg(box2.transform.position),
                                    box2.mass),
            },
            new SystemParamsMsg(springConstant, damperConstant, equilibriumSpringLength),
            Time.deltaTime
        );

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
        UnityUpdateMsg msg = new UnityUpdateMsg(
            0,  // Reset
            new UnityObjDataMsg[2] {
                new UnityObjDataMsg(box1.id,
                                    true,
                                    Vector3ToPointMsg(box1.transform.position),
                                    box1.mass),
                new UnityObjDataMsg(box2.id,
                                    true,
                                    Vector3ToPointMsg(box2.transform.position),
                                    box2.mass),
            },
            new SystemParamsMsg(springConstant, damperConstant, equilibriumSpringLength),
            1  // Placeholder, should not matter
        );

        ros.Publish(pubTopicName, msg);

        box1.lastUpdatedPos = box1.transform.position;
        box2.lastUpdatedPos = box2.transform.position;
    }

    void SendTerminate()
    {
         UnityUpdateMsg msg = new UnityUpdateMsg(
            2,  // Terminate
            new UnityObjDataMsg[2] {
                new UnityObjDataMsg(box1.id,
                                    false,
                                    Vector3ToPointMsg(box1.transform.position),
                                    box1.mass),
                new UnityObjDataMsg(box2.id,
                                    false,
                                    Vector3ToPointMsg(box2.transform.position),
                                    box2.mass),
            },
            new SystemParamsMsg(springConstant, damperConstant, equilibriumSpringLength),
            1  // Placeholder, should not matter
        );

        ros.Publish(pubTopicName, msg);
    }

    void SubscribeCallback(ObjectStateMsg response)
    {
        BoxParams updatedBox = GetBoxParamsFromId(response.id);
        updatedBox.lastUpdatedPos = PointMsgToVector3(response.position);
        systemStatus = SystemStatus.Active;
    }

    BoxParams GetBoxParamsFromId(string id) {
        // Lazy initialisation
        if (idBoxMap.Count == 0) {
            idBoxMap[box1.id] = box1;
            idBoxMap[box2.id] = box2;
        }
        return idBoxMap[id];
    }

    Vector3 PointMsgToVector3(PointMsg msg) {
        return new Vector3((float) msg.x, (float) msg.y, (float) msg.z);
    }

    PointMsg Vector3ToPointMsg(Vector3 vec) {
        return new PointMsg(vec.x, vec.y, vec.z);
    }
}
