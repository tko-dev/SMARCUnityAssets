using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core; //Clock

namespace DefaultNamespace
{

    public interface ISensor
    {
        void Setup(GameObject robot);
        bool UpdateSensor(double deltaTime);
    }

    public class Sensor<T> : MonoBehaviour, ISensor
        where T : Unity.Robotics.ROSTCPConnector.MessageGeneration.Message, new()
    {
        protected GameObject robot;
        protected GameObject robotMotionModel;
        protected Rigidbody rb;
        protected FixedJoint joint;


        [SerializeField]
        public string linkName = "";
        protected string robotLinkName;
        protected GameObject linkGo;

        public bool sensorEnabled = true;
        public string topic = "";
        public float frequency = 10f;
        private float period => 1.0f/frequency;
        private double lastTime;

        protected ROSConnection ros;
        protected T ros_msg;
        
        protected void MoveToLink()
        // Call this in Update function of anything that extends this
        {
            if(linkGo != null)
            {
                transform.SetPositionAndRotation(
                    linkGo.transform.position,
                    linkGo.transform.rotation);
            }
        }

        protected void SetLink()
        // Call this if you over-write the Setup method below
        {
            robotLinkName = robot.name + "_" + linkName;
            linkGo = Utils.FindDeepChildWithName(robot, robotLinkName);
            if(linkGo == null)
            {
                Debug.Log("Link GO was null for "+ robotLinkName);
                return;
            }
            // Set the pose of the sensor to the pose of the link
            // so that when we set the joint, it tracks gud.
            transform.SetPositionAndRotation(
                linkGo.transform.position,
                linkGo.transform.rotation);

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

        public void Setup(GameObject robot)
        // Call this in the PrepareRobot bit
        {
            this.robot = robot;
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
            if(topic == null) Debug.Log("Topic is null!");
            if(ros_msg == null) Debug.Log("ros_msg is null!");
            if(ros == null) Debug.Log("ros is null?!?!");
            if(pub) ros.Publish(topic, ros_msg);

            // always update timing after an update
            lastTime = Clock.NowTimeInSeconds;
        }




    }
}