using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force;
using UnityEditor.EditorTools; // ForcePoints
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
        

        [Header("Auto-set, do not touch")]
        [SerializeField] RopeGenerator generator;
        [SerializeField] bool isBuoy = false;
        [SerializeField] float ropeDiameter;
        [SerializeField] float ropeCollisionDiameter;
        [SerializeField] float segmentLength;
        [SerializeField] float segmentRigidbodyMass;
        [SerializeField] float segmentGravityMass;
        bool attached = false;
        bool bypassedRope = false; 

        CapsuleCollider capsule;
        ConfigurableJoint ropeJoint;
        Rigidbody rb;

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

        (Vector3, Vector3) SpherePositions()
        {
            float d = segmentLength/2 - ropeDiameter/4;
            return ( new Vector3(0,0,d), new Vector3(0,0,-d) );
        }


        void SetupConfigJoint(ConfigurableJoint joint, Vector3 anchorPosition)
        {
            // This setup was found here
            // https://forums.tigsource.com/index.php?topic=64389.msg1389271#msg1389271
            // where there are vids demonstrating even KNOTS :D
            joint.anchor = anchorPosition;
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

            var visualScale = new Vector3(ropeDiameter, ropeDiameter, ropeDiameter);
            frontVis_tf.localScale = visualScale;
            backVis_tf.localScale = visualScale;
            middleVis_tf.localScale = new Vector3(ropeDiameter, (segmentLength/2)-(ropeDiameter/4), ropeDiameter);
        }

        void SetupBits()
        {
            // scale and locate all the little bits and bobs that make up
            // this rope segment depending on the parameters above.
            // Because settings these by hand is a pain.
            var (frontSpherePos, backSpherePos) = SpherePositions();

            capsule = GetComponent<CapsuleCollider>();
            capsule.radius = ropeCollisionDiameter/2;
            capsule.center = new Vector3(0, ropeCollisionDiameter/2-ropeDiameter/2, 0);
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
            sphere.localPosition = new Vector3(0, ropeDiameter, 0);
            var rad = segmentLength-ropeDiameter;
            var scale = new Vector3(rad, rad, rad);
            sphere.localScale = scale;
            // and make it collidable
            var collider = sphere.GetComponent<SphereCollider>();
            collider.radius = rad;
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

        void OnCollisionEnter(Collision collision)
        {
            if(!isBuoy) return;
            if(attached) return;
            
            if (collision.gameObject.TryGetComponent(out RopeHook rh))
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());

                var hookGO = collision.gameObject;
                var frontConfigJoint = gameObject.AddComponent<ConfigurableJoint>();
                var (frontSpherePos, backSpherePos) = SpherePositions();
                SetupConfigJoint(frontConfigJoint, frontSpherePos);

                try
                {
                    frontConfigJoint.autoConfigureConnectedAnchor = false;
                    frontConfigJoint.connectedAnchor = hookGO.transform.Find("ConnectionPoint").localPosition;
                }
                catch(Exception)
                {
                    Debug.Log("Hook object did not have a ConnectionPoint child, connecting where we touched...");
                }

                var hookAB = hookGO.GetComponent<ArticulationBody>();
                frontConfigJoint.connectedArticulationBody = hookAB;
                var hookBaseLinkGO = Utils.FindDeepChildWithName(hookAB.transform.root.gameObject, "base_link");
                var hookBaseLinkAB = hookBaseLinkGO.GetComponent<ArticulationBody>();
                frontConfigJoint.connectedMassScale = 0.1f * (hookBaseLinkAB.mass / rb.mass);

                // Set up the first rope link in the chain to have the same "joint pulling force"
                // as the base link itself so the base link can be pulled around without exploding the rope!
                var firstRopeLinkObject = generator.RopeContainer.transform.GetChild(0);
                var firstJoint = firstRopeLinkObject.GetComponent<Joint>();
                var baseLinkGO = Utils.FindDeepChildWithName(firstRopeLinkObject.root.gameObject, "base_link");
                var baselinkAB = baseLinkGO.GetComponent<ArticulationBody>();
                firstJoint.connectedMassScale = baselinkAB.mass / rb.mass;
                
                // Set the rope joint to break when the rope is carrying the entire robot.
                // This should happen when the rope is _tight_, meaning the distance between hook
                // and robot is equal (or almost) to the rope length.
                // At that point, we can replace the entire rope with a single linkage
                // and discard the rope entirely.
                // This should make the physics of the drone-rope-auv system more stable
                // and closer to theoretical control papers about suspended load control.
                // But since we still need 2 rigid bodies to connect 2 articulation body systems,
                // we will keep the first and last parts of the rope as part of the AB-RB-RB-AB chain.
                // See OnJointBreak!
                ropeJoint.breakForce = 2;

                attached = true;
            }
        }

        void OnJointBreak(float breakForce)
        {
            Debug.Log($"Broke at {breakForce}N");
            if(!attached)
            {
                Debug.Log($"{gameObject.name}: Rope broke before it was attached?!");
                return;
            }
            // the rope breaking means its tight and carrying something.
            // so we replace the middle links of the rope with a STICK
            // to make the physics more stable!

            // first, nuke the middle siblings between the first ropelink and this buoy
            var container = generator.RopeContainer.transform;
            for(int i=container.childCount-1; i>0; i--)
            {
                // reverse loop because we're gonna remove things from the collection
                var child = container.GetChild(i);
                if(child.name == gameObject.name) continue;
                if(child.name == container.GetChild(0).name) continue;
                // its a middle sibling. murder.
                Destroy(child.gameObject);
            }

            // then, re-create the joint that just broke.
            ropeJoint = gameObject.AddComponent<ConfigurableJoint>();
            var (frontSpherePos, backSpherePos) = SpherePositions();
            SetupConfigJoint(ropeJoint, backSpherePos);
            // then, connect this ropelinks back-joint to the first ropelink
            ropeJoint.connectedBody = container.GetChild(0).GetComponent<Rigidbody>();
            bypassedRope = true;
        }

        void OnDrawGizmos()
        {
            if(!bypassedRope) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, generator.RopeContainer.transform.GetChild(0).position);
        }


        
    }

}
