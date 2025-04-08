using Reverie.Core.Missions;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Core.CustomEntities;

/// <summary>
/// A mission indicator that appears in the world and shows mission details on hover
/// </summary>
public class MissionIndicator : WorldUIEntity
{
    private readonly Mission mission;

    private readonly Texture2D iconTexture;
    private float hoverFadeIn = 0f;
    private const float HOVER_FADE_SPEED = 0.1f;

    private const int PANEL_WIDTH = 220;
    private const int PADDING = 10;

    private readonly float bobAmount = (float)Math.PI;

    public MissionIndicator(Vector2 worldPosition, Mission mission): base(worldPosition, 48, 48)
    {
        this.mission = mission;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;

        if (iconTexture != null)
        {
            Width = iconTexture.Width;
            Height = iconTexture.Height;
        }

        CustomDraw = DrawIndicator;
        OnClick += HandleClick;
    }

    public MissionIndicator(NPC npc, Mission mission, Vector2? offset = null) : base(npc, offset ?? new Vector2(0, -40), 32, 32)
    {
        this.mission = mission;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;

        if (iconTexture != null)
        {
            Width = iconTexture.Width;
            Height = iconTexture.Height;
        }

        CustomDraw = DrawIndicator;
        OnClick += HandleClick;
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

    private void DrawIndicator(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (iconTexture == null)
            return;

        screenPos += Offset;

        var scale = IsHovering ? 1.1f : 1f;

        var glowColor = IsHovering ? Color.White : Color.White * 0.8f;

        spriteBatch.Draw(
            iconTexture,
            screenPos + new Vector2(Width / 2, Height / 2),
            null,
            glowColor * opacity,
            0f,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
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

        var lineCount = 3;
        if (mission.Objective.Count > 0)
        {
            lineCount += mission.Objective[0].Objectives.Count;
        }

        var panelHeight = 20 + lineCount * 20 + PADDING * 2;

        var panelX = screenPos.X + Width + 5;
        var panelY = screenPos.Y;

        if (panelX + PANEL_WIDTH > Main.screenWidth)
        {
            panelX = screenPos.X - PANEL_WIDTH - 5;
        }

        if (panelY + panelHeight > Main.screenHeight)
        {
            panelY = Main.screenHeight - panelHeight;
        }

        var panelRect = new Rectangle(
            (int)panelX,
            (int)panelY,
            PANEL_WIDTH,
            panelHeight
        );

        var textY = panelRect.Y + PADDING;

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

        textY += 20;

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
        textY += 30;

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            "Click to accept",
            panelRect.X + PANEL_WIDTH / 2,
            panelRect.Y + panelHeight - PADDING - 5,
            Color.Yellow * opacity,
            Color.Black * opacity,
            new Vector2(0.5f, 0f),
            0.8f
        );
    }

    private void HandleClick()
    {
        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        missionPlayer.StartMission(mission.ID);
        IsVisible = false;
    }
}
