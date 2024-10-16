using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force;

namespace Rope
{
    [RequireComponent(typeof(Rigidbody))]
    public class Pulley : MonoBehaviour
    {
        [Header("Connected Bodies")]
        public ArticulationBody ConnectedABOne;
        public Rigidbody ConnectedRBOne;
        public ArticulationBody ConnectedABTwo;
        public Rigidbody ConnectedRBTwo;
        MixedBody EndOne;
        MixedBody EndTwo;

        ConfigurableJoint pulleyJointOne, pulleyJointTwo;

        public float ropeLength;

        float partOneLength, partTwoLength;
        float ropeVelocity;
        

        Rigidbody AddIneffectiveRB(GameObject o)
        {
            Rigidbody rb = o.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.inertiaTensor = Vector3.one * 1e-6f;
            rb.drag = 0;
            rb.angularDrag = 0;
            rb.mass = 0.1f;
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


        ConfigurableJoint AttachToPulley(MixedBody end)
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
            // Spherical connection to the end object
            var sphericalToEndJoint = AddSphericalJoint(distanceToSpherical);
            
            // Base -> Sphere -> Linear+Sphere -> End
            sphericalToBaseJoint.connectedBody = baseRB;
            distanceToSphericalJoint.connectedBody = sphericalToBaseRB;
            end.ConnectToJoint(sphericalToEndJoint);
            // sphericalToEndJoint.connectedBody = end;

            return distanceToSphericalJoint;
        }

        void UpdateJointLimit(ConfigurableJoint joint, float length)
        {
            SoftJointLimit jointLimit = new SoftJointLimit();
            jointLimit.limit = length;
            joint.linearLimit = jointLimit;
        }



        void Start()
        {
            EndOne = new MixedBody(ConnectedABOne, ConnectedRBOne);
            EndTwo = new MixedBody(ConnectedABTwo, ConnectedRBTwo);
            pulleyJointOne = AttachToPulley(EndOne);
            pulleyJointTwo = AttachToPulley(EndTwo);
            partOneLength = Vector3.Distance(EndOne.position, transform.position);
            partTwoLength = Vector3.Distance(EndTwo.position, transform.position);
            ropeLength = Mathf.Max(partOneLength + partTwoLength, ropeLength);
            UpdateJointLimit(pulleyJointOne, partOneLength);
            UpdateJointLimit(pulleyJointTwo, partTwoLength);
        }

        void FixedUpdate()
        {
            // Update the joint limits to keep the rope taut
            if(partOneLength >= ropeLength || partTwoLength >= ropeLength) 
                // Rope is tight, does not move at all
                // TODO what if not?
                return;
            else
            {
                float ropeAccel = (pulleyJointOne.currentForce.magnitude - pulleyJointTwo.currentForce.magnitude) / (EndOne.mass + EndTwo.mass);
                ropeVelocity += ropeAccel * Time.fixedDeltaTime;
                partOneLength += ropeVelocity * Time.fixedDeltaTime;
                partTwoLength = ropeLength - partOneLength;
                UpdateJointLimit(pulleyJointOne, partOneLength);
                UpdateJointLimit(pulleyJointTwo, partTwoLength);
            }
            
        }


    }
}