// using UnityEngine;

// public class Catenary : MonoBehaviour
// {
//     [Range(0.01f, 1f)]
//     public float wireRadius = 0.02f;
//     [Range(1.5f, 100f)]
//     public float wireCatenary = 10f;
//     [Range(0.1f, 10f)]
//     public float wireResolution = 0.1f;
//     public Transform p1;
//     public Transform p2;

//     public PrimitiveType primitive = PrimitiveType.Cube;

//     private float initialWireResolution;
//     private GameObject[] ropeSegments;
//     private ArticulationBody[] segmentArticulationBodies;

//     void Start()
//     {
//         initialWireResolution = wireResolution;
//         Regenerate();
//     }

//     void Update()
//     {
//         Regenerate();
//     }

//     float CosH(float t)
//     {
//         return (Mathf.Exp(t) + Mathf.Exp(-t)) / 2;
//     }

//     public float CalculateCatenary(float a, float x)
//     {
//         return a * CosH(x / a);
//     }

//     public void Regenerate()
//     {
//         float distance = Vector3.Distance(p1.position, p2.position);
//         int nPoints = Mathf.Max(2, (int)(distance / initialWireResolution) + 1); // Ensure at least two points

//         Vector3[] wirePoints = new Vector3[nPoints];
//         wirePoints[0] = p1.position;
//         wirePoints[nPoints - 1] = p2.position;

//         Vector3 dir = (p2.position - p1.position).normalized;
//         float offset = CalculateCatenary(wireCatenary, -distance / 2);

//         for (int i = 1; i < nPoints - 1; ++i)
//         {
//             Vector3 wirePoint = p1.position + i * initialWireResolution * dir;

//             float x = i * initialWireResolution - distance / 2;
//             wirePoint.y = wirePoint.y - (offset - CalculateCatenary(wireCatenary, x));

//             wirePoints[i] = wirePoint;
//         }
//         GenerateWithPrimitive(wirePoints);
//     }

//     private void GenerateWithPrimitive(Vector3[] wirePoints)
//     {
//         if (primitive == PrimitiveType.Plane || primitive == PrimitiveType.Quad)
//             primitive = PrimitiveType.Cube;

//         int numSegments = wirePoints.Length - 1;

//         if (ropeSegments == null || ropeSegments.Length != numSegments)
//         {
//             // Destroy old segments if they exist
//             if (ropeSegments != null)
//             {
//                 foreach (var segment in ropeSegments)
//                 {
//                     Destroy(segment);
//                 }
//             }

//             // Create new segments
//             ropeSegments = new GameObject[numSegments];
//             segmentArticulationBodies = new ArticulationBody[numSegments];
//             for (int i = 0; i < numSegments; i++)
//             {
//                 ropeSegments[i] = GameObject.CreatePrimitive(primitive);
//                 ropeSegments[i].transform.parent = transform;
//                 ropeSegments[i].GetComponent<Renderer>().material.color = Color.yellow; // Optional: set rope color

//                 // Add ArticulationBody
//                 var ab = ropeSegments[i].AddComponent<ArticulationBody>();
//                 ab.mass = 0.1f; // Set the mass of the rope segment
//                 ab.jointType = ArticulationJointType.FixedJoint;
//                 segmentArticulationBodies[i] = ab;
//             }
//         }

//         for (int segment = 0; segment < numSegments; ++segment)
//         {
//             Vector3 start = wirePoints[segment];
//             Vector3 end = wirePoints[segment + 1];
//             Vector3 midPoint = (start + end) / 2;

//             ropeSegments[segment].transform.position = midPoint;

//             Vector3 scale = ropeSegments[segment].transform.localScale;
//             scale.x = wireRadius * 2;
//             scale.z = wireRadius * 2;
//             scale.y = Vector3.Distance(start, end);
//             ropeSegments[segment].transform.localScale = scale;

//             ropeSegments[segment].transform.up = (end - start).normalized;
//         }

//         // Connect segments with articulation joints
//         for (int i = 0; i < numSegments - 1; i++)
//         {
//             ConnectArticulationBodies(segmentArticulationBodies[i], segmentArticulationBodies[i + 1]);
//         }
//     }

//     private void ConnectArticulationBodies(ArticulationBody parent, ArticulationBody child)
//     {
//         child.transform.parent = parent.transform;
//         child.jointType = ArticulationJointType.FixedJoint;

//         var joint = child.GetComponent<ArticulationBody>();
//         joint.anchorPosition = Vector3.zero;
//         joint.anchorRotation = Quaternion.identity;
//         joint.parentAnchorPosition = Vector3.zero;
//         joint.parentAnchorRotation = Quaternion.identity;
//     }

//     private float GetHeightByPrimitive(PrimitiveType type)
//     {
//         switch (type)
//         {
//             case PrimitiveType.Cube:
//                 return 1.0f;
//             case PrimitiveType.Capsule:
//             case PrimitiveType.Cylinder:
//                 return 2.0f;
//             default:
//                 return 1.0f;
//         }
//     }
// }


using UnityEngine;

public class Catenary : MonoBehaviour
{
    [Range(0.01f, 1f)]
    public float wireRadius = 0.02f;
    [Range(1.5f, 100f)]
    public float wireCatenary = 10f;
    [Range(0.1f, 10f)]
    public float wireResolution = 0.1f;
    public Transform p1;
    public Transform p2;

    public PrimitiveType primitive = PrimitiveType.Cube;

    private float initialWireResolution;
    private GameObject[] ropeSegments;
    private Rigidbody[] segmentRigidbodies;

    void Start()
    {
        initialWireResolution = wireResolution;
        Regenerate();
    }

    void Update()
    {
        Regenerate();
    }

    float CosH(float t)
    {
        return (Mathf.Exp(t) + Mathf.Exp(-t)) / 2;
    }

    public float CalculateCatenary(float a, float x)
    {
        return a * CosH(x / a);
    }

    public void Regenerate()
    {
        float distance = Vector3.Distance(p1.position, p2.position);
        int nPoints = Mathf.Max(2, (int)(distance / initialWireResolution) + 1); // Ensure at least two points

        Vector3[] wirePoints = new Vector3[nPoints];
        wirePoints[0] = p1.position;
        wirePoints[nPoints - 1] = p2.position;

        Vector3 dir = (p2.position - p1.position).normalized;
        float offset = CalculateCatenary(wireCatenary, -distance / 2);

        for (int i = 1; i < nPoints - 1; ++i)
        {
            Vector3 wirePoint = p1.position + i * initialWireResolution * dir;

            float x = i * initialWireResolution - distance / 2;
            wirePoint.y = wirePoint.y - (offset - CalculateCatenary(wireCatenary, x));

            wirePoints[i] = wirePoint;
        }
        GenerateWithPrimitive(wirePoints);
    }

    private void GenerateWithPrimitive(Vector3[] wirePoints)
    {
        if (primitive == PrimitiveType.Plane || primitive == PrimitiveType.Quad)
            primitive = PrimitiveType.Cube;

        int numSegments = wirePoints.Length - 1;

        if (ropeSegments == null || ropeSegments.Length != numSegments)
        {
            // Destroy old segments if they exist
            if (ropeSegments != null)
            {
                foreach (var segment in ropeSegments)
                {
                    Destroy(segment);
                }
            }

            // Create new segments
            ropeSegments = new GameObject[numSegments];
            segmentRigidbodies = new Rigidbody[numSegments];
            for (int i = 0; i < numSegments; i++)
            {
                ropeSegments[i] = GameObject.CreatePrimitive(primitive);
                ropeSegments[i].transform.parent = transform;
                ropeSegments[i].GetComponent<Renderer>().material.color = Color.yellow; // Optional: set rope color

                // Add Rigidbody and set mass
                var rb = ropeSegments[i].AddComponent<Rigidbody>();
                rb.mass = 0.01f; // Set the mass of the rope segment
                segmentRigidbodies[i] = rb;
            }
        }

        for (int segment = 0; segment < numSegments; ++segment)
        {
            Vector3 start = wirePoints[segment];
            Vector3 end = wirePoints[segment + 1];
            Vector3 midPoint = (start + end) / 2;

            ropeSegments[segment].transform.position = midPoint;

            Vector3 scale = ropeSegments[segment].transform.localScale;
            scale.x = wireRadius * 2;
            scale.z = wireRadius * 2;
            scale.y = Vector3.Distance(start, end);
            ropeSegments[segment].transform.localScale = scale;

            ropeSegments[segment].transform.up = (end - start).normalized;
        }
    }

    private void FixedUpdate()
    {
        ApplyBuoyancy();
    }

    private void ApplyBuoyancy()
    {
        foreach (var rb in segmentRigidbodies)
        {
            if (rb != null)
            {
                // Calculate buoyancy force
                float buoyancyForce = rb.mass * Physics.gravity.magnitude;
                rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Force);
            }
        }
    }

    private float GetHeightByPrimitive(PrimitiveType type)
    {
        switch (type)
        {
            case PrimitiveType.Cube:
                return 1.0f;
            case PrimitiveType.Capsule:
            case PrimitiveType.Cylinder:
                return 2.0f;
            default:
                return 1.0f;
        }
    }
}
