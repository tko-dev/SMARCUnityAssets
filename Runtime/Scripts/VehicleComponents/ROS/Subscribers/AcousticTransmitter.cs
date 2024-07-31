using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Smarc; // StringStampedMsg

using TX = VehicleComponents.Acoustics.Transceiver;

namespace VehicleComponents.ROS.Subscribers
{

    [RequireComponent(typeof(TX))]
    public class AcousticTransmitter : MonoBehaviour
    {       
        ROSConnection ros;
        [Header("ROS Subscriber")]
        [Tooltip("The topic will be namespaced under the root objects name if the given topic does not start with '/'.")]
        public string topic;

        TX tx;
        StringStampedMsg ROSMsg;
        
        void Start()
        {
            if(topic[0] != '/') topic = $"/{transform.root.name}/{topic}";
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<StringStampedMsg>(topic, UpdateMessage);

            ROSMsg = new StringStampedMsg();

            tx = GetComponent<TX>();
            if(tx == null)
            {
                Debug.Log("No transceiver found!");
                return;
            }
        }

        void UpdateMessage(StringStampedMsg msg)
        {
            tx.Write(msg.data);
        }

    }
}