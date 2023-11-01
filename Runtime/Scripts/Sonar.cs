using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace DefaultNamespace
{
    public class Sonar : MonoBehaviour
    {
        public int beam_count = 500;
        public float beam_breath_deg = 45;
        public float max_distance = 100;
        // we use this one to keep the latest hit in memory and
        // accessible to outside easily.
        public RaycastHit[] hits;
        public bool drawRays = true;
        private JobHandle handle;

        private NativeArray<RaycastHit> results;
        private NativeArray<RaycastCommand> commands;

        private Color rayColor;

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
                Commands[i] = new RaycastCommand(Origin, direction, Max_distance);
            }
        }
    }
}