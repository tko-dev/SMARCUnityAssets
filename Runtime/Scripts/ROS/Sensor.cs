using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core; //Clock

namespace DefaultNamespace
{

    public interface ISensor
    {
        void Setup(GameObject robot, string linkSeparator);
        bool UpdateSensor(double deltaTime);
    }

    public class Sensor<T> : MonoBehaviour, ISensor
        where T : Unity.Robotics.ROSTCPConnector.MessageGeneration.Message, new()
    {
        protected GameObject robot;
        protected GameObject robotMotionModel;
        protected Rigidbody rb;
        protected FixedJoint joint;
        protected GameObject linkGo;
        protected string robotLinkName;

        [Header("General sensor settings")]
        [Tooltip("The name of the link the sensor should be attached to.")]
        public string linkName = "";
        public bool sensorEnabled = true;
        [Tooltip("Set to true if this is a camera, as they need special handling.")]
        public bool isROSCamera = false;
        [Tooltip("Robot name will be pre-pended automatically unless topic starts with a /")]
        public string topic = "";
        public float frequency = 10f;

        private float period => 1.0f/frequency;
        private double lastTime;

        protected ROSConnection ros;
        protected T ros_msg;

        protected string linkSeparator;

        
        protected void SetLink()
        // Call this if you over-write the Setup method below
        {
            robotLinkName = robot.name + linkSeparator + linkName;

            // First, check if another sensormsg has
            // called this method before us;
            // if so, just quit.
            if(GetComponent<Rigidbody>())
            {
                Debug.Log($"({robotLinkName}) already had SetLink ran before.");
                return;
            }

            linkGo = Utils.FindDeepChildWithName(robot, robotLinkName);
            if(linkGo == null)
            {
                Debug.Log($"({robotLinkName}) Link GO was null for.");
                return;
            }

            // Set the pose of the sensor to the pose of the link
            // so that when we set the joint, it tracks gud.
            transform.SetPositionAndRotation(
                linkGo.transform.position,
                linkGo.transform.rotation);
            // ...except if its a camera, ROS
            // defines it with Y forw, Z right, X up (mapped to unity)
            // instead of ZXY
            // so we gotta turn our ZXY camera to match the YZX frame
            if(isROSCamera)
            {
                transform.Rotate(Vector3.up, 90);
                transform.Rotate(Vector3.right, -90);
                transform.Rotate(Vector3.forward, 180);
            }

            // and finally attacht the rigid body of this sensor
            // to the motion model's rigid body with a link.
            // the robot object should be under a motion_model parent
            robotMotionModel = robot.transform.parent.gameObject;
            // First, we need a rigidbody on our sensor
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            // no mass, sensors are _inside_ and should not affect
            // the motion at all.
            // _YET_ (?!)
            rb.mass = 0f;
            rb.drag = 0f;
            rb.angularDrag = 0f;
            // and then the joint to the motion model
            // so that the sensor follows the motion model
            joint = gameObject.AddComponent<FixedJoint>();
            // and finally connect this sensor objects rigid body
            // to the motion model rigid body with this new joint
            joint.connectedBody = robotMotionModel.GetComponent<Rigidbody>();
            // set the mass scales so that the sensor basically has 0
            // effect on the main body
            joint.massScale = 1e5f;
            joint.connectedMassScale = 1e-5f;


        }

        public void Setup(GameObject robot, string linkSeparator)
        // Call this in the PrepareRobot bit
        {
            this.robot = robot;
            this.linkSeparator = linkSeparator;
            // Only add the robot name to the topic if the given topic is relative!
            if(topic[0] != '/') topic = "/" + robot.name + "/" + topic;
            ros_msg = new T();
            SetLink();

            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<T>(topic);
            lastTime = Clock.NowTimeInSeconds;
        }

        public virtual bool UpdateSensor(double deltaTime)
        {
            Debug.Log("The sensor with topic <" + topic + "> needs to override UpdateSensor method!");
            return false;
        }

        // Let the child classes implement this as they see fit.
        // void Start()
        // {
        // }

        void Update()
        {
            if(!sensorEnabled) return;
            // If enough time has passed since last update
            var deltaTime = Clock.NowTimeInSeconds - lastTime;
            if(deltaTime < period) return;

            // do the update
            var pub = UpdateSensor(deltaTime);
            // but only publish if the update was good
            if(topic == null) Debug.Log($"({robotLinkName}) Topic is null!");
            if(ros_msg == null) Debug.Log($"({robotLinkName}) ros_msg is null!");
            if(ros == null) Debug.Log($"({robotLinkName}) ros is null?!?!");
            if(pub) ros.Publish(topic, ros_msg);

            // always update timing after an update
            lastTime = Clock.NowTimeInSeconds;
        }




    }
}