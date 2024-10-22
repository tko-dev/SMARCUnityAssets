using UnityEngine;
using Force;

namespace Rope
{
    public class Pulley : RopeSystemBase
    {
        [Header("Load One")]
        public ArticulationBody LoadOneAB;
        public Rigidbody LoadOneRB;
        
        [Header("Load Two")]
        public ArticulationBody LoadTwoAB;
        public Rigidbody LoadTwoRB;

        ConfigurableJoint distanceJointOne, distanceJointTwo;
        LineRenderer sideOneLR, sideTwoLR;


        [Header("Debug")]
        public float sideOneLimit;
        public float sideTwoLimit;
        public float ropeVelocity;
        

        public override void SetupEnds()
        {
            if(LoadOneAB) LoadOneRB = ConvertABToRB(LoadOneAB);
            if(LoadTwoAB) LoadTwoRB = ConvertABToRB(LoadTwoAB);
            distanceJointOne = AttachBody(LoadOneRB);
            distanceJointTwo = AttachBody(LoadTwoRB);
            sideOneLR = distanceJointOne.gameObject.GetComponent<LineRenderer>();
            sideTwoLR = distanceJointTwo.gameObject.GetComponent<LineRenderer>();
            sideOneLimit = Vector3.Distance(LoadOneRB.position, transform.position);
            sideTwoLimit = Vector3.Distance(LoadTwoRB.position, transform.position);
            ropeVelocity = 0;
            SetRopeTargetLength(distanceJointOne, sideOneLimit);
            SetRopeTargetLength(distanceJointTwo, sideTwoLimit);
        }

        void FixedUpdate()
        {
            float sideOneDistance = Vector3.Distance(LoadOneRB.position, transform.position);
            float sideTwoDistance = Vector3.Distance(LoadTwoRB.position, transform.position);

            bool sideOneSlack = sideOneDistance < sideOneLimit;
            bool sideTwoSlack = sideTwoDistance < sideTwoLimit;

            // Visualize all the time
            sideOneLR.SetPosition(0, transform.position);
            sideOneLR.SetPosition(1, LoadOneRB.position);
            sideOneLR.startColor = sideOneSlack ? Color.green : Color.red;
            sideOneLR.endColor = sideOneLR.startColor;
            sideTwoLR.SetPosition(0, transform.position);
            sideTwoLR.SetPosition(1, LoadTwoRB.position);
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
                SetRopeTargetLength(distanceJointTwo, sideTwoLimit);
                return;
            }
            if(sideTwoSlack)
            {
                sideOneLimit = RopeLength - sideTwoDistance;
                ropeVelocity = 0;
                SetRopeTargetLength(distanceJointOne, sideOneLimit);
                return;
            }

            // no side is slack, pull around
            float ropeAccel = (distanceJointOne.currentForce.magnitude - distanceJointTwo.currentForce.magnitude) / (LoadOneRB.mass + LoadTwoRB.mass);
            ropeVelocity += ropeAccel * Time.fixedDeltaTime;
            sideOneLimit += ropeVelocity * Time.fixedDeltaTime;
            sideOneLimit = Mathf.Clamp(sideOneLimit, 0, RopeLength);
            sideTwoLimit = RopeLength - sideOneLimit;

            SetRopeTargetLength(distanceJointOne, sideOneLimit);
            SetRopeTargetLength(distanceJointTwo, sideTwoLimit);
        }

    }
}