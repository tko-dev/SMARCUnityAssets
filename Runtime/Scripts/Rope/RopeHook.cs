using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rope
{
    [RequireComponent(typeof(Collider))]
    public class RopeHook : MonoBehaviour
    {

       
        [Tooltip("Radius of capsule colliders while there is a rope inside the box collider of the hook, to let the rope slide around the hook nicely.")]
        public float Enlargement = 0.08f;

        CapsuleCollider[] capsules;
        public float[] heights, rads;

        public float ShrinkageCooldown = 1f;
        public float cooldown = -1f;



        void Awake()
        {
            var colliders = transform.Find("Colliders");
            var count = colliders.transform.childCount;
            capsules = new CapsuleCollider[count];
            rads = new float[count];
            heights = new float[count];

            for(int i=0; i < colliders.transform.childCount; i++)
            {
                capsules[i] = colliders.GetChild(i).GetComponent<CapsuleCollider>();
                heights[i] = capsules[i].height;
                rads[i] = capsules[i].radius;
            }

        }
        
        void Enlarge(float enlargement)
        {
            for(int i=0; i<capsules.Length; i++)
            {
                capsules[i].radius = rads[i] + enlargement;
                // capsules[i].height = heights[i] + enlargement;
                capsules[i].center = new Vector3(0, 0, -capsules[i].radius);
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            // Enlarge the capsule collider to keep the rope segments from slipping through when
            // they inevitably separate due to forces.
            // Definitely a hack, but I cant find another way to let the rope
            // slide over nicely without creating a million rope segments...
            RopeLink rl;
            if(collider.gameObject.TryGetComponent(out rl))
            {
                Enlarge(Enlargement);
                cooldown = ShrinkageCooldown;
            }
        }

        void OnTriggerStay(Collider collider)
        {
            RopeLink rl;
            if(collider.gameObject.TryGetComponent(out rl))
            {
                cooldown = ShrinkageCooldown;
            }
        }

        void FixedUpdate()
        {
            if(cooldown == -1f) return;

            cooldown -= Time.fixedDeltaTime;
            if(cooldown < Time.fixedDeltaTime)
            {
                Enlarge(0);
                cooldown = -1f;
            }
        }

    }
}