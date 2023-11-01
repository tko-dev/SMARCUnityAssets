using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.Sam
{
    public class SystemsCheckAction : Action<SystemsCheckActionGoal, SystemsCheckActionResult, SystemsCheckActionFeedback, SystemsCheckGoal, SystemsCheckResult, SystemsCheckFeedback>
    {
        public const string k_RosMessageName = "sam_msgs/SystemsCheckAction";
        public override string RosMessageName => k_RosMessageName;


        public SystemsCheckAction() : base()
        {
            this.action_goal = new SystemsCheckActionGoal();
            this.action_result = new SystemsCheckActionResult();
            this.action_feedback = new SystemsCheckActionFeedback();
        }

        public static SystemsCheckAction Deserialize(MessageDeserializer deserializer) => new SystemsCheckAction(deserializer);

        SystemsCheckAction(MessageDeserializer deserializer)
        {
            this.action_goal = SystemsCheckActionGoal.Deserialize(deserializer);
            this.action_result = SystemsCheckActionResult.Deserialize(deserializer);
            this.action_feedback = SystemsCheckActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
