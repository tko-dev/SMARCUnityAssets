using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils = DefaultNamespace.Utils;

namespace Rope
{
    [RequireComponent(typeof(Collider))]
    public class RopeHook : MonoBehaviour
    {

        [Header("Debug")]
        public bool DrawForces = false;


        bool TestGrab(Collision collision)
        {
            RopeLink rl;
            if(collision.gameObject.TryGetComponent(out rl))
            {
                // we want to ignore collisions with the rope depending on the "up" direction of the
                // hook and the velocity of the collision
                // essentially, only grab the rope if it's moving in the same direction as the hook
                // and the rope is "above" the hook
                // both of these should be true if the force on the collider is
                // towards the "down" direction of the hook
                Vector3 hookUp = transform.up;
                Vector3 forceDirection = collision.impulse.normalized;

                if(DrawForces)
                {
                    Debug.DrawRay(collision.contacts[0].point, forceDirection, Color.red, 1.0f);
                    Debug.DrawRay(collision.contacts[0].point, hookUp, Color.green, 1.0f);
                }

                if (Vector3.Dot(forceDirection, hookUp) > 0.5f)
                {
                    return true;
                }
            }
            return false;
        }

        void OnCollisionStay(Collision collision)
        {
            if(TestGrab(collision))
            {

                var rl = collision.gameObject.GetComponent<RopeLink>();
                var generator = rl.GetGenerator();
                generator.DestroyRope();

                var winch = gameObject.AddComponent<Winch>();
                winch.RopeLength = generator.RopeLength;
                winch.RopeDiameter = generator.RopeDiameter;
                
                var vehicleConnection = Utils.FindDeepChildWithName(generator.transform.root.gameObject, generator.VehicleConnectionName);
                winch.AttachLoad(vehicleConnection);

            }
        }

    }
}