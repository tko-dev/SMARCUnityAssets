using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force;
using UnityEditor.EditorTools; // ForcePoints
using Utils = DefaultNamespace.Utils;

namespace Rope
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ConfigurableJoint))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RopeLink : MonoBehaviour
    {
        CapsuleCollider capsule;
        ConfigurableJoint joint;
        Rigidbody rb;


        float ropeDiameter;
        float ropeCollisionDiameter;
        float segmentLength;
        float segmentRigidbodyMass;
        float segmentGravityMass;
        float buoyGravityMass;
        bool attached = false;


        [Header("Rope physics")]
        [Tooltip("Stiffness properties of the rope (spring, damper, maxForce)")]
        public float spring = 0.1f;
        public float damper = 0.1f;
        public float maximumForce = 1000f;
        

        [Header("Auto-set, do not touch")]
        public GameObject firstRopeLinkObject;
        public bool isBuoy = false;

        public void SetRopeParams(float ropeDiameter,
                                  float ropeCollisionDiameter,
                                  float segmentLength,
                                  float segmentRigidbodyMass,
                                  float segmentGravityMass,
                                  bool buoy,
                                  float buoyGravityMass,
                                  GameObject firstRopeLinkObject)
        {
            this.ropeDiameter = ropeDiameter;
            this.ropeCollisionDiameter = ropeCollisionDiameter;
            this.segmentLength = segmentLength;
            this.segmentRigidbodyMass = segmentRigidbodyMass;
            this.segmentGravityMass = buoy? buoyGravityMass : segmentGravityMass;
            this.isBuoy = buoy;
            this.firstRopeLinkObject = firstRopeLinkObject;

            SetupBits();
            SetupJoint();
            SetupBalloon();
        }



        SoftJointLimitSpring makeSJLS(float spring, float damper)
        {
            var sjls = new SoftJointLimitSpring();
            sjls.damper = damper;
            sjls.spring = spring;
            return sjls;
        }

        JointDrive makeJD(float spring, float damper, float maximumForce)
        {
            var drive = new JointDrive();
            drive.positionSpring = spring;
            drive.positionDamper = damper;
            drive.maximumForce = maximumForce;
            return drive;
        }

        (Vector3, Vector3) spherePositions()
        {
            float d = segmentLength/2 - ropeDiameter/4;
            return ( new Vector3(0,0,d), new Vector3(0,0,-d) );
        }


        void SetupJoint()
        {
            joint = GetComponent<ConfigurableJoint>();

            // center of rotation for front and back links
            // also where we put things like force points
            var (frontSpherePos, backSpherePos) = spherePositions();

            // This setup was found here
            // https://forums.tigsource.com/index.php?topic=64389.msg1389271#msg1389271
            // where there are vids demonstrating even KNOTS :D
            joint.anchor = backSpherePos;
            joint.enableCollision = false;
            joint.enablePreprocessing = false;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
           

            joint.angularXLimitSpring = makeSJLS(spring, damper);
            joint.angularYZLimitSpring = makeSJLS(spring, damper);
            joint.xDrive = makeJD(spring, damper, maximumForce);
            joint.yDrive = makeJD(spring, damper, maximumForce);
            joint.zDrive = makeJD(spring, damper, maximumForce);
            joint.angularXDrive = makeJD(spring, damper, maximumForce);
            joint.angularYZDrive = makeJD(spring, damper, maximumForce);
            joint.slerpDrive = makeJD(spring, damper, maximumForce); 
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
            var (frontSpherePos, backSpherePos) = spherePositions();

            capsule = GetComponent<CapsuleCollider>();
            capsule.radius = ropeCollisionDiameter/2;
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

                var fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.enablePreprocessing = false;
                var hookAB = collision.gameObject.GetComponent<ArticulationBody>();
                fixedJoint.connectedArticulationBody = hookAB;
                var hookBaseLinkGO = Utils.FindDeepChildWithName(hookAB.transform.root.gameObject, "base_link");
                var hookBaseLinkAB = hookBaseLinkGO.GetComponent<ArticulationBody>();
                fixedJoint.connectedMassScale = 0.1f * (hookBaseLinkAB.mass / rb.mass);

                // Set up the first rope link in the chain to have the same "joint pulling force"
                // as the base link itself so the base link can be pulled around without exploding the rope!
                var firstJoint = firstRopeLinkObject.GetComponent<Joint>();
                var baseLinkGO = Utils.FindDeepChildWithName(firstRopeLinkObject.transform.root.gameObject, "base_link");
                var baselinkAB = baseLinkGO.GetComponent<ArticulationBody>();
                firstJoint.connectedMassScale = baselinkAB.mass / rb.mass;

                attached = true;
            }
        }

        
    }

}
