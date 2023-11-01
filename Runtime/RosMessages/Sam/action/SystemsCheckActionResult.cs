using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Sam
{
    public class SystemsCheckActionResult : ActionResult<SystemsCheckResult>
    {
        public const string k_RosMessageName = "sam_msgs/SystemsCheckActionResult";
        public override string RosMessageName => k_RosMessageName;


        public SystemsCheckActionResult() : base()
        {
            this.result = new SystemsCheckResult();
        }

        public SystemsCheckActionResult(HeaderMsg header, GoalStatusMsg status, SystemsCheckResult result) : base(header, status)
        {
            this.result = result;
        }
        public static SystemsCheckActionResult Deserialize(MessageDeserializer deserializer) => new SystemsCheckActionResult(deserializer);

        SystemsCheckActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = SystemsCheckResult.Deserialize(deserializer);
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
