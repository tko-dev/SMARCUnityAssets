using UnityEngine;
using Force;

namespace Rope
{
    public class RopeSystemBase : MonoBehaviour
    {

        [Header("Rope Properties")]
        public float RopeDiameter = 0.1f;
        [Tooltip("Length of the rope. A winch can extend this much and a Pulley can have this distance between the two ends.")]
        public float RopeLength = 5f;

        [Tooltip("Set false if you want to call Setup() manually, maybe as a result of a button press or event.")]
        public bool SetupOnStart = true;


        void Start()
        {
            if(!SetupOnStart) return;
            Setup();
        }

        // also called by editor script
        public void Setup()
        {
            var selfRB = GetComponent<Rigidbody>();
            if(selfRB == null) AddIneffectiveRB(gameObject);
            SetupEnds();
        }



        public virtual void SetupEnds()
        {
            Debug.LogWarning("SetupEnds() not implemented in " + GetType());
        }


        protected static Rigidbody AddIneffectiveRB(GameObject o)
        {
            Rigidbody rb = o.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.inertiaTensor = Vector3.one * 1e-6f;
            rb.drag = 0.01f;
            rb.angularDrag = 0.01f;
            rb.mass = 0.1f;
            return rb;
        }

        static ConfigurableJoint AddConfigurableJoint(GameObject o)
        {
            ConfigurableJoint joint = o.AddComponent<ConfigurableJoint>();
            joint.enableCollision = false;
            joint.enablePreprocessing = false;
            joint.autoConfigureConnectedAnchor = false;
            return joint;
        }

        protected static ConfigurableJoint AddSphericalJoint(GameObject o)
        {
            var joint = AddConfigurableJoint(o);
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            return joint;
        }

        protected static ConfigurableJoint AddRopeJoint(GameObject o, float RopeLength)
        {
            var j = AddConfigurableJoint(o);
            j.xMotion = ConfigurableJointMotion.Locked;
            j.yMotion = ConfigurableJointMotion.Limited;
            j.zMotion = ConfigurableJointMotion.Locked;
            j.angularXMotion = ConfigurableJointMotion.Free;
            j.angularYMotion = ConfigurableJointMotion.Locked;
            j.angularZMotion = ConfigurableJointMotion.Free;
            j.linearLimit = new SoftJointLimit
            {
                limit = RopeLength+0.1f,
                bounciness = 0,
                contactDistance = 0,
            };
            j.linearLimitSpring = new SoftJointLimitSpring
            {
                spring = 5000,
                damper = 4000,
            };
            return j;
        }

        protected static void SetRopeTargetLength(ConfigurableJoint joint, float length)
        {
            joint.linearLimit = new SoftJointLimit
            {
                limit = length+0.01f,
                bounciness = 0,
                contactDistance = 0,
            };
        }


        protected ConfigurableJoint AttachBody(MixedBody load)
        {
            Rigidbody baseRB = GetComponent<Rigidbody>();

            GameObject rope = new GameObject("Rope");
            rope.transform.parent = transform.parent;
            rope.transform.position = transform.position;
            rope.transform.rotation = transform.rotation;
            var ropeRB = AddIneffectiveRB(rope);
            
            var ropeJoint = AddRopeJoint(rope, RopeLength);
            ropeJoint.connectedBody = baseRB;

            // Spherical connection to the load
            var sphericalToLoadJoint = AddSphericalJoint(rope);
            load.ConnectToJoint(sphericalToLoadJoint);
            // if(load.ab) sphericalToLoadJoint.swapBodies = false;
            // else sphericalToLoadJoint.swapBodies = true;


            // Add a linerenderer to visualize the rope and its tight/slack state
            var lr = rope.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = RopeDiameter;
            lr.startColor = Color.blue;
            lr.endColor = lr.startColor;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, load.position);

            return ropeJoint;
        }
    }
}