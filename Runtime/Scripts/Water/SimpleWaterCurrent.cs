using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force; // for force points

namespace DefaultNamespace.Water
{
    [RequireComponent(typeof(BoxCollider))]
    public class SimpleWaterCurrent : MonoBehaviour, IWaterCurrent
    {        
        public Vector3 current = new Vector3(0,0,0);
        Collider col;

        void Awake()
        {
            col = GetComponent<Collider>();
        }

        public Vector3 GetCurrentAt(Vector3 position)
        {
            return current;
        }

        void OnTriggerStay(Collider col)
        {
            if(col.gameObject.TryGetComponent<ForcePoint>(out ForcePoint fp))
            {
                fp.ApplyCurrent(GetCurrentAt(col.transform.position));
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
            Vector3 c = GetCurrentAt(transform.position);
            Gizmos.color = new Color(c.x, c.y, c.z, 1f);
            Gizmos.DrawRay(transform.position, c);
            Gizmos.color = new Color(c.x, c.y, c.z, 1);
            Gizmos.DrawSphere(transform.position, 0.3f);
        }

    }
}
