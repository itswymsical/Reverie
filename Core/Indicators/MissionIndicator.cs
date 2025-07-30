using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using Terraria.GameContent;

namespace Reverie.Core.Indicators;

public class MissionIndicator : ScreenIndicator
{
    private readonly Mission mission;
    private readonly Texture2D iconTexture;

    private const int PANEL_WIDTH = 220;
    private const int PADDING = 10;

    public static bool ShowDebugHitbox = false;

    public override AnimationType AnimationStyle => AnimationType.Wag;

    public MissionIndicator(Vector2 worldPosition, Mission mission, AnimationType? animationType = null)
         : base(worldPosition, 48, 48, animationType)
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

    public static MissionIndicator CreateForNPC(NPC npc, Mission mission, AnimationType? animationType = null)
    {
        var indicator = new MissionIndicator(npc.Top, mission, animationType);
        indicator.TrackEntity(npc, new Vector2(0, -40));
        return indicator;
    }

    protected override void PostUpdate() { }

    private void DrawIndicator(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (ShowDebugHitbox)
        {
            DrawDebugHitbox(spriteBatch, screenPos, opacity);
        }

        var scale = GetAnimationScale();
        var glowColor = IsHovering ? Color.White : Color.White * 0.8f;
        var rotation = GetAnimationRotation();
        spriteBatch.Draw(
            iconTexture,
            screenPos,
            null,
            glowColor * opacity,
            rotation,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            scale,
            SpriteEffects.None,
            0f
        );

        if (IsHovering)
        {
            DrawPanel(spriteBatch, opacity * GetHoverOpacity());
        }
    }

    private void DrawDebugHitbox(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        var zoom = Main.GameViewMatrix.Zoom.X;
        var scaledWidth = (int)(Width * zoom);
        var scaledHeight = (int)(Height * zoom);

        var hitboxRect = new Rectangle(
            (int)screenPos.X - scaledWidth / 2,
            (int)screenPos.Y - scaledHeight / 2,
            scaledWidth,
            scaledHeight
        );

        var pixel = TextureAssets.MagicPixel.Value;
        var borderColor = IsHovering ? Color.Lime : Color.Red;
        borderColor *= opacity * 0.8f;

        // Draw border lines
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Bottom - 2, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, 2, hitboxRect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.Right - 2, hitboxRect.Y, 2, hitboxRect.Height), borderColor);

        var fillColor = IsHovering ? Color.Lime : Color.Red;
        fillColor *= opacity * 0.2f;
        spriteBatch.Draw(pixel, hitboxRect, fillColor);
    }

    private void DrawPanel(SpriteBatch spriteBatch, float opacity)
    {
        if (mission == null)
            return;

        // Get proper screen position for UI panel positioning
        var screenPos = GetScreenPosition();

        var lineCount = 3;
        if (mission.Objective.Count > 0)
        {
            lineCount += mission.Objective[0].Objectives.Count;
        }

        var panelHeight = 20 + lineCount * 20 + PADDING * 2;

        // Position panel to the right of the icon
        var panelX = screenPos.X + Width / 2 + 10;
        var panelY = screenPos.Y - panelHeight / 2;

        // Adjust if panel would go off screen
        if (panelX + PANEL_WIDTH > Main.screenWidth)
        {
            panelX = screenPos.X - Width / 2 - PANEL_WIDTH - 10;
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
            PANEL_WIDTH,
            panelHeight
        );

        var textY = panelRect.Y + PADDING;

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
        var employerName = "Unknown";
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
            var rewardX = panelRect.X + PADDING;
            foreach (var reward in mission.Rewards)
            {
                if (reward.type <= 0)
                    continue;

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