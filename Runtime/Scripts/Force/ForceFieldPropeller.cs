using UnityEngine;

using VehicleComponents.Actuators;

namespace Force
{
    
    public class ForceFieldPropeller : ForceFieldBase
    {

        public float fov, maxRange;

        Propeller prop;

        void Awake()
        {
            prop = getProp();
            if (prop == null)
            {
                Debug.LogError("ForceFieldPropeller: No Propeller found in the hierarchy!");
                enabled = false;
            }
        }

        Propeller getProp()
        {
            var current = transform;
            while (current.parent != null)
            {
                current = current.parent;
                var prop = current.GetComponent<Propeller>();
                if (prop != null) return prop;
            }
            return null;
        }

        protected override Vector3 Field(Vector3 position)
        {
            var relativePosition = position - transform.position;
            if(relativePosition.magnitude > maxRange) return Vector3.zero;
            if(Vector3.Angle(transform.forward, relativePosition) > fov) return Vector3.zero;
            return relativePosition;
        }

        void OnDrawGizmos()
        {
            // Draw a camera frustum in front of the propeller
            var propeller = getProp();
            if (propeller != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawFrustum(
                    center: Vector3.zero,
                    fov: fov,
                    maxRange: maxRange * (propeller.reverse? -1 : 1),
                    minRange: 0,
                    aspect: 1
                );
            }
        }
    }


}