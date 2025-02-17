using UnityEngine;

public class CelestialTimeService : MonoBehaviour
{
    private float startTime;

    public int resolution;
    public int multiplier;

    void Start()
    {
        startTime = Time.time;
    }

    public int GetCelestialTime()
    {
        return (int)((Time.time - startTime) * resolution * multiplier);
    }

    public int GetCelestialTimeResolution() {
        return resolution;
    }
}
