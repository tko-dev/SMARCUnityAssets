using UnityEngine;

namespace Force
{
    public interface IForceField
    {
        public Vector3 GetForceAt(Vector3 position);
    }
}