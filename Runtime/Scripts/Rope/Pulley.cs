using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rope
{
    [RequireComponent(typeof(Rigidbody))]
    public class Pulley : MonoBehaviour
    {
        [Tooltip("The two objects at the ends of the rope")]
        public Rigidbody EndOne, EndTwo;

        ConfigurableJoint pulleyJointOne, pulleyJointTwo;
        public float pulleyLengthOne = 1f;
        public float pulleyLengthTwo = 2f;


        void Start()
        {
            pulleyJointOne = AttachToPulley(EndOne, pulleyLengthOne);
            pulleyJointTwo = AttachToPulley(EndTwo, pulleyLengthTwo);
        }

        Rigidbody AddIneffectiveRB(GameObject o)
        {
            Rigidbody rb = o.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.inertiaTensor = Vector3.one * 1e-6f;
            rb.drag = 0;
            rb.angularDrag = 0;
            return rb;
        }

        ConfigurableJoint AddSphericalJoint(GameObject o)
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

        ConfigurableJoint AddDistanceJoint(GameObject o)
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


        ConfigurableJoint AttachToPulley(Rigidbody end, float limit)
        {
            Rigidbody baseRB = GetComponent<Rigidbody>();

            // Spherical connection to this object
            GameObject sphericalToBase = new GameObject("SphericalToBase");
            sphericalToBase.transform.parent = transform.parent;
            var sphericalToBaseRB = AddIneffectiveRB(sphericalToBase);
            var sphericalToBaseJoint = AddSphericalJoint(sphericalToBase);
            
            // Linear connection to the previous sphere
            GameObject distanceToSpherical = new GameObject("DistanceToSpherical");
            distanceToSpherical.transform.parent = transform.parent;
            var distanceToSphericalRB = AddIneffectiveRB(distanceToSpherical);
            var distanceToSphericalJoint = AddDistanceJoint(distanceToSpherical);
            SoftJointLimit jointLimit = new SoftJointLimit();
            jointLimit.limit = limit;
            distanceToSphericalJoint.linearLimit = jointLimit;
            // Spherical connection to the end object
            var sphericalToEndJoint = AddSphericalJoint(distanceToSpherical);
            
            // Base -> Sphere -> Linear+Sphere -> End
            sphericalToBaseJoint.connectedBody = baseRB;
            distanceToSphericalJoint.connectedBody = sphericalToBaseRB;
            sphericalToEndJoint.connectedBody = end;

            return distanceToSphericalJoint;
        }

        void FixedUpdate()
        {
            SoftJointLimit jointLimitOne = new SoftJointLimit();
            jointLimitOne.limit = pulleyLengthOne;
            pulleyJointOne.linearLimit = jointLimitOne;

            SoftJointLimit jointLimitTwo = new SoftJointLimit();
            jointLimitTwo.limit = pulleyLengthTwo;
            pulleyJointTwo.linearLimit = jointLimitTwo;
            
        }

    }
}