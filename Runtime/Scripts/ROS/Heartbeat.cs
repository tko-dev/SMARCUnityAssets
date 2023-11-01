using RosMessageTypes.Std;

namespace DefaultNamespace
{
    public class Heartbeat : Sensor<EmptyMsg>
    {
        public override bool UpdateSensor(double deltaTime)
        {
           return true; 
        }
    }
}