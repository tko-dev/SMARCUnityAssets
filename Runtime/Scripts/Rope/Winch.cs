using UnityEngine;

using Force;

namespace Rope
{
    [RequireComponent(typeof(Rigidbody))]
    public class Winch : MonoBehaviour
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
        public float RopeSpeed;
        public float RopeDiameter = 0.1f;
        

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
            end = new MixedBody(ConnectedAB, ConnectedRB);
            distanceJoint = AttachToPulley(end);
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