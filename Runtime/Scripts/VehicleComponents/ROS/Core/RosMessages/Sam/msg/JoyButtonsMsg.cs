//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Sam
{
    [Serializable]
    public class JoyButtonsMsg : Message
    {
        public const string k_RosMessageName = "sam_msgs/JoyButtons";
        public override string RosMessageName => k_RosMessageName;

        public Std.HeaderMsg header;
        public bool teleop_enable;
        public bool assited_driving;
        //  left joystick
        public float left_x;
        public float left_y;
        //  right joystick
        public float right_x;
        public float right_y;
        //  d-pad
        public bool d_up;
        public bool d_down;
        public bool d_left;
        public bool d_right;
        //  shoulder buttons
        public bool shoulder_l1;
        public bool shoulder_r1;
        public float shoulder_l2;
        public float shoulder_r2;

        public JoyButtonsMsg()
        {
            this.header = new Std.HeaderMsg();
            this.teleop_enable = false;
            this.assited_driving = false;
            this.left_x = 0.0f;
            this.left_y = 0.0f;
            this.right_x = 0.0f;
            this.right_y = 0.0f;
            this.d_up = false;
            this.d_down = false;
            this.d_left = false;
            this.d_right = false;
            this.shoulder_l1 = false;
            this.shoulder_r1 = false;
            this.shoulder_l2 = 0.0f;
            this.shoulder_r2 = 0.0f;
        }

        public JoyButtonsMsg(Std.HeaderMsg header, bool teleop_enable, bool assited_driving, float left_x, float left_y, float right_x, float right_y, bool d_up, bool d_down, bool d_left, bool d_right, bool shoulder_l1, bool shoulder_r1, float shoulder_l2, float shoulder_r2)
        {
            this.header = header;
            this.teleop_enable = teleop_enable;
            this.assited_driving = assited_driving;
            this.left_x = left_x;
            this.left_y = left_y;
            this.right_x = right_x;
            this.right_y = right_y;
            this.d_up = d_up;
            this.d_down = d_down;
            this.d_left = d_left;
            this.d_right = d_right;
            this.shoulder_l1 = shoulder_l1;
            this.shoulder_r1 = shoulder_r1;
            this.shoulder_l2 = shoulder_l2;
            this.shoulder_r2 = shoulder_r2;
        }

        public static JoyButtonsMsg Deserialize(MessageDeserializer deserializer) => new JoyButtonsMsg(deserializer);

        private JoyButtonsMsg(MessageDeserializer deserializer)
        {
            this.header = Std.HeaderMsg.Deserialize(deserializer);
            deserializer.Read(out this.teleop_enable);
            deserializer.Read(out this.assited_driving);
            deserializer.Read(out this.left_x);
            deserializer.Read(out this.left_y);
            deserializer.Read(out this.right_x);
            deserializer.Read(out this.right_y);
            deserializer.Read(out this.d_up);
            deserializer.Read(out this.d_down);
            deserializer.Read(out this.d_left);
            deserializer.Read(out this.d_right);
            deserializer.Read(out this.shoulder_l1);
            deserializer.Read(out this.shoulder_r1);
            deserializer.Read(out this.shoulder_l2);
            deserializer.Read(out this.shoulder_r2);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.teleop_enable);
            serializer.Write(this.assited_driving);
            serializer.Write(this.left_x);
            serializer.Write(this.left_y);
            serializer.Write(this.right_x);
            serializer.Write(this.right_y);
            serializer.Write(this.d_up);
            serializer.Write(this.d_down);
            serializer.Write(this.d_left);
            serializer.Write(this.d_right);
            serializer.Write(this.shoulder_l1);
            serializer.Write(this.shoulder_r1);
            serializer.Write(this.shoulder_l2);
            serializer.Write(this.shoulder_r2);
        }

        public override string ToString()
        {
            return "JoyButtonsMsg: " +
            "\nheader: " + header.ToString() +
            "\nteleop_enable: " + teleop_enable.ToString() +
            "\nassited_driving: " + assited_driving.ToString() +
            "\nleft_x: " + left_x.ToString() +
            "\nleft_y: " + left_y.ToString() +
            "\nright_x: " + right_x.ToString() +
            "\nright_y: " + right_y.ToString() +
            "\nd_up: " + d_up.ToString() +
            "\nd_down: " + d_down.ToString() +
            "\nd_left: " + d_left.ToString() +
            "\nd_right: " + d_right.ToString() +
            "\nshoulder_l1: " + shoulder_l1.ToString() +
            "\nshoulder_r1: " + shoulder_r1.ToString() +
            "\nshoulder_l2: " + shoulder_l2.ToString() +
            "\nshoulder_r2: " + shoulder_r2.ToString();
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