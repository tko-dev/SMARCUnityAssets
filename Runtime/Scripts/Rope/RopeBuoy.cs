using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force; // ForcePoints

namespace Rope
{
    [RequireComponent(typeof(Collider))]
    public class RopeBuoy : MonoBehaviour
    {
        Joint joint;

        void Awake()
        {
            joint = GetComponent<Joint>();
        }

        void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.name == "Horizontal")
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                joint.connectedArticulationBody = collision.gameObject.GetComponent<ArticulationBody>();
                transform.Find("ForcePoint").gameObject.SetActive(false);
            }
            Debug.Log($"Colliding with:{collision.gameObject.name}");
        }

        
    }
}