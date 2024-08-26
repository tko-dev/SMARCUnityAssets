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
            float hitDistIntensity = (sonar.MaxRange - hit.distance) / sonar.MaxRange;

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

            if(sonar.DrawHits)
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


    public enum SonarType
    {
        FLS,
        SSS,
        MBES
    }


    public class Sonar : Sensor
    {

        [Header("Sonar")]
        [Tooltip("Numer of rays cast per beam. Beam = A fan of rays.")]
        public int NumRaysPerBeam = 500;
        [Tooltip("Total opening angle of _each_ beam.")]
        public float BeamBreadthDeg = 90;
        [Tooltip("How many beams(fans) are in this arrangement of sonar. For FLS, they will be arranged left-to-right with fan opening in the forward axis. For SSS and MBES, they will be arranged left-to-right with fan opening also left-to-right.")]
        public int NumBeams = 1;
        public float MaxRange = 100;
        public SonarType Type = SonarType.MBES;
        [Tooltip("-3dB opening angle of each beam. For beam-pattern related return intensity calculations.")]
        public float BeamBreadth3DecibelsDeg = 60;
        [Tooltip("Angle from the forward-right plane (usually horizontal-ish) of the beam. For SSS, 180-(2*(tilt+BeamBreath)) = nadir. For FLS, just the tilt downwards.")]        
        public float TiltAngleDeg = 15;
        [Tooltip("For FLS: FOV of the beams")]
        public float FLSFOVDeg = 30;

        // we use this one to keep the latest hit in memory and
        // accessible to outside easily.
        [HideInInspector] public SonarHit[] SonarHits;

        [Header("Visuals")]
        [Tooltip("Draw rays in the scene view as lines?")]
        public bool DrawRays = false;
        [Tooltip("Just draw the hit points as 1m-long lines?")]
        public bool DrawHits = true;
        private Color rayColor;

        [Header("Load")]
        public float TimeShareInFixedUpdate;


        // Unity job structure for long-term casting of rays.
        private JobHandle handle;
        private NativeArray<RaycastHit> results;
        private NativeArray<RaycastCommand> commands;

        [HideInInspector] public int TotalRayCount => NumRaysPerBeam * NumBeams;
        [HideInInspector] public float DegreesPerRayInBeam => BeamBreadthDeg/(NumRaysPerBeam-1);
        [HideInInspector] public float DegreesPerBeamInFLS => FLSFOVDeg/(NumBeams-1);

        [HideInInspector] public List<float> BeamProfile;


        new void OnValidate()
        {
            base.OnValidate();
            if(Type == SonarType.SSS) NumBeams = 2;
            if(Type == SonarType.MBES)
            {
                NumBeams = 1;  
                TiltAngleDeg = -1;
            } 
            if(NumRaysPerBeam <= 0) NumRaysPerBeam = 1;
        }


        new void Awake()
        {
            base.Awake();
            rayColor = Color.white;
            InitHits();
            InitBeamProfileSimple();
        }

        void InitHits()
        {
            // Initialize all the hits as empty so we can just update them later
            // rather than spamming new ones
            SonarHits = new SonarHit[TotalRayCount];
            for(int i=0; i<TotalRayCount; i++)
            {
                SonarHits[i] = new SonarHit(this);
            }
        }

        void InitBeamProfileGaussian()
        {
            // Initialize the Gaussian beam profile
            // The Gaussian is specified be its full width half max (fwhm), 
            float CalculateGaussianIntensity(float beamAngle, float beamCenter, float sigma)
            {
                var gaussianIntensity = Mathf.Exp(-(Mathf.Pow(beamAngle - beamCenter, 2) / (2 * Mathf.Pow(sigma, 2))));
                return gaussianIntensity;
            }
            var angleStepDeg = BeamBreadthDeg / (NumRaysPerBeam - 1.0f);
            var fwhmSigma = BeamBreadth3DecibelsDeg / (2 * Mathf.Sqrt(2.0f * Mathf.Log(2.0f)));
            BeamProfile = new List<float>();
            for(int i=0; i<NumRaysPerBeam; i++)
            {
                var beamAngleDeg = -BeamBreadthDeg / 2 + i * angleStepDeg;
                float intensity =
                    CalculateGaussianIntensity(beamAngle: beamAngleDeg, beamCenter: 0.0f, sigma: fwhmSigma);
                BeamProfile.Add(intensity);
            }
        }
        
        void InitBeamProfileSimple()
        {
            // Initialize simple beam profile
            BeamProfile = new List<float>();
            for(int i=0; i<NumRaysPerBeam; i++)
            {
                BeamProfile.Add(1.0f);
            }
        }

        public static (int, int) BeamNumRayNumFromRayIndex(int i, int NumRaysPerBeam)
        {
                var rayNum = i % NumRaysPerBeam;
                int beamNum = (int)i / (int)NumRaysPerBeam;
                return (beamNum, rayNum);
        }
            
            
        public override bool UpdateSensor(double deltaTime)
        {
            var t0 = Time.realtimeSinceStartup;
            if (results.Length > 0)
            {
                // Wait for the batch processing job to complete
                handle.Complete();

                // TODO this should be done in parallel too...?
                for(int i=0; i < TotalRayCount; i++)
                {
                    var hit = results[i];
                    var (beamNum, rayNum) = Sonar.BeamNumRayNumFromRayIndex(i, NumRaysPerBeam);
                    SonarHits[i].Update(hit, BeamProfile[rayNum]);  
                    if (DrawRays && hit.point != Vector3.zero)
                    {
                        if(i < TotalRayCount / 2) rayColor = Color.blue;
                        if(i >= TotalRayCount / 2) rayColor = Color.red;
                        Debug.DrawLine(transform.position, hit.point, rayColor, 1f);
                    }
                }
                
                // Dispose the buffers
                results.Dispose();
                commands.Dispose();
            }


            results = new NativeArray<RaycastHit>(TotalRayCount, Allocator.Persistent);
            commands = new NativeArray<RaycastCommand>(TotalRayCount, Allocator.Persistent);

            var setupJob = new SetupSonarRaycastJob()
            {
                Commands = commands,
                NumRaysPerBeam = NumRaysPerBeam,
                SonarUp = transform.up,
                SonarForward = transform.forward,
                SonarRight = transform.right,
                SonarPosition = transform.position,
                Type = Type,
                DegreesPerRayInBeam = DegreesPerRayInBeam,
                BeamBreadthDeg = BeamBreadthDeg,
                MaxRange = MaxRange,
                TiltAngleDeg = TiltAngleDeg,
                FLSFOVDeg = FLSFOVDeg,
                DegreesPerBeamInFLS = DegreesPerBeamInFLS
            };

            JobHandle deps = setupJob.Schedule(commands.Length, 10, default(JobHandle));
            handle = RaycastCommand.ScheduleBatch(commands, results, 20, deps);

            var t1 = Time.realtimeSinceStartup;
            TimeShareInFixedUpdate = (t1-t0)/Time.fixedDeltaTime;
            if(TimeShareInFixedUpdate > 0.5f) Debug.LogWarning($"Sonar in {transform.root.name}/{transform.name} took more than half the time in a fixedUpdate!");

            return true;
        }

        [BurstCompile]
        struct SetupSonarRaycastJob : IJobParallelFor
        {
            public NativeArray<RaycastCommand> Commands;
            public int NumRaysPerBeam;
            public Vector3 SonarUp;
            public Vector3 SonarForward;
            public Vector3 SonarRight;
            public Vector3 SonarPosition;
            public SonarType Type;
            public float DegreesPerRayInBeam;
            public float BeamBreadthDeg;
            public float MaxRange;
            public float TiltAngleDeg;
            public float FLSFOVDeg;
            public float DegreesPerBeamInFLS;

            public void Execute(int i)
            {
                var direction = new Vector3(0, 0, 1);
                var (beamNum, rayNum) = Sonar.BeamNumRayNumFromRayIndex(i, NumRaysPerBeam);
                var rayAngle = (rayNum * DegreesPerRayInBeam) - BeamBreadthDeg/2;
                if(Type == SonarType.FLS)
                {
                    // FLS beams are defined as vertical fans, sweeping side-to-side
                    // so we start a ray forward first.
                    direction = SonarForward;
                    // then we rotate _that_ to the ray angle within the beam, around the side-axis
                    // plus the tilt angle which is measured from the horizontal plane down
                    direction = Quaternion.AngleAxis(rayAngle+TiltAngleDeg, SonarRight) * direction;
                    // then we rotate it to the beam angle, around the UP axis
                    var beamAngle = (beamNum * DegreesPerBeamInFLS) - FLSFOVDeg/2;
                    direction = Quaternion.AngleAxis(beamAngle, SonarUp) * direction;
                }
                else
                {
                    // MBES or SSS
                    // start a beam looking directly down
                    direction = -SonarUp;
                    // rotate it around the forward axis by its ray number in the beam
                    // offset half-way so the middle is directly down.
                    direction = Quaternion.AngleAxis(rayAngle, SonarForward) * direction;
                    if(Type == SonarType.SSS)
                    {
                        // SSS usually has 2 beams, port and starboard
                        // their position is measured from the horizontal axis towards the vertical axis
                        // called the tilt angle, so we need to further rotate the rays accordingly
                        var side = (beamNum * 2)-1; // port or starboard
                        var tiltAngleWithSide = side * (90-TiltAngleDeg);
                        direction = Quaternion.AngleAxis(tiltAngleWithSide, SonarForward) * direction;
                    }
                }
                
                // and finally, cast dem rays boi.
                Commands[i] = new RaycastCommand(SonarPosition, direction, QueryParameters.Default, MaxRange);
            }
        }
    }


}