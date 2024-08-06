using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force; // ForcePoints

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

        SphereCollider frontFP_sphereCollider, backFP_sphereCollider;
        Transform frontFP_tf, backFP_tf, frontVis_tf, backVis_tf, middleVis_tf;
        ForcePoint frontFP, backFP;

        float RopeDiameter;
        float RopeCollisionDiameter;
        float SegmentLength;

        [Header("Rope physics")]
        public float spring = 0.1f;
        public float damper = 0.1f;
        public float maximumForce = 1000f;


        public void SetRopeSizes(float rd, float rcd, float sl)
        {
            RopeDiameter = rd;
            RopeCollisionDiameter = rcd;
            SegmentLength = sl;
            SetupBits();
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
            float d = SegmentLength/2 - RopeDiameter/4;
            return ( new Vector3(0,0,d), new Vector3(0,0,-d) );
        }


        public void SetupJoint()
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

        void SetupBits()
        {
            // scale and locate all the little bits and bobs that make up
            // this rope segment depending on the parameters above.
            // Because settings these by hand is a pain.
            var (frontSpherePos, backSpherePos) = spherePositions();

            capsule = GetComponent<CapsuleCollider>();
            capsule.radius = RopeCollisionDiameter/2;
            capsule.height = SegmentLength+RopeCollisionDiameter; // we want the collision to overlap with the child's

            frontFP_tf = transform.Find("ForcePoint_F");
            backFP_tf = transform.Find("ForcePoint_B");
            frontVis_tf = transform.Find("Visuals/Front");
            backVis_tf = transform.Find("Visuals/Back");
            middleVis_tf = transform.Find("Visuals/Middle");

            frontFP_tf.localPosition = frontSpherePos;
            frontVis_tf.localPosition = frontSpherePos;
            backFP_tf.localPosition = backSpherePos;
            backVis_tf.localPosition = backSpherePos;

            var visualScale = new Vector3(RopeDiameter, RopeDiameter, RopeDiameter);
            frontVis_tf.localScale = visualScale;
            backVis_tf.localScale = visualScale;
            middleVis_tf.localScale = new Vector3(RopeDiameter, (SegmentLength/2)-(RopeDiameter/4), RopeDiameter);

            frontFP_sphereCollider = frontFP_tf.GetComponent<SphereCollider>();
            backFP_sphereCollider = backFP_tf.GetComponent<SphereCollider>();

            frontFP_sphereCollider.radius = RopeDiameter/2;
            backFP_sphereCollider.radius = RopeDiameter/2;

            frontFP = frontFP_tf.GetComponent<ForcePoint>();
            backFP = backFP_tf.GetComponent<ForcePoint>();

            frontFP.depthBeforeSubmerged = RopeDiameter/5;
            backFP.depthBeforeSubmerged = RopeDiameter/5;
        }

        void Awake()
        {
            // disable self-collisions
            rb = GetComponent<Rigidbody>();
            var ropeTagged = GameObject.FindGameObjectsWithTag(gameObject.tag);
            var ownC = GetComponent<Collider>();
            foreach(var other in ropeTagged)
            {
                Collider c;
                if(other.TryGetComponent(out c))
                {
                    Physics.IgnoreCollision(c, ownC);
                }
            }
        }

        
    }

}
