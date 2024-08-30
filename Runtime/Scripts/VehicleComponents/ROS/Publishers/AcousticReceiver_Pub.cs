using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Smarc; // StringStampedMsg
using TX = VehicleComponents.Acoustics.Transceiver;
using StringStamped = VehicleComponents.Acoustics.StringStamped;

namespace VehicleComponents.ROS.Publishers
{

    [RequireComponent(typeof(TX))]
    public class AcousticReceiver_Pub : MonoBehaviour
    {
        ROSConnection ros;
        [Header("ROS Publisher")]
        [Tooltip("The topic will be namespaced under the root objects name if the given topic does not start with '/'.")]
        public string topic;

        TX tx;
        StringStampedMsg ROSMsg;


        void Start()
        {
            if(topic[0] != '/') topic = $"/{transform.root.name}/{topic}";
            ROSMsg = new StringStampedMsg();
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<StringStampedMsg>(topic);

            tx = GetComponent<TX>();
            if(tx == null)
            {
                Debug.Log("No transceiver found!");
                return;
            }
        }

        void FixedUpdate()
        {
            StringStamped dp = tx.Read();
            if(dp != null)
            {
                ROSMsg.data = dp.Data;
                ROSMsg.time_sent = new TimeStamp(dp.TimeSent);
                ROSMsg.time_received = new TimeStamp(dp.TimeReceived);
                ros.Publish(topic, ROSMsg);
            }
            
        }
    }
}