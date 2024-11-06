using UnityEngine;

namespace Force
{
    public class ForceFieldStatic : ForceFieldBase
    {

        [Header("Static Force Field")]
        [Tooltip("The force that will be applied to all ForcePoints in this field.")]
        public Vector3 staticForce;

        protected override Vector3 Field(Vector3 position)
        {
            return staticForce;
        }
    }
}