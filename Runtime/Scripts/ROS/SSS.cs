using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Smarc;

namespace DefaultNamespace
{
    public class SSS : Sensor<SidescanMsg>
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
