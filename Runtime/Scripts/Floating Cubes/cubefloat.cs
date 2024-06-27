using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float waterLevel = 0.0f; // The y position of the water surface
    public float buoyancyStrength = 10.0f; // The strength of the buoyant force
    public float dragInWater = 1.0f; // Drag applied when the object is in water
    public float angularDragInWater = 1.0f; // Angular drag applied when the object is in water

    private ArticulationBody articulationBody;
    private Rigidbody rigidbody;

    void Start()
    {
        articulationBody = GetComponent<ArticulationBody>();
        rigidbody = GetComponent<Rigidbody>();

        if (articulationBody == null && rigidbody == null)
        {
            Debug.LogError("No ArticulationBody or Rigidbody found on the object!");
        }
    }

    void FixedUpdate()
    {
        ApplyBuoyancy();
    }

    void ApplyBuoyancy()
    {
        if (articulationBody != null)
        {
            ApplyBuoyancyToArticulationBody();
        }
        else if (rigidbody != null)
        {
            ApplyBuoyancyToRigidbody();
        }
    }

    void ApplyBuoyancyToArticulationBody()
    {
        // Calculate how much of the object is submerged
        float submergedVolume = Mathf.Clamp01((waterLevel - articulationBody.transform.position.y) / transform.localScale.y);
        float buoyantForce = buoyancyStrength * submergedVolume;

        // Apply the buoyant force
        articulationBody.AddForce(Vector3.up * buoyantForce, ForceMode.Acceleration);

        // Apply drag if submerged
        if (articulationBody.transform.position.y < waterLevel)
        {
            articulationBody.AddForce(-articulationBody.velocity * dragInWater, ForceMode.Acceleration);
            articulationBody.AddTorque(-articulationBody.angularVelocity * angularDragInWater, ForceMode.Acceleration);
        }
    }

    void ApplyBuoyancyToRigidbody()
    {
        // Calculate how much of the object is submerged
        float submergedVolume = Mathf.Clamp01((waterLevel - rigidbody.transform.position.y) / transform.localScale.y);
        float buoyantForce = buoyancyStrength * submergedVolume;

        // Apply the buoyant force
        rigidbody.AddForce(Vector3.up * buoyantForce, ForceMode.Acceleration);

        // Apply drag if submerged
        if (rigidbody.transform.position.y < waterLevel)
        {
            rigidbody.AddForce(-rigidbody.velocity * dragInWater, ForceMode.Acceleration);
            rigidbody.AddTorque(-rigidbody.angularVelocity * angularDragInWater, ForceMode.Acceleration);
        }
    }
}

