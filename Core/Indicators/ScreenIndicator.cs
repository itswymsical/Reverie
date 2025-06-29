using Reverie.Core.Missions.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.UI;

namespace Reverie.Core.Indicators;
public enum AnimationType
{
    Hover,
    Wag,
    Swivel,
    Shake
}

public class ScreenIndicator
{
    #region Properties
    public Vector2 WorldPosition { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    /// <summary>
    /// do not use this field outside of debugging purposes.
    /// </summary>
    public Entity trackingEntity;
    /// <summary>
    /// do not use this field outside of debugging purposes.
    /// </summary>
    public Vector2 trackingOffset;
    private bool isTracking => trackingEntity != null && trackingEntity.active;

    public bool IsHovering { get; private set; }
    public bool WasHovering { get; private set; }
    public bool JustHovered => IsHovering && !WasHovering;
    public bool JustStoppedHovering => !IsHovering && WasHovering;
    public bool IsVisible { get; set; } = true;

    public float AnimationTimer { get; private set; } = 0f;
    public Vector2 Offset { get; set; } = Vector2.Zero;

    public event Action OnClick;

    // Animation system
    public virtual AnimationType AnimationStyle => instanceAnimationType ?? AnimationType.Hover;
    private AnimationType? instanceAnimationType = null;
    protected float hoverFadeIn = 0f;
    protected float animationTimer = 0f;
    protected bool animationActive = false;

    private const float HOVER_FADE_SPEED = 0.12f;
    private const float WAG_DURATION = 0.8f;
    private const float WAG_INTENSITY = 0.25f;
    private const float SWIVEL_DURATION = 1.2f;
    private const float SWIVEL_INTENSITY = 0.4f;
    private const float SHAKE_DURATION = 0.4f;
    private const float SHAKE_INTENSITY = 0.15f;
    #endregion

    public delegate void DrawDelegate(SpriteBatch spriteBatch, Vector2 worldPos, float opacity);
    public DrawDelegate OnDrawWorld;

    public ScreenIndicator(Vector2 worldPosition, int width, int height, AnimationType? animationType = null)
    {
        WorldPosition = worldPosition;
        Width = width;
        Height = height;
        instanceAnimationType = animationType;
    }

    /// <summary>
    /// Sets the animation type for this specific instance
    /// </summary>
    public void SetAnimationType(AnimationType animationType)
    {
        instanceAnimationType = animationType;
    }

    /// <summary>
    /// Sets this entity to track another entity's position
    /// </summary>
    public void TrackEntity(Entity entity, Vector2 offset)
    {
        trackingEntity = entity;
        trackingOffset = offset;

        if (entity != null && entity.active)
        {
            UpdateTrackingPosition();
        }
    }

    private void UpdateTrackingPosition()
    {
        if (trackingEntity is NPC npc)
        {
            WorldPosition = npc.Top + trackingOffset;
        }
        else
        {
            WorldPosition = trackingEntity.position + trackingOffset;
        }
    }

    public virtual void Update()
    {
        AnimationTimer += 0.1f;

        if (isTracking)
        {
            UpdateTrackingPosition();
        }

        WasHovering = IsHovering;
        IsHovering = CheckHovering();

        UpdateAnimations();

        if (IsHovering && Main.mouseLeft && Main.mouseLeftRelease)
        {
            OnClick?.Invoke();
        }

        CustomUpdate();
    }

    /// <summary>
    /// Handles all animation logic based on the Type property
    /// </summary>
    protected virtual void UpdateAnimations()
    {
        // Handle hover fade for all animation types
        if (IsHovering && hoverFadeIn < 1f)
        {
            hoverFadeIn += HOVER_FADE_SPEED;
            if (hoverFadeIn > 1f) hoverFadeIn = 1f;

            if (JustHovered)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                StartAnimation();
            }
        }
        else if (!IsHovering && hoverFadeIn > 0f)
        {
            hoverFadeIn -= HOVER_FADE_SPEED;
            if (hoverFadeIn < 0f) hoverFadeIn = 0f;
        }

        // Update specific animation
        if (animationActive)
        {
            animationTimer += 1f / 60f;

            switch (AnimationStyle)
            {
                case AnimationType.Wag:
                    if (animationTimer >= WAG_DURATION)
                    {
                        animationActive = false;
                        animationTimer = 0f;
                    }
                    break;

                case AnimationType.Swivel:
                    if (animationTimer >= SWIVEL_DURATION)
                    {
                        animationActive = false;
                        animationTimer = 0f;
                    }
                    break;

                case AnimationType.Shake:
                    if (animationTimer >= SHAKE_DURATION)
                    {
                        animationActive = false;
                        animationTimer = 0f;
                    }
                    break;
            }
        }

        // Apply base offset based on animation type
        ApplyAnimationOffset();
    }

    /// <summary>
    /// Starts the animation when hovering begins
    /// </summary>
    protected virtual void StartAnimation()
    {
        if (AnimationStyle != AnimationType.Hover)
        {
            animationActive = true;
            animationTimer = 0f;
        }
    }

    /// <summary>
    /// Applies offset based on current animation
    /// </summary>
    protected virtual void ApplyAnimationOffset()
    {
        switch (AnimationStyle)
        {
            case AnimationType.Hover:
                // Simple gentle bobbing
                Offset = new Vector2(0, (float)Math.Sin(AnimationTimer * 0.8f) * 3f);
                break;

            case AnimationType.Wag:
                // Gentle bobbing (rotation handled in GetAnimationRotation)
                Offset = new Vector2(0, (float)Math.Sin(AnimationTimer * 0.8f) * 2.5f);
                break;

            case AnimationType.Swivel:
                // Gentle bobbing (rotation handled in GetAnimationRotation)
                Offset = new Vector2(0, (float)Math.Sin(AnimationTimer * 0.8f) * 3.5f);
                break;

            case AnimationType.Shake:
                // Base bobbing plus shake offset when active
                var baseOffset = new Vector2(0, (float)Math.Sin(AnimationTimer * 0.8f) * 2f);
                if (animationActive)
                {
                    var progress = animationTimer / SHAKE_DURATION;
                    var intensity = SHAKE_INTENSITY * (1f - progress);
                    var shakeX = (float)(Main.rand.NextDouble() - 0.5) * intensity * 15f;
                    var shakeY = (float)(Main.rand.NextDouble() - 0.5) * intensity * 15f;
                    baseOffset += new Vector2(shakeX, shakeY);
                }
                Offset = baseOffset;
                break;
        }
    }

    /// <summary>
    /// Gets rotation value for current animation (used in draw methods)
    /// </summary>
    public virtual float GetAnimationRotation()
    {
        if (!animationActive)
            return 0f;

        switch (AnimationStyle)
        {
            case AnimationType.Wag:
                // Quick left-right finger wag that slows down
                var wagProgress = animationTimer / WAG_DURATION;
                var wagIntensity = WAG_INTENSITY * (1f - wagProgress * wagProgress); // Quadratic falloff
                return (float)Math.Sin(wagProgress * Math.PI * 5) * wagIntensity;

            case AnimationType.Swivel:
                // Smooth circular swivel motion
                var swivelProgress = animationTimer / SWIVEL_DURATION;
                var swivelIntensity = SWIVEL_INTENSITY * (float)Math.Sin(swivelProgress * Math.PI); // Sine wave falloff
                return swivelProgress * (float)Math.PI * 2 * swivelIntensity;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// Gets scale multiplier for current animation
    /// </summary>
    public virtual float GetAnimationScale()
    {
        var baseScale = 1f;

        if (IsHovering)
        {
            switch (AnimationStyle)
            {
                case AnimationType.Hover:
                    baseScale = 1.15f;
                    break;
                case AnimationType.Wag:
                    baseScale = 1.2f;
                    break;
                case AnimationType.Swivel:
                    baseScale = 1.1f;
                    break;
                case AnimationType.Shake:
                    baseScale = 1.05f;
                    break;
            }
        }

        return baseScale;
    }

    /// <summary>
    /// Gets opacity multiplier for hover fade
    /// </summary>
    public virtual float GetHoverOpacity() => hoverFadeIn;

    /// <summary>
    /// Override in derived classes for custom update logic
    /// </summary>
    protected virtual void CustomUpdate() { }

    /// <summary>
    /// Checks if the mouse is hovering over this entity
    /// </summary>
    public bool CheckHovering()
    {
        if (!IsVisible)
            return false;

        // Convert world position to screen position for mouse checking
        var worldPosWithOffset = WorldPosition + Offset;
        var screenPos = WorldToScreen(worldPosWithOffset);

        // Use zoom for scaling the hitbox
        var zoom = Main.GameViewMatrix.Zoom.X;
        var scaledWidth = (int)(Width * zoom);
        var scaledHeight = (int)(Height * zoom);

        var hitboxRect = new Rectangle(
            (int)screenPos.X - scaledWidth / 2,
            (int)screenPos.Y - scaledHeight / 2,
            scaledWidth,
            scaledHeight
        );

        return hitboxRect.Contains(Main.MouseScreen.ToPoint());
    }

    /// <summary>
    /// Draws the entity in world space (called during world rendering)
    /// </summary>
    public virtual void DrawInWorld(SpriteBatch spriteBatch)
    {
        if (!IsVisible)
            return;

        // Use world position directly - no conversion needed since we're in world space
        var worldPosWithOffset = WorldPosition + Offset;

        // Call custom draw method if provided
        OnDrawWorld?.Invoke(spriteBatch, worldPosWithOffset, 1f);
    }

    /// <summary>
    /// Legacy screen-space draw method (kept for backwards compatibility)
    /// </summary>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible)
            return;

        var screenPos = WorldToScreen(WorldPosition + Offset);
        OnDrawWorld?.Invoke(spriteBatch, screenPos, 1f);
    }

    /// <summary>
    /// Converts world coordinates to screen coordinates
    /// </summary>
    public static Vector2 WorldToScreen(Vector2 worldPos)
    {
        return Vector2.Transform(worldPos - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix);
    }

    /// <summary>
    /// Converts screen coordinates to world coordinates
    /// </summary>
    public static Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return Vector2.Transform(screenPos, Matrix.Invert(Main.GameViewMatrix.TransformationMatrix)) + Main.screenPosition;
    }
}

/// <summary>
/// Unified manager for all screen indicators in the world
/// </summary>
public class ScreenIndicatorManager : ModSystem
{
    public static ScreenIndicatorManager Instance { get; set; }
    public ScreenIndicatorManager() => Instance = this;

    private readonly List<ScreenIndicator> indicators = [];
    private readonly Dictionary<int, ScreenIndicator> npcIndicators = [];
    private readonly Dictionary<int, object> npcTracking = [];

    public override void Unload()
    {
        if (!Main.dedServ)
        {
            Instance = null;
        }
        indicators.Clear();
        npcIndicators.Clear();
        npcTracking.Clear();
    }

    public override void OnWorldUnload()
    {
        base.OnWorldUnload();
        indicators.Clear();
        npcIndicators.Clear();
        npcTracking.Clear();
    }

    public T CreateIndicator<T>(Vector2 worldPosition, params object[] args) where T : ScreenIndicator
    {
        var indicator = (T)Activator.CreateInstance(typeof(T), new object[] { worldPosition }.Concat(args).ToArray());
        indicators.Add(indicator);
        return indicator;
    }

    public T CreateIndicatorForNPC<T>(NPC npc, object trackingKey, params object[] args) where T : ScreenIndicator
    {
        int npcIndex = npc.whoAmI;

        // Check if we need to replace existing indicator
        if (npcIndicators.ContainsKey(npcIndex))
        {
            if (npcTracking.ContainsKey(npcIndex) && !npcTracking[npcIndex].Equals(trackingKey))
            {
                var oldIndicator = npcIndicators[npcIndex];
                indicators.Remove(oldIndicator);
                npcIndicators.Remove(npcIndex);

                var indicator = CreateIndicatorForNPCInternal<T>(npc, trackingKey, args);
                return indicator;
            }

            return (T)npcIndicators[npcIndex];
        }

        return CreateIndicatorForNPCInternal<T>(npc, trackingKey, args);
    }

    private T CreateIndicatorForNPCInternal<T>(NPC npc, object trackingKey, params object[] args) where T : ScreenIndicator
    {
        int npcIndex = npc.whoAmI;

        T indicator;
        if (typeof(T) == typeof(MissionIndicator))
        {
            var mission = (Mission)args[0];
            var animationType = args.Length > 1 ? (AnimationType?)args[1] : null;
            indicator = (T)(object)MissionIndicator.CreateForNPC(npc, mission, animationType);
        }
        else
        {
            throw new NotSupportedException($"Indicator type {typeof(T).Name} not supported");
        }

        indicators.Add(indicator);
        npcIndicators[npcIndex] = indicator;
        npcTracking[npcIndex] = trackingKey;

        return indicator;
    }

    public void RemoveIndicatorForNPC(int npcIndex)
    {
        if (npcIndicators.TryGetValue(npcIndex, out ScreenIndicator indicator))
        {
            indicators.Remove(indicator);
            npcIndicators.Remove(npcIndex);
            npcTracking.Remove(npcIndex);
        }
    }

    public void RemoveIndicatorsForKey(object trackingKey)
    {
        var npcsToRemove = npcTracking
            .Where(kvp => kvp.Value.Equals(trackingKey))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var npcIndex in npcsToRemove)
        {
            RemoveIndicatorForNPC(npcIndex);
        }
    }

    public void RemoveIndicatorsOfType<T>() where T : ScreenIndicator
    {
        var toRemove = indicators.OfType<T>().ToList();
        foreach (var indicator in toRemove)
        {
            indicators.Remove(indicator);

            // Remove from NPC tracking if applicable
            var npcEntry = npcIndicators.FirstOrDefault(kvp => kvp.Value == indicator);
            if (npcEntry.Key != 0)
            {
                npcIndicators.Remove(npcEntry.Key);
                npcTracking.Remove(npcEntry.Key);
            }
        }
    }

    public bool HasIndicatorForNPC(int npcIndex) => npcIndicators.ContainsKey(npcIndex);

    public T GetIndicatorForNPC<T>(int npcIndex) where T : ScreenIndicator
    {
        return npcIndicators.TryGetValue(npcIndex, out ScreenIndicator indicator) && indicator is T typed ? typed : null;
    }

    public object GetTrackingKeyForNPC(int npcIndex)
    {
        return npcTracking.TryGetValue(npcIndex, out object key) ? key : null;
    }

    public override void PostUpdateEverything()
    {
        for (var i = indicators.Count - 1; i >= 0; i--)
        {
            indicators[i].Update();

            if (!indicators[i].IsVisible)
            {
                // Clean up NPC tracking
                var npcEntry = npcIndicators.FirstOrDefault(kvp => kvp.Value == indicators[i]);
                if (npcEntry.Key != 0)
                {
                    npcIndicators.Remove(npcEntry.Key);
                    npcTracking.Remove(npcEntry.Key);
                }

                indicators.RemoveAt(i);
            }
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int invasionIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Invasion Progress Bars"));
        if (invasionIndex != -1)
        {
            layers.Insert(invasionIndex + 1, new LegacyGameInterfaceLayer(
                "Reverie: Screen Indicators",
                delegate {
                    Instance.Draw(Main.spriteBatch);
                    return true;
                },
                InterfaceScaleType.Game)
            );
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var indicator in indicators)
        {
            indicator.Draw(spriteBatch);
        }
    }

    public void ClearAllIndicators()
    {
        indicators.Clear();
        npcIndicators.Clear();
        npcTracking.Clear();
    }

    // Convenience methods for mission indicators only
    public MissionIndicator CreateMissionIndicator(Vector2 worldPosition, Mission mission, AnimationType? animationType = null)
    {
        return CreateIndicator<MissionIndicator>(worldPosition, mission, animationType);
    }

    public MissionIndicator CreateMissionIndicatorForNPC(NPC npc, Mission mission, AnimationType? animationType = null)
    {
        return CreateIndicatorForNPC<MissionIndicator>(npc, mission.ID, mission, animationType);
    }

    // Legacy support for missions
    public void RemoveIndicatorsForMission(int missionID) => RemoveIndicatorsForKey(missionID);
}