using UnityEngine;

using Force;

namespace Rope
{
    public class Winch : RopeSystemBase
    {
        [Header("Hanging Load")]
        [Tooltip("Due to how ABs are solved, the AB will be converted to an RB when its attached to the winch for stability.")]
        public ArticulationBody LoadAB;
        public Rigidbody LoadRB;
    
        ConfigurableJoint ropeJoint;
        LineRenderer lineRenderer;

        [Header("Winch Controls")]
        public float RopeSpeed;

        [Header("Winch")]
        public float CurrentLength = 3f;
        public float MinLength = 0.1f;

        bool setup = false;

        
        public void AttachLoad(GameObject load)
        {
            LoadAB = load.GetComponent<ArticulationBody>();
            LoadRB = load.GetComponent<Rigidbody>();
        }

        public override void SetupEnds()
        {
            if(LoadAB) LoadRB = ConvertABToRB(LoadAB);
            ropeJoint = AttachBody(LoadRB);
            lineRenderer = ropeJoint.gameObject.GetComponent<LineRenderer>();
            RopeSpeed = 0;
            SetRopeTargetLength(ropeJoint, CurrentLength);
            setup = true;
        }

        void Update()
        {
            if(!setup) return;
            float distance = Vector3.Distance(LoadRB.position, transform.position);
            bool ropeSlack = distance < CurrentLength;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, LoadRB.position);
            lineRenderer.startColor = ropeSlack ? Color.green : Color.red;
            lineRenderer.endColor = lineRenderer.startColor;
        }

        void FixedUpdate()
        {   
            if(!setup) return;
            if(Mathf.Abs(RopeSpeed) == 0) return;

            CurrentLength += RopeSpeed * Time.fixedDeltaTime;
            CurrentLength = Mathf.Clamp(CurrentLength, MinLength, RopeLength);
            if(CurrentLength == MinLength || CurrentLength == RopeLength)
            {
                RopeSpeed = 0;
                return;
            }
            SetRopeTargetLength(ropeJoint, CurrentLength);
        }

    }
}