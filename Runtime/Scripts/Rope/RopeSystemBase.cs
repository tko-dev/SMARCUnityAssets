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
            rb.drag = 0;
            rb.angularDrag = 0;
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
            return joint;
        }

        protected static ConfigurableJoint AddRopeJoint(GameObject o, float RopeLength)
        {
            var j = AddConfigurableJoint(o);
            j.xMotion = ConfigurableJointMotion.Locked;
            j.yMotion = ConfigurableJointMotion.Limited;
            j.zMotion = ConfigurableJointMotion.Locked;
            j.angularXMotion = ConfigurableJointMotion.Free;
            j.angularYMotion = ConfigurableJointMotion.Free;
            j.angularZMotion = ConfigurableJointMotion.Free;
            j.yDrive = new JointDrive
            {
                positionSpring = 1000,
                positionDamper = 100,
                maximumForce = 1000,
            };
            j.linearLimit = new SoftJointLimit
            {
                limit = RopeLength+0.1f,
                bounciness = 0,
                contactDistance = 0,
            };
            return j;
        }

        protected static void SetRopeTargetLength(ConfigurableJoint joint, float length)
        {
            joint.targetPosition = new Vector3(0, length, 0);
        }

        protected static Rigidbody ConvertABToRB(ArticulationBody ab)
        {
            var rbGo = new GameObject(ab.name + "_RB");
            rbGo.transform.position = ab.transform.position;
            rbGo.transform.rotation = ab.transform.rotation;
            rbGo.transform.localScale = ab.transform.localScale;
            // put the new object in the world space, since its an AB, it might have
            // parental relations that we dont want to keep in the RB. We want this
            // RB to be a simple pointmass that _kinda looks like_ the AB.
            var rb = rbGo.AddComponent<Rigidbody>();
            rb.mass = ab.mass;
            rb.drag = ab.linearDamping;
            rb.angularDrag = ab.angularDamping;
            rb.useGravity = ab.useGravity;
            rb.centerOfMass = ab.centerOfMass;
            rb.inertiaTensor = ab.inertiaTensor;
            rb.inertiaTensorRotation = ab.inertiaTensorRotation;
            return rb;
        }

        protected ConfigurableJoint AttachBody(Rigidbody load)
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
            var sphericalToLoadJoint = AddSphericalJoint(load.gameObject);
            sphericalToLoadJoint.connectedBody = ropeRB;

            // Add a linerenderer to visualize the rope and its tight/slack state
            var lr = rope.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = RopeDiameter;

            return ropeJoint;
        }
    }
}