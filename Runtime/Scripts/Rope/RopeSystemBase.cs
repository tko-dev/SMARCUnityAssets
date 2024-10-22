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


        void Awake()
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

        protected static ConfigurableJoint AddRopeJoint(GameObject o)
        {
            var ropeJoint = AddConfigurableJoint(o);
            ropeJoint.xMotion = ConfigurableJointMotion.Locked;
            ropeJoint.yMotion = ConfigurableJointMotion.Limited;
            ropeJoint.zMotion = ConfigurableJointMotion.Locked;
            ropeJoint.angularXMotion = ConfigurableJointMotion.Free;
            ropeJoint.angularYMotion = ConfigurableJointMotion.Free;
            ropeJoint.angularZMotion = ConfigurableJointMotion.Free;
            return ropeJoint;
        }

        protected static void UpdateJointLimit(ConfigurableJoint joint, float length)
        {
            var jointLimit = new SoftJointLimit
            {
                limit = length
            };
            joint.linearLimit = jointLimit;
        }

        protected ConfigurableJoint AttachBody(MixedBody end)
        {
            Rigidbody baseRB = GetComponent<Rigidbody>();

            GameObject rope = new GameObject("Rope");
            rope.transform.parent = transform.parent;
            rope.transform.position = transform.position;
            rope.transform.rotation = transform.rotation;
            var ropeRB = AddIneffectiveRB(rope);
            var ropeJoint = AddRopeJoint(rope);

            // Spherical connection on the end object
            var sphericalOnEndJoint = AddSphericalJoint(end.gameObject);

            ropeJoint.connectedBody = baseRB;
            sphericalOnEndJoint.connectedBody = ropeRB;

            // Add a linerenderer to visualize the rope and its tight/slack state
            var lr = rope.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = RopeDiameter;

            return ropeJoint;
        }
    }
}