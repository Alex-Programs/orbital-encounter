using UnityEngine;
using System.Collections.Generic;

public class KeplerOrchestrator : MonoBehaviour
{
    public KeplerPredictor keplerPredictor;
    public CelestialTimeService timeService;
    public Material celestialBodyMaterial;
    public float emissionStrength = 1.0f;  // New emission strength parameter

    private Dictionary<KeplerPredictor.CelestialBody, GameObject> bodyObjects = new Dictionary<KeplerPredictor.CelestialBody, GameObject>();

    public DottedLineDrawer dottedLineDrawer;
    public Material keplerTrackMaterial;
    public int drawTrackDistance;
    public float trackDotsTimeInterval;
    public float dotsVerticalDistance;

    void Start()
    {
        if (keplerPredictor == null)
        {
            Debug.LogError("KeplerPredictor not assigned to KeplerOrchestrator.");
            return;
        }

        if (timeService == null)
        {
            Debug.LogError("CelestialTimeService not assigned to KeplerOrchestrator.");
            return;
        }

        if (celestialBodyMaterial == null)
        {
            Debug.LogError("Celestial Body Material not assigned to KeplerOrchestrator.");
            return;
        }

        foreach (var body in keplerPredictor.bodies)
        {
            GameObject bodyObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bodyObject.name = body.name;
            bodyObject.transform.localScale = Vector3.one * body.radius * 2;
            bodyObject.transform.SetParent(transform, false);

            var renderer = bodyObject.GetComponent<Renderer>();
            renderer.material = new Material(celestialBodyMaterial);

            if (ColorUtility.TryParseHtmlString(body.colorHex, out Color color))
            {
                renderer.material.color = color;
                renderer.material.SetColor("_EmissionColor", color * emissionStrength);
                renderer.material.EnableKeyword("_EMISSION");
            }
            else
            {
                Debug.LogWarning($"Failed to parse color for {body.name}. Using default color.");
                renderer.material.color = Color.white;
                renderer.material.SetColor("_EmissionColor", Color.white * emissionStrength);
                renderer.material.EnableKeyword("_EMISSION");
            }

            body.gameObject = bodyObject;
            bodyObjects[body] = bodyObject;
        }
    }

    void Update()
    {
        int timestep = timeService.GetCelestialTime();
        foreach (var body in keplerPredictor.bodies)
        {
            if (bodyObjects.TryGetValue(body, out GameObject bodyObject))
            {
                bodyObject.transform.position = keplerPredictor.GetKeplerPosition(body, timestep, (float)timeService.GetCelestialTimeResolution());
            }
        }

        DrawTracks();
    }

    void DrawTracks()
    {
        int currentTime = timeService.GetCelestialTime();
        int dotsInterval = (int)(trackDotsTimeInterval * (float)timeService.GetCelestialTimeResolution());

        foreach (var body in keplerPredictor.bodies)
        {
            List<Vector3> dotsSubset = new List<Vector3>();

            int i = 0;
            for (int timeStep = currentTime; timeStep < currentTime + drawTrackDistance; timeStep++) {
                if (i % dotsInterval == 0) {
                    Vector3 pos = keplerPredictor.GetKeplerPosition(body, timeStep, (float)timeService.GetCelestialTimeResolution());
                    pos.y = dotsVerticalDistance * (i / dotsInterval);
                    dotsSubset.Add(pos);
                }

                i++;
            }

            Debug.Log($"Drawing {dotsSubset.Count} dots");

            dottedLineDrawer.DrawDots(keplerTrackMaterial, dotsSubset, 6.0f);
        }
    }
}