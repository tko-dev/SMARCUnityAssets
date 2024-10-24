using UnityEngine;
using Force;

namespace Rope
{
    public class RopeSystemBase : MonoBehaviour
    {

        [Header("Rope Properties")]
        public float RopeDiameter = 0.1f;
        [Tooltip("Length of the rope. A winch can extend this much and a Pulley can have this distance between the two ends.")]
        public float RopeLength = 5f;

        [Tooltip("Set false if you want to call Setup() manually, maybe as a result of a button press or event.")]
        public bool SetupOnStart = true;


        void Start()
        {
            if(!SetupOnStart) return;
            Setup();
        }

        // also called by editor script
        public void Setup()
        {
            var selfRB = GetComponent<Rigidbody>();
            if(selfRB == null) AddIneffectiveRB(gameObject);
            SetupOnStart = false;
            SetupEnds();
        }
        public void UnSetup()
        {
            UnSetupEnds();
        }


        public virtual void UnSetupEnds()
        {
            Debug.LogWarning("UnSetupEnds() not implemented in " + GetType());
        }
        public virtual void SetupEnds()
        {
            Debug.LogWarning("SetupEnds() not implemented in " + GetType());
        }


        protected static Rigidbody AddIneffectiveRB(GameObject o)
        {
            Rigidbody rb = o.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.inertiaTensor = Vector3.one * 1e-6f;
            rb.drag = 1f;
            rb.angularDrag = 1f;
            rb.mass = 0.1f;
            return rb;
        }



        protected SpringJoint AttachBody(MixedBody load)
        {
            Rigidbody baseRB = GetComponent<Rigidbody>();

            GameObject rope = new GameObject($"{transform.name}_Rope");
            rope.transform.parent = transform.parent;
            rope.transform.position = transform.position;
            rope.transform.rotation = transform.rotation;
            var ropeRB = AddIneffectiveRB(rope);
            
            var ropeJoint = rope.AddComponent<SpringJoint>();
            ropeJoint.enablePreprocessing = false;
            ropeJoint.enableCollision = false;
            ropeJoint.autoConfigureConnectedAnchor = false;
            ropeJoint.connectedBody = baseRB;
            ropeJoint.anchor = Vector3.zero;
            ropeJoint.connectedAnchor = Vector3.zero;
            ropeJoint.spring = 5000;
            ropeJoint.damper = 500;
            ropeJoint.maxDistance = RopeLength;

            var loadJoint = rope.AddComponent<CharacterJoint>();
            loadJoint.enablePreprocessing = false;
            loadJoint.enableCollision = false;
            loadJoint.autoConfigureConnectedAnchor = false;
            load.ConnectToJoint(loadJoint);
            loadJoint.anchor = Vector3.zero;
            loadJoint.connectedAnchor = Vector3.zero;

            // Add a linerenderer to visualize the rope and its tight/slack state
            var lr = rope.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = RopeDiameter;
            lr.startColor = Color.blue;
            lr.endColor = lr.startColor;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, load.position);

            return ropeJoint;
        }
    }
}