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

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0, 1f, 0.05f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }

        void OnDrawGizmosSelected()
        {
            BoxCollider col = GetComponent<BoxCollider>();
            Vector3 size = col.bounds.size;
            // Random numbers. 5 seems ok for inside of a box.
            // Actually getting the exact volume of an arbitrary box from
            // a collider seems like a PITA, so i shant.
            double sizeDiv = 5;
            double spacing = 1;
            for(double x = -(size.x)/sizeDiv; x < (size.x)/sizeDiv; x += spacing)
                for(double y = -(size.y)/sizeDiv; y < (size.y)/sizeDiv; y += spacing)
                    for(double z = -(size.z)/sizeDiv; z < (size.z)/sizeDiv; z += spacing)
                    {
                        Vector3 p = new Vector3((float)x,(float)y,(float)z);
                        Vector3 c = GetCurrentAt(p);
                        Gizmos.color = new Color(c.x, c.y, c.z, 1f);
                        Gizmos.DrawRay(transform.position + p, c);
                        Gizmos.color = new Color(c.x, c.y, c.z, 0.1f);
                        Gizmos.DrawSphere(transform.position + p, 0.1f);
                    }
        }

    }
}
