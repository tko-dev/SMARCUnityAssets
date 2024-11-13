using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Force
{
    [RequireComponent(typeof(Collider))]
    public class ForceFieldBase : MonoBehaviour
    {        
        [Header("Force Field Base")]
        [Tooltip("If enabled, only ForcePoints that are UNDER water will be affected")]
        public bool onlyUnderwater = false;
        [Tooltip("If enabled, only ForcePoints that are ABOVE water will be affected")]
        public bool onlyAboveWater = false;

        public bool IncludeInVisualizer = true;

        Collider col;

        protected virtual Vector3 Field(Vector3 position)
        {
            Debug.Log($"{this} does not implement Field(Vector3 position)!");
            return Vector3.zero;
        }
        void Awake()
        {
            col = GetComponent<Collider>();
        }

        bool IsInside(Vector3 point)
        {
            var closest = col.ClosestPoint(point);
            return closest == point;
        }

        public Vector3 GetRandomPointInside()
        {
            //TODO this can be improved for arbitrary collider shapes
            return col.bounds.center + Random.insideUnitSphere * col.bounds.extents.magnitude;
        }

        public Vector3 GetForceAt(Vector3 position)
        {
            if(!IsInside(position)) return Vector3.zero;
            return Field(position);
        }

        void OnTriggerStay(Collider objCol)
        {
            if(objCol.gameObject.TryGetComponent<ForcePoint>(out ForcePoint fp))
            {
                fp.ApplyForce(GetForceAt(objCol.transform.position), onlyUnderwater, onlyAboveWater);
            }
        }

    }
}
