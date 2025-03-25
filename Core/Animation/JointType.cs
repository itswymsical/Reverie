using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using static Terraria.Player;

namespace Reverie.Common.Animation;

public enum JointType
{
    Head,
    Body,
    Legs,
    LeftArm,
    RightArm
}

public class JointState
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f;
    public CompositeArmStretchAmount StretchAmount { get; set; } = CompositeArmStretchAmount.Full;
    
    public JointState Clone()
    {
        return new JointState
        {
            Position = this.Position,
            Rotation = this.Rotation,
            StretchAmount = this.StretchAmount
        };
    }

    public static JointState Lerp(JointState a, JointState b, float t)
    {
        return new JointState
        {
            Position = Vector2.Lerp(a.Position, b.Position, t),
            Rotation = MathHelper.Lerp(a.Rotation, b.Rotation, t),
            StretchAmount = t < 0.5f ? a.StretchAmount : b.StretchAmount
        };
    }
}