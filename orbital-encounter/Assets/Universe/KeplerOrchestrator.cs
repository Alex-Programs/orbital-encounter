using UnityEngine;
using System;
using System.Collections.Generic;

public class KeplerOrchestrator : MonoBehaviour
{
    private const double CELESTIAL_SCALE_FACTOR = 1e7;
    private const double AU = 149597870.7; // Astronomical Unit in km

    [SerializeField] private float timeScale = 1f;

    // Art style scale factors
    public float planetSizeScale = 100f;
    public float moonSizeScale = 200f;
    public float planetOrbitScale = 1f;
    public float moonOrbitScale = 1f;
    public float solarScale = 1f;

    private List<CelestialBody> celestialBodies = new List<CelestialBody>();

    void Start()
    {
        InitializeSolarSystem();
        CreateGameObjects();
    }

    void Update()
    {
        UpdatePositions(Time.time * timeScale);
    }

    private void InitializeSolarSystem()
    {
        // Sun (fixed at the center)
        var sun = AddBody(new CelestialBody
        {
            Name = "Sun",
            Radius = 696340 / CELESTIAL_SCALE_FACTOR * solarScale,
            Mass = 1.989e30,
            ColorHex = "#FFD700",
            IsPlanet = true
        });

        // Mercury
        AddBody(new CelestialBody
        {
            Name = "Mercury",
            Radius = 2439.7 / CELESTIAL_SCALE_FACTOR,
            Mass = 3.285e23,
            ColorHex = "#A0522D",
            Orbit = new KeplerOrbit(0.387 * AU / CELESTIAL_SCALE_FACTOR, 0.206, 88),
            Parent = sun,
            IsPlanet = true
        });

        // Venus
        AddBody(new CelestialBody
        {
            Name = "Venus",
            Radius = 6051.8 / CELESTIAL_SCALE_FACTOR,
            Mass = 4.867e24,
            ColorHex = "#DEB887",
            Orbit = new KeplerOrbit(0.723 * AU / CELESTIAL_SCALE_FACTOR, 0.007, 224.7),
            Parent = sun,
            IsPlanet = true
        });

        // Earth
        var earth = AddBody(new CelestialBody
        {
            Name = "Earth",
            Radius = 6371 / CELESTIAL_SCALE_FACTOR,
            Mass = 5.972e24,
            ColorHex = "#4169E1",
            Orbit = new KeplerOrbit(AU / CELESTIAL_SCALE_FACTOR, 0.0167, 365.25),
            Parent = sun,
            IsPlanet = true
        });

        // Moon
        AddBody(new CelestialBody
        {
            Name = "Moon",
            Radius = 1737.1 / CELESTIAL_SCALE_FACTOR,
            Mass = 7.342e22,
            ColorHex = "#A9A9A9",
            Orbit = new KeplerOrbit(384399 / CELESTIAL_SCALE_FACTOR, 0.0549, 27.322),
            Parent = earth,
            IsPlanet = false
        });

        // Mars
        var mars = AddBody(new CelestialBody
        {
            Name = "Mars",
            Radius = 3389.5 / CELESTIAL_SCALE_FACTOR,
            Mass = 6.39e23,
            ColorHex = "#CD5C5C",
            Orbit = new KeplerOrbit(1.524 * AU / CELESTIAL_SCALE_FACTOR, 0.0934, 687),
            Parent = sun,
            IsPlanet = true
        });

        // Mars' moons
        AddBody(new CelestialBody
        {
            Name = "Phobos",
            Radius = 11.267 / CELESTIAL_SCALE_FACTOR,
            Mass = 1.0659e16,
            ColorHex = "#8B4513",
            Orbit = new KeplerOrbit(9377 / CELESTIAL_SCALE_FACTOR, 0.0151, 0.319),
            Parent = mars,
            IsPlanet = false
        });

        AddBody(new CelestialBody
        {
            Name = "Deimos",
            Radius = 6.2 / CELESTIAL_SCALE_FACTOR,
            Mass = 1.4762e15,
            ColorHex = "#A0522D",
            Orbit = new KeplerOrbit(23460 / CELESTIAL_SCALE_FACTOR, 0.00033, 1.262),
            Parent = mars,
            IsPlanet = false
        });

        // Jupiter
        var jupiter = AddBody(new CelestialBody
        {
            Name = "Jupiter",
            Radius = 69911 / CELESTIAL_SCALE_FACTOR,
            Mass = 1.898e27,
            ColorHex = "#DEB887",
            Orbit = new KeplerOrbit(5.203 * AU / CELESTIAL_SCALE_FACTOR, 0.0489, 4333),
            Parent = sun,
            IsPlanet = true
        });

        // Jupiter's major moons
        AddBody(new CelestialBody
        {
            Name = "Io",
            Radius = 1821.6 / CELESTIAL_SCALE_FACTOR,
            Mass = 8.931e22,
            ColorHex = "#FFD700",
            Orbit = new KeplerOrbit(421700 / CELESTIAL_SCALE_FACTOR, 0.0041, 1.769),
            Parent = jupiter,
            IsPlanet = false
        });

        AddBody(new CelestialBody
        {
            Name = "Europa",
            Radius = 1560.8 / CELESTIAL_SCALE_FACTOR,
            Mass = 4.799e22,
            ColorHex = "#F5F5DC",
            Orbit = new KeplerOrbit(671100 / CELESTIAL_SCALE_FACTOR, 0.009, 3.551),
            Parent = jupiter,
            IsPlanet = false
        });

        AddBody(new CelestialBody
        {
            Name = "Ganymede",
            Radius = 2634.1 / CELESTIAL_SCALE_FACTOR,
            Mass = 1.482e23,
            ColorHex = "#808080",
            Orbit = new KeplerOrbit(1070400 / CELESTIAL_SCALE_FACTOR, 0.0013, 7.154),
            Parent = jupiter,
            IsPlanet = false
        });

        AddBody(new CelestialBody
        {
            Name = "Callisto",
            Radius = 2410.3 / CELESTIAL_SCALE_FACTOR,
            Mass = 1.076e23,
            ColorHex = "#696969",
            Orbit = new KeplerOrbit(1882700 / CELESTIAL_SCALE_FACTOR, 0.0074, 16.689),
            Parent = jupiter,
            IsPlanet = false
        });

        // Saturn
        var saturn = AddBody(new CelestialBody
        {
            Name = "Saturn",
            Radius = 58232 / CELESTIAL_SCALE_FACTOR,
            Mass = 5.683e26,
            ColorHex = "#F4A460",
            Orbit = new KeplerOrbit(9.537 * AU / CELESTIAL_SCALE_FACTOR, 0.0565, 10759),
            Parent = sun,
            IsPlanet = true
        });

        // Saturn's major moons
        AddBody(new CelestialBody
        {
            Name = "Titan",
            Radius = 2574.73 / CELESTIAL_SCALE_FACTOR,
            Mass = 1.345e23,
            ColorHex = "#DAA520",
            Orbit = new KeplerOrbit(1221870 / CELESTIAL_SCALE_FACTOR, 0.0288, 15.945),
            Parent = saturn,
            IsPlanet = false
        });

        AddBody(new CelestialBody
        {
            Name = "Rhea",
            Radius = 763.8 / CELESTIAL_SCALE_FACTOR,
            Mass = 2.306e21,
            ColorHex = "#D3D3D3",
            Orbit = new KeplerOrbit(527108 / CELESTIAL_SCALE_FACTOR, 0.001, 4.518),
            Parent = saturn,
            IsPlanet = false
        });

        AddBody(new CelestialBody
        {
            Name = "Iapetus",
            Radius = 734.5 / CELESTIAL_SCALE_FACTOR,
            Mass = 1.805e21,
            ColorHex = "#8B4513",
            Orbit = new KeplerOrbit(3560820 / CELESTIAL_SCALE_FACTOR, 0.0286, 79.322),
            Parent = saturn,
            IsPlanet = false
        });

        // Uranus
        var uranus = AddBody(new CelestialBody
        {
            Name = "Uranus",
            Radius = 25362 / CELESTIAL_SCALE_FACTOR,
            Mass = 8.681e25,
            ColorHex = "#00CED1",
            Orbit = new KeplerOrbit(19.191 * AU / CELESTIAL_SCALE_FACTOR, 0.0472, 30688.5),
            Parent = sun,
            IsPlanet = true
        });

        // Uranus' major moons
        AddBody(new CelestialBody
        {
            Name = "Titania",
            Radius = 788.9 / CELESTIAL_SCALE_FACTOR,
            Mass = 3.42e21,
            ColorHex = "#A9A9A9",
            Orbit = new KeplerOrbit(435910 / CELESTIAL_SCALE_FACTOR, 0.0011, 8.706),
            Parent = uranus,
            IsPlanet = false
        });

        AddBody(new CelestialBody
        {
            Name = "Oberon",
            Radius = 761.4 / CELESTIAL_SCALE_FACTOR,
            Mass = 2.88e21,
            ColorHex = "#8B4513",
            Orbit = new KeplerOrbit(583520 / CELESTIAL_SCALE_FACTOR, 0.0014, 13.463),
            Parent = uranus,
            IsPlanet = false
        });
    }

    private CelestialBody AddBody(CelestialBody body)
    {
        celestialBodies.Add(body);
        if (body.Parent != null)
        {
            body.Parent.Satellites.Add(body);
        }
        return body;
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
            if (body.Orbit == null) continue; // Skip the Sun

            Vector3 position = body.Orbit.GetPosition(timeSeconds, body.IsPlanet ? planetOrbitScale : moonOrbitScale);

            if (body.Parent != null)
            {
                position += body.Parent.GameObject.transform.position;
            }

            body.GameObject.transform.position = position;
        }
    }
}

public class CelestialBody
{
    public string Name { get; set; }
    public double Radius { get; set; } // in km (scaled)
    public double Mass { get; set; } // in kg
    public string ColorHex { get; set; }
    public KeplerOrbit Orbit { get; set; }
    public GameObject GameObject { get; set; }
    public CelestialBody Parent { get; set; }
    public List<CelestialBody> Satellites { get; set; } = new List<CelestialBody>();
    public bool IsPlanet { get; set; }
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
}
