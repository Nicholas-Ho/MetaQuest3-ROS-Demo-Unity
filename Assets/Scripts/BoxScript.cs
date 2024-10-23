using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class BoxScript : MonoBehaviour
{
    ROSConnection ros;

    public float publishFrequency = 0.5f;
    
    private string topicName = "position";
    private float timeElapsed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PointMsg>(topicName);
    }

    // Update is called once per frame
    void Update()
    {
        PointMsg positionMsg = new PointMsg(
            transform.position.x,
            transform.position.y,
            transform.position.z
        );
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishFrequency) {
            ros.Publish(topicName, positionMsg);
            timeElapsed = 0;
        }
    }
}
