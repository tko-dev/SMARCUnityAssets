using System; //Bit converter
using System.Linq;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Random = UnityEngine.Random;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace DefaultNamespace
{
    public class SonarHit
    {
        public RaycastHit hit;
        public float intensity;
        Sonar sonar;

        public static readonly Dictionary<string, float> simpleMaterialReflectivity = new Dictionary<string, float>()
        {
            {"Rock", 0.8f},
            {"Mud", 0.2f}
        };

        public SonarHit(Sonar sonar)
        {
            intensity = -1;
            this.sonar = sonar;
        }

        public void Update(RaycastHit hit)
        {
            this.hit = hit;
            this.intensity = GetIntensity();
        }


        public float GetMaterialReflectivity()
        {
            // Return some default value for things that dont hit
            // 0 intensity = no hit
            if(!(hit.collider))
            {
                return 0f;
            }
            if(!(hit.collider.material))
            {
                return 0.5f;
            }

            string name = hit.collider.material.name;
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

        public float GetIntensity()
        {
            // intensity of hit between 1-255
            // It is a function of
            // 1) The distance traveled by the beam -> distance
            float hitDistIntensity = (sonar.max_distance - hit.distance) / sonar.max_distance;

            // 2) The angle of hit -> angle between the ray and normal
            // the hit originated from transform position, and hit sonarHit
            float hitAngle = Vector3.Angle(sonar.transform.position - hit.point, hit.normal);
            float hitAngleIntensity = Mathf.Sin(hitAngle*Mathf.Deg2Rad);

            // 3) The properties of the point of hit -> material
            // if available, use the material of the hit object to determine the reflectivitity.
            float hitMaterialIntensity = GetMaterialReflectivity();

            float intensity = hitDistIntensity * hitAngleIntensity * hitMaterialIntensity;
            if(intensity > 1) intensity=1;
            if(intensity < 0) intensity=0;

            if(sonar.drawHits)
            {
                Color c = new Color(hitDistIntensity, hitAngleIntensity, hitMaterialIntensity, intensity);
                // Color c = new Color(0.5f, 0.5f, hitMaterialIntensity, 1f);
                // Color c = new Color(0.5f, hitAngleIntensity, 0.5f, 1f);
                // Color c = new Color(hitDistIntensity, 0.5f, 0.5f, 1f);
                Debug.DrawRay(hit.point, Vector3.up, c, 1f);
                // Debug.Log($"d:{hitDistIntensity}, a:{hitAngleIntensity}, mat:{hitMaterialIntensity}, intens:{intensity}");
            }
            return intensity;
        }

        public byte[] GetBytes()
        {
            // so first, we gotta convert the unity points to ros points
            // then x,y,z need to be byte-ified
            // then a fourth "intensity" needs to be created and byte-ified
            var point = hit.point.To<FLU>();

            var xb = BitConverter.GetBytes(point.x);
            var yb = BitConverter.GetBytes(point.y);
            var zb = BitConverter.GetBytes(point.z);

            byte[] ib = {(byte)(intensity*255)};

            int totalBytes = xb.Length + yb.Length + zb.Length+ ib.Length;
            byte[] ret = new byte[totalBytes];
            // src, offset, dest, offset, count
            // Imma hard-code the offsets and counts, to act as a weird
            // error catching mechanism
            Buffer.BlockCopy(xb, 0, ret, 0, 4);
            Buffer.BlockCopy(yb, 0, ret, 4, 4);
            Buffer.BlockCopy(zb, 0, ret, 8, 4);
            Buffer.BlockCopy(ib, 0, ret, 12,1);

            return ret;
        }
    }
    public class Sonar : MonoBehaviour
    {
        public int beam_count = 500;
        public float beam_breath_deg = 45;
        public float max_distance = 100;

        // we use this one to keep the latest hit in memory and
        // accessible to outside easily.
        public SonarHit[] sonarHits;
        public bool drawRays = false;
        public bool drawHits = true;
        private JobHandle handle;

        private NativeArray<RaycastHit> results;
        private NativeArray<RaycastCommand> commands;

        private Color rayColor;





        public void Awake()
        {
            rayColor = Color.white; //Random.ColorHSV();
            // Initialize all the hits as empty so we can just update them later
            // rather than spamming new ones
            sonarHits = new SonarHit[beam_count];
            for(int i=0; i<beam_count; i++)
            {
                sonarHits[i] = new SonarHit(this);
            }
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
                    sonarHits[i].Update(hit);
                    if (drawRays && hit.point != Vector3.zero) Debug.DrawLine(transform.position, hit.point, rayColor);
                }


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