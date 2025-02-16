using UnityEngine;
using System;
using System.Collections.Generic;

public struct Vector3d
{
    public double x, y, z;
    public Vector3d(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
    public double Magnitude() => Math.Sqrt(x * x + y * y + z * z);
    public Vector3d Normalized()
    {
        double mag = Magnitude();
        return mag > 0 ? new Vector3d(x / mag, y / mag, z / mag) : new Vector3d(0, 0, 0);
    }
    public static Vector3d operator +(Vector3d a, Vector3d b) => new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Vector3d operator -(Vector3d a, Vector3d b) => new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Vector3d operator *(Vector3d a, double d) => new Vector3d(a.x * d, a.y * d, a.z * d);
    public static Vector3d operator *(double d, Vector3d a) => a * d;
    public static Vector3d operator /(Vector3d a, double d) => new Vector3d(a.x / d, a.y / d, a.z / d);
    public Vector3 ToVector3() => new Vector3((float)x, (float)y, (float)z);
}

public class NBodyPredictor : MonoBehaviour
{
    public KeplerOrchestrator keplerOrchestrator;
    public int predictionSteps = 50000;
    public float timestep = 0.01f;
    public float minGravityThreshold = 01f;
    private const double G = 10000000000000000;

    // Internal storage for predicted trajectories.
    private class TrackData
    {
        public float mass;
        public List<double> times;
        public List<Vector3d> positions;
        public List<Vector3d> velocities;
        public TrackData(Vector3 initialPosition, Vector3 initialVelocity, float mass, double startTime, int capacity)
        {
            this.mass = mass;
            times = new List<double>(capacity + 1) { startTime };
            positions = new List<Vector3d>(capacity + 1) { new Vector3d(initialPosition.x, initialPosition.y, initialPosition.z) };
            velocities = new List<Vector3d>(capacity + 1) { new Vector3d(initialVelocity.x, initialVelocity.y, initialVelocity.z) };
        }
    }

    private Dictionary<int, TrackData> tracks = new Dictionary<int, TrackData>();
    private int nextTrackId = 0;
    private double accumulatedSimTime = 0.0;

    void Update()
    {
        if (keplerOrchestrator == null)
            return;

        accumulatedSimTime += Time.deltaTime * keplerOrchestrator.timeScale;
        int steps = (int)Math.Floor(accumulatedSimTime / timestep);
        for (int i = 0; i < steps; i++)
            UpdateTracks();
        accumulatedSimTime %= timestep;
    }

    public int AddTrack(Vector3 position, Vector3 velocity, float mass)
    {
        if (keplerOrchestrator == null)
            return -1;
        double simTime = Time.time * keplerOrchestrator.timeScale;
        int id = nextTrackId++;
        var track = new TrackData(position, velocity, mass, simTime, predictionSteps);
        SimulatePrediction(track, predictionSteps);
        tracks[id] = track;
        return id;
    }

    public void RemoveTrack(int trackId)
    {
        tracks.Remove(trackId);
    }

    public List<(float time, Vector3 position, Vector3 velocity)> GetTrack(int trackId, float resolution)
    {
        var result = new List<(float, Vector3, Vector3)>();
        if (!tracks.TryGetValue(trackId, out var track))
        {
            Debug.LogWarning($"Track with ID {trackId} not found.");
            return result;
        }
        int stepInterval = Mathf.Max(1, Mathf.RoundToInt(resolution / timestep));
        for (int i = 0; i < track.times.Count; i += stepInterval)
            result.Add(((float)track.times[i], track.positions[i].ToVector3(), track.velocities[i].ToVector3()));
        return result;
    }

    public (Vector3 position, Vector3 velocity) GetTrackCurrent(int trackId)
    {
        if (tracks.TryGetValue(trackId, out var track))
            return (track.positions[0].ToVector3(), track.velocities[0].ToVector3());
        return (Vector3.zero, Vector3.zero);
    }

    // Advances each track one timestep using Velocity Verlet integration.
    private void UpdateTracks()
    {
        foreach (var kvp in tracks)
        {
            var track = kvp.Value;
            // Discard the oldest sample.
            // Discard only if there's more than one sample
            if (track.times.Count > 1)
            {
                track.times.RemoveAt(0);
                track.positions.RemoveAt(0);
                track.velocities.RemoveAt(0);
            }

            int last = track.times.Count - 1;
            double t = track.times[last];
            Vector3d x = track.positions[last];
            Vector3d v = track.velocities[last];

            Vector3d a = ComputeAcceleration(x, t);
            Debug.Log($"Acceleration: {a.x}, {a.y}, {a.z}");

            Vector3d xNew = x + v * timestep + a * (0.5 * timestep * timestep);
            double tNew = t + timestep;
            Vector3d aNew = ComputeAcceleration(xNew, tNew);
            Vector3d vNew = v + (a + aNew) * (0.5 * timestep);

            Debug.Log($"Ship Pos: {xNew.x}, {xNew.y}, {xNew.z}");

            track.times.Add(tNew);
            track.positions.Add(xNew);
            track.velocities.Add(vNew);
        }
    }

    // Precomputes a predicted trajectory using Velocity Verlet.
    private void SimulatePrediction(TrackData track, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            int last = track.times.Count - 1;
            double t = track.times[last];
            Vector3d x = track.positions[last];
            Vector3d v = track.velocities[last];

            Vector3d a = ComputeAcceleration(x, t);
            Debug.Log($"Acceleration: {a.x}, {a.y}, {a.z}");
            Vector3d xNew = x + v * timestep + a * (0.5 * timestep * timestep);
            double tNew = t + timestep;
            Vector3d aNew = ComputeAcceleration(xNew, tNew);
            Vector3d vNew = v + (a + aNew) * (0.5 * timestep);

            Debug.Log($"Ship Pos: {xNew.x}, {xNew.y}, {xNew.z}");

            track.times.Add(tNew);
            track.positions.Add(xNew);
            track.velocities.Add(vNew);
        }
    }

    // Computes gravitational acceleration at a given position and simulation time.
    private Vector3d ComputeAcceleration(Vector3d position, double simTime)
    {
        Vector3d acc = new Vector3d(0, 0, 0);
        foreach (var body in keplerOrchestrator.GetCelestialBodies())
        {
            Vector3 bodyPosF = body.GetPosition((float)simTime, keplerOrchestrator);
            Vector3d bodyPos = new Vector3d(bodyPosF.x, bodyPosF.y, bodyPosF.z);
            Vector3d diff = bodyPos - position;
            double r = diff.Magnitude();
            Vector3d dir = diff.Normalized();
            double aMag = G * body.Mass / (r * r);
            if (aMag < minGravityThreshold)
                continue;
            acc += dir * aMag;
        }
        return acc;
    }
}
