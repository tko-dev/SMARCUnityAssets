using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Force
{
    [RequireComponent(typeof(BoxCollider))]
    public class ForceFieldStatic : MonoBehaviour, IForceField
    {        
        [Header("Force Field")]
        [Tooltip("The force vector of the field. Will be applied on ForcePoints")]
        public Vector3 force = new Vector3(0,0,0);
        [Tooltip("If enabled, only ForcePoints that are UNDER water will be affected")]
        public bool onlyUnderwater = false;
        [Tooltip("If enabled, only ForcePoints that are ABOVE water will be affected")]
        public bool onlyAboveWater = false;
        Collider col;

        void Awake()
        {
            col = GetComponent<Collider>();
        }

        public Vector3 GetForceAt(Vector3 position)
        {
            return force;
        }

        void OnTriggerStay(Collider col)
        {
            if(col.gameObject.TryGetComponent<ForcePoint>(out ForcePoint fp))
            {
                fp.ApplyForce(GetForceAt(col.transform.position), onlyUnderwater, onlyAboveWater);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0, 1f, 0.01f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 c = GetForceAt(transform.position);
            Gizmos.color = new Color(c.x, c.y, c.z, 1f);
            Gizmos.DrawRay(transform.position, c);
            Gizmos.color = new Color(c.x, c.y, c.z, 1);
            Gizmos.DrawSphere(transform.position, 0.3f);
        }

    }
}
