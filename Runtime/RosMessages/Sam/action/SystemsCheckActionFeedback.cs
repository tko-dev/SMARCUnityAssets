using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Sam
{
    public class SystemsCheckActionFeedback : ActionFeedback<SystemsCheckFeedback>
    {
        public const string k_RosMessageName = "sam_msgs/SystemsCheckActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public SystemsCheckActionFeedback() : base()
        {
            this.feedback = new SystemsCheckFeedback();
        }

        public SystemsCheckActionFeedback(HeaderMsg header, GoalStatusMsg status, SystemsCheckFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static SystemsCheckActionFeedback Deserialize(MessageDeserializer deserializer) => new SystemsCheckActionFeedback(deserializer);

        SystemsCheckActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = SystemsCheckFeedback.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.feedback);
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
