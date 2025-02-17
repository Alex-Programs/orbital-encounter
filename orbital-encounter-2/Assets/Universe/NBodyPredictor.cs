using UnityEngine;
using System;
using System.Collections.Generic;

public class NBodyPredictor : MonoBehaviour
{
    public CelestialTimeService timeService;
    public KeplerPredictor keplerPredictor;

    public float forceMult;

    public (Vector3, Vector3) DoTimeStep(Vector3 position, Vector3 velocity, int timestep)
{
    Vector3 newVelocity = velocity;
    float resolution = (float)timeService.GetCelestialTimeResolution();
    
    // Store acceleration data for preprocessing
    List<(Vector3 acceleration, float magnitude)> accelerations = new List<(Vector3, float)>();
    float totalAccelMagnitude = 0f;

    // Calculate all accelerations first
    for (int i = 0; i < keplerPredictor.bodies.Count; i++)
    {
        KeplerPredictor.CelestialBody body = keplerPredictor.bodies[i];
        Vector3 body_pos = keplerPredictor.GetKeplerPosition(body, timestep, resolution);
        float dist = (body_pos - position).magnitude;
        Vector3 direction_vector = (body_pos - position).normalized;
        float acceleration_mag = forceMult * body.mass / (dist * dist);
        Vector3 acceleration_vector = (direction_vector * acceleration_mag) / resolution;
        
        accelerations.Add((acceleration_vector, acceleration_vector.magnitude));
        totalAccelMagnitude += acceleration_vector.magnitude;
    }

    // Sort accelerations by magnitude (descending)
    accelerations.Sort((a, b) => b.magnitude.CompareTo(a.magnitude));

    // Check if the largest acceleration is > 80% of total
    if (accelerations.Count > 0 && accelerations[0].magnitude > totalAccelMagnitude * 0.8f)
    {
        // Use only the dominant acceleration
        newVelocity += accelerations[0].acceleration;
    }
    else
    {
        // Use only top 3 accelerations
        int count = Math.Min(3, accelerations.Count);
        for (int i = 0; i < count; i++)
        {
            newVelocity += accelerations[i].acceleration;
        }
    }

    // Calculate final position
    Vector3 velocity_delta = (newVelocity - velocity) / 2;
    Vector3 newPosition = position + ((velocity + velocity_delta) / resolution);
    
    return (newVelocity, newPosition);
}
}
