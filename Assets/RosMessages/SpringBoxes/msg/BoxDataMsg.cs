//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.SpringBoxes
{
    [Serializable]
    public class BoxDataMsg : Message
    {
        public const string k_RosMessageName = "spring_boxes/BoxData";
        public override string RosMessageName => k_RosMessageName;

        public bool update;
        public Geometry.PointMsg position;
        public double mass;

        public BoxDataMsg()
        {
            this.update = false;
            this.position = new Geometry.PointMsg();
            this.mass = 0.0;
        }

        public BoxDataMsg(bool update, Geometry.PointMsg position, double mass)
        {
            this.update = update;
            this.position = position;
            this.mass = mass;
        }

        public static BoxDataMsg Deserialize(MessageDeserializer deserializer) => new BoxDataMsg(deserializer);

        private BoxDataMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.update);
            this.position = Geometry.PointMsg.Deserialize(deserializer);
            deserializer.Read(out this.mass);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.update);
            serializer.Write(this.position);
            serializer.Write(this.mass);
        }

        public override string ToString()
        {
            return "BoxDataMsg: " +
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