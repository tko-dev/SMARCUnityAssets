using UnityEngine;

namespace DefaultNamespace.Water
{
    public class ObjectWaterQueryModel : WaterQueryModel
    {

        private GameObject objectToTrack;
        public override float GetWaterLevelAt(Vector3 position)
        {
            return objectToTrack.transform.position.y;
        }
    }
}
