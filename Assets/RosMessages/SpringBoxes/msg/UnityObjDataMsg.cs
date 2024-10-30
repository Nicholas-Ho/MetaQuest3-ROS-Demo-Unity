//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.SpringBoxes
{
    [Serializable]
    public class UnityObjDataMsg : Message
    {
        public const string k_RosMessageName = "spring_boxes/UnityObjData";
        public override string RosMessageName => k_RosMessageName;

        public string id;
        public bool update;
        public Geometry.PointMsg position;
        public double mass;

        public UnityObjDataMsg()
        {
            this.id = "";
            this.update = false;
            this.position = new Geometry.PointMsg();
            this.mass = 0.0;
        }

        public UnityObjDataMsg(string id, bool update, Geometry.PointMsg position, double mass)
        {
            this.id = id;
            this.update = update;
            this.position = position;
            this.mass = mass;
        }

        public static UnityObjDataMsg Deserialize(MessageDeserializer deserializer) => new UnityObjDataMsg(deserializer);

        private UnityObjDataMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.id);
            deserializer.Read(out this.update);
            this.position = Geometry.PointMsg.Deserialize(deserializer);
            deserializer.Read(out this.mass);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.id);
            serializer.Write(this.update);
            serializer.Write(this.position);
            serializer.Write(this.mass);
        }

        public override string ToString()
        {
            return "UnityObjDataMsg: " +
            "\nid: " + id.ToString() +
            "\nupdate: " + update.ToString() +
            "\nposition: " + position.ToString() +
            "\nmass: " + mass.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
