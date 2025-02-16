using UnityEngine;
using System.Collections.Generic;

public class KeplerPredictor : MonoBehaviour
{
    [System.Serializable]
    public class CelestialBody
    {
        public string name;
        public float mass;
        public string colorHex;
        public float orbitRadius;
        public float eccentricity;
        public float radius;
        public CelestialBody parent;
        public BodyType type;
        public GameObject gameObject;
    }

    public enum BodyType { Sun, Planet, Moon }

    public float planetarySizeScale = 1.0f;
    public float planetaryOrbitScale = 1.0f;

    public float lunarSizeScale = 1.0f;
    public float lunarOrbitScale = 1.0f;

    public float speedScale = 1.0f;

    public List<CelestialBody> bodies = new List<CelestialBody>();

    void Awake()
    {
        GenerateToySolarSystem();
    }

    void GenerateToySolarSystem()
    {
        CelestialBody sun = new CelestialBody { name = "Sun", mass = 30000f, colorHex = "#FFFF00", radius = 50f, type = BodyType.Sun };

        CelestialBody planet1 = new CelestialBody { name = "Planet A", mass = 20000f, colorHex = "#00FF00", orbitRadius = 500f * planetaryOrbitScale, eccentricity = 0.1f, radius = 20f * planetarySizeScale, parent = sun, type = BodyType.Planet };
        CelestialBody planet2 = new CelestialBody { name = "Planet B", mass = 25000f, colorHex = "#0000FF", orbitRadius = 1000f * planetaryOrbitScale, eccentricity = 0.2f, radius = 25f * planetarySizeScale, parent = sun, type = BodyType.Planet };

        CelestialBody moon1 = new CelestialBody { name = "Moon A1", mass = 5000f, colorHex = "#AAAAAA", orbitRadius = 100f * lunarOrbitScale, eccentricity = 0.05f, radius = 5f * lunarSizeScale, parent = planet1, type = BodyType.Moon };
        CelestialBody moon2 = new CelestialBody { name = "Moon B1", mass = 7000f, colorHex = "#BBBBBB", orbitRadius = 150f * lunarOrbitScale, eccentricity = 0.08f, radius = 7f * lunarSizeScale, parent = planet2, type = BodyType.Moon };

        bodies.Add(sun);
        bodies.Add(planet1);
        bodies.Add(planet2);
        bodies.Add(moon1);
        bodies.Add(moon2);
    }

    public Vector3 GetKeplerPosition(CelestialBody body, float timestep)
    {
        // For bodies with no parent (sun), assume fixed at zero.
        if (body.parent == null)
            return Vector3.zero;

        // Semi-major axis (a). (Simplified assumption)
        float a = body.orbitRadius / (1 - body.eccentricity);
        // Mean motion: n = sqrt(M_parent / a^3) [G=1 assumed]
        float n = Mathf.Sqrt(body.parent.mass / (a * a * a));

        // Convert timestep (ms) to seconds
        float t = timestep * speedScale / 1000f;
        // Mean anomaly: M = n * t (wrap to [0,2π])
        float M = n * t % (2 * Mathf.PI);
        float E = SolveKepler(M, body.eccentricity);
        float trueAnomaly = 2 * Mathf.Atan2(Mathf.Sqrt(1 + body.eccentricity) * Mathf.Sin(E / 2),
                                             Mathf.Sqrt(1 - body.eccentricity) * Mathf.Cos(E / 2));
        // Distance: r = a * (1 - e * cosE)
        float r = a * (1 - body.eccentricity * Mathf.Cos(E));
        Vector3 localPos = new Vector3(Mathf.Cos(trueAnomaly) * r, 0, Mathf.Sin(trueAnomaly) * r);
        // Parent's position is added recursively.
        return localPos + GetKeplerPosition(body.parent, timestep);
    }

    private float SolveKepler(float M, float e, int iterations = 5)
    {
        float E = M;
        for (int i = 0; i < iterations; i++)
        {
            E = E - (E - e * Mathf.Sin(E) - M) / (1 - e * Mathf.Cos(E));
        }
        return E;
    }
}
