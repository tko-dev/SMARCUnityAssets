using UnityEngine;

namespace Force
{
    public enum StaticForceFieldMode
    {
        Vector,
        Attarction,
        Repulsion
    }

    public class ForceFieldStatic : ForceFieldBase
    {

        [Header("Static Force Field")]
        public StaticForceFieldMode mode = StaticForceFieldMode.Vector;

        [Tooltip("Magnitude will be over-written by ForceMagnitude.")]
        public Vector3 ForceVector;
        public float ForceMagnitude = 1f;

        protected override Vector3 Field(Vector3 position)
        {
            switch(mode)
            {
                case StaticForceFieldMode.Vector:
                    return ForceVector.normalized * ForceMagnitude;

                case StaticForceFieldMode.Repulsion:
                    return (position - transform.position).normalized * ForceMagnitude;

                case StaticForceFieldMode.Attarction:
                    return (transform.position - position).normalized * ForceMagnitude;

                default:
                    return Vector3.zero;
            }
        }


    void OnDrawGizmos()
    {
        switch(mode)
            {
                case StaticForceFieldMode.Vector:
                    Gizmos.color = new Color(Mathf.Abs(ForceVector.x), Mathf.Abs(ForceVector.y), Mathf.Abs(ForceVector.z), 0.1f);
                    Collider collider = GetComponent<Collider>();
                    if (collider != null)
                    {
                        Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
                    }
                    else
                    {
                        Gizmos.DrawCube(transform.position, Vector3.one);
                    }
                    break;

                case StaticForceFieldMode.Repulsion:
                    Gizmos.color = new Color(0, 0, 1, 0.1f);
                    collider = GetComponent<Collider>();
                    if (collider != null)
                    {
                        Gizmos.DrawSphere(collider.bounds.center, collider.bounds.extents.magnitude / 2);
                        Gizmos.DrawSphere(collider.bounds.center, collider.bounds.extents.magnitude / 4);
                    }
                    else
                    {
                        Gizmos.DrawSphere(transform.position, 0.5f);
                        Gizmos.DrawSphere(transform.position, 0.25f);
                    }
                    break;

                case StaticForceFieldMode.Attarction:
                    Gizmos.color = new Color(1, 0, 0, 0.1f);
                    collider = GetComponent<Collider>();
                    if (collider != null)
                    {
                        Gizmos.DrawSphere(collider.bounds.center, collider.bounds.extents.magnitude / 2);
                        Gizmos.DrawSphere(collider.bounds.center, collider.bounds.extents.magnitude / 4);
                    }
                    else
                    {
                        Gizmos.DrawSphere(transform.position, 0.5f);
                        Gizmos.DrawSphere(transform.position, 0.25f);
                    }
                    break;

                default:
                    return;
            }
    }


    }
}