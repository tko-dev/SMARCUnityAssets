using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Sam
{
    public class GetGPSFixActionFeedback : ActionFeedback<GetGPSFixFeedback>
    {
        public const string k_RosMessageName = "sam_msgs/GetGPSFixActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public GetGPSFixActionFeedback() : base()
        {
            this.feedback = new GetGPSFixFeedback();
        }

        public GetGPSFixActionFeedback(HeaderMsg header, GoalStatusMsg status, GetGPSFixFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static GetGPSFixActionFeedback Deserialize(MessageDeserializer deserializer) => new GetGPSFixActionFeedback(deserializer);

        GetGPSFixActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = GetGPSFixFeedback.Deserialize(deserializer);
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
