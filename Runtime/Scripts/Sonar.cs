using System.Linq;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class Sonar : MonoBehaviour
    {
        public int beam_count = 500;
        public float beam_breath_deg = 45;
        public float max_distance = 100;
        public float gain = 1;

        // we use this one to keep the latest hit in memory and
        // accessible to outside easily.
        public RaycastHit[] hits;
        public bool drawRays = false;
        public bool drawHits = true;
        private JobHandle handle;

        private NativeArray<RaycastHit> results;
        private NativeArray<RaycastCommand> commands;

        private Color rayColor;


        public static readonly Dictionary<string, float> simpleMaterialReflectivity = new Dictionary<string, float>()
        {
            {"Rock", 0.8f},
            {"Mud", 0.2f}
        };


        public static float GetMaterialReflectivity(string name)
        {
            // name can have " (instance of)" added to it,
            // remove that...
            if(name.Contains("("))
            {
                name = name.Split("(")[0].Trim();
            }

            // if its a simple one, just return that
            if(simpleMaterialReflectivity.ContainsKey(name))
            {
                return simpleMaterialReflectivity[name];
            }
            // if its a complex material that we want a function for,
            // switch for it here?
            // TODO that switch lol
            return 0.5f;
        }

        public float GetSonarHitIntensity(RaycastHit sonarHit)
        {
            // intensity of hit between 1-255
            // It is a function of
            // 1) The distance traveled by the beam -> distance
            float hitDistIntensity = (max_distance - sonarHit.distance) / max_distance;

            // 2) The angle of hit -> angle between the ray and normal
            // the hit originated from transform position, and hit sonarHit
            float hitAngle = Vector3.Angle(transform.position - sonarHit.point, sonarHit.normal);
            float hitAngleIntensity = Mathf.Sin(hitAngle*Mathf.Deg2Rad);

            // 3) The properties of the point of hit -> material
            // if available, use the material of the hit object to determine the reflectivitity.
            float hitMaterialIntensity = Sonar.GetMaterialReflectivity(sonarHit.collider.material.name);

            float intensity = hitDistIntensity * hitAngleIntensity * hitMaterialIntensity;
            intensity *= gain;
            if(intensity > 1) intensity=1;
            if(intensity < 0) intensity=0;

            if(drawHits)
            {
                Color c = new Color(hitDistIntensity, hitAngleIntensity, hitMaterialIntensity, intensity);
                // Color c = new Color(0.5f, 0.5f, hitMaterialIntensity, 1f);
                // Color c = new Color(0.5f, hitAngleIntensity, 0.5f, 1f);
                // Color c = new Color(hitDistIntensity, 0.5f, 0.5f, 1f);
                Debug.DrawRay(sonarHit.point, Vector3.up, c, 1f);
                // Debug.Log($"d:{hitDistIntensity}, a:{hitAngleIntensity}, mat:{hitMaterialIntensity}, intens:{intensity}");
            }
            return intensity;
        }


        public void Awake()
        {
            rayColor = Color.white; //Random.ColorHSV();
            hits = new RaycastHit[beam_count];
        }



        public void FixedUpdate()
        {
            if (results.Length > 0)
            {
                // Wait for the batch processing job to complete
                handle.Complete();

                for(int i=0; i<beam_count; i++)
                {
                    var hit = results[i];
                    hits[i] = hit;
                    if (drawRays && hit.point != Vector3.zero) Debug.DrawLine(transform.position, hit.point, rayColor);
                }

                // foreach (var vector3 in results.Select(result => result.point))
                // {
                //     if (vector3 != Vector3.zero) Debug.DrawLine(transform.position, vector3, rayColor);
                // }


                // Dispose the buffers
                results.Dispose();
                commands.Dispose();
            }


            results = new NativeArray<RaycastHit>(beam_count, Allocator.TempJob);
            commands = new NativeArray<RaycastCommand>(beam_count, Allocator.TempJob);

            var transform1 = transform;
            var setupJob = new SetupJob()
            {
                Commands = commands,
                Origin = transform1.position,
                Direction = -transform1.up,
                Rotation_axis = transform1.forward,
                Max_distance = max_distance,
                Beam_breath_deg = beam_breath_deg,
                Beam_count = beam_count
            };

            JobHandle deps = setupJob.Schedule(commands.Length, 10, default(JobHandle));
            handle = RaycastCommand.ScheduleBatch(commands, results, 20, deps);
        }


        [BurstCompile]
        struct SetupJob : IJobParallelFor
        {
            public NativeArray<RaycastCommand> Commands;
            public Vector3 Origin;
            public Vector3 Direction;
            public Vector3 Rotation_axis;
            public float Max_distance;
            public float Beam_breath_deg;
            public float Beam_count;

            public void Execute(int i)
            {
                var beamBreathDeg = -Beam_breath_deg / 2 + i * Beam_breath_deg / Beam_count;
                Vector3 direction = Quaternion.AngleAxis(beamBreathDeg, Rotation_axis) * Direction;
                Commands[i] = new RaycastCommand(Origin, direction, QueryParameters.Default, Max_distance);
            }
        }
    }
}