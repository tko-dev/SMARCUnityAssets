using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force;

namespace GameUI
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TrailRenderer))]
    public class ForceFieldParticle : MonoBehaviour
    {
        [Header("Force Field Particle")]
        public float Lifetime = 1f;
        public float Size = 0.1f;
        public bool DeactivateWhenOut = true;

        float remainingLifetime = 0f;
        ForcePoint FP;
        Rigidbody RB;
        TrailRenderer TR;
        SimpleGizmo FPgizmo;
        Queue<ForceFieldParticle> queue;

        void Awake()
        {
            var fp = transform.Find("ForcePoint");
            if(fp != null)
            {
                FP = fp.GetComponent<ForcePoint>();
            }
            else
            {
                Debug.LogWarning("ForceFieldParticle does not have a ForcePoint child!");
                gameObject.SetActive(false);
            }
            
            RB = GetComponent<Rigidbody>();
            TR = GetComponent<TrailRenderer>();
            FPgizmo = FP.GetComponent<SimpleGizmo>();
        }
        


        public void Spawn(Vector3 position, Queue<ForceFieldParticle> queue)
        {
            transform.position = position;
            RB.velocity = Vector3.zero;
            remainingLifetime = Lifetime;      
            gameObject.SetActive(true);
            TR.time = Lifetime;
            TR.startWidth = Size;
            TR.endWidth = 0f;
            FPgizmo.radius = Size;
            this.queue = queue;
        }


        void FixedUpdate()
        {
            if(queue == null) return;
            remainingLifetime -= Time.fixedDeltaTime;
            bool dead = remainingLifetime <= 0 || (DeactivateWhenOut && RB.GetAccumulatedForce() == Vector3.zero);
            if(dead)
            {
                TR.Clear();
                gameObject.SetActive(false);
                queue.Enqueue(this);
                return;
            }
        }


    }
}