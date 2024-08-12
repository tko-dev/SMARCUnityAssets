using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force; // ForcePoints

namespace Rope
{
    [RequireComponent(typeof(Collider))]
    public class RopeBuoy : MonoBehaviour
    {
        FixedJoint joint;


        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out RopeHook rh))
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                joint = gameObject.AddComponent<FixedJoint>();
                joint.enablePreprocessing = false;
                joint.connectedArticulationBody = collision.gameObject.GetComponent<ArticulationBody>();
            }
        }

        
    }
}