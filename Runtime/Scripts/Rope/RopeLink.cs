using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force;
using Utils = DefaultNamespace.Utils;
using System;

namespace Rope
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ConfigurableJoint))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RopeLink : MonoBehaviour
    {
        [Header("Rope physics")]
        [Tooltip("Stiffness properties of the rope (spring, damper, maxForce)")]
        public float spring = 0.1f;
        public float damper = 0.1f;
        public float maximumForce = 1000f;
        

        // we want these saved with the object (so you dont have to re-generate 100 times...),
        // but not shown in editor since they are set by the RopeGenerator on creation.
        [HideInInspector][SerializeField] RopeGenerator generator;
        [HideInInspector][SerializeField] bool isBuoy = false;
        [HideInInspector][SerializeField] float ropeDiameter;
        [HideInInspector][SerializeField] float ropeCollisionDiameter;
        [HideInInspector][SerializeField] float segmentLength;
        [HideInInspector][SerializeField] float segmentRigidbodyMass;
        [HideInInspector][SerializeField] float segmentGravityMass;
        bool attached = false;
        GameObject connectedHookGO;

        CapsuleCollider capsule;
        [HideInInspector] public ConfigurableJoint ropeJoint, hookJoint;
        Rigidbody rb;

        readonly string hookConnectionPointName = "ConnectionPoint";

        public void SetRopeParams(RopeGenerator ropeGenerator, bool isBuoy)
        {
            generator = ropeGenerator;
            ropeDiameter = generator.RopeDiameter;
            ropeCollisionDiameter = generator.RopeCollisionDiameter;
            segmentLength = generator.SegmentLength;
            segmentRigidbodyMass = generator.SegmentRBMass;
            segmentGravityMass = isBuoy? generator.BuoyGrams * 0.001f : generator.IdealMassPerSegment;

            this.isBuoy = isBuoy;

            SetupBits();
            // center of rotation for front and back links
            // also where we put things like force points
            var (frontSpherePos, backSpherePos) = SpherePositions();
            ropeJoint = GetComponent<ConfigurableJoint>();
            SetupConfigJoint(ropeJoint, backSpherePos);
            SetupBalloon();
        }



        SoftJointLimitSpring MakeSJLS(float spring, float damper)
        {
            var sjls = new SoftJointLimitSpring
            {
                damper = damper,
                spring = spring
            };
            return sjls;
        }

        JointDrive MakeJD(float spring, float damper, float maximumForce)
        {
            var drive = new JointDrive
            {
                positionSpring = spring,
                positionDamper = damper,
                maximumForce = maximumForce
            };
            return drive;
        }

        public (Vector3, Vector3) SpherePositions()
        {
            return ( new Vector3(0,0, segmentLength), new Vector3(0,0,0) );
        }


        public void SetupConfigJoint(ConfigurableJoint joint, Vector3 anchorPosition)
        {
            // This setup was found here
            // https://forums.tigsource.com/index.php?topic=64389.msg1389271#msg1389271
            // where there are vids demonstrating even KNOTS :D
            joint.anchor = anchorPosition;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = -anchorPosition;
            joint.enableCollision = false;
            joint.enablePreprocessing = false;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
           

            joint.angularXLimitSpring = MakeSJLS(spring, damper);
            joint.angularYZLimitSpring = MakeSJLS(spring, damper);
            joint.xDrive = MakeJD(spring, damper, maximumForce);
            joint.yDrive = MakeJD(spring, damper, maximumForce);
            joint.zDrive = MakeJD(spring, damper, maximumForce);
            joint.angularXDrive = MakeJD(spring, damper, maximumForce);
            joint.angularYZDrive = MakeJD(spring, damper, maximumForce);
            joint.slerpDrive = MakeJD(spring, damper, maximumForce); 
        }

        void SetupForcePoint(Transform FP_tf, Vector3 position)
        {
            FP_tf.localPosition = position;
            var FP_sphereCollider = FP_tf.GetComponent<SphereCollider>();
            FP_sphereCollider.radius = ropeDiameter/2;
            var FP = FP_tf.GetComponent<ForcePoint>();
            FP.depthBeforeSubmerged = ropeDiameter;
            FP.mass = segmentGravityMass;
            FP.addGravity = true;
        }

        void SetupVisuals(Vector3 frontSpherePos, Vector3 backSpherePos)
        {
            var frontVis_tf = transform.Find("Visuals/Front");
            var backVis_tf = transform.Find("Visuals/Back");
            var middleVis_tf = transform.Find("Visuals/Middle");

            frontVis_tf.localPosition = frontSpherePos;
            backVis_tf.localPosition = backSpherePos;
            middleVis_tf.localPosition = (backSpherePos+frontSpherePos)/2;

            var visualScale = new Vector3(ropeDiameter, ropeDiameter, ropeDiameter);
            frontVis_tf.localScale = visualScale;
            backVis_tf.localScale = visualScale;
            middleVis_tf.localScale = new Vector3(ropeDiameter, segmentLength/2, ropeDiameter);
        }

        void SetupBits()
        {
            // scale and locate all the little bits and bobs that make up
            // this rope segment depending on the parameters above.
            // Because settings these by hand is a pain.
            var (frontSpherePos, backSpherePos) = SpherePositions();

            capsule = GetComponent<CapsuleCollider>();
            capsule.radius = ropeCollisionDiameter/2;
            capsule.center = new Vector3(0, ropeCollisionDiameter/2-ropeDiameter/2, segmentLength/2);
            capsule.height = segmentLength+ropeCollisionDiameter; // we want the collision to overlap with the child's

            // Having the rope be _so tiny_ is problematic for
            // physics calculations.
            // But having it be heavy is problematic for lifting
            // with drone and such.
            // So we set the mass of the rigidbody to be large, and 
            // apply our own custom gravity(with ForcePoints) with small mass.
            // Mass is large in the RB for interactions, but gravity is small
            // for lifting.
            rb = GetComponent<Rigidbody>();
            rb.mass = segmentRigidbodyMass;
            rb.useGravity = false;

            SetupForcePoint(transform.Find("ForcePoint_F"), frontSpherePos);
            SetupForcePoint(transform.Find("ForcePoint_B"), backSpherePos);
            SetupVisuals(frontSpherePos, backSpherePos);
        }

        void SetupBalloon()
        {
            if(!isBuoy) return;

            // Add a visual sphere to the rope as the buoy balloon
            var visuals = transform.Find("Visuals");
            Transform sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            sphere.SetParent(visuals);
            sphere.localPosition = new Vector3(0, ropeDiameter, segmentLength);
            var rad = segmentLength-ropeDiameter;
            var scale = new Vector3(rad, rad, rad);
            sphere.localScale = scale;
            // and make it collidable
            var collider = sphere.GetComponent<SphereCollider>();
            collider.radius = rad;
        }

        public void SetupConnectionToPrevLink(Transform prevLink)
        {
            ropeJoint = GetComponent<ConfigurableJoint>();
            var linkZ = prevLink.localPosition.z + generator.SegmentLength;
            transform.localPosition = new Vector3(0, 0, linkZ);
            transform.rotation = prevLink.rotation;
            ropeJoint.autoConfigureConnectedAnchor = false;
            ropeJoint.connectedBody = prevLink.GetComponent<Rigidbody>();
            ropeJoint.connectedAnchor = ropeJoint.anchor + new Vector3(0, 0, segmentLength);
        }

        public void SetupConnectionToVehicle(
            GameObject vehicleBaseLinkConnection,
            GameObject baseLink)
        {
            // First link in the chain, not connected to another link
            ropeJoint = GetComponent<ConfigurableJoint>();
            ropeJoint.connectedArticulationBody = vehicleBaseLinkConnection.GetComponent<ArticulationBody>();

            transform.localPosition = new Vector3(0, 0, generator.SegmentLength/2);
            transform.rotation = vehicleBaseLinkConnection.transform.rotation;

            // make the first link not collide with its attached base link
            if(baseLink.TryGetComponent<Collider>(out Collider baseCollider))
            {
                var linkCollider = GetComponent<Collider>();
                Physics.IgnoreCollision(linkCollider, baseCollider);
            }           
        }

        public void ConnectToHook(GameObject hookGO)
        {
            hookJoint = gameObject.AddComponent<ConfigurableJoint>();
            var (frontSpherePos, backSpherePos) = SpherePositions();
            SetupConfigJoint(hookJoint, frontSpherePos);

            try
            {
                hookJoint.autoConfigureConnectedAnchor = false;
                hookJoint.connectedAnchor = hookGO.transform.Find(hookConnectionPointName).localPosition;
            }
            catch(Exception)
            {
                Debug.Log("Hook object did not have a ConnectionPoint child, connecting where we touched...");
            }

            var hookAB = hookGO.GetComponent<ArticulationBody>();
            hookJoint.connectedArticulationBody = hookAB;
            attached = true;
        }


        public void SetMassScaleForPulling(float pullerOverPulledMassRatio)
        {
            if(attached)
            {
                hookJoint.massScale = pullerOverPulledMassRatio / rb.mass;
            }
            else
            {
                ropeJoint.connectedMassScale = pullerOverPulledMassRatio / rb.mass;
            }
        }


        void OnCollisionEnter(Collision collision)
        {
            if(!isBuoy) return;
            if(attached) return;
            
            if (collision.gameObject.TryGetComponent(out RopeHook rh))
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());

                connectedHookGO = collision.gameObject;
                ConnectToHook(connectedHookGO);
                // by default the rope is usually too light to actually pull the vehicle
                // it is attached to.
                // we dont change that here, but instead check if the rope is tight in
                // FixedUpdate() and replace the rope with sticks (for physics stability) that
                // _can_ pull the vehicle.
                // The many-links version of the rope is basically only for visual goals like
                // cameras identifying the rope and such.
            }
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            ropeJoint = GetComponent<ConfigurableJoint>();

            // disable self-collisions
            var ropeLinks = FindObjectsByType<RopeLink>(FindObjectsSortMode.None);
            var ownC = GetComponent<Collider>();
            foreach(var other in ropeLinks)
                if (other.gameObject.TryGetComponent(out Collider c))
                    Physics.IgnoreCollision(c, ownC);
        }

        void FixedUpdate()
        {
            if(!isBuoy) return;
            if(!attached) return;
            // if its a buoy rope bit, and attached to a hook
            // check if the distance from the front join of buoy
            // to back-joint of first link is about equal to rope length
            // that means the rope is tight.
            // then we can replace the rope with a stick to make it more stable in sim.
            var firstRopeLinkObject = generator.RopeContainer.transform.GetChild(0);
            var firstJoint = firstRopeLinkObject.GetComponent<Joint>();
            var vehicleJointPos = firstJoint.transform.position + firstJoint.anchor;
            var hookJointPos = transform.position + hookJoint.anchor;
            var directLength = Vector3.Distance(vehicleJointPos, hookJointPos);
            if(Mathf.Abs(directLength-generator.RopeLength) <= generator.RopeReplacementAccuracy)
            {
                Debug.Log($"Rope length reached {directLength}m and got replaced!");
                generator.ReplaceRopeWithStick(connectedHookGO);
            }
        }
        
        
    }

}
