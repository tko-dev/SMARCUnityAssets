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

        float d1, d2;
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
            d1 = Vector3.Distance(EndOne.position, transform.position);
            d2 = Vector3.Distance(EndTwo.position, transform.position);
            ropeLength = Mathf.Max(d1 + d2, ropeLength);
            UpdateJointLimit(pulleyJointOne, d1);
            UpdateJointLimit(pulleyJointTwo, d2);
        }

        void FixedUpdate()
        {
            // Update the joint limits to keep the rope taut
            float ropeAccel = (pulleyJointOne.currentForce.magnitude - pulleyJointTwo.currentForce.magnitude) / (EndOne.mass + EndTwo.mass);
            Debug.DrawLine(EndOne.position, EndOne.position + pulleyJointOne.currentForce, Color.red);
            Debug.DrawLine(EndTwo.position, EndTwo.position + pulleyJointTwo.currentForce, Color.red);
            if(d1 >= ropeLength || d2 >= ropeLength) 
                ropeVelocity = 0;
            else
            {
                ropeVelocity += ropeAccel * Time.fixedDeltaTime;
                d1 += ropeVelocity * Time.fixedDeltaTime;
                d2 -= ropeVelocity * Time.fixedDeltaTime;
                UpdateJointLimit(pulleyJointOne, d1);
                UpdateJointLimit(pulleyJointTwo, d2);
            }

            
            
        }

    }
}