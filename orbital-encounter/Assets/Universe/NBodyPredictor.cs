using UnityEngine;
using System.Collections.Generic;

public class NBodyPredictor : MonoBehaviour
{
    public CelestialTimeService timeService;
    public KeplerPredictor keplerPredictor;

    public float forceMult;

    public (Vector3, Vector3) DoTimeStep(Vector3 position, Vector3 velocity, int timestep)
    {
        Vector3 new_velocity = velocity;

        for (int i = 0; i < keplerPredictor.bodies.Count; i++) {
            KeplerPredictor.CelestialBody body = keplerPredictor.bodies[i];

            Vector3 body_pos = keplerPredictor.GetKeplerPosition(body, timestep);
            float dist = (body_pos - position).magnitude;

            Vector3 direction_vector = (body_pos - position).normalized;
            float acceleration_mag = forceMult * body.mass / (dist * dist);
            Vector3 acceleration_vector = direction_vector * acceleration_mag;

            new_velocity = new_velocity + acceleration_vector;
        }

        Vector3 velocity_delta = (new_velocity - velocity) / 2;
        Vector3 new_position = position + velocity + velocity_delta;

        return (new_velocity, new_position);
    }


}
