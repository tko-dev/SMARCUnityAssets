using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Sam
{
    public class SystemsCheckActionGoal : ActionGoal<SystemsCheckGoal>
    {
        public const string k_RosMessageName = "sam_msgs/SystemsCheckActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public SystemsCheckActionGoal() : base()
        {
            this.goal = new SystemsCheckGoal();
        }

        public SystemsCheckActionGoal(HeaderMsg header, GoalIDMsg goal_id, SystemsCheckGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static SystemsCheckActionGoal Deserialize(MessageDeserializer deserializer) => new SystemsCheckActionGoal(deserializer);

        SystemsCheckActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = SystemsCheckGoal.Deserialize(deserializer);
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
