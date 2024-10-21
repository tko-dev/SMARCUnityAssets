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
        [Header("Rope Joints")]
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
        [HideInInspector][SerializeField] float segmentMass;
        bool isTightTowardsVehicle = false;
        bool isTightTowardsBuoy = false;
        float tightTowardsVehicleLength;
        float tightTowardsBuoyLength;

        CapsuleCollider capsule;
        [HideInInspector] public ConfigurableJoint ropeJoint;
        Rigidbody rb;

        [HideInInspector][SerializeField] Transform firstSegmentTransform;
        [HideInInspector][SerializeField] Transform lastSegmentTransform;


        // Called by RopeGenerator
        public void SetRopeParams(RopeGenerator ropeGenerator, bool isBuoy)
        {
            generator = ropeGenerator;
            ropeDiameter = generator.RopeDiameter;
            ropeCollisionDiameter = generator.RopeCollisionDiameter;
            segmentLength = generator.SegmentLength;
            segmentMass = isBuoy? generator.BuoyGrams * 0.001f : generator.SegmentMass;

            this.isBuoy = isBuoy;

            SetupBits();
            // center of rotation for front and back links
            // also where we put things like force points
            var (frontSpherePos, backSpherePos) = SpherePositions();
            ropeJoint = GetComponent<ConfigurableJoint>();
            SetupConfigJoint(ropeJoint, backSpherePos);
            SetupBalloon();
        }

        public void AssignFirstAndLastSegments()
        {
            firstSegmentTransform = generator.RopeContainer.transform.GetChild(0);
            lastSegmentTransform = generator.RopeContainer.transform.GetChild(generator.NumSegments-1);
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
            GameObject vehicleConnectionLink,
            GameObject vehicleBaseLink)
        {
            // First link in the chain, not connected to another link
            ropeJoint = GetComponent<ConfigurableJoint>();
            ropeJoint.connectedArticulationBody = vehicleConnectionLink.GetComponent<ArticulationBody>();

            transform.position = vehicleConnectionLink.transform.position;
            transform.rotation = vehicleConnectionLink.transform.rotation;

            // make the first link not collide with its attached base link
            if(vehicleBaseLink.TryGetComponent<Collider>(out Collider baseCollider))
            {
                var linkCollider = GetComponent<Collider>();
                Physics.IgnoreCollision(linkCollider, baseCollider);
            }

            // disable the back force point as it is ON the joint
            var backFP = transform.Find("ForcePoint_B");
            backFP.gameObject.SetActive(false);
        }

        public RopeGenerator GetGenerator()
        {
            return generator;
        }


        SoftJointLimitSpring MakeSJLS(float spring, float damper)
        {
            return new SoftJointLimitSpring
            {
                damper = damper,
                spring = spring
            };
        }

        JointDrive MakeJD(float spring, float damper, float maximumForce)
        {
            return new JointDrive
            {
                positionSpring = spring,
                positionDamper = damper,
                maximumForce = maximumForce
            };
        }

        (Vector3, Vector3) SpherePositions()
        {
            return ( new Vector3(0,0, segmentLength), new Vector3(0,0,0) );
        }


        void SetupConfigJoint(ConfigurableJoint joint, Vector3 anchorPosition)
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
            FP.DepthBeforeSubmerged = ropeDiameter*5;

            if(isBuoy)
            {
                FP.AirDrag = generator.BuoyLinearDrag;
                FP.UnderwaterDrag = generator.BuoyLinearDrag;
            }
             
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

            rb = GetComponent<Rigidbody>();
            rb.mass = segmentMass;
            rb.centerOfMass = new Vector3(0, 0, segmentLength/2);

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

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            ropeJoint = GetComponent<ConfigurableJoint>();

            
            tightTowardsVehicleLength = Vector3.Distance(firstSegmentTransform.position, transform.position);
            tightTowardsBuoyLength = Vector3.Distance(lastSegmentTransform.position, transform.position);

            // disable self-collisions
            var ropeLinks = FindObjectsByType<RopeLink>(FindObjectsSortMode.None);
            var ownC = GetComponent<Collider>();
            foreach(var other in ropeLinks)
                if (other.gameObject.TryGetComponent(out Collider c))
                    Physics.IgnoreCollision(c, ownC);
        }

        void FixedUpdate()
        {
            var distanceToVehicle = Vector3.Distance(firstSegmentTransform.position, transform.position);
            isTightTowardsVehicle = Mathf.Abs(distanceToVehicle-tightTowardsVehicleLength) <= generator.RopeTightnessTolerance;
            var distanceToBuoy = Vector3.Distance(lastSegmentTransform.position, transform.position);
            isTightTowardsBuoy = Mathf.Abs(distanceToBuoy-tightTowardsBuoyLength) <= generator.RopeTightnessTolerance;
        }

        void OnDrawGizmos()
        {
            if(!generator.DrawGizmos) return;

            Gizmos.color = isTightTowardsVehicle? Color.blue : Color.green;
            var p = transform.position + transform.forward*segmentLength/3;
            Gizmos.DrawSphere(p, ropeDiameter*1.1f);
            
            Gizmos.color = isTightTowardsBuoy? Color.red : Color.green;
            p = transform.position + transform.forward*segmentLength*2/3;
            Gizmos.DrawSphere(p, ropeDiameter*1.1f);
        }
        
        
    }

}
