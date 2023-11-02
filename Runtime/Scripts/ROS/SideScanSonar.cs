using RosMessageTypes.Smarc;
using UnityEngine;

namespace DefaultNamespace
{
    public class SideScanSonar : Sensor<SidescanMsg>
    {
        Sonar sonarPort;
        Sonar sonarStrb;
        void Start()
        {
            sonarPort = transform.Find("SSS Port").GetComponent<Sonar>();
            sonarStrb = transform.Find("SSS Strb").GetComponent<Sonar>();
        }

        public override bool UpdateSensor(double deltaTime)
        {
            return true;
        }
    }
}
