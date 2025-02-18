using UnityEngine;
using System.Collections.Generic;
using System;

public class NBodyTrackManager : MonoBehaviour
{
    public CelestialTimeService timeService;
    public NBodyPredictor nBodyPredictor;

    public int lookaheadDistance = 5000;
    public int lookaheadSteps = 250;

    public struct NBodyTrackPoint {
        public Vector3 position;
        public Vector3 velocity;
        public int timestep;
    }

    public struct NBodyTrack {
        public List<NBodyTrackPoint> points;
        public Color color;
    }

    private Dictionary<int, NBodyTrack> trackDictionary = new();

    public void CreateTrack(int id, Vector3 initialPosition, Vector3 initialVelocity, Color color) {
        List<NBodyTrackPoint> trackPoints = new List<NBodyTrackPoint>();

        int startingTime = timeService.GetCelestialTime();

        Vector3 currentPos = initialPosition;
        Vector3 currentVel = initialVelocity;

        for (int i = 0; i < lookaheadSteps / 8; i++) {
            trackPoints.Add(new NBodyTrackPoint {
                position = currentPos,
                velocity = currentVel,
                timestep = startingTime + i
            });

            Debug.Log($"{i}: {currentPos.x}, {currentPos.y}, {currentPos.z} | {currentVel.x}, {currentVel.y}, {currentVel.z}");

            (currentVel, currentPos) = nBodyPredictor.DoTimeStep(currentPos, currentVel, startingTime + i);
        }

        trackDictionary[id] = new NBodyTrack {
            points = trackPoints,
            color = color,
        };
    }

    void Update()
    {
        foreach (var id in trackDictionary.Keys)
        {
            ShuffleTrack(id);
        }
    }

    public (Vector3 position, Vector3 velocity)? GetTrackPoint(int id, int timestamp) {
        if (!trackDictionary.ContainsKey(id)) return null;

        List<NBodyTrackPoint> trackPoints = trackDictionary[id].points;
        foreach (var point in trackPoints) {
            if (point.timestep == timestamp) {
                return (point.position, point.velocity);
            }
        }
        return null;
    }

    void ShuffleTrack(int id) {
        if (!trackDictionary.ContainsKey(id)) return;

        int currentTime = timeService.GetCelestialTime();
        List<NBodyTrackPoint> trackPoints = trackDictionary[id].points;

        int deleteCount = 0;
        while (trackPoints.Count > 0 && trackPoints[0].timestep < currentTime) {
            trackPoints.RemoveAt(0);
            deleteCount++;
        }

        int toFillDelta = Math.Min(lookaheadDistance - trackPoints.Count, lookaheadSteps);
        int aheadCount = Math.Max(deleteCount, toFillDelta);

        if (aheadCount > 0) {
            Vector3 currentPos = trackPoints[^1].position;
            Vector3 currentVel = trackPoints[^1].velocity;
            int lastTime = trackPoints[^1].timestep;

            for (int i = 0; i < aheadCount; i++) {
                (currentVel, currentPos) = nBodyPredictor.DoTimeStep(currentPos, currentVel, lastTime + i);
                trackPoints.Add(new NBodyTrackPoint {
                    position = currentPos,
                    velocity = currentVel,
                    timestep = lastTime + i + 1
                });
            }
        }
    }

    public List<NBodyTrack> GetAllTracks()
    {
        List<NBodyTrack> allTracks = new List<NBodyTrack>();
        
        foreach (var track in trackDictionary.Values)
        {
            allTracks.Add(track);
        }
        
        return allTracks;
    }
}
