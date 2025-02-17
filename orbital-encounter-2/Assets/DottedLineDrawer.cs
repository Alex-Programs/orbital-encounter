using UnityEngine;
using System.Collections.Generic;

public class DottedLineDrawer : MonoBehaviour
{
    [Header("Size Settings")]
    [Tooltip("Base size multiplier relative to distance")]
    public float screenSizeFactor = 0.1f;
    [Tooltip("Minimum sphere scale")] 
    public float minScale = 0.1f;
    [Tooltip("Maximum sphere scale")]
    public float maxScale = 10f;

    private struct DrawCommand
    {
        public Material material;
        public List<Vector3> points;
        public float baseSize;
    }

    private List<DrawCommand> commands = new List<DrawCommand>();
    private Mesh sphereMesh;
    private Camera mainCam;

    void Start() => sphereMesh = GetSphereMesh();

    public void DrawDots(Material material, List<Vector3> points, float baseSize = 1f)
    {
        commands.Add(new DrawCommand {
            material = material,
            points = new List<Vector3>(points),
            baseSize = baseSize
        });
    }

    void LateUpdate()
    {
        mainCam = Camera.main;
        if (mainCam == null || commands.Count == 0) return;

        Vector3 camPos = mainCam.transform.position;
        
        foreach (var cmd in commands)
        {
            foreach (var point in cmd.points)
            {
                // Calculate distance-based scale
                float distance = Vector3.Distance(point, camPos);
                float scale = cmd.baseSize * screenSizeFactor * distance;
                Debug.Log($"Scale: {scale}");
                
                scale = Mathf.Clamp(scale, minScale, maxScale);

                Graphics.DrawMesh(
                    sphereMesh,
                    Matrix4x4.TRS(point, Quaternion.identity, Vector3.one * scale),
                    cmd.material,
                    0
                );
            }
        }
        commands.Clear();
    }

    Mesh GetSphereMesh()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
        return mesh;
    }
}