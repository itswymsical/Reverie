using System.Collections.Generic;
using Terraria.Graphics.CameraModifiers;

namespace Reverie.Core.Cinematics.Camera;

public class CameraSystem : ModSystem
{
    public static int shake = 0; //note: this is fine.

    private static PanModifier panModifier = new();
    private static MoveModifier moveModifier = new();
    private static CameraPathModifier pathModifier = new();

    private static bool _isLocked = false;
    private static Vector2 _lockedPosition = Vector2.Zero;
    public static bool IsLocked => _isLocked;
    public static Vector2 LockedPosition => _lockedPosition;

    public static void LockCamera(Vector2 position)
    {
        // Add some debugging to see what's happening
        try
        {
            _isLocked = true;
            _lockedPosition = position;
        }
        catch (Exception ex)
        {
            // Log the error to see what's actually happening
            ModContent.GetInstance<Reverie>().Logger.Error($"LockCamera error: {ex.Message}");
        }
    }

    /// <summary>
    /// Unlocks the camera, returning control to normal
    /// </summary>
    public static void UnlockCamera()
    {
        _isLocked = false;
    }

    public static void CreateCameraPath(List<Vector2> waypoints, List<int> durations, Func<Vector2, Vector2, float, Vector2> easing = null)
    {
        pathModifier.Reset();
        pathModifier.WayPoints = waypoints;
        pathModifier.SegmentDurations = durations;
        pathModifier.EaseFunction = easing ?? Vector2.SmoothStep;
    }

    /// <summary>
    /// Sets up a panning animation for the screen. Great for use with things like boss spawns or defeats, or other events happening near the player.
    /// </summary>
    /// <param name="duration"> How long the animation should last </param>
    /// <param name="target"> Where the camera should pan to </param>
    /// <param name="secondaryTarget"> Where the camera will scroll to after panning to the initial target. Leave blank to keep the camera in place at the initial target </param>
    /// <param name="easeIn"> Changes the easing function for the motion from the player to the primary target. Default is Vector2.Smoothstep </param>
    /// <param name="easeOut"> Changes the easing function for the motion from the primary/secondary target back to the player. Default is Vector2.Smoothstep </param>
    /// <param name="easePan"> Changes the easing function for the motion from primary to secondary target if applicable. Default is Vector2.Lerp </param>
    public static void DoPanAnimation(int duration, Vector2 target, Vector2 secondaryTarget = default, Func<Vector2, Vector2, float, Vector2> easeIn = null, Func<Vector2, Vector2, float, Vector2> easeOut = null, Func<Vector2, Vector2, float, Vector2> easePan = null)
    {
        panModifier.UseOffsetOrigin = false;
        panModifier.TotalDuration = duration;
        panModifier.PrimaryTarget = target;
        panModifier.SecondaryTarget = secondaryTarget;

        panModifier.EaseInFunction = easeIn ?? Vector2.SmoothStep;
        panModifier.EaseOutFunction = easeOut ?? Vector2.SmoothStep;
        panModifier.PanFunction = easePan ?? Vector2.Lerp;
    }

    /// <summary>
    /// Sets up a panning animation for the screen from a custom starting position.
    /// </summary>
    /// <param name="duration"> How long the animation should last </param>
    /// <param name="origin"> The starting position for the camera pan </param>
    /// <param name="target"> Where the camera should pan to </param>
    /// <param name="easePan"> Changes the easing function for the motion from origin to target. Default is Vector2.Lerp </param>
    public static void DoPanAnimationOffset(int duration, Vector2 origin, Vector2 target, Func<Vector2, Vector2, float, Vector2> easePan = null)
    {
        panModifier.UseOffsetOrigin = true;
        panModifier.TotalDuration = duration;
        panModifier.PrimaryTarget = origin;
        panModifier.SecondaryTarget = target;

        // For custom origin pans, we only need the pan function since we're not involving the player position
        panModifier.PanFunction = easePan ?? Vector2.Lerp;
    }

    /// <summary>
    /// Moves the camera to a set point, with an animation of the specified duration, and stays there. Use ReturnCamera to retrieve it later.
    /// </summary>
    /// <param name="duration"> How long it takes the camera to get to it's destination </param>
    /// <param name="target"> Where the camera should end up </param>
    /// <param name="ease"> The easing function the camera should follow on it's journey. Default is Vector2.Smoothstep </param>
    public static void MoveCameraOut(int duration, Vector2 target, Func<Vector2, Vector2, float, Vector2> ease = null)
    {
        moveModifier.Timer = 0;
        moveModifier.MovementDuration = duration;
        moveModifier.Target = target;
        moveModifier.Returning = false;

        moveModifier.EaseFunction = ease ?? Vector2.SmoothStep;
    }

    /// <summary>
    /// Returns the camera to the player after it has been sent out by MoveCameraOut.
    /// </summary>
    /// <param name="duration"> How long it takes for the camera to get back to the player </param>
    /// <param name="ease"> The easing function the camera should follow on it's journey. Default is Vector2.Smoothstep </param>
    public static void ReturnCamera(int duration, Func<Vector2, Vector2, float, Vector2> ease = null)
    {
        moveModifier.Timer = 0;
        moveModifier.MovementDuration = duration;
        moveModifier.Returning = true;

        moveModifier.EaseFunction = ease ?? Vector2.SmoothStep;
    }

    public override void PostUpdateEverything()
    {
        panModifier.PassiveUpdate();
        moveModifier.PassiveUpdate();
        pathModifier.PassiveUpdate();
    }

    public override void ModifyScreenPosition()
    {
        if (_isLocked)
        {
            Main.screenPosition = _lockedPosition - new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
            return;
        }

        if (panModifier.TotalDuration > 0 && panModifier.PrimaryTarget != Vector2.Zero)
            Main.instance.CameraModifiers.Add(panModifier);

        if (moveModifier.MovementDuration > 0 && moveModifier.Target != Vector2.Zero)
            Main.instance.CameraModifiers.Add(moveModifier);

        if (pathModifier.WayPoints.Count > 1) // Add this block
            Main.instance.CameraModifiers.Add(pathModifier);

        var mult = 1f;
        Main.instance.CameraModifiers.Add(new PunchCameraModifier(Main.LocalPlayer.position, Main.rand.NextFloat(3.14f).ToRotationVector2(), shake * mult, 15f, 30, 2000, "Shake"));

        if (shake > 0)
            shake--;
    }

    public static void Reset()
    {
        shake = 0;
        _isLocked = false;
        _lockedPosition = Vector2.Zero;

        panModifier.Reset();
        moveModifier.Reset();
        pathModifier.Reset();
    }

    public override void OnWorldLoad()
    {
        Reset();
    }

    public override void Unload()
    {
        panModifier = null;
        moveModifier = null;
        pathModifier = null;
    }
}