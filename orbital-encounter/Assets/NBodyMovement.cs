using UnityEngine;

public class NBodyMovement : MonoBehaviour
{
    public NBodyPredictor predictor;
    public float initialSpeed = 1f;
    public float mass = 1f;

    private int trackId = -1;
    private bool isInitialized = false;

    void Start()
    {
        if (predictor == null)
        {
            Debug.LogError("NBodyPredictor reference is missing!");
            return;
        }

        Vector3 initialVelocity = transform.forward * initialSpeed;
        trackId = predictor.AddTrack(transform.position, initialVelocity, mass);
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        var (position, velocity) = predictor.GetTrackCurrent(trackId);
        transform.position = position;
        if (velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    void OnDestroy()
    {
        if (isInitialized)
        {
            predictor.RemoveTrack(trackId);
        }
    }
}
