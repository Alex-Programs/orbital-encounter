using UnityEngine;
using System;
using System.Collections.Generic;

// A simple double‑precision vector.
public struct Vector3d
{
    public double x, y, z;
    public Vector3d(double x, double y, double z)
    {
        this.x = x; this.y = y; this.z = z;
    }
    public double Magnitude()
    {
        return Math.Sqrt(x * x + y * y + z * z);
    }
    public Vector3d Normalized()
    {
        double m = Magnitude();
        return m > 0 ? new Vector3d(x / m, y / m, z / m) : new Vector3d(0, 0, 0);
    }
    public static Vector3d operator +(Vector3d a, Vector3d b)
    {
        return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static Vector3d operator -(Vector3d a, Vector3d b)
    {
        return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static Vector3d operator *(Vector3d a, double d)
    {
        return new Vector3d(a.x * d, a.y * d, a.z * d);
    }
    public static Vector3d operator /(Vector3d a, double d)
    {
        return new Vector3d(a.x / d, a.y / d, a.z / d);
    }
    public Vector3 ToVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }
}

// Track stores simulation state in double‐precision.
public class NBodyTrack
{
    public List<double> Times { get; private set; }
    public List<Vector3d> Positions { get; private set; }
    public List<Vector3d> Velocities { get; private set; }
    public double Mass { get; private set; }

    public NBodyTrack(Vector3 initialPosition, Vector3 initialVelocity, float mass, int capacity, double initialTime)
    {
        // Reserve capacity+1 for the initial sample.
        Times = new List<double>(capacity + 1) { initialTime };
        Positions = new List<Vector3d>(capacity + 1) { new Vector3d(initialPosition.x, initialPosition.y, initialPosition.z) };
        Velocities = new List<Vector3d>(capacity + 1) { new Vector3d(initialVelocity.x, initialVelocity.y, initialVelocity.z) };
        Mass = mass;
    }
}

public class NBodyPredictor : MonoBehaviour
{
    public KeplerOrchestrator keplerOrchestrator; // Must have public timeScale
    public int predictionSteps = 50000;
    public float timestep = 0.01f; // Simulation seconds
    public float minGravityThreshold = 0.01f;

    private Dictionary<int, NBodyTrack> tracks = new Dictionary<int, NBodyTrack>();
    private int nextTrackId = 0;
    private double accumulatedSimTime = 0.0; // In simulation seconds

    // Use a double‑precision gravitational constant in game units.
    private const double G = 1000000.0;

    void Update()
    {
        if (keplerOrchestrator == null)
            return;

        // Use the orchestrator's timeScale so simulation time = Time.time * timeScale.
        accumulatedSimTime += Time.deltaTime * keplerOrchestrator.timeScale;
        int stepsToTake = (int)Math.Floor(accumulatedSimTime / timestep);
        for (int i = 0; i < stepsToTake; i++)
        {
            UpdateTracks();
        }
        accumulatedSimTime %= timestep;
    }

    public int AddTrack(Vector3 position, Vector3 velocity, float mass)
    {
        if (keplerOrchestrator == null)
            return -1;
        double simTime = Time.time * keplerOrchestrator.timeScale;
        int trackId = nextTrackId++;
        NBodyTrack track = new NBodyTrack(position, velocity, mass, predictionSteps, simTime);
        tracks[trackId] = track;
        PredictTrack(track);
        return trackId;
    }

    public void RemoveTrack(int trackId)
    {
        tracks.Remove(trackId);
    }

    // Returns a list of (time, position, velocity) samples converting double→float.
    public List<(float time, Vector3 position, Vector3 velocity)> GetTrack(int trackId, float resolution)
    {
        if (!tracks.TryGetValue(trackId, out NBodyTrack track))
        {
            Debug.LogWarning($"Track with ID {trackId} not found.");
            return new List<(float, Vector3, Vector3)>();
        }

        List<(float time, Vector3 position, Vector3 velocity)> result =
            new List<(float time, Vector3 position, Vector3 velocity)>();
        int stepsBetweenSamples = Mathf.Max(1, Mathf.RoundToInt(resolution / timestep));

        for (int i = 0; i < track.Times.Count; i += stepsBetweenSamples)
        {
            result.Add(((float)track.Times[i], track.Positions[i].ToVector3(), track.Velocities[i].ToVector3()));
        }
        return result;
    }

    public (Vector3 position, Vector3 velocity) GetTrackCurrent(int trackId)
    {
        tracks.TryGetValue(trackId, out NBodyTrack track);

        return (track.Positions[0].ToVector3(), track.Velocities[0].ToVector3());
    }

    // Advances each track one simulation step.
    private void UpdateTracks()
    {
        foreach (var track in tracks.Values)
        {
            for (int i = 0; i < track.Positions.Count; i++)
            {
                Debug.Log($"Track position {i}: {track.Positions[i].ToVector3()}");
            }
            // Remove oldest sample.
            track.Times.RemoveAt(0);
            track.Positions.RemoveAt(0);
            track.Velocities.RemoveAt(0);

            double newTime = track.Times[track.Times.Count - 1] + timestep;
            Vector3d lastPos = track.Positions[track.Positions.Count - 1];
            Vector3d lastVel = track.Velocities[track.Velocities.Count - 1];

            Vector3d totalForce = CalculateTotalForce(lastPos, track.Mass, newTime);
            Vector3d acceleration = totalForce / track.Mass;
            Vector3d newVel = lastVel + acceleration * timestep;
            Vector3d newPos = lastPos + newVel * timestep;

            track.Times.Add(newTime);
            track.Velocities.Add(newVel);
            track.Positions.Add(newPos);
        }
    }

    // Predicts an entire track from the current state.
    private void PredictTrack(NBodyTrack track)
    {
        double simTime = track.Times[0];
        Vector3d pos = track.Positions[0];
        Vector3d vel = track.Velocities[0];

        for (int i = 0; i < predictionSteps; i++)
        {
            simTime += timestep;
            Debug.Log($"Sim time: {simTime}");
            Vector3d totalForce = CalculateTotalForce(pos, track.Mass, simTime);
            Vector3d acceleration = totalForce / track.Mass;
            vel += acceleration * timestep;
            pos += vel * timestep;

            track.Times.Add(simTime);
            track.Velocities.Add(vel);
            track.Positions.Add(pos);
        }
    }

    // Computes the total gravitational force at a given simulation time.
    private Vector3d CalculateTotalForce(Vector3d position, double mass, double simTime)
    {
        Vector3d totalForce = new Vector3d(0, 0, 0);
        foreach (var celestialBody in keplerOrchestrator.GetCelestialBodies())
        {
            // Get the body's position (in float) at simTime then convert to double.
            Vector3 bodyPosF = celestialBody.GetPosition((float)simTime, keplerOrchestrator);
            Vector3d bodyPos = new Vector3d(bodyPosF.x, bodyPosF.y, bodyPosF.z);
            Vector3d dir = bodyPos - position;
            double dist = dir.Magnitude();
            Debug.Log($"Distance: {dist}");
            if (dist < 0.001)
                continue;
            Vector3d normDir = dir.Normalized();
            double forceMagnitude = G * mass * celestialBody.Mass / (dist * dist);
            if (forceMagnitude > minGravityThreshold)
                totalForce += normDir * forceMagnitude;
        }
        return totalForce;
    }
}
