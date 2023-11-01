using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.Sam
{
    public class GetGPSFixAction : Action<GetGPSFixActionGoal, GetGPSFixActionResult, GetGPSFixActionFeedback, GetGPSFixGoal, GetGPSFixResult, GetGPSFixFeedback>
    {
        public const string k_RosMessageName = "sam_msgs/GetGPSFixAction";
        public override string RosMessageName => k_RosMessageName;


        public GetGPSFixAction() : base()
        {
            this.action_goal = new GetGPSFixActionGoal();
            this.action_result = new GetGPSFixActionResult();
            this.action_feedback = new GetGPSFixActionFeedback();
        }

        public static GetGPSFixAction Deserialize(MessageDeserializer deserializer) => new GetGPSFixAction(deserializer);

        GetGPSFixAction(MessageDeserializer deserializer)
        {
            this.action_goal = GetGPSFixActionGoal.Deserialize(deserializer);
            this.action_result = GetGPSFixActionResult.Deserialize(deserializer);
            this.action_feedback = GetGPSFixActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
