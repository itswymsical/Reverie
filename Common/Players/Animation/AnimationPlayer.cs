using Reverie.Core.Animation;
using Reverie.Utilities.Extensions;

using System.Collections.Generic;

using Terraria.DataStructures;
using static Terraria.Player;

namespace Reverie.Common.Players.Animation;

public enum AnimationState
{
    Idle,
    Jumping,
    Walking,
    Dash,
    Drink,
    Swing
}

public partial class AnimationPlayer : ModPlayer
{
    public ProceduralAnimation currentAnimation = null;
    public float animationTime = 0f;
    private readonly Dictionary<SegmentType, SegmentState> defaultJointStates = [];
    private float lastGameUpdateCount = 0f;
    public bool needsReset = false;

    private readonly Dictionary<string, ProceduralAnimation> animations = [];

    public override void Initialize()
    {
        SaveDefaultPositions();

        RegisterAnimations();

        if (animations.TryGetValue("Idle", out var idleAnim))
        {
            PlayAnimation(idleAnim);
        }

        lastGameUpdateCount = Main.GameUpdateCount;
    }

    private void SaveDefaultPositions()
    {
        defaultJointStates[SegmentType.Head] = new SegmentState
        {
            Position = Player.headPosition,
            Rotation = Player.headRotation
        };

        defaultJointStates[SegmentType.Body] = new SegmentState
        {
            Position = Player.bodyPosition,
            Rotation = Player.bodyRotation
        };

        defaultJointStates[SegmentType.Legs] = new SegmentState
        {
            Position = Player.legPosition,
            Rotation = Player.legRotation
        };

        defaultJointStates[SegmentType.LeftArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        defaultJointStates[SegmentType.RightArm] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
    }

    private void RegisterAnimations()
    {
        animations["Idle"] = IdleAnimation();
        animations["UseIdle"] = IdleUseAnimation();
        animations["Walking"] = WalkingAnimation();
        animations["UseWalking"] = WalkingUseAnimation();
    }

    public void RegisterAnimation(ProceduralAnimation animation)
    {
        if (!string.IsNullOrEmpty(animation.Id))
        {
            animations[animation.Id] = animation;
        }
    }

    public void PlayAnimation(string animationId)
    {
        if (animations.TryGetValue(animationId, out var animation))
        {
            PlayAnimation(animation);
        }
    }

    public void PlayAnimation(ProceduralAnimation animation)
    {
        if (animation == null)
            return;

        // Only restart if it's a different animation
        if (currentAnimation != animation)
        {
            currentAnimation = animation;
            animationTime = 0f;
            needsReset = true;
        }
    }

    public override void PostUpdate()
    {
        if (!Main.gamePaused)
        {
            UpdateAnimationState();

            if (currentAnimation != null)
            {
                animationTime += 1f / 60f;

                if (animationTime > currentAnimation.Duration)
                {
                    if (currentAnimation.Looping)
                    {
                        animationTime %= currentAnimation.Duration;
                    }
                    else
                    {
                        animationTime = currentAnimation.Duration;

                        // Auto-transition to idle when non-looping animation ends
                        if (animations.TryGetValue("Idle", out var idleAnim))
                        {
                            currentAnimation = idleAnim;
                            animationTime = 0f;
                        }
                    }
                }
            }

            lastGameUpdateCount = Main.GameUpdateCount;
        }
    }

    private void UpdateAnimationState()
    {
        if (Player.IsMoving() && !Player.IsUsingItem() && !Player.IsJumping())
        {
            PlayAnimation("Walking");
        }
        else if (!currentAnimation?.Looping ?? true)
        {
        }
        else if (Player.afkCounter > 0)
        {
            PlayAnimation("Idle");
        }
        else if (Player.IsUsingItem())
        {
            PlayAnimation("UseIdle");
        }
        else if (Player.IsUsingItem() && Player.IsMoving())
        {
            PlayAnimation("UseWalk");
        }
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (Main.gameMenu || Player.sleeping.isSleeping)
            return;

        if (needsReset)
        {
            ResetPositions(ref drawInfo);
            needsReset = false;
        }

        if (currentAnimation != null)
        {
            ApplyAnimationState(ref drawInfo);
        }
    }

    private void ResetPositions(ref PlayerDrawSet drawInfo)
    {
        if (defaultJointStates.TryGetValue(SegmentType.Head, out var headState))
        {
            drawInfo.drawPlayer.headPosition = headState.Position;
            drawInfo.drawPlayer.headRotation = headState.Rotation;
        }

        if (defaultJointStates.TryGetValue(SegmentType.Body, out var bodyState))
        {
            drawInfo.drawPlayer.bodyPosition = bodyState.Position;
            drawInfo.drawPlayer.bodyRotation = bodyState.Rotation;
        }

        if (defaultJointStates.TryGetValue(SegmentType.Legs, out var legsState))
        {
            drawInfo.drawPlayer.legPosition = legsState.Position;
            drawInfo.drawPlayer.legRotation = legsState.Rotation;
        }

        drawInfo.drawPlayer.fullRotation = 0f;
    }

    private void ApplyAnimationState(ref PlayerDrawSet drawInfo)
    {
        var currentState = currentAnimation.GetStateAtTime(animationTime);

        // Apply defined joints from the animation
        foreach (var pair in currentState)
        {
            ApplySegmentState(pair.Key, pair.Value, ref drawInfo);
        }

    }

    private void ApplySegmentState(SegmentType jointType, SegmentState state, ref PlayerDrawSet drawInfo)
    {
        var rotation = state.Rotation;
        var position = state.Position;

        if (drawInfo.drawPlayer.direction == -1)
        {
            rotation = -rotation;
            position.X = -position.X;
        }

        switch (jointType)
        {
            case SegmentType.Head:
                drawInfo.drawPlayer.headRotation = rotation;
                drawInfo.drawPlayer.headPosition = position;
                break;

            case SegmentType.Body:
                drawInfo.drawPlayer.bodyRotation = rotation;
                drawInfo.drawPlayer.bodyPosition = position;
                break;

            case SegmentType.Legs:
                drawInfo.drawPlayer.legRotation = rotation;
                drawInfo.drawPlayer.legPosition = position;
                break;

            case SegmentType.LeftArm:
                drawInfo.drawPlayer.SetCompositeArmFront(
                    true,
                    state.StretchAmount,
                    rotation
                );
                break;

            case SegmentType.RightArm:
                drawInfo.drawPlayer.SetCompositeArmBack(
                    true,
                    state.StretchAmount,
                    rotation
                );
                break;
        }
    }

    private ProceduralAnimation IdleAnimation()
    {
        var idle = new ProceduralAnimation("Idle", "Idle", 2.0f);
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
            Position = new Vector2(0, -1f),
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

    private ProceduralAnimation IdleUseAnimation()
    {
        var idle = new ProceduralAnimation("Idle", "Idle", 2.0f);
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

        var mid = new AnimationFrame(1.0f);
        mid.Parts[SegmentType.Body] = new SegmentState
        {
            Position = new Vector2(0, -1f),
            Rotation = 0.05f
        };
        mid.Parts[SegmentType.Head] = new SegmentState
        {
            Position = new Vector2(0, -0.5f),
            Rotation = -0.02f
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

        idle.AddFrame(start);
        idle.AddFrame(mid);
        idle.AddFrame(end);

        return idle;
    }

    private ProceduralAnimation WalkingAnimation()
    {
        // Create walking animation
        var walking = new ProceduralAnimation("Walking", "Walking", 0.8f);
        walking.Looping = true;
        // Add frames with joint states...
        // First frame
        var frame1 = new AnimationFrame(0f);
        frame1.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.05f
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

    private ProceduralAnimation WalkingUseAnimation()
    {
        // Create walking animation
        var walking = new ProceduralAnimation("Walking", "Walking", 0.8f);
        walking.Looping = true;
        // Add frames with joint states...
        // First frame
        var frame1 = new AnimationFrame(0f);
        frame1.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.05f
        };
        frame1.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(-0.5f, 0),
            Rotation = 0.1f
        };
        // Mid frame
        var frame2 = new AnimationFrame(0.4f);
        frame2.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = 0.05f
        };
        frame2.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(0.5f, 0),
            Rotation = -0.1f
        };

        // End frame (same as first to loop smoothly)
        var frame3 = new AnimationFrame(0.8f);
        frame3.Parts[SegmentType.Body] = new SegmentState
        {
            Position = Vector2.Zero,
            Rotation = -0.05f
        };
        frame3.Parts[SegmentType.Legs] = new SegmentState
        {
            Position = new Vector2(-0.5f, 0),
            Rotation = 0.1f
        };

        walking.AddFrame(frame1);
        walking.AddFrame(frame2);
        walking.AddFrame(frame3);
        return walking;
    }
}