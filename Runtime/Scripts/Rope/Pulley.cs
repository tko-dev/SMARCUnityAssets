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

        ConfigurableJoint distanceJointOne, distanceJointTwo;
        LineRenderer sideOneLR, sideTwoLR;

        [Header("Rope Properties")]
        public float RopeLength;
        public float RopeDiameter = 0.1f;

        [Header("Debug")]
        public float sideOneLimit;
        public float sideTwoLimit;
        public float ropeVelocity;
        

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
            var sphericalToBase = new GameObject("SphericalToBase");
            sphericalToBase.transform.parent = transform.parent;
            var sphericalToBaseRB = AddIneffectiveRB(sphericalToBase);
            var sphericalToBaseJoint = AddSphericalJoint(sphericalToBase);
            
            // Linear connection to the previous sphere
            var distanceToSpherical = new GameObject("DistanceToSpherical");
            distanceToSpherical.transform.parent = transform.parent;
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

        void UpdateJointLimit(ConfigurableJoint joint, float length)
        {
            var jointLimit = new SoftJointLimit
            {
                limit = length
            };
            joint.linearLimit = jointLimit;
        }



        void Start()
        {
            EndOne = new MixedBody(ConnectedABOne, ConnectedRBOne);
            EndTwo = new MixedBody(ConnectedABTwo, ConnectedRBTwo);
            distanceJointOne = AttachToPulley(EndOne);
            distanceJointTwo = AttachToPulley(EndTwo);
            sideOneLR = distanceJointOne.gameObject.GetComponent<LineRenderer>();
            sideTwoLR = distanceJointTwo.gameObject.GetComponent<LineRenderer>();
            sideOneLimit = Vector3.Distance(EndOne.position, transform.position);
            sideTwoLimit = Vector3.Distance(EndTwo.position, transform.position);
            ropeVelocity = 0;
            UpdateJointLimit(distanceJointOne, sideOneLimit);
            UpdateJointLimit(distanceJointTwo, sideTwoLimit);
        }

        void FixedUpdate()
        {

            float sideOneDistance = Vector3.Distance(EndOne.position, transform.position);
            float sideTwoDistance = Vector3.Distance(EndTwo.position, transform.position);

            bool sideOneSlack = sideOneDistance < sideOneLimit;
            bool sideTwoSlack = sideTwoDistance < sideTwoLimit;

            // Visualize all the time
            sideOneLR.SetPosition(0, transform.position);
            sideOneLR.SetPosition(1, EndOne.position);
            sideOneLR.startColor = sideOneSlack ? Color.green : Color.red;
            sideOneLR.endColor = sideOneLR.startColor;
            sideTwoLR.SetPosition(0, transform.position);
            sideTwoLR.SetPosition(1, EndTwo.position);
            sideTwoLR.startColor = sideTwoSlack ? Color.green : Color.red;
            sideTwoLR.endColor = sideTwoLR.startColor;
            
            // both sides are slack, rope doesnt do anything at all
            if(sideOneSlack && sideTwoSlack)
            {
                ropeVelocity = 0;
                return; 
            }

            // one side is slack, let the other have all the slack rope
            if(sideOneSlack)
            {
                sideTwoLimit = RopeLength - sideOneDistance;
                ropeVelocity = 0;
                UpdateJointLimit(distanceJointTwo, sideTwoLimit);
                return;
            }
            if(sideTwoSlack)
            {
                sideOneLimit = RopeLength - sideTwoDistance;
                ropeVelocity = 0;
                UpdateJointLimit(distanceJointOne, sideOneLimit);
                return;
            }

            // no side is slack, pull around
            float ropeAccel = (distanceJointOne.currentForce.magnitude - distanceJointTwo.currentForce.magnitude) / (EndOne.mass + EndTwo.mass);
            ropeVelocity += ropeAccel * Time.fixedDeltaTime;
            sideOneLimit += ropeVelocity * Time.fixedDeltaTime;
            sideOneLimit = Mathf.Clamp(sideOneLimit, 0, RopeLength);
            sideTwoLimit = RopeLength - sideOneLimit;

            UpdateJointLimit(distanceJointOne, sideOneLimit);
            UpdateJointLimit(distanceJointTwo, sideTwoLimit);
        }

    }
}