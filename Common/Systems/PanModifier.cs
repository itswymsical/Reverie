using Terraria.Graphics.CameraModifiers;

namespace Reverie.Common.Systems;

internal class PanModifier : ICameraModifier
{
    public int TotalDuration;
    public Vector2 PrimaryTarget;
    public Vector2 SecondaryTarget;

    public Func<Vector2, Vector2, float, Vector2> EaseInFunction = Vector2.SmoothStep;
    public Func<Vector2, Vector2, float, Vector2> EaseOutFunction = Vector2.SmoothStep;
    public Func<Vector2, Vector2, float, Vector2> PanFunction = Vector2.Lerp;

    private int Timer;
    public bool UseOffsetOrigin { get; set; }

    public string UniqueIdentity => "Reverie Camera Pan";
    public bool Finished => Timer >= TotalDuration;

    public void PassiveUpdate()
    {
        if (TotalDuration > 0 && PrimaryTarget != Vector2.Zero)
        {
            if (Timer < TotalDuration)
                Timer++;
        }
    }

    public void Update(ref CameraInfo cameraPosition)
    {
        if (TotalDuration > 0 && PrimaryTarget != Vector2.Zero)
        {
            var offset = new Vector2(-Main.screenWidth / 2f, -Main.screenHeight / 2f);

            if (UseOffsetOrigin) // Position -> NewPosition (or Player)
            {
                if (Timer < TotalDuration)
                {
                    cameraPosition.CameraPosition = PanFunction(PrimaryTarget + offset, SecondaryTarget + offset, Timer / (float)TotalDuration);
                }
            }
            else // Player -> Position -> Player
            {
                if (Timer < TotalDuration / 2)
                {
                    cameraPosition.CameraPosition = EaseInFunction(cameraPosition.OriginalCameraCenter + offset, PrimaryTarget + offset, Timer / (float)(TotalDuration / 2));
                }
                else if (SecondaryTarget != Vector2.Zero)
                {
                    cameraPosition.CameraPosition = PanFunction(PrimaryTarget + offset, SecondaryTarget + offset, (Timer - TotalDuration / 2) / (float)(TotalDuration / 2));
                }
                else
                {
                    cameraPosition.CameraPosition = EaseOutFunction(PrimaryTarget + offset, cameraPosition.OriginalCameraCenter + offset, (Timer - TotalDuration / 2) / (float)(TotalDuration / 2));
                }
            }
        }
    }

    public void Reset()
    {
        TotalDuration = 0;
        PrimaryTarget = Vector2.Zero;
        SecondaryTarget = Vector2.Zero;
        Timer = 0;
    }
}