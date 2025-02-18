using UnityEngine;
using Mirror;

public class CelestialTimeService : MonoBehaviour
{
    private double startTime;

    public int resolution;
    public int multiplier;

    void Start()
    {
        startTime = NetworkTime.time;
    }

    public int GetCelestialTime()
    {
        return (int)((NetworkTime.time - startTime) * resolution * multiplier);
    }

    public int GetCelestialTimeResolution() {
        return resolution;
    }
}
