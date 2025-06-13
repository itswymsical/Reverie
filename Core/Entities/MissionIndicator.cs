using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using Reverie.Utilities;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Core.Entities;

/// <summary>
/// An indicator of missions, appears in the world and shows mission details
/// </summary>
public class MissionIndicator : UIEntity
{
    private readonly Mission mission;

    private readonly Texture2D iconTexture;

    private float hoverFadeIn = 0f;
    private const float HOVER_FADE_SPEED = 0.1f;

    private const int PANEL_WIDTH = 220;
    private const int PADDING = 10;

    private readonly float bobAmount = (float)Math.PI;

    public static bool ShowDebugHitbox = false;

    public MissionIndicator(Vector2 worldPosition, Mission mission)
         : base(worldPosition, 48, 48)
    {
        this.mission = mission;

        iconTexture = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Missions/Indicator", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        if (iconTexture != null)
        {
            Width = iconTexture.Width;
            Height = iconTexture.Height;
        }

        OnDrawWorld = DrawIndicator;

        OnClick += HandleClick;
    }

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


    private void DrawIndicator(SpriteBatch spriteBatch, Vector2 worldPos, float opacity)
    {
        if (iconTexture == null)
            return;

        var scale = IsHovering ? 1.1f : 1f;
        var glowColor = IsHovering ? Color.White : Color.White * 0.8f;

        spriteBatch.Draw(
            iconTexture,
            worldPos,
            null,
            glowColor * opacity,
            0f,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            scale,
            SpriteEffects.None,
            0f
        );

        if (ShowDebugHitbox)
        {
            DrawDebugHitbox(spriteBatch, worldPos, opacity);
        }

        if (IsHovering)
        {
            DrawPanel(spriteBatch, worldPos, opacity * hoverFadeIn);
        }
    }

    private void DrawDebugHitbox(SpriteBatch spriteBatch, Vector2 worldPos, float opacity)
    {
        // Don't apply zoom to the hitbox dimensions - use the original Width/Height
        var scaledWidth = Width;
        var scaledHeight = Height;

        // Calculate hitbox rectangle centered on worldPos (drawing in world space)
        var hitboxRect = new Rectangle(
            (int)worldPos.X - scaledWidth / 2,
            (int)worldPos.Y - scaledHeight / 2,
            scaledWidth,
            scaledHeight
        );

        // Create a 1x1 white pixel texture for drawing rectangles
        var pixel = TextureAssets.MagicPixel.Value;

        // Draw hitbox border (red for normal, green for hovering)
        var borderColor = IsHovering ? Color.Lime : Color.Red;
        borderColor *= opacity * 0.8f;

        // Draw border lines (top, bottom, left, right)
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Bottom - 2, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, 2, hitboxRect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.Right - 2, hitboxRect.Y, 2, hitboxRect.Height), borderColor);

        // Draw semi-transparent fill
        var fillColor = IsHovering ? Color.Lime : Color.Red;
        fillColor *= opacity * 0.2f;
        spriteBatch.Draw(pixel, hitboxRect, fillColor);

        // Enhanced debug info to track coordinate issues
        var zoom = Main.GameViewMatrix.Zoom.Y;
        var screenPos = WorldToScreen(worldPos);

        // Get additional debug info
        var baseWorldPos = WorldPosition; // The stored world position
        var currentOffset = Offset; // Current animation offset
        var trackingInfo = "";
        if (trackingEntity != null)
        {
            if (trackingEntity is NPC npc)
            {
                trackingInfo = $"NPC.Top: {npc.Top}\nTrackOffset: {trackingOffset}\nNPC.Pos: {npc.position}";
            }
        }

        var debugText = $"BaseWorldPos: {baseWorldPos}\nCurrentOffset: {currentOffset}\nFinalWorldPos: {worldPos}\nScreenPos: {screenPos}\nScreenPosition: {Main.screenPosition}\nZoom: {zoom:F2}\nMouse: {Main.MouseScreen}\nHover: {IsHovering}\n{trackingInfo}";

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            debugText,
            hitboxRect.X,
            hitboxRect.Y - 200,
            Color.Yellow * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.6f
        );

        // Draw center crosshair
        var centerX = hitboxRect.X + hitboxRect.Width / 2;
        var centerY = hitboxRect.Y + hitboxRect.Height / 2;
        spriteBatch.Draw(pixel, new Rectangle(centerX - 5, centerY - 1, 10, 2), Color.White * opacity);
        spriteBatch.Draw(pixel, new Rectangle(centerX - 1, centerY - 5, 2, 10), Color.White * opacity);

        // Helper method for WorldToScreen conversion
        Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    private void DrawPanel(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (mission == null)
            return;

        float zoom = Main.GameViewMatrix.Zoom.X;

        int lineCount = 3;
        if (mission.Objective.Count > 0)
        {
            lineCount += mission.Objective[0].Objectives.Count;
        }

        int panelHeight = 20 + (lineCount * 20) + PADDING * 2;

        // Position panel to the right of the icon
        float panelX = screenPos.X + (Width * zoom / 2) + 10;
        float panelY = screenPos.Y - (panelHeight / 2);

        // Adjust if panel would go off screen
        if (panelX + PANEL_WIDTH > Main.screenWidth)
        {
            panelX = screenPos.X - (Width * zoom / 2) - PANEL_WIDTH - 10;
        }

        if (panelY + panelHeight > Main.screenHeight)
        {
            panelY = Main.screenHeight - panelHeight - 10;
        }

        if (panelY < 10)
        {
            panelY = 10;
        }

        Rectangle panelRect = new Rectangle(
            (int)panelX,
            (int)panelY,
            PANEL_WIDTH,
            panelHeight
        );

        // Draw mission details
        int textY = panelRect.Y + PADDING;

        // Mission name
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            mission.Name,
            panelRect.X + PADDING,
            textY,
            new Color(192, 151, 83, (int)(255 * opacity)), // Gold
            Color.Black * opacity,
            Vector2.Zero,
            1f
        );
        textY += 25;

        // Employer name
        string employerName = "Unknown";
        if (mission.ProviderNPC > 0)
        {
            employerName = Lang.GetNPCNameValue(mission.ProviderNPC);
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            $"From: {employerName}",
            panelRect.X + PADDING,
            textY,
            Color.White * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.9f
        );
        textY += 24;

        // Mission description
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            mission.Description,
            panelRect.X + PADDING,
            textY,
            Color.White * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.8f
        );
        textY += 28;

        // Draw rewards
        if (mission.Rewards.Count > 0 || mission.Experience > 0)
        {
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                "Rewards:",
                panelRect.X + PADDING,
                textY,
                Color.White * opacity,
                Color.Black * opacity,
                Vector2.Zero,
                0.8f
            );
            textY += 20;

            // Draw item rewards
            int rewardX = panelRect.X + PADDING;
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

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            "Click to accept",
            panelRect.X + 4,
            panelRect.Y + panelHeight + 22,
            Color.Yellow * opacity,
            Color.Black * opacity,
            default,
            0.8f
        );
    }

    private void HandleClick()
    {
        try
        {
            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            missionPlayer.StartMission(mission.ID);
            IsVisible = false;
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error starting mission: {ex.Message}");
        }
    }
}