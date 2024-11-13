using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force;

namespace GameUI
{
    public class ForceFieldVisualizer : MonoBehaviour
    {   
        [Header("Force Field Visualizer")]
        public GameObject ParticlePrefab;
        public int ParticleCount = 100;
        public float ParticleLifetime = 1f;
        public float ParticleSize = 0.1f;

        [Tooltip("If enabled, the particles will be recycled when they are out of all fields")]
        public bool RecycleWhenOut = true;


        ForceFieldBase[] fields;
        Queue<ForceFieldParticle> particleQueue;

        void Awake()
        {
            if(ParticlePrefab == null)
            {
                Debug.LogError("ParticlePrefab is null!");
                enabled = false;
            }
        }


        void Start()
        {
            fields = FindObjectsByType<ForceFieldBase>(FindObjectsSortMode.None);
            particleQueue = new Queue<ForceFieldParticle>(ParticleCount);
            for(int i = 0; i < ParticleCount; i++)
            {
                var go = Instantiate(ParticlePrefab, transform);
                go.SetActive(false);
                var FFP = go.GetComponent<ForceFieldParticle>();
                FFP.Lifetime = Random.Range(0, ParticleLifetime);
                FFP.DeactivateWhenOut = RecycleWhenOut;
                FFP.Size = ParticleSize;
                particleQueue.Enqueue(FFP);
            }
        }

        void FixedUpdate()
        {
            if(particleQueue.Count == 0) return;
            do
            {
                var particle = particleQueue.Dequeue();
                if(particle.gameObject.activeSelf) continue;
                // pick one of the fields randomly
                var field = fields[Random.Range(0, fields.Length)];
                // spawn the particle at a random position inside the field
                var position = field.GetRandomPointInside();
                particle.Spawn(position, particleQueue);

            } while(particleQueue.Count > 0);
        }
        
        
    }
}