using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Sam
{
    public class GetGPSFixActionResult : ActionResult<GetGPSFixResult>
    {
        public const string k_RosMessageName = "sam_msgs/GetGPSFixActionResult";
        public override string RosMessageName => k_RosMessageName;


        public GetGPSFixActionResult() : base()
        {
            this.result = new GetGPSFixResult();
        }

        public GetGPSFixActionResult(HeaderMsg header, GoalStatusMsg status, GetGPSFixResult result) : base(header, status)
        {
            this.result = result;
        }
        public static GetGPSFixActionResult Deserialize(MessageDeserializer deserializer) => new GetGPSFixActionResult(deserializer);

        GetGPSFixActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = GetGPSFixResult.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.result);
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
