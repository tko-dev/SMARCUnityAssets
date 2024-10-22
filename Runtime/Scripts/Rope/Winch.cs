using UnityEngine;

using Force;

namespace Rope
{
    public class Winch : RopeSystemBase
    {
        [Header("Connected Body")]
        public ArticulationBody ConnectedAB;
        public Rigidbody ConnectedRB;
    
        MixedBody end;
        ConfigurableJoint distanceJoint;
        LineRenderer lineRenderer;

        [Header("Winch")]
        public float CurrentLength = 3f;
        [Tooltip("If true, the current length will be set to the distance between this object and the connected body.")]
        public float MinLength = 0.1f;

        [Header("Winch Controls")]
        public float RopeSpeed;
        

        public override void SetupEnds()
        {
            end = new MixedBody(ConnectedAB, ConnectedRB);
            distanceJoint = AttachBody(end);
            lineRenderer = distanceJoint.gameObject.GetComponent<LineRenderer>();
            RopeSpeed = 0;
            UpdateJointLimit(distanceJoint, CurrentLength);
        }

        void Update()
        {
            float distance = Vector3.Distance(end.position, transform.position);
            bool ropeSlack = distance < CurrentLength;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, end.position);
            lineRenderer.startColor = ropeSlack ? Color.green : Color.red;
            lineRenderer.endColor = lineRenderer.startColor;
        }

        void FixedUpdate()
        {   
            if(Mathf.Abs(RopeSpeed) == 0) return;

            CurrentLength += RopeSpeed * Time.fixedDeltaTime;
            CurrentLength = Mathf.Clamp(CurrentLength, MinLength, RopeLength);
            if(CurrentLength == MinLength || CurrentLength == RopeLength)
            {
                RopeSpeed = 0;
                return;
            }
            UpdateJointLimit(distanceJoint, CurrentLength);
        }

    }
}