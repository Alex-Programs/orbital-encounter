using System.Collections.Generic;
using UnityEngine;

public class NBodyTrackDrawer : MonoBehaviour
{
    public CelestialTimeService timeService;
    public NBodyTrackManager trackManager;
    public DottedLineDrawer dottedLineDrawer;

    public float dotsTimeInterval;
    public float dotVerticalDistance;

    public Material friendlyTrackMaterial;

    // Update is called once per frame
    void Update()
    {
        List<NBodyTrackManager.NBodyTrack> allTracks = trackManager.GetAllTracks();

        for (int i = 0; i < allTracks.Count; i++) {
            NBodyTrackManager.NBodyTrack track = allTracks[i];

            List<Vector3> dotsSubset = new List<Vector3>();

            int dotsInterval = (int)(dotsTimeInterval * (float)timeService.GetCelestialTimeResolution());

            for (int j = dotsInterval; j < track.points.Count; j++) {
                NBodyTrackManager.NBodyTrackPoint point = track.points[j];

                if (j % dotsInterval == 0) {
                    Vector3 pos = point.position;
                    pos.y = dotVerticalDistance * (j / dotsInterval);
                    dotsSubset.Add(pos);
                }
            }

            dottedLineDrawer.DrawDots(friendlyTrackMaterial, dotsSubset, 6.0f);
        }
    }
}
