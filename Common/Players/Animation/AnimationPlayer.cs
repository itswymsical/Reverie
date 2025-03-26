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

public enum AnimationLayer
{
    Base,
    LowerBody,
    UpperBody,
    Arms,
    Head,
    Override
}

public partial class AnimationPlayer : ModPlayer
{
    // Animation state tracking
    private readonly Dictionary<SegmentType, SegmentState> defaultJointStates = [];
    private float lastGameUpdateCount = 0f;
    public bool needsReset = false;

    // Layer-based animation system
    private readonly Dictionary<AnimationLayer, ActiveAnimation> activeAnimations = [];
    private readonly Dictionary<string, ProceduralAnimation> registeredAnimations = [];

    // Blend factors for transitioning between animations
    private const float DEFAULT_BLEND_DURATION = 0.25f; // 0.25 seconds default blend time

    public override void Initialize()
    {
        SaveDefaultPositions();
        RegisterAnimations();

        // Initialize with idle animation on base layer
        if (registeredAnimations.TryGetValue("Idle", out var idleAnim))
        {
            PlayAnimation(AnimationLayer.Base, idleAnim);
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
        // Register base animations
        registeredAnimations["Idle"] = IdleAnimation();
        registeredAnimations["Walking"] = WalkingAnimation();

        // Register arm animations
        registeredAnimations["DrinkPotion"] = DrinkPotionAnimation();

        // Register leg animations
        registeredAnimations["Jump"] = JumpAnimation();
        registeredAnimations["Dash"] = DashAnimation();

        // More animations can be registered here
    }

    public void RegisterAnimation(ProceduralAnimation animation)
    {
        if (!string.IsNullOrEmpty(animation.Id))
        {
            registeredAnimations[animation.Id] = animation;
        }
    }

    public void PlayAnimation(AnimationLayer layer, string animationId, float blendDuration = DEFAULT_BLEND_DURATION)
    {
        if (registeredAnimations.TryGetValue(animationId, out var animation))
        {
            PlayAnimation(layer, animation, blendDuration);
        }
    }

    public void PlayAnimation(AnimationLayer layer, ProceduralAnimation animation, float blendDuration = DEFAULT_BLEND_DURATION)
    {
        if (animation == null)
            return;

        // Check if we already have an animation on this layer
        if (activeAnimations.TryGetValue(layer, out var currentAnim))
        {
            // If it's the same animation, don't restart
            if (currentAnim.Animation == animation)
                return;

            // Start blending from current animation
            currentAnim.BlendOutDuration = blendDuration;
            currentAnim.IsBlendingOut = true;
        }

        // Create new active animation instance
        var newActiveAnim = new ActiveAnimation
        {
            Animation = animation,
            Time = 0f,
            Layer = layer,
            BlendInDuration = blendDuration,
            BlendFactor = 0f,
        };

        activeAnimations[layer] = newActiveAnim;
        needsReset = true;
    }

    public override void PostUpdate()
    {
        if (!Main.gamePaused)
        {
            UpdatePlayerState();
            UpdateAnimations();
            lastGameUpdateCount = Main.GameUpdateCount;
        }
    }

    private void UpdatePlayerState()
    {
        // Detect player actions and play appropriate animations

        // Override layer takes priority
        if (Player.ItemAnimationActive)
        {
            if (Player.HeldItem.useStyle == ItemUseStyleID.DrinkLong || Player.HeldItem.useStyle == ItemUseStyleID.DrinkLiquid)
            {
                PlayAnimation(AnimationLayer.Arms, "DrinkPotion");
            }
        }

        // Handle movement states
        if (Player.IsJumping())
        {
            PlayAnimation(AnimationLayer.Base, "Jump");
        }
        else if (Player.dashDelay > 0)
        {
            PlayAnimation(AnimationLayer.UpperBody, "Dash");
        }
        else if (Player.IsMoving())
        {
            // Only change base animation if no other animations are active
            if (!activeAnimations.ContainsKey(AnimationLayer.Override))
            {
                PlayAnimation(AnimationLayer.Base, "Walking");
            }
        }
        else if (!activeAnimations.ContainsKey(AnimationLayer.Override))
        {
            // Return to idle if we're not doing anything else
            if (Player.afkCounter > 0)
            {
                PlayAnimation(AnimationLayer.Base, "Idle");
            }
        }
    }

    private void UpdateAnimations()
    {
        float deltaTime = 1f / 60f; // Assuming 60fps
        var finishedAnimations = new List<AnimationLayer>();

        foreach (var pair in activeAnimations)
        {
            var layer = pair.Key;
            var activeAnim = pair.Value;

            // Update time
            activeAnim.Time += deltaTime;

            // Handle blending
            if (activeAnim.IsBlendingIn)
            {
                activeAnim.BlendFactor = MathHelper.Clamp(activeAnim.Time / activeAnim.BlendInDuration, 0f, 1f);
                if (activeAnim.BlendFactor >= 1f)
                {
                    activeAnim.IsBlendingIn = false;
                }
            }
            else if (activeAnim.IsBlendingOut)
            {
                activeAnim.BlendFactor = MathHelper.Clamp(1f - (activeAnim.Time / activeAnim.BlendOutDuration), 0f, 1f);
                if (activeAnim.BlendFactor <= 0f)
                {
                    finishedAnimations.Add(layer);
                }
            }

            // Check for animation completion
            if (!activeAnim.IsBlendingOut && !activeAnim.Animation.Looping &&
                activeAnim.Time >= activeAnim.Animation.Duration)
            {
                // Non-looping animation has finished
                if (layer == AnimationLayer.Base)
                {
                    // Auto-transition base layer back to idle
                    PlayAnimation(AnimationLayer.Base, "Idle");
                }
                else
                {
                    // Start blending out other layers
                    activeAnim.IsBlendingOut = true;
                    activeAnim.Time = 0f; // Reset time for blend out
                }
            }
        }

        // Remove finished animations
        foreach (var layer in finishedAnimations)
        {
            activeAnimations.Remove(layer);
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

        if (activeAnimations.Count > 0)
        {
            ApplyAnimationStates(ref drawInfo);
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

    private void ApplyAnimationStates(ref PlayerDrawSet drawInfo)
    {
        // Define which layers affect which joints
        var layerJointMap = new Dictionary<AnimationLayer, List<SegmentType>>
        {
            [AnimationLayer.Base] = new List<SegmentType>
            {
                SegmentType.Head, SegmentType.Body, SegmentType.Legs,
                SegmentType.LeftArm, SegmentType.RightArm
            },
            [AnimationLayer.LowerBody] = new List<SegmentType> { SegmentType.Legs },
            [AnimationLayer.UpperBody] = new List<SegmentType> { SegmentType.Body },
            [AnimationLayer.Arms] = new List<SegmentType> { SegmentType.LeftArm, SegmentType.RightArm },
            [AnimationLayer.Head] = new List<SegmentType> { SegmentType.Head },
            [AnimationLayer.Override] = new List<SegmentType>
            {
                SegmentType.Head, SegmentType.Body, SegmentType.Legs,
                SegmentType.LeftArm, SegmentType.RightArm
            }
        };

        // First, gather all animation states by layer priority
        var layerOrder = new List<AnimationLayer>
        {
            AnimationLayer.Base,
            AnimationLayer.LowerBody,
            AnimationLayer.UpperBody,
            AnimationLayer.Head,
            AnimationLayer.Arms,
            AnimationLayer.Override
        };

        // Final blended states for each joint
        var finalJointStates = new Dictionary<SegmentType, SegmentState>();

        // Process each layer in order of priority
        foreach (var layer in layerOrder)
        {
            if (!activeAnimations.TryGetValue(layer, out var activeAnim))
                continue;

            float time = activeAnim.Time;
            if (!activeAnim.Animation.Looping && time > activeAnim.Animation.Duration)
            {
                time = activeAnim.Animation.Duration;
            }
            else if (activeAnim.Animation.Looping)
            {
                time %= activeAnim.Animation.Duration;
            }

            var stateAtTime = activeAnim.Animation.GetStateAtTime(time);

            // Only process joints that this layer affects
            if (layerJointMap.TryGetValue(layer, out var affectedJoints))
            {
                foreach (var joint in affectedJoints)
                {
                    if (stateAtTime.TryGetValue(joint, out var state))
                    {
                        if (finalJointStates.TryGetValue(joint, out var existingState) && activeAnim.BlendFactor < 1f)
                        {
                            // Blend with existing state if we're not at full weight
                            finalJointStates[joint] = SegmentState.Lerp(existingState, state, activeAnim.BlendFactor);
                        }
                        else
                        {
                            // Either first layer to affect this joint or full weight
                            finalJointStates[joint] = state;
                        }
                    }
                }
            }
        }

        // Apply final joint states
        foreach (var pair in finalJointStates)
        {
            ApplySegmentState(pair.Key, pair.Value, ref drawInfo);
        }
    }

    private void ApplySegmentState(SegmentType jointType, SegmentState state, ref PlayerDrawSet drawInfo)
    {
        var rotation = state.Rotation;
        var position = state.Position;

        // Handle direction
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

    //private ProceduralAnimation IdleAnimation()
    //{
    //    var idle = new ProceduralAnimation("Idle", "Idle", 2.0f);
    //    idle.Looping = true;

    //    var start = new AnimationFrame(0f);
    //    start.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0f
    //    };
    //    start.Parts[SegmentType.Head] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0f
    //    };
    //    start.Parts[SegmentType.LeftArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.1f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };
    //    start.Parts[SegmentType.RightArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0.1f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };

    //    var mid = new AnimationFrame(1.0f);
    //    mid.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = new Vector2(0, -1f),
    //        Rotation = 0.05f
    //    };
    //    mid.Parts[SegmentType.Head] = new SegmentState
    //    {
    //        Position = new Vector2(0, -0.5f),
    //        Rotation = -0.02f
    //    };
    //    mid.Parts[SegmentType.LeftArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0.1f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };
    //    mid.Parts[SegmentType.RightArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.1f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };

    //    var end = new AnimationFrame(2.0f);
    //    end.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0f
    //    };
    //    end.Parts[SegmentType.Head] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0f
    //    };
    //    end.Parts[SegmentType.LeftArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.1f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };
    //    end.Parts[SegmentType.RightArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0.1f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };

    //    idle.AddFrame(start);
    //    idle.AddFrame(mid);
    //    idle.AddFrame(end);

    //    return idle;
    //}

    //private ProceduralAnimation IdleUseAnimation()
    //{
    //    var idle = new ProceduralAnimation("Idle", "Idle", 2.0f);
    //    idle.Looping = true;

    //    var start = new AnimationFrame(0f);
    //    start.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0f
    //    };
    //    start.Parts[SegmentType.Head] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0f
    //    };

    //    var mid = new AnimationFrame(1.0f);
    //    mid.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = new Vector2(0, -1f),
    //        Rotation = 0.05f
    //    };
    //    mid.Parts[SegmentType.Head] = new SegmentState
    //    {
    //        Position = new Vector2(0, -0.5f),
    //        Rotation = -0.02f
    //    };

    //    var end = new AnimationFrame(2.0f);
    //    end.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0f
    //    };
    //    end.Parts[SegmentType.Head] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0f
    //    };

    //    idle.AddFrame(start);
    //    idle.AddFrame(mid);
    //    idle.AddFrame(end);

    //    return idle;
    //}

    //private ProceduralAnimation WalkingAnimation()
    //{
    //    // Create walking animation
    //    var walking = new ProceduralAnimation("Walking", "Walking", 0.8f);
    //    walking.Looping = true;
    //    // Add frames with joint states...
    //    // First frame
    //    var frame1 = new AnimationFrame(0f);
    //    frame1.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.05f
    //    };
    //    frame1.Parts[SegmentType.Legs] = new SegmentState
    //    {
    //        Position = new Vector2(-0.5f, 0),
    //        Rotation = 0.1f
    //    };
    //    frame1.Parts[SegmentType.LeftArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.2f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };
    //    frame1.Parts[SegmentType.RightArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0.2f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };
    //    // Mid frame
    //    var frame2 = new AnimationFrame(0.4f);
    //    frame2.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0.05f
    //    };
    //    frame2.Parts[SegmentType.Legs] = new SegmentState
    //    {
    //        Position = new Vector2(0.5f, 0),
    //        Rotation = -0.1f
    //    };
    //    frame2.Parts[SegmentType.LeftArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0.2f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };
    //    frame2.Parts[SegmentType.RightArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.2f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };
    //    // End frame (same as first to loop smoothly)
    //    var frame3 = new AnimationFrame(0.8f);
    //    frame3.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.05f
    //    };
    //    frame3.Parts[SegmentType.Legs] = new SegmentState
    //    {
    //        Position = new Vector2(-0.5f, 0),
    //        Rotation = 0.1f
    //    };
    //    frame3.Parts[SegmentType.LeftArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.2f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };
    //    frame3.Parts[SegmentType.RightArm] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0.2f,
    //        StretchAmount = CompositeArmStretchAmount.Full
    //    };
    //    walking.AddFrame(frame1);
    //    walking.AddFrame(frame2);
    //    walking.AddFrame(frame3);
    //    return walking;
    //}

    //private ProceduralAnimation WalkingUseAnimation()
    //{
    //    // Create walking animation
    //    var walking = new ProceduralAnimation("Walking", "Walking", 0.8f);
    //    walking.Looping = true;
    //    // Add frames with joint states...
    //    // First frame
    //    var frame1 = new AnimationFrame(0f);
    //    frame1.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.05f
    //    };
    //    frame1.Parts[SegmentType.Legs] = new SegmentState
    //    {
    //        Position = new Vector2(-0.5f, 0),
    //        Rotation = 0.1f
    //    };
    //    // Mid frame
    //    var frame2 = new AnimationFrame(0.4f);
    //    frame2.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = 0.05f
    //    };
    //    frame2.Parts[SegmentType.Legs] = new SegmentState
    //    {
    //        Position = new Vector2(0.5f, 0),
    //        Rotation = -0.1f
    //    };

    //    // End frame (same as first to loop smoothly)
    //    var frame3 = new AnimationFrame(0.8f);
    //    frame3.Parts[SegmentType.Body] = new SegmentState
    //    {
    //        Position = Vector2.Zero,
    //        Rotation = -0.05f
    //    };
    //    frame3.Parts[SegmentType.Legs] = new SegmentState
    //    {
    //        Position = new Vector2(-0.5f, 0),
    //        Rotation = 0.1f
    //    };

    //    walking.AddFrame(frame1);
    //    walking.AddFrame(frame2);
    //    walking.AddFrame(frame3);
    //    return walking;
    //}
}