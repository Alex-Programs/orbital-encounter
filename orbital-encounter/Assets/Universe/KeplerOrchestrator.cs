using UnityEngine;
using System;
using System.Collections.Generic;

public class KeplerOrchestrator : MonoBehaviour
{
    private const double G = 6.67430e-11; // Gravitational constant
    private const double CELESTIAL_SCALE_FACTOR = 1e7;
    private const double AU = 149597870.7; // Astronomical Unit in km

    [SerializeField] public float timeScale = 1f;

    // Art style scale factors
    public float planetSizeScale = 100f;
    public float moonSizeScale = 100f;
    public float planetOrbitScale = 1f;
    public float moonOrbitScale = 1f;
    public float solarScale = 1f;

    private List<CelestialBody> celestialBodies = new List<CelestialBody>();

    void Start()
    {
        InitializeCustomSystem();
        CreateGameObjects();
    }

    void Update()
    {
        UpdatePositions(Time.time * timeScale);
    }

    private void InitializeCustomSystem()
    {
        // Central star
        var centralStar = AddBody(new CelestialBody
        {
            Name = "Central Star",
            Radius = 696340 / CELESTIAL_SCALE_FACTOR * solarScale,
            Mass = 1.989e30,
            ColorHex = "#FFD700",
            IsPlanet = true
        });

        // Planet 1
        var planet1 = AddBody(new CelestialBody
        {
            Name = "Planet 1",
            Radius = 6371 / CELESTIAL_SCALE_FACTOR,
            Mass = 5.972e24,
            ColorHex = "#4169E1",
            OrbitRadius = 1.5 * AU / CELESTIAL_SCALE_FACTOR,
            Eccentricity = 0.01,
            Parent = centralStar,
            IsPlanet = true
        });

        // Moon of Planet 1
        AddBody(new CelestialBody
        {
            Name = "Moon 1",
            Radius = 1737.1 / CELESTIAL_SCALE_FACTOR,
            Mass = 7.342e22,
            ColorHex = "#A9A9A9",
            OrbitRadius = 200000 / CELESTIAL_SCALE_FACTOR,
            Eccentricity = 0.0549,
            Parent = planet1,
            IsPlanet = false
        });

        // Planet 2
        var planet2 = AddBody(new CelestialBody
        {
            Name = "Planet 2",
            Radius = 69911 / CELESTIAL_SCALE_FACTOR,
            Mass = 1.898e27,
            ColorHex = "#DEB887",
            OrbitRadius = 3.5 * AU / CELESTIAL_SCALE_FACTOR,
            Eccentricity = 0.0489,
            Parent = centralStar,
            IsPlanet = true
        });

        // Moons of Planet 2
        AddBody(new CelestialBody
        {
            Name = "Moon 2A",
            Radius = 1821.6 / CELESTIAL_SCALE_FACTOR,
            Mass = 8.931e22,
            ColorHex = "#FFD700",
            OrbitRadius = 600000 / CELESTIAL_SCALE_FACTOR,
            Eccentricity = 0.0041,
            Parent = planet2,
            IsPlanet = false
        });

        AddBody(new CelestialBody
        {
            Name = "Moon 2B",
            Radius = 2634.1 / CELESTIAL_SCALE_FACTOR,
            Mass = 1.482e23,
            ColorHex = "#808080",
            OrbitRadius = 1370400 / CELESTIAL_SCALE_FACTOR,
            Eccentricity = 0.0013,
            Parent = planet2,
            IsPlanet = false
        });

        // Add more planets and moons as needed
    }

    private CelestialBody AddBody(CelestialBody body)
    {
        celestialBodies.Add(body);
        if (body.Parent != null)
        {
            body.Parent.Satellites.Add(body);
            body.Orbit = new KeplerOrbit(body.OrbitRadius, body.Eccentricity, CalculateOrbitalPeriod(body));
        }
        return body;
    }

    private double CalculateOrbitalPeriod(CelestialBody body)
    {
        // Using Kepler's Third Law
        double a = body.OrbitRadius * CELESTIAL_SCALE_FACTOR; // Convert back to meters
        double M = body.Parent.Mass;
        return 2 * Math.PI * Math.Sqrt(a * a * a / (G * M)) / (24 * 3600); // Convert to days
    }

    private void CreateGameObjects()
    {
        foreach (var body in celestialBodies)
        {
            body.GameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.GameObject.name = body.Name;

            var renderer = body.GameObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));

            if (ColorUtility.TryParseHtmlString(body.ColorHex, out Color color))
            {
                renderer.material.color = color;
            }
            else
            {
                Debug.LogWarning($"Failed to parse color for {body.Name}. Using default color.");
                renderer.material.color = Color.white;
            }

            float scale = body.IsPlanet ?
                            (float)(body.Radius * 2 * planetSizeScale) :
                            (float)(body.Radius * 2 * moonSizeScale);

            body.GameObject.transform.localScale = new Vector3(scale, scale, scale);

            body.GameObject.transform.SetParent(transform, false);
        }
    }

    private void UpdatePositions(float timeSeconds)
    {
        foreach (var body in celestialBodies)
        {
            if (body.Orbit == null) continue; // Skip the central star

            Vector3 position = body.Orbit.GetPosition(timeSeconds, body.IsPlanet ? planetOrbitScale : moonOrbitScale);

            if (body.Parent != null)
            {
                position += body.Parent.GameObject.transform.position;
            }

            body.GameObject.transform.position = position;
        }
    }

    public List<CelestialBody> GetCelestialBodies()
    {
        return celestialBodies;
    }

    public Vector3 GetCelestialBodyPosition(CelestialBody body, float time)
    {
        if (body.Orbit == null)
        {
            // This is the central star, return its fixed position
            return body.GameObject.transform.position;
        }

        Vector3 position = body.Orbit.GetPosition(time, body.IsPlanet ? planetOrbitScale : moonOrbitScale);

        if (body.Parent != null)
        {
            position += GetCelestialBodyPosition(body.Parent, time);
        }

        return position;
    }
}

public class CelestialBody
{
    public string Name { get; set; }
    public double Radius { get; set; } // in km (scaled)
    public double Mass { get; set; } // in kg
    public string ColorHex { get; set; }
    public double OrbitRadius { get; set; } // in km (scaled)
    public double Eccentricity { get; set; }
    public KeplerOrbit Orbit { get; set; }
    public GameObject GameObject { get; set; }
    public CelestialBody Parent { get; set; }
    public List<CelestialBody> Satellites { get; set; } = new List<CelestialBody>();
    public bool IsPlanet { get; set; }

    public Vector3 GetPosition(float time, KeplerOrchestrator orchestrator)
    {
        return orchestrator.GetCelestialBodyPosition(this, time);
    }
}

public class KeplerOrbit
{
    private double semiMajorAxis;
    private double eccentricity;
    private double meanMotion;

    public KeplerOrbit(double semiMajorAxis, double eccentricity, double periodInDays)
    {
        this.semiMajorAxis = semiMajorAxis;
        this.eccentricity = eccentricity;
        this.meanMotion = 2 * Math.PI / (periodInDays * 24 * 3600);
    }

    public Vector3 GetPosition(double timeSeconds, float orbitScale)
    {
        double meanAnomaly = meanMotion * timeSeconds;
        double eccentricAnomaly = SolveKepler(meanAnomaly);

        double x = semiMajorAxis * (Math.Cos(eccentricAnomaly) - eccentricity) * orbitScale;
        double y = semiMajorAxis * Math.Sqrt(1 - eccentricity * eccentricity) * Math.Sin(eccentricAnomaly) * orbitScale;

        return new Vector3((float)x, 0, (float)y);
    }

    private double SolveKepler(double M)
    {
        double E = M;
        for (int i = 0; i < 10; i++)
        {
            E = E - (E - eccentricity * Math.Sin(E) - M) / (1 - eccentricity * Math.Cos(E));
        }
        return E;
    }

    public Vector3 GetPosition(double timeSeconds)
    {
        return GetPosition(timeSeconds, 1f);
    }
}
