using Reverie.Core.Missions;
using Reverie.Utilities;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Core.Entities;

/// <summary>
/// A mission indicator that appears in the world and shows mission details on hover
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

        OnDraw = DrawIndicator;

        OnClick += HandleClick;
    }

    public static MissionIndicator CreateForNPC(NPC npc, Mission mission)
    {
        var indicator = new MissionIndicator(npc.Top, mission);
        indicator.TrackEntity(npc, new Vector2(0, -40)); // Position above NPC's head
        return indicator;
    }

    protected override void CustomUpdate()
    {
        // Only proceed with normal update if not loading texture
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

    private void DrawIndicator(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (iconTexture == null)
            return;
        

        var scale = IsHovering ? 1.1f : 1f;

        var glowColor = IsHovering ? Color.White : Color.White * 0.8f;

        spriteBatch.Draw(
            iconTexture,
            screenPos + new Vector2(Width / 2, Height / 2),
            null,
            glowColor * opacity,
            0f,
            new Vector2(iconTexture.Width, iconTexture.Height),
            scale,
            SpriteEffects.None,
            0f
        );

        if (IsHovering)
        {
            DrawPanel(spriteBatch, screenPos, opacity * hoverFadeIn);
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