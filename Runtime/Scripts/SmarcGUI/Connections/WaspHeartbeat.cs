using System;
using System.Collections;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;


namespace SmarcGUI.Connections
{

    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class WaspHeartbeatMsg
    {
        public string AgentType;
        public string AgentUuid;
        public string[] Levels;
        public string Name;
        public float Rate;
        public double Stamp;
        public string Type = "Heartbeat";

        public WaspHeartbeatMsg(string agentType, string agentUuid, string[] levels, string name, float rate)
        {
            AgentType = agentType;
            AgentUuid = agentUuid;
            Levels = levels;
            Name = name;
            Rate = rate;
        }

        public string ToJson()
        {
            Stamp = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
            return JsonConvert.SerializeObject(this);
        }

        public WaspHeartbeatMsg(string jsonString)
        {
            JsonConvert.PopulateObject(jsonString, this);
        }
    }

    public class WaspHeartbeat : MQTTPublisher
    {
        public WaspUnitType UnitType;
        public string Context = "smarc";
        public string AgentType;
        public string AgentName;
        public string TopicBase;

        WaspHeartbeatMsg msg;

        public float HeartbeatRate = 1.0f;
        public string AgentUUID{get; private set;}

        public bool HasPublihed = false;

        void Awake()
        {
            var robotGO = Utils.FindParentWithTag(gameObject, "robot", false);
            AgentName = $"{Environment.UserName}_Unity_{robotGO.name}";
            AgentType = UnitType.ToString();
            
            TopicBase = $"{Context}/unit/{AgentType}/simulation/{AgentName}/";

            AgentUUID = Guid.NewGuid().ToString();
            mqttClient = FindFirstObjectByType<MQTTClientGUI>();

            msg = new WaspHeartbeatMsg(
                agentType: AgentType,
                agentUuid: AgentUUID,
                levels: new string[]{WaspLevels.sensor.ToString(), WaspLevels.direct_execution.ToString(), WaspLevels.tst_execution.ToString()},
                name: AgentName,
                rate: HeartbeatRate);            
        }

        void Start()
        {
            Context = mqttClient.Context;
        }

        void OnEnable()
        {
            Context = mqttClient.Context;
        }

        public override void StartPublishing()
        {
            Context = mqttClient.Context;
            publish = true;
            StartCoroutine(HeartbeatCoroutine());
            HasPublihed = true;
        }

        public override void StopPublishing()
        {
            publish = false;
        }

        IEnumerator HeartbeatCoroutine()
        {
            while (publish)
            {
                mqttClient.Publish(TopicBase + "heartbeat", msg.ToJson());
                yield return new WaitForSeconds(HeartbeatRate);
            }
        }
    }
}