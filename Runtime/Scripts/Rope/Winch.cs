using UnityEngine;

using Force;

namespace Rope
{
    [RequireComponent(typeof(Rigidbody))]
    public class Winch : RopeSystemBase
    {
        [Header("Connected Body")]
        public ArticulationBody ConnectedAB;
        public Rigidbody ConnectedRB;
    
        MixedBody end;
        ConfigurableJoint distanceJoint;
        LineRenderer lineRenderer;

        [Header("Rope Properties")]
        public float CurrentLength = 3f;
        public float MaxLength = 5f;
        public float MinLength = 0.1f;

        [Header("Controllable Properties")]
        public float RopeSpeed;
        
        

        void Start()
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
            CurrentLength = Mathf.Clamp(CurrentLength, MinLength, MaxLength);
            if(CurrentLength == MinLength || CurrentLength == MaxLength)
            {
                RopeSpeed = 0;
                return;
            }
            UpdateJointLimit(distanceJoint, CurrentLength);
        }

        void OnDrawGizmos()
        {
            if (end != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, end.position);
            }
        }

    }
}