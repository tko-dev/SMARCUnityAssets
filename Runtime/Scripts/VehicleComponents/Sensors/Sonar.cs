using System; //Bit converter
using System.Linq;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Random = UnityEngine.Random;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace VehicleComponents.Sensors
{
    public class SonarHit
    {
        public RaycastHit hit;
        public float intensity;
        public int label;
        Sonar sonar;

        public static readonly Dictionary<string, float> simpleMaterialReflectivity = new Dictionary<string, float>()
        {
            {"Rock", 0.8f},
            {"Mud", 0.2f},
            {"Buoy", 0.99f},  // Buoy, Algae, and Rope are currently just wild guesses
            {"Algae", 0.25f},
            {"Rope", 0.4f}
        };

        public static readonly Dictionary<string, int> materialLabels = new Dictionary<string, int>()
        {
            // Labels are assigned based on importance, lower values will be over-written by higher values 
            {"Rock", 1},
            {"Mud", 1},
            {"Buoy", 3},  // Buoy, Algae, and Rope are currently just wild guesses
            {"Algae", 2},
            {"Rope", 4}
        };

        public SonarHit(Sonar sonar)
        {
            intensity = -1;
            label = 0;
            this.sonar = sonar;
        }

        public void Update(RaycastHit hit, float beam_intensity)
        {
            
            intensity = GetIntensity(beam_intensity);
            label = GetMaterialLabel();
            this.hit = hit;
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
        
        public int GetMaterialLabel()
        {
            // Return some default value for things that dont hit
            // 0 is default for no hit, hit w/o material, hit w/o named material no is materialLabels{}
            if(!(hit.collider))
            {
                return 0;
            }
            
            if(!(hit.collider.material))
            {
                return 0;
            }

            string name = hit.collider.material.name;
            // name can have " (instance of)" added to it,
            // remove that...
            if(name.Contains("("))
            {
                name = name.Split("(")[0].Trim();
            }

            // Return the label
            if(materialLabels.ContainsKey(name))
            {
                return materialLabels[name];
            }
            // if the named material has ne specified label
            return 0;
        }

        public float GetIntensity(float beamIntensity)
        {
            // intensity of hit between 1-255
            // It is a function of
            // 1) The distance traveled by the beam -> distance
            float hitDistIntensity = (sonar.max_distance - hit.distance) / sonar.max_distance;

            // 2) The angle of hit -> angle between the ray and normal
            // the hit originated from transform position, and hit sonarHit
            float hitAngle = Vector3.Angle(sonar.transform.position - hit.point, hit.normal);
            float hitAngleIntensity = Mathf.Abs(Mathf.Cos(hitAngle*Mathf.Deg2Rad));

            // 3) The properties of the point of hit -> material
            // if available, use the material of the hit object to determine the reflectivitity.
            float hitMaterialIntensity = GetMaterialReflectivity();

            // Lambert's cosine law:
            // Intensity = K * Ensonification at point * Reflectivity at p * abs(cos(incidence angle at p))
            // We just set K = 1 here.
            // Ensonification = distance traveled
            // Reflectivity = material prop.
            // Angle is obvious.
            // beamIntensity accounts for the angular dependence of the ensonification intensity, beam profile
            float intensity = beamIntensity * hitDistIntensity * hitAngleIntensity * hitMaterialIntensity;
            if(intensity > 1) intensity=1;
            if(intensity < 0) intensity=0;

            if(sonar.drawHits)
            {
                // Color c = new Color(hitDistIntensity, hitAngleIntensity, hitMaterialIntensity, intensity);
                // Color c = new Color(0.5f, 0.5f, hitMaterialIntensity, 1f);
                // Color c = new Color(0.5f, hitAngleIntensity, 0.5f, 1f);
                // Color c = new Color(hitDistIntensity, 0.5f, 0.5f, 1f);
                Color c = new Color(intensity, 0f, 0f, 1f);
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
            var point = hit.point.To<ENU>();

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



    public class Sonar : Sensor
    {

        [Header("Sonar")]
        public int beam_count = 500;
        public float beam_breadth_deg = 90;
        public float beam_fwhm_deg = 60;
        public float max_distance = 100;

        // we use this one to keep the latest hit in memory and
        // accessible to outside easily.
        public SonarHit[] sonarHits;
        [Tooltip("Draw rays in the scene view as lines?")]
        public bool drawRays = false;
        [Tooltip("Just draw the hit points as 1m-long lines?")]
        public bool drawHits = true;
        private JobHandle handle;

        private NativeArray<RaycastHit> results;
        private NativeArray<RaycastCommand> commands;

        private Color rayColor;

        public List<float> beamProfile;



        public void Start()
        {
            rayColor = Color.white; //Random.ColorHSV();
            InitHits();
            InitBeamProfileSimple();
        }

        public void InitHits()
        {
            // Initialize all the hits as empty so we can just update them later
            // rather than spamming new ones
            sonarHits = new SonarHit[beam_count];
            for(int i=0; i<beam_count; i++)
            {
                sonarHits[i] = new SonarHit(this);
            }
        }

        public void InitBeamProfileGaussian()
        {
            // Initialize the Gaussian beam profile
            // The Gaussian is specified be its full width half max (fwhm), 
            float CalculateGaussianIntensity(float beamAngle, float beamCenter, float sigma)
            {
                var gaussianIntensity = Mathf.Exp(-(Mathf.Pow(beamAngle - beamCenter, 2) / (2 * Mathf.Pow(sigma, 2))));
                return gaussianIntensity;
            }
            var angleStepDeg = beam_breadth_deg / (beam_count - 1.0f);
            var fwhmSigma = beam_fwhm_deg / (2 * Mathf.Sqrt(2.0f * Mathf.Log(2.0f)));
            beamProfile = new List<float>();
            for(int i=0; i<beam_count; i++)
            {
                var beamAngleDeg = -beam_breadth_deg / 2 + i * angleStepDeg;
                float intensity =
                    CalculateGaussianIntensity(beamAngle: beamAngleDeg, beamCenter: 0.0f, sigma: fwhmSigma);
                beamProfile.Add(intensity);
            }
        }
        
        public void InitBeamProfileSimple()
        {
            // Initialize simple beam profile
            beamProfile = new List<float>();
            for(int i=0; i<beam_count; i++)
            {
                beamProfile.Add(1.0f);
            }
        }
            
            
        public override bool UpdateSensor(double deltaTime)
        {
            if (results.Length > 0)
            {
                // Wait for the batch processing job to complete
                handle.Complete();

                for(int i=0; i<beam_count; i++)
                {
                    var hit = results[i];
                    sonarHits[i].Update(hit, beamProfile[i]);
                    if (drawRays && hit.point != Vector3.zero) Debug.DrawLine(transform.position, hit.point, rayColor);
                }


                // Dispose the buffers
                results.Dispose();
                commands.Dispose();
            }


            results = new NativeArray<RaycastHit>(beam_count, Allocator.Persistent);
            commands = new NativeArray<RaycastCommand>(beam_count, Allocator.Persistent);

            var transform1 = transform;
            var setupJob = new SetupJob()
            {
                Commands = commands,
                Origin = transform1.position,
                Direction = -transform1.up,
                Rotation_axis = transform1.forward,
                Max_distance = max_distance,
                Beam_breath_deg = beam_breadth_deg,
                Beam_count = beam_count
            };

            JobHandle deps = setupJob.Schedule(commands.Length, 10, default(JobHandle));
            handle = RaycastCommand.ScheduleBatch(commands, results, 20, deps);

            return true;
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
                var beamBreathDeg = -Beam_breath_deg / 2 + i * Beam_breath_deg / (Beam_count - 1);
                Vector3 direction = Quaternion.AngleAxis(beamBreathDeg, Rotation_axis) * Direction;
                Commands[i] = new RaycastCommand(Origin, direction, QueryParameters.Default, Max_distance);
            }
        }
    }
}