using UnityEngine;

public class NBodyInfluenced : MonoBehaviour
{
    public CelestialTimeService timeService;
    public NBodyTrackManager trackManager;

    public int id;

    public Vector3 initialVelocity;

    void Start()
    {
        trackManager.CreateTrack(0, transform.position, initialVelocity);
    }

    void Update()
    {
        var trackPoint = trackManager.GetTrackPoint(id, timeService.GetCelestialTime());

        if (trackPoint.HasValue)
        {
            transform.position = trackPoint.Value.position;
        }
    }
}
