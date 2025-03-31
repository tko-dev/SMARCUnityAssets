using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using VehicleComponents.ROS.Publishers;
using VehicleComponents.ROS.Core;


namespace SmarcGUI.Connections
{
    public class ROSClientGUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_InputField ServerAddressInput;
        public TMP_InputField PortInput;
        public Button ConnectButton;
        public Button DisconnectButton;
        public Toggle PublishControllerToRosToggle;

        ROSConnection rosCon;
        public bool IsConnected = false;

        string ServerAddress => ServerAddressInput.text;
        int ServerPort => int.Parse(PortInput.text);

        ROSBehaviour[] rosBehaviours;

        Joy_Pub joyPub;

        void Awake()
        {
            var joyPubs = FindObjectsByType<Joy_Pub>(FindObjectsSortMode.None);
            if(joyPubs.Length >= 1)
            {   
                if(joyPubs.Length > 1)
                {
                    Debug.LogWarning("Multiple Joy_Pub components found in the scene. Using the first one and disabling the rest.");
                    for(int i = 1; i < joyPubs.Length; i++)
                    {
                        joyPubs[i].enabled = false;
                    }
                }
                joyPub = joyPubs[0];
                joyPub.enabled = false;
            }

            rosBehaviours = FindObjectsByType<ROSBehaviour>(FindObjectsSortMode.None);
        }

        void Start()
        {
            rosCon = ROSConnection.GetOrCreateInstance();
            ServerAddressInput.text = rosCon.RosIPAddress.ToString();
            PortInput.text = rosCon.RosPort.ToString();
            ConnectButton.onClick.AddListener(OnConnect);
            DisconnectButton.onClick.AddListener(OnDisconnect);
            if(joyPub!=null) PublishControllerToRosToggle.onValueChanged.AddListener(value => joyPub.enabled = value);
            else PublishControllerToRosToggle.interactable = false;
            rosCon.ShowHud = false;
        }

        void ConnectionInputsInteractable(bool interactable)
        {
            ConnectButton.interactable = interactable;
            ServerAddressInput.interactable = interactable;
            PortInput.interactable = interactable;
            DisconnectButton.interactable = !interactable;
            PublishControllerToRosToggle.interactable = interactable;
        }

        void OnConnect()
        {
            Debug.Log($"Connecting to ROS-TCP bridge at: {ServerAddress}:{ServerPort}");
            rosCon.Connect(ServerAddress, ServerPort);
            ConnectionInputsInteractable(false);
            IsConnected = true;
            foreach(var b in rosBehaviours)
            {
                Debug.Log($"Enabling: {b.gameObject.name} ros behaviour.");
                b.enabled = true;
            }
        }

        void OnDisconnect()
        {   
            Debug.Log($"Disconnecting from ROS-TCP bridge at: {ServerAddress}:{ServerPort}");
            rosCon.Disconnect();
            ConnectionInputsInteractable(true);
            IsConnected = false;
            foreach(var b in rosBehaviours)
            {
                Debug.Log($"Disabling: {b.gameObject.name} ros behaviour.");
                b.enabled = false;
            }
        }

    }
}