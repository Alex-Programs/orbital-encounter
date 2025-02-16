using UnityEngine;

public class CelestialTimeService : MonoBehaviour
{
    private float startTime;
    void Start()
    {
        startTime = Time.time;
    }

    public int GetCelestialTime()
    {
        return (int)((Time.time - startTime) * 1000);
    }
}
