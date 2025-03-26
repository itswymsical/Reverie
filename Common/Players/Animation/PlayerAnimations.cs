using Reverie.Core.Animation;
using static Terraria.Player;

namespace Reverie.Common.Players.Animation;

public partial class AnimationPlayer
{
    private ProceduralAnimation IdleAnimation()
    {
        var idle = new ProceduralAnimation("Idle", "Idle Animation", 2.0f);
        idle.Looping = true;

        var start = new AnimationFrame(0f);
        start.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        start.Parts[SegmentType.Head] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        start.Parts[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        start.Parts[SegmentType.RightArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        var mid = new AnimationFrame(1.0f);
        mid.Parts[SegmentType.Body] = new SegmentState
        {
            Position = new Vector2(0, -0.5f),
            Rotation = 0.05f
        };
        mid.Parts[SegmentType.Head] = new SegmentState
        {
            Position = new Vector2(0, -0.5f),
            Rotation = -0.02f
        };
        mid.Parts[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        mid.Parts[SegmentType.RightArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        var end = new AnimationFrame(2.0f);
        end.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        end.Parts[SegmentType.Head] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        end.Parts[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        end.Parts[SegmentType.RightArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        idle.AddFrame(start);
        idle.AddFrame(mid);
        idle.AddFrame(end);

        return idle;
    }

    private ProceduralAnimation WalkingAnimation()
    {
        var walking = new ProceduralAnimation("Walking", "Walking Animation", 0.8f);
        walking.Looping = true;

        // First frame
        var frame1 = new AnimationFrame(0f);
        frame1.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.05f
        };
        frame1.Parts[SegmentType.Head] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.02f
        };
        frame1.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(-0.5f, 0),
            Rotation = 0.1f
        };
        frame1.Parts[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        frame1.Parts[SegmentType.RightArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        // Mid frame
        var frame2 = new AnimationFrame(0.4f);
        frame2.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.05f
        };
        frame2.Parts[SegmentType.Head] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.02f
        };
        frame2.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(0.5f, 0),
            Rotation = -0.1f
        };
        frame2.Parts[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        frame2.Parts[SegmentType.RightArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        // End frame (same as first to loop smoothly)
        var frame3 = new AnimationFrame(0.8f);
        frame3.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.05f
        };
        frame3.Parts[SegmentType.Head] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.02f
        };
        frame3.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(-0.5f, 0),
            Rotation = 0.1f
        };
        frame3.Parts[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        frame3.Parts[SegmentType.RightArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        walking.AddFrame(frame1);
        walking.AddFrame(frame2);
        walking.AddFrame(frame3);

        return walking;
    }

    private ProceduralAnimation JumpAnimation()
    {
        var jump = new ProceduralAnimation("Jump", "Jump Animation", 0.6f);
        jump.Looping = false; // Non-looping animation

        // Starting frame (preparation)
        var start = new AnimationFrame(0f);
        start.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(0, -.75f),
            Rotation = 0.1f
        };


        // Mid frame (in air)
        var mid = new AnimationFrame(0.3f);
        mid.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(0, -.5f),
            Rotation = -0.1f
        };

        // End frame (landing)
        var end = new AnimationFrame(0.6f);
        end.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(0, -.25f),
            Rotation = 0.05f
        };

        jump.AddFrame(start);
        jump.AddFrame(mid);
        jump.AddFrame(end);

        return jump;
    }

    private ProceduralAnimation DashAnimation()
    {
        var dash = new ProceduralAnimation("Dash", "Dash Animation", 0.5f);
        dash.Looping = false;

        // Start frame
        var start = new AnimationFrame(0f);
        start.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.1f // Lean forward
        };
        start.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(1f, 0),
            Rotation = 0.15f
        };

        // End frame
        var end = new AnimationFrame(0.5f);
        end.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0f // Return to upright
        };
        end.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(0, 0),
            Rotation = 0f
        };

        dash.AddFrame(start);
        dash.AddFrame(end);

        return dash;
    }
    private ProceduralAnimation DrinkPotionAnimation()
    {
        var drink = new ProceduralAnimation("DrinkPotion", "Drink Potion Animation", 0.5f);
        drink.Looping = false;

        var start = new AnimationFrame(0f);
        start.Parts[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -1f,
            StretchAmount = CompositeArmStretchAmount.Quarter
        };

        var mid = new AnimationFrame(0.3f);
        mid.Parts[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -1.6f,
            StretchAmount = CompositeArmStretchAmount.ThreeQuarters
        };

        var end = new AnimationFrame(0.5f);
        end.Parts[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -1.8f,
            StretchAmount = CompositeArmStretchAmount.ThreeQuarters
        };

        drink.AddFrame(start);
        drink.AddFrame(mid);
        drink.AddFrame(end);

        return drink;
    }
}
