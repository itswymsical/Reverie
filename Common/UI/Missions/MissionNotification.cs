using System.Collections.Generic;

using Terraria.UI;
using Terraria.Audio;
using Terraria.GameContent;

using Reverie.Core.Missions;
using Reverie.Utilities;

namespace Reverie.Common.UI.Missions;
public class MissionNotification : IInGameNotification
{

    private bool isFadingOut = false;
    private float fadeoutProgress = 0f;
    private float fadeInProgress = 0f;
    private float alpha = 0f;
    private const float FADE_IN_SPEED = 0.03f;
    private const float FADE_OUT_SPEED = 0.02f;

    private readonly Mission currentMission;
    private List<Objective> activeObjectives;

    private Texture2D iconTexture;

    private const string EMPTY_CHECKBOX = "☐";
    private const string CHECKED_CHECKBOX = "☑";

    private const int TitlePanelWidth = 180;
    private const int PanelHeight = 34;
    private const int TextPadding = 10;

    public bool ShouldBeRemoved => isFadingOut && fadeoutProgress >= 1.0f;

    public MissionNotification(Mission mission)
    {
        currentMission = mission;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/ObjectiveInfo").Value;

        activeObjectives = new List<Objective>();
        if (mission.ObjectiveIndex.Count > 0 && mission.CurrentIndex < mission.ObjectiveIndex.Count)
        {
            var currentSet = mission.ObjectiveIndex[mission.CurrentIndex];
            foreach (var objective in currentSet.Objectives)
            {
                activeObjectives.Add(objective);
            }
        }

        fadeInProgress = 0f;
        fadeoutProgress = 0f;
        isFadingOut = false;

        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (isFadingOut)
        {
            alpha = 1.0f - fadeoutProgress;
        }
        else
        {
            alpha = fadeInProgress;
        }

        alpha = MathHelper.Clamp(alpha, 0f, 1f);

        DrawObjectivePanel(spriteBatch, bottomAnchorPosition);
    }

    private void DrawObjectivePanel(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (iconTexture == null || activeObjectives.Count == 0)
            return;

        int objCount = activeObjectives.Count;
        int totalHeight = PanelHeight +
                              (PanelHeight * objCount);

        int posX = Main.screenWidth - TitlePanelWidth - 200;
        int posY = 386;

        Rectangle panelRect = new Rectangle(posX, posY, TitlePanelWidth, totalHeight);
        Color panelColor = new Color(83, 85, 192, (int)(225 * alpha));

        //psuedo draw additive effect
        DrawUtils.DrawPanel(spriteBatch, panelRect, panelColor);
        DrawUtils.DrawPanel(spriteBatch, panelRect, panelColor);

        Vector2 titlePos = new Vector2(posX, posY + PanelHeight / (float)Math.PI);

        Vector2 iconPos = new Vector2(posX + 16, posY + PanelHeight / (float)Math.PI + 10);
        spriteBatch.Draw(
            iconTexture,
            iconPos,
            null,
            Color.White * alpha,
            0f,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            1f,
            SpriteEffects.None,
            0f
        );

        string missionName = currentMission.Name;
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            missionName,
            titlePos.X + 22,
            titlePos.Y,
            Color.White * alpha,
            Color.Black * alpha,
            Vector2.Zero,
            1f
        );

        int yOffset = PanelHeight;

        for (int i = 0; i < activeObjectives.Count; i++)
        {
            var objective = activeObjectives[i];

            string status = objective.IsCompleted ? CHECKED_CHECKBOX : EMPTY_CHECKBOX;
            string objectiveText;

            if (objective.RequiredCount > 1)
            {
                objectiveText = $"{status} {objective.Description} [{objective.CurrentCount}/{objective.RequiredCount}]";
            }
            else
            {
                objectiveText = $"{status} {objective.Description}";
            }

            Vector2 textPos = new Vector2(posX + TextPadding, posY + yOffset + PanelHeight / (float)Math.PI);

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                objectiveText,
                textPos.X,
                textPos.Y,
                objective.IsCompleted ? new Color(150, 255, 150, (int)(255 * alpha)) : Color.White * alpha,
                Color.Black * alpha,
                Vector2.Zero,
                0.65f
            );

            yOffset += PanelHeight - 13;
        }
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom)
    {

    }

    public void Update()
    {
        bool isInventoryOpen = Main.playerInventory;

        if (isInventoryOpen)
        {
            isFadingOut = false;
            fadeInProgress += FADE_IN_SPEED;
            fadeInProgress = Math.Min(fadeInProgress, 1.0f);
        }
        else if (!isFadingOut)
        {
            isFadingOut = true;
            fadeoutProgress = 0f;
        }

        if (isFadingOut)
        {
            fadeoutProgress += FADE_OUT_SPEED;
            fadeoutProgress = Math.Min(fadeoutProgress, 1.0f);
        }
    }

    public void StartFadeOut()
    {
        isFadingOut = true;
        fadeoutProgress = 0f;
    }
}