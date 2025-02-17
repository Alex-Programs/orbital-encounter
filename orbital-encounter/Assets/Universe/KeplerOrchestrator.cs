using UnityEngine;
using System.Collections.Generic;

public class KeplerOrchestrator : MonoBehaviour
{
    public KeplerPredictor keplerPredictor;
    public CelestialTimeService timeService;
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

        foreach (var body in keplerPredictor.bodies)
        {
            GameObject bodyObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bodyObject.name = body.name;
            // Diameter = 2 * radius.
            bodyObject.transform.localScale = Vector3.one * body.radius * 2;
            // Parent under this orchestrator in the hierarchy.
            bodyObject.transform.SetParent(transform, false);

            var renderer = bodyObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
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
