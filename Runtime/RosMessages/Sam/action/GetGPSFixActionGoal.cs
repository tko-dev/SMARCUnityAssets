using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Sam
{
    public class GetGPSFixActionGoal : ActionGoal<GetGPSFixGoal>
    {
        public const string k_RosMessageName = "sam_msgs/GetGPSFixActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public GetGPSFixActionGoal() : base()
        {
            this.goal = new GetGPSFixGoal();
        }

        public GetGPSFixActionGoal(HeaderMsg header, GoalIDMsg goal_id, GetGPSFixGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static GetGPSFixActionGoal Deserialize(MessageDeserializer deserializer) => new GetGPSFixActionGoal(deserializer);

        GetGPSFixActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = GetGPSFixGoal.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.goal_id);
            serializer.Write(this.goal);
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
