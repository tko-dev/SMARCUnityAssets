using UnityEngine;

namespace DefaultNamespace.Water
{
    public interface IWaterCurrent
    {
        public Vector3 GetCurrentAt(Vector3 position);
    }
}