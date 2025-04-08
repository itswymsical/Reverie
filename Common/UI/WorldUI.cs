using Reverie.Core.Missions;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Common.UI;

/// <summary>
/// A world entity that acts as a UI element, allowing for both world positioning and UI interactions
/// </summary>
public class WorldUIEntity
{
    public Vector2 WorldPosition { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    private Entity trackingEntity;
    private Vector2 trackingOffset;
    private bool isTracking => trackingEntity != null && trackingEntity.active;

    public bool IsHovering { get; private set; }
    public bool WasHovering { get; private set; }
    public bool JustHovered => IsHovering && !WasHovering;
    public bool JustStoppedHovering => !IsHovering && WasHovering;
    public bool IsVisible { get; set; } = true;

    public float AnimationTimer { get; private set; } = 0f;
    public Vector2 Offset { get; set; } = Vector2.Zero;

    public event Action OnClick;

    public delegate void DrawDelegate(SpriteBatch spriteBatch, Vector2 screenPos, float opacity);
    public DrawDelegate CustomDraw;

    public WorldUIEntity(Vector2 worldPosition, int width, int height)
    {
        WorldPosition = worldPosition;
        Width = width;
        Height = height;
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

    /// <summary>
    /// Updates the position based on the tracking entity
    /// </summary>
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

    /// <summary>
    /// Updates the entity's position, animation, and interaction state
    /// </summary>
    public virtual void Update()
    {
        AnimationTimer += 0.1f;

        if (isTracking)
        {
            UpdateTrackingPosition();
        }

        WasHovering = IsHovering;
        IsHovering = CheckHovering();

        if (IsHovering && Main.mouseLeft && Main.mouseLeftRelease)
        {
            OnClick?.Invoke();
        }

        CustomUpdate();
    }

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

        var zoom = Main.GameViewMatrix.Zoom.X;

        var screenPos = WorldToScreen(WorldPosition + Offset);

        var scaledWidth = (int)(Width * zoom);
        var scaledHeight = (int)(Height * zoom);

        return new Rectangle(
            (int)screenPos.X - scaledWidth / 2,
            (int)screenPos.Y - scaledHeight / 2,
            scaledWidth,
            scaledHeight
        ).Contains(Main.MouseScreen.ToPoint());
    }

    /// <summary>
    /// Draws the entity in the world
    /// </summary>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible)
            return;

        var screenPos = WorldToScreen(WorldPosition + Offset);

        CustomDraw?.Invoke(spriteBatch, screenPos, 1f);
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
/// A mission indicator that appears in the world and shows mission details on hover
/// </summary>
public class MissionIndicator : WorldUIEntity
{
    private Mission mission;

    private Texture2D iconTexture;
    private float hoverFadeIn = 0f;
    private const float HOVER_FADE_SPEED = 0.1f;

    private const int DetailPanelWidth = 220;
    private const int DetailPadding = 10;

    private float bobAmount = (float)Math.PI;

    public MissionIndicator(Vector2 worldPosition, Mission mission)
        : base(worldPosition, 32, 32)
    {
        this.mission = mission;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;

        if (iconTexture != null)
        {
            Width = iconTexture.Width;
            Height = iconTexture.Height;
        }

        CustomDraw = DrawMissionIndicator;

        OnClick += HandleClick;
    }

    /// <summary>
    /// Creates a mission indicator that follows an NPC
    /// </summary>
    public static MissionIndicator CreateForNPC(NPC npc, Mission mission)
    {
        var indicator = new MissionIndicator(npc.Top, mission);
        indicator.TrackEntity(npc, new Vector2(0, -40));
        return indicator;
    }

    protected override void CustomUpdate()
    {
        if (IsHovering && hoverFadeIn < 1f)
        {
            hoverFadeIn += HOVER_FADE_SPEED;
            if (hoverFadeIn > 1f) hoverFadeIn = 1f;

            if (JustHovered)
                SoundEngine.PlaySound(SoundID.MenuTick);
        }
        else if (!IsHovering && hoverFadeIn > 0f)
        {
            hoverFadeIn -= HOVER_FADE_SPEED;
            if (hoverFadeIn < 0f) hoverFadeIn = 0f;
        }

        Offset = new Vector2(0, (float)Math.Sin(AnimationTimer) * bobAmount);
    }

    private void DrawMissionIndicator(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (iconTexture == null)
            return;

        // Get current zoom level
        var zoom = Main.GameViewMatrix.Zoom.X;

        var scale = (IsHovering ? 1.1f : 1f) * zoom;

        // Draw with glowing effect when hovering
        var glowColor = IsHovering ? Color.White : Color.White * 0.8f;

        // Draw the icon centered
        spriteBatch.Draw(
            iconTexture,
            screenPos,
            null,
            glowColor * opacity,
            0f,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            scale / zoom, // Divide by zoom to counteract the matrix scaling
            SpriteEffects.None,
            0f
        );

        // Draw detail panel if hovering
        if (IsHovering && hoverFadeIn > 0)
        {
            DrawDetailPanel(spriteBatch, screenPos, opacity * hoverFadeIn);
        }
    }

    private void DrawDetailPanel(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (mission == null)
            return;

        // Get current zoom level
        var zoom = Main.GameViewMatrix.Zoom.X;

        // Calculate panel size based on content
        var lineCount = 3; // Mission name, description, rewards
        if (mission.Objective.Count > 0)
        {
            lineCount += mission.Objective[0].Objectives.Count;
        }

        var panelHeight = 20 + lineCount * 20 + DetailPadding * 2;

        // Position panel to the right of the icon
        var panelX = screenPos.X + Width * zoom / 2 + 10;
        var panelY = screenPos.Y - panelHeight / 2;

        // Adjust if panel would go off screen
        if (panelX + DetailPanelWidth > Main.screenWidth)
        {
            panelX = screenPos.X - Width * zoom / 2 - DetailPanelWidth - 10;
        }

        if (panelY + panelHeight > Main.screenHeight)
        {
            panelY = Main.screenHeight - panelHeight - 10;
        }

        if (panelY < 10)
        {
            panelY = 10;
        }

        var panelRect = new Rectangle(
            (int)panelX,
            (int)panelY,
            DetailPanelWidth,
            panelHeight
        );

        // Draw panel border
        var borderColor = new Color(192, 151, 83, (int)(255 * opacity)); // Gold color
        var borderRect = new Rectangle(panelRect.X - 1, panelRect.Y - 1, panelRect.Width + 2, panelRect.Height + 2);

        // Draw mission details
        var textY = panelRect.Y + DetailPadding;

        // Mission name
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            mission.Name,
            panelRect.X + DetailPadding,
            textY,
            new Color(192, 151, 83, (int)(255 * opacity)), // Gold
            Color.Black * opacity,
            Vector2.Zero,
            1f
        );
        textY += 25;

        // Employer name
        var employerName = "Unknown";
        if (mission.Employer > 0)
        {
            employerName = Lang.GetNPCNameValue(mission.Employer);
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            $"From: {employerName}",
            panelRect.X + DetailPadding,
            textY,
            Color.White * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.9f
        );
        textY += 20;

        // Mission description
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            mission.Description,
            panelRect.X + DetailPadding,
            textY,
            Color.White * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.8f
        );
        textY += 30;

        // Draw objectives
        if (mission.Objective.Count > 0)
        {
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                "Objectives:",
                panelRect.X + DetailPadding,
                textY,
                Color.White * opacity,
                Color.Black * opacity,
                Vector2.Zero,
                0.8f
            );
            textY += 20;

            var firstSet = mission.Objective[0];
            foreach (var objective in firstSet.Objectives)
            {
                string objectiveText;
                if (objective.RequiredCount > 1)
                {
                    objectiveText = $"• {objective.Description} (0/{objective.RequiredCount})";
                }
                else
                {
                    objectiveText = $"• {objective.Description}";
                }

                Utils.DrawBorderStringFourWay(
                    spriteBatch,
                    FontAssets.MouseText.Value,
                    objectiveText,
                    panelRect.X + DetailPadding,
                    textY,
                    Color.White * opacity,
                    Color.Black * opacity,
                    Vector2.Zero,
                    0.75f
                );
                textY += 20;
            }
        }

        // Draw rewards
        if (mission.Rewards.Count > 0 || mission.Experience > 0)
        {
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                "Rewards:",
                panelRect.X + DetailPadding,
                textY,
                Color.White * opacity,
                Color.Black * opacity,
                Vector2.Zero,
                0.8f
            );
            textY += 20;

            // Draw item rewards
            var rewardX = panelRect.X + DetailPadding;
            foreach (var reward in mission.Rewards)
            {
                if (reward.type <= 0)
                    continue;

                // Draw item icon
                spriteBatch.Draw(
                    TextureAssets.Item[reward.type].Value,
                    new Vector2(rewardX, textY),
                    null,
                    Color.White * opacity,
                    0f,
                    Vector2.Zero,
                    0.8f,
                    SpriteEffects.None,
                    0f
                );

                // Draw item count if more than 1
                if (reward.stack > 1)
                {
                    Utils.DrawBorderStringFourWay(
                        spriteBatch,
                        FontAssets.ItemStack.Value,
                        reward.stack.ToString(),
                        rewardX + 10,
                        textY + 10,
                        Color.White * opacity,
                        Color.Black * opacity,
                        Vector2.Zero,
                        0.8f
                    );
                }

                rewardX += 40;
            }

            // Draw experience reward
            if (mission.Experience > 0)
            {
                Utils.DrawBorderStringFourWay(
                    spriteBatch,
                    FontAssets.MouseText.Value,
                    $"{mission.Experience} XP",
                    rewardX,
                    textY,
                    new Color(73, 213, 255, (int)(255 * opacity)), // Light blue for XP
                    Color.Black * opacity,
                    Vector2.Zero,
                    0.8f
                );
            }
        }

        // Draw "Click to accept" at the bottom
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            "Click icon to accept",
            panelRect.X + DetailPanelWidth / 2,
            panelRect.Y + panelHeight - DetailPadding - 5,
            Color.Yellow * opacity,
            Color.Black * opacity,
            new Vector2(0.5f, 0), // Center horizontally
            0.8f
        );
    }

    private void HandleClick()
    {
        // Play sound
        SoundEngine.PlaySound(SoundID.MenuOpen);

        // Accept the mission
        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        missionPlayer.StartMission(mission.ID);

        // Hide this indicator
        IsVisible = false;
    }
}

/// <summary>
/// Manages the creation and tracking of mission indicators in the world
/// </summary>
public class MissionIndicatorManager : ModSystem
{
    private static MissionIndicatorManager _instance;
    public static MissionIndicatorManager Instance => _instance;

    // Collection of all active mission indicators
    private List<MissionIndicator> indicators = new List<MissionIndicator>();

    // Track which NPCs have indicators
    private Dictionary<int, MissionIndicator> npcIndicators = new Dictionary<int, MissionIndicator>();
    private Dictionary<int, int> npcMissionTracking = new Dictionary<int, int>();

    public override void Load()
    {
        _instance = this;
    }

    public override void Unload()
    {
        _instance = null;
        indicators.Clear();
        npcIndicators.Clear();
    }

    /// <summary>
    /// Creates a mission indicator at a specific world position
    /// </summary>
    public MissionIndicator CreateIndicator(Vector2 worldPosition, Mission mission)
    {
        var indicator = new MissionIndicator(worldPosition, mission);
        indicators.Add(indicator);
        return indicator;
    }

    /// <summary>
    /// Creates a mission indicator for an NPC
    /// </summary>
    public MissionIndicator CreateIndicatorForNPC(NPC npc, Mission mission)
    {
        int npcIndex = npc.whoAmI;

        // Check if this NPC already has an indicator for this mission
        if (npcIndicators.ContainsKey(npcIndex))
        {
            // If the NPC has an indicator but for a different mission, update it
            if (npcMissionTracking.ContainsKey(npcIndex) && npcMissionTracking[npcIndex] != mission.ID)
            {
                // Remove old indicator
                var oldIndicator = npcIndicators[npcIndex];
                indicators.Remove(oldIndicator);
                npcIndicators.Remove(npcIndex);

                // Create new one
                var indicator = MissionIndicator.CreateForNPC(npc, mission);
                indicators.Add(indicator);
                npcIndicators[npcIndex] = indicator;
                npcMissionTracking[npcIndex] = mission.ID;
                return indicator;
            }

            // Return existing indicator
            return npcIndicators[npcIndex];
        }

        // Create new indicator
        var newIndicator = MissionIndicator.CreateForNPC(npc, mission);
        indicators.Add(newIndicator);
        npcIndicators[npcIndex] = newIndicator;
        npcMissionTracking[npcIndex] = mission.ID;

        return newIndicator;
    }

    public void ClearAllNotifications()
    {
        indicators.Clear();
        npcIndicators.Clear();
        npcMissionTracking.Clear();
    }

    public override void PostUpdateEverything()
    {
        // Update all indicators
        for (var i = indicators.Count - 1; i >= 0; i--)
        {
            indicators[i].Update();

            // Remove hidden indicators
            if (!indicators[i].IsVisible)
            {
                // Also remove from NPC tracking if applicable
                foreach (var kvp in new Dictionary<int, MissionIndicator>(npcIndicators))
                {
                    if (kvp.Value == indicators[i])
                    {
                        npcIndicators.Remove(kvp.Key);
                        break;
                    }
                }

                indicators.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Draws all mission indicators
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var indicator in indicators)
        {
            indicator.Draw(spriteBatch);
        }
    }
}