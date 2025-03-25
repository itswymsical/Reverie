using Reverie.Common.Animation;
using Reverie.Common.Systems;
using Reverie.Utilities;
using Reverie.Utilities.Extensions;
using System.Collections.Generic;
using Terraria.DataStructures;
using static Terraria.Player;

namespace Reverie.Common.Players;

public enum AnimationState
{
    Idle,
    Jumping,
    Walking,
    Dash,
    Drink,
    Swing
}

public class AnimationPlayer : ModPlayer
{
    private ReverieAnimation currentAnimation = null;
    private float animationTime = 0f;
    private Dictionary<JointType, JointState> defaultJointStates = new Dictionary<JointType, JointState>();
    private float lastGameUpdateCount = 0f;
    private bool needsReset = false;

    private Dictionary<string, ReverieAnimation> animations = new Dictionary<string, ReverieAnimation>();

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
        defaultJointStates[JointType.Head] = new JointState
        {
            Position = Player.headPosition,
            Rotation = Player.headRotation
        };

        defaultJointStates[JointType.Body] = new JointState
        {
            Position = Player.bodyPosition,
            Rotation = Player.bodyRotation
        };

        defaultJointStates[JointType.Legs] = new JointState
        {
            Position = Player.legPosition,
            Rotation = Player.legRotation
        };

        defaultJointStates[JointType.LeftArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        defaultJointStates[JointType.RightArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
    }

    private void RegisterAnimations()
    {
        animations["Idle"] = CreateIdleAnimation();
        animations["UseIdle"] = CreateIdleUseAnimation();
        animations["Walking"] = CreateWalkingAnimation();
        animations["UseWalking"] = CreateWalkingUseAnimation();
    }

    public void RegisterAnimation(ReverieAnimation animation)
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

    public void PlayAnimation(ReverieAnimation animation)
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

        if (!Player.IsUsingItem())
        {
            if (Player.HeldItem.DealsDamage())
            {
                PlayerExtensions.DrawItemBehindPlayer(Player, ref drawInfo);
            }
        }


        if (Main.gamePaused)
        {
            if (currentAnimation != null)
            {
                ApplyAnimationState(ref drawInfo);
            }
            return;
        }

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
        if (defaultJointStates.TryGetValue(JointType.Head, out var headState))
        {
            drawInfo.drawPlayer.headPosition = headState.Position;
            drawInfo.drawPlayer.headRotation = headState.Rotation;
        }

        if (defaultJointStates.TryGetValue(JointType.Body, out var bodyState))
        {
            drawInfo.drawPlayer.bodyPosition = bodyState.Position;
            drawInfo.drawPlayer.bodyRotation = bodyState.Rotation;
        }

        if (defaultJointStates.TryGetValue(JointType.Legs, out var legsState))
        {
            drawInfo.drawPlayer.legPosition = legsState.Position;
            drawInfo.drawPlayer.legRotation = legsState.Rotation;
        }

        drawInfo.drawPlayer.fullRotation = 0f;
    }

    private void ApplyAnimationState(ref PlayerDrawSet drawInfo)
    {
        Dictionary<JointType, JointState> currentState = currentAnimation.GetStateAtTime(animationTime);

        // Apply defined joints from the animation
        foreach (var pair in currentState)
        {
            ApplyJointState(pair.Key, pair.Value, ref drawInfo);
        }

    }

    private void ApplyJointState(JointType jointType, JointState state, ref PlayerDrawSet drawInfo)
    {
        float rotation = state.Rotation;
        Vector2 position = state.Position;

        if (drawInfo.drawPlayer.direction == -1)
        {
            rotation = -rotation;
            position.X = -position.X;
        }

        switch (jointType)
        {
            case JointType.Head:
                drawInfo.drawPlayer.headRotation = rotation;
                drawInfo.drawPlayer.headPosition = position;
                break;

            case JointType.Body:
                drawInfo.drawPlayer.bodyRotation = rotation;
                drawInfo.drawPlayer.bodyPosition = position;
                break;

            case JointType.Legs:
                drawInfo.drawPlayer.legRotation = rotation;
                drawInfo.drawPlayer.legPosition = position;
                break;

            case JointType.LeftArm:
                drawInfo.drawPlayer.SetCompositeArmFront(
                    true,
                    state.StretchAmount,
                    rotation
                );
                break;

            case JointType.RightArm:
                drawInfo.drawPlayer.SetCompositeArmBack(
                    true,
                    state.StretchAmount,
                    rotation
                );
                break;
        }
    }

    private ReverieAnimation CreateIdleAnimation()
    {
        ReverieAnimation idle = new ReverieAnimation("Idle", "Idle", 2.0f);
        idle.Looping = true;

        AnimationFrame start = new AnimationFrame(0f);
        start.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        start.Joints[JointType.Head] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        start.Joints[JointType.LeftArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        start.Joints[JointType.RightArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        AnimationFrame mid = new AnimationFrame(1.0f);
        mid.Joints[JointType.Body] = new JointState
        {
            Position = new Vector2(0, -1f),
            Rotation = 0.05f
        };
        mid.Joints[JointType.Head] = new JointState
        {
            Position = new Vector2(0, -0.5f),
            Rotation = -0.02f
        };
        mid.Joints[JointType.LeftArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        mid.Joints[JointType.RightArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };

        AnimationFrame end = new AnimationFrame(2.0f);
        end.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        end.Joints[JointType.Head] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        end.Joints[JointType.LeftArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.1f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        end.Joints[JointType.RightArm] = new JointState
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

    private ReverieAnimation CreateIdleUseAnimation()
    {
        ReverieAnimation idle = new ReverieAnimation("Idle", "Idle", 2.0f);
        idle.Looping = true;

        AnimationFrame start = new AnimationFrame(0f);
        start.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        start.Joints[JointType.Head] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };

        AnimationFrame mid = new AnimationFrame(1.0f);
        mid.Joints[JointType.Body] = new JointState
        {
            Position = new Vector2(0, -1f),
            Rotation = 0.05f
        };
        mid.Joints[JointType.Head] = new JointState
        {
            Position = new Vector2(0, -0.5f),
            Rotation = -0.02f
        };

        AnimationFrame end = new AnimationFrame(2.0f);
        end.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };
        end.Joints[JointType.Head] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0f
        };

        idle.AddFrame(start);
        idle.AddFrame(mid);
        idle.AddFrame(end);

        return idle;
    }

    private ReverieAnimation CreateWalkingAnimation()
    {
        // Create walking animation
        ReverieAnimation walking = new ReverieAnimation("Walking", "Walking", 0.8f);
        walking.Looping = true;
        // Add frames with joint states...
        // First frame
        AnimationFrame frame1 = new AnimationFrame(0f);
        frame1.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.05f
        };
        frame1.Joints[JointType.Legs] = new JointState
        {
            Position = new Vector2(-0.5f, 0),
            Rotation = 0.1f
        };
        frame1.Joints[JointType.LeftArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        frame1.Joints[JointType.RightArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        // Mid frame
        AnimationFrame frame2 = new AnimationFrame(0.4f);
        frame2.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0.05f
        };
        frame2.Joints[JointType.Legs] = new JointState
        {
            Position = new Vector2(0.5f, 0),
            Rotation = -0.1f
        };
        frame2.Joints[JointType.LeftArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        frame2.Joints[JointType.RightArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        // End frame (same as first to loop smoothly)
        AnimationFrame frame3 = new AnimationFrame(0.8f);
        frame3.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.05f
        };
        frame3.Joints[JointType.Legs] = new JointState
        {
            Position = new Vector2(-0.5f, 0),
            Rotation = 0.1f
        };
        frame3.Joints[JointType.LeftArm] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.2f,
            StretchAmount = CompositeArmStretchAmount.Full
        };
        frame3.Joints[JointType.RightArm] = new JointState
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

    private ReverieAnimation CreateWalkingUseAnimation()
    {
        // Create walking animation
        ReverieAnimation walking = new ReverieAnimation("Walking", "Walking", 0.8f);
        walking.Looping = true;
        // Add frames with joint states...
        // First frame
        AnimationFrame frame1 = new AnimationFrame(0f);
        frame1.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.05f
        };
        frame1.Joints[JointType.Legs] = new JointState
        {
            Position = new Vector2(-0.5f, 0),
            Rotation = 0.1f
        };
        // Mid frame
        AnimationFrame frame2 = new AnimationFrame(0.4f);
        frame2.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = 0.05f
        };
        frame2.Joints[JointType.Legs] = new JointState
        {
            Position = new Vector2(0.5f, 0),
            Rotation = -0.1f
        };

        // End frame (same as first to loop smoothly)
        AnimationFrame frame3 = new AnimationFrame(0.8f);
        frame3.Joints[JointType.Body] = new JointState
        {
            Position = Vector2.Zero,
            Rotation = -0.05f
        };
        frame3.Joints[JointType.Legs] = new JointState
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