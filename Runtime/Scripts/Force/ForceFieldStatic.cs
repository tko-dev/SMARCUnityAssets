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
    }
}