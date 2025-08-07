using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.CameraModifiers;

namespace Reverie.Core.Cinematics.Camera;

internal class CameraPathModifier : ICameraModifier
{
    public List<Vector2> WayPoints = [];
    public List<int> SegmentDurations = [];
    public Func<Vector2, Vector2, float, Vector2> EaseFunction = Vector2.SmoothStep;

    private int TotalTimer;
    private int CurrentSegment;
    private int SegmentTimer;

    public string UniqueIdentity => "Reverie Camera Path";
    public bool Finished => CurrentSegment >= WayPoints.Count - 1;

    public void PassiveUpdate()
    {
        if (WayPoints.Count > 1 && !Finished)
        {
            TotalTimer++;
            SegmentTimer++;

            // Check if current segment is complete
            if (CurrentSegment < SegmentDurations.Count && SegmentTimer >= SegmentDurations[CurrentSegment])
            {
                CurrentSegment++;
                SegmentTimer = 0;
            }
        }
    }

    public void Update(ref CameraInfo cameraPosition)
    {
        if (WayPoints.Count > 1 && CurrentSegment < WayPoints.Count - 1)
        {
            var startPoint = WayPoints[CurrentSegment];
            var endPoint = WayPoints[CurrentSegment + 1];
            var duration = CurrentSegment < SegmentDurations.Count ? SegmentDurations[CurrentSegment] : 60;

            var progress = Math.Min(SegmentTimer / (float)duration, 1f);
            var offset = new Vector2(-Main.screenWidth / 2f, -Main.screenHeight / 2f);

            cameraPosition.CameraPosition = EaseFunction(startPoint + offset, endPoint + offset, progress);
        }
    }

    public void Reset()
    {
        WayPoints.Clear();
        SegmentDurations.Clear();
        TotalTimer = 0;
        CurrentSegment = 0;
        SegmentTimer = 0;
    }
}
