using UnityEngine;
using Force;

namespace Rope
{
    public class Pulley : RopeSystemBase
    {
        [Header("Body One")]
        public ArticulationBody ConnectedABOne;
        public Rigidbody ConnectedRBOne;
        MixedBody EndOne;
        
        [Header("Body Two")]
        public ArticulationBody ConnectedABTwo;
        public Rigidbody ConnectedRBTwo;
        MixedBody EndTwo;

        ConfigurableJoint distanceJointOne, distanceJointTwo;
        LineRenderer sideOneLR, sideTwoLR;


        [Header("Debug")]
        public float sideOneLimit;
        public float sideTwoLimit;
        public float ropeVelocity;
        

        public override void SetupEnds()
        {
            EndOne = new MixedBody(ConnectedABOne, ConnectedRBOne);
            EndTwo = new MixedBody(ConnectedABTwo, ConnectedRBTwo);
            distanceJointOne = AttachBody(EndOne);
            distanceJointTwo = AttachBody(EndTwo);
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