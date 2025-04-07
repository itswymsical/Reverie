using Reverie.Core.Missions;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Common.UI.Missions;

/// <summary>
/// Displays a notification above an NPC's head when they have an available mission
/// Shows mission details on hover and allows clicking to accept the mission
/// </summary>
public class NPCMissionNotification : IInGameNotification
{
    private readonly NPC targetNPC;
    private readonly Mission availableMission;
    private readonly int missionId;
    private readonly Vector2 positionAnchor;

    private bool isHovering = false;
    private bool clicked = false;

    private float hoverFadeIn = 0f;
    private const float HOVER_FADE_SPEED = 0.1f;

    private float pulseAnimation = 0f;
    private float hoverOffsetY = 0f;

    private Texture2D missionIconTexture;

    private const int DetailPanelWidth = 220;
    private const int DetailPanelPadding = 10;

    public bool ShouldBeRemoved => targetNPC == null || !targetNPC.active || clicked;

    public NPCMissionNotification(NPC npc, Mission mission, Vector2 position)
    {
        targetNPC = npc;
        availableMission = mission;
        missionId = mission.ID;
        positionAnchor = position;

        missionIconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (targetNPC == null || !targetNPC.active || availableMission == null)
            return;

        pulseAnimation = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 4f;

        DrawMissionIcon(spriteBatch);

        if (isHovering)
        {
            DrawMissionDetailPanel(spriteBatch);
        }
    }

    private void DrawMissionIcon(SpriteBatch spriteBatch)
    {
        if (missionIconTexture == null)
            return;

        hoverOffsetY = pulseAnimation;
        var iconPos = targetNPC.Center + new Vector2((-missionIconTexture.Width / 2f) * targetNPC.width, -missionIconTexture.Height - 16f - hoverOffsetY);
        var drawPos = Vector2.Transform(iconPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);

        Rectangle hoverRect = new Rectangle(
            (int)drawPos.X,
            (int)drawPos.Y,
            missionIconTexture.Width,
            missionIconTexture.Height
        );

        bool wasHovering = isHovering;
        isHovering = hoverRect.Contains(Main.MouseScreen.ToPoint());

        if (!wasHovering && isHovering)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        if (isHovering && hoverFadeIn < 1f)
        {
            hoverFadeIn += HOVER_FADE_SPEED;
            if (hoverFadeIn > 1f) hoverFadeIn = 1f;
        }
        else if (!isHovering && hoverFadeIn > 0f)
        {
            hoverFadeIn -= HOVER_FADE_SPEED;
            if (hoverFadeIn < 0f) hoverFadeIn = 0f;
        }

        float scale = isHovering ? 1.1f : 1f;

        Color glowColor = isHovering ? Color.White : Color.White * 0.8f;

        spriteBatch.Draw(
            missionIconTexture,
            drawPos + new Vector2(missionIconTexture.Width / 2f, missionIconTexture.Height / 2f),
            null,
            glowColor,
            0f,
            new Vector2(missionIconTexture.Width / 2f, missionIconTexture.Height / 2f),
            scale,
            SpriteEffects.None,
            0f
        );

        if (isHovering && Main.mouseLeft && Main.mouseLeftRelease)
        {
            HandleIconClicked();
        }
    }

    private void DrawMissionDetailPanel(SpriteBatch spriteBatch)
    {
        if (availableMission == null)
            return;

        int lineCount = 3;
        if (availableMission.Objective.Count > 0)
        {
            lineCount += availableMission.Objective[0].Objectives.Count;
        }

        int panelHeight = 20 + (lineCount * 20) + DetailPanelPadding * 2;

        var iconPos = positionAnchor + new Vector2(-missionIconTexture.Width / 2f, -missionIconTexture.Height - 10f - hoverOffsetY);
        var screenPos = Vector2.Transform(iconPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);

        float panelX = screenPos.X + missionIconTexture.Width + 5;
        float panelY = screenPos.Y;

        if (panelX + DetailPanelWidth > Main.screenWidth)
        {
            panelX = screenPos.X - DetailPanelWidth - 5;
        }

        if (panelY + panelHeight > Main.screenHeight)
        {
            panelY = Main.screenHeight - panelHeight;
        }

        Rectangle panelRect = new Rectangle(
            (int)panelX,
            (int)panelY,
            DetailPanelWidth,
            panelHeight
        );

        Color panelColor = new Color(0, 0, 0, (int)(200 * hoverFadeIn));

        int textY = panelRect.Y + DetailPanelPadding;

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            availableMission.Name,
            panelRect.X + DetailPanelPadding,
            textY,
            new Color(192, 151, 83, (int)(255 * hoverFadeIn)), // Gold
            Color.Black * hoverFadeIn,
            Vector2.Zero,
            1f
        );
        textY += 25;

        string employerName = "Unknown";
        if (availableMission.Employer > 0)
        {
            employerName = Lang.GetNPCNameValue(availableMission.Employer);
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            $"From: {employerName}",
            panelRect.X + DetailPanelPadding,
            textY,
            Color.White * hoverFadeIn,
            Color.Black * hoverFadeIn,
            Vector2.Zero,
            0.9f
        );
        textY += 20;

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            availableMission.Description,
            panelRect.X + DetailPanelPadding,
            textY,
            Color.White * hoverFadeIn,
            Color.Black * hoverFadeIn,
            Vector2.Zero,
            0.8f
        );
        textY += 30;

        if (availableMission.Rewards.Count > 0 || availableMission.Experience > 0)
        {
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                "Rewards:",
                panelRect.X + DetailPanelPadding,
                textY + 22,
                Color.White * hoverFadeIn,
                Color.Black * hoverFadeIn,
                Vector2.Zero,
                0.8f
            );
            textY += 20;

            int rewardX = panelRect.X + DetailPanelPadding;
            foreach (var reward in availableMission.Rewards)
            {
                if (reward.type <= 0)
                    continue;

                spriteBatch.Draw(
                    TextureAssets.Item[reward.type].Value,
                    new Vector2(rewardX, textY + 22),
                    null,
                    Color.White * hoverFadeIn,
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
                        textY + 28,
                        Color.White * hoverFadeIn,
                        Color.Black * hoverFadeIn,
                        Vector2.Zero,
                        0.8f
                    );
                }

                rewardX += 40;
            }

            if (availableMission.Experience > 0)
            {
                Utils.DrawBorderStringFourWay(
                    spriteBatch,
                    FontAssets.MouseText.Value,
                    $"{availableMission.Experience} XP",
                    rewardX,
                    textY + 22,
                    new Color(73, 213, 255, (int)(255 * hoverFadeIn)), // Light blue for XP
                    Color.Black * hoverFadeIn,
                    Vector2.Zero,
                    0.8f
                );
            }

            textY += 30;
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            "Click to accept",
            panelRect.X + DetailPanelWidth,
            panelRect.Y + panelHeight + 24,
            Color.Yellow * hoverFadeIn,
            Color.Black * hoverFadeIn,
            new Vector2(0.5f, 0f),
            0.8f
        );

        if (Main.mouseLeft && Main.mouseLeftRelease && panelRect.Contains(Main.MouseScreen.ToPoint()))
        {
            HandleIconClicked();
        }
    }

    private void HandleIconClicked()
    {
        SoundEngine.PlaySound(SoundID.MenuOpen);

        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        missionPlayer.StartMission(missionId);

        clicked = true;
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom)
    {
    }

    public void Update()
    {
    }

    public void StartFadeOut()
    {
    }
}
