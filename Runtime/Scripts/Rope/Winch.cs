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
        [HideInInspector][SerializeField] MixedBody loadBody;
    
        [HideInInspector][SerializeField] SpringJoint ropeJoint;
        [HideInInspector][SerializeField] LineRenderer lineRenderer;

        [Header("Winch Controls")]
        public float TargetLength = 0.5f;
        public float WinchSpeed = 0.5f;

        [Header("Winch")]
        public float CurrentRopeSpeed;
        public float CurrentLength = 0.5f;
        public float MinLength = 0.1f;

        [HideInInspector][SerializeField] bool setup = false;

        
        public void AttachLoad(GameObject load)
        {
            LoadAB = load.GetComponent<ArticulationBody>();
            LoadRB = load.GetComponent<Rigidbody>();
        }

        public override void SetupEnds()
        {
            loadBody = new MixedBody(LoadAB, LoadRB);
            ropeJoint = AttachBody(loadBody);
            lineRenderer = ropeJoint.gameObject.GetComponent<LineRenderer>();
            CurrentRopeSpeed = 0;
            ropeJoint.maxDistance = CurrentLength;
            setup = true;
            Update();
            FixedUpdate();
        }
        public override void UnSetupEnds()
        {
            if (Application.isPlaying)Destroy(ropeJoint.gameObject);
            else DestroyImmediate(ropeJoint.gameObject);

            setup = false;
        }

        void OnValidate()
        {
            TargetLength = Mathf.Clamp(TargetLength, MinLength, RopeLength);
        }
        
        void Awake()
        {
            if(loadBody == null) loadBody = new MixedBody(LoadAB, LoadRB);
        }

        void Update()
        {
            if(!setup) return;
            float distance = Vector3.Distance(loadBody.position, transform.position);
            bool ropeSlack = distance < CurrentLength;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, loadBody.position);
            lineRenderer.startColor = ropeSlack ? Color.green : Color.red;
            lineRenderer.endColor = lineRenderer.startColor;
        }

        void FixedUpdate()
        {   
            if(!setup) return;
            

            // simple speed control
            var lenDiff = TargetLength - CurrentLength;
            if(Mathf.Abs(lenDiff) > 0.025)
            {
                CurrentRopeSpeed = lenDiff > 0 ? WinchSpeed : -WinchSpeed;
            }
            else
            {
                CurrentRopeSpeed = 0;
                return;
            }

            CurrentLength += CurrentRopeSpeed * Time.fixedDeltaTime;
            CurrentLength = Mathf.Clamp(CurrentLength, MinLength, RopeLength);
            if(CurrentLength == MinLength || CurrentLength == RopeLength)
            {
                CurrentRopeSpeed = 0;
                return;
            }
            ropeJoint.maxDistance = CurrentLength;
        }

    }
}