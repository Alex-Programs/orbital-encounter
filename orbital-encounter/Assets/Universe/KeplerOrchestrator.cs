using UnityEngine;
using System.Collections.Generic;

public class KeplerOrchestrator : MonoBehaviour
{
    public KeplerPredictor keplerPredictor;
    public CelestialTimeService timeService;
    public Material celestialBodyMaterial;  // New public material reference
    
    private Dictionary<KeplerPredictor.CelestialBody, GameObject> bodyObjects = new Dictionary<KeplerPredictor.CelestialBody, GameObject>();

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
            renderer.material = new Material(celestialBodyMaterial);  // Create instance of the material

            if (ColorUtility.TryParseHtmlString(body.colorHex, out Color color))
            {
                renderer.material.color = color;
            }
            else
            {
                Debug.LogWarning($"Failed to parse color for {body.name}. Using default color.");
                renderer.material.color = Color.white;
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
    }
}