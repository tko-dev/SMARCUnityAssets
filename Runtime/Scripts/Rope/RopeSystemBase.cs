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


        [Header("Carrier Object")]
        [Tooltip("The physics object that will carry the rope system")]
        public ArticulationBody CarrierAB;
        public Rigidbody CarrierRB;
        MixedBody carrierBody;


        void Awake()
        {
            SetupSystem();
        }

        public void SetupSystem()
        {
            carrierBody = new MixedBody(CarrierAB, CarrierRB);
            var fixedJoint = gameObject.AddComponent<FixedJoint>();
            carrierBody.ConnectToJoint(fixedJoint);
            SetupEnds();
        }

        protected virtual void SetupEnds()
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

        protected static ConfigurableJoint AddSphericalJoint(GameObject o)
        {
            ConfigurableJoint joint = o.AddComponent<ConfigurableJoint>();
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.anchor = Vector3.zero;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = Vector3.zero;
            return joint;
        }

        protected static ConfigurableJoint AddDistanceJoint(GameObject o)
        {
            ConfigurableJoint joint = o.AddComponent<ConfigurableJoint>();
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
            joint.anchor = Vector3.zero;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = Vector3.zero;
            return joint;
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

            // Spherical connection to this object
            var sphericalToBase = new GameObject("Rope_SphericalToBase");
            sphericalToBase.transform.parent = transform;
            var sphericalToBaseRB = AddIneffectiveRB(sphericalToBase);
            var sphericalToBaseJoint = AddSphericalJoint(sphericalToBase);
            
            // Linear connection to the previous sphere
            var distanceToSpherical = new GameObject("Rope_DistanceToSpherical");
            distanceToSpherical.transform.parent = transform;
            var distanceToSphericalRB = AddIneffectiveRB(distanceToSpherical);
            var distanceToSphericalJoint = AddDistanceJoint(distanceToSpherical);
            // Spherical connection to the end object
            var sphericalToEndJoint = AddSphericalJoint(distanceToSpherical);
            
            // Base -> Sphere -> Linear+Sphere -> End
            sphericalToBaseJoint.connectedBody = baseRB;
            distanceToSphericalJoint.connectedBody = sphericalToBaseRB;
            end.ConnectToJoint(sphericalToEndJoint);

            // Add a linerenderer to visualize the rope and its tight/slack state
            var lr = distanceToSpherical.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = RopeDiameter;

            return distanceToSphericalJoint;
        }
    }
}