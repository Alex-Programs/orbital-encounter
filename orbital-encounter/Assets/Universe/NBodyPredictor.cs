using UnityEngine;
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

        for (int i = 0; i < keplerPredictor.bodies.Count; i++) {
            KeplerPredictor.CelestialBody body = keplerPredictor.bodies[i];

            Vector3 body_pos = keplerPredictor.GetKeplerPosition(body, timestep, resolution);
            float dist = (body_pos - position).magnitude;

            Vector3 direction_vector = (body_pos - position).normalized;
            float acceleration_mag = forceMult * body.mass / (dist * dist);
            Vector3 acceleration_vector = direction_vector * acceleration_mag;

            acceleration_vector = acceleration_vector / resolution;

            newVelocity = newVelocity + acceleration_vector;
        }

        Vector3 velocity_delta = (newVelocity - velocity) / 2;
        Vector3 newPosition = position + ((velocity + velocity_delta) / resolution);

        return (newVelocity, newPosition);
    }
}
