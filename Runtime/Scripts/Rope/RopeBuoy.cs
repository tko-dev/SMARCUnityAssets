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
            RopeHook rh;
            if(collision.gameObject.TryGetComponent(out rh))
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                gameObject.AddComponent<FixedJoint>();
                joint = gameObject.GetComponent<FixedJoint>();
                joint.enablePreprocessing = false;
                joint.connectedArticulationBody = collision.gameObject.GetComponent<ArticulationBody>();
            }
        }

        
    }
}