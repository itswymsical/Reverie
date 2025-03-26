using static Terraria.Player;

namespace Reverie.Core.Animation;

public enum SegmentType
{
    Head,
    Body,
    Legs,
    LeftArm,
    RightArm
}

public class SegmentState
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f;
    public CompositeArmStretchAmount StretchAmount { get; set; } = CompositeArmStretchAmount.Full;

    public SegmentState Clone()
    {
        return new SegmentState
        {
            Position = Position,
            Rotation = Rotation,
            StretchAmount = StretchAmount
        };
    }

    public static SegmentState Lerp(SegmentState a, SegmentState b, float t)
    {
        return new SegmentState
        {
            Position = Vector2.Lerp(a.Position, b.Position, t),
            Rotation = MathHelper.Lerp(a.Rotation, b.Rotation, t),
            StretchAmount = t < 0.5f ? a.StretchAmount : b.StretchAmount
        };
    }
}