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

    private Mission currentMission;
    private List<Objective> activeObjectives;

    private Texture2D iconTexture;
    private Texture2D nextButtonTexture;
    private Texture2D prevButtonTexture;

    private const string EMPTY_CHECKBOX = "☐";
    private const string CHECKED_CHECKBOX = "☑";

    private const int TitlePanelWidth = 180;
    private const int PanelHeight = 34;
    private const int TextPadding = 10;
    private const int ButtonSize = 16;

    // For mission cycling
    private List<Mission> activeMissions;
    private List<Mission> availableMissions;
    private int currentMissionIndex = 0;
    private bool showingAvailableMissions = false;
    public bool ShouldBeRemoved => false;

    public MissionNotification(Mission mission)
    {
        currentMission = mission;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;
        nextButtonTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Dialogue/ArrowForward").Value;
        prevButtonTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Dialogue/ArrowBack").Value;

        LoadMissions();
        UpdateActiveObjectives();

        fadeInProgress = 0f;
        fadeoutProgress = 0f;
        isFadingOut = false;

        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private void LoadMissions()
    {
        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        activeMissions = new List<Mission>(missionPlayer.ActiveMissions());
        availableMissions = new List<Mission>(missionPlayer.AvailableMissions());

        if ((showingAvailableMissions && availableMissions.Count == 0 && activeMissions.Count > 0) ||
            (!showingAvailableMissions && activeMissions.Count == 0 && availableMissions.Count > 0))
        {
            showingAvailableMissions = !showingAvailableMissions;
        }

        var currentList = showingAvailableMissions ? availableMissions : activeMissions;

        if (currentList.Count == 0)
            return;

        if (currentMission == null || !currentList.Contains(currentMission))
        {
            currentMission = currentList[0];
            currentMissionIndex = 0;
        }
        else
        {
            currentMissionIndex = currentList.FindIndex(m => m.ID == currentMission.ID);
            if (currentMissionIndex < 0 && currentList.Count > 0)
                currentMissionIndex = 0;
        }
    }

    private void UpdateActiveObjectives()
    {
        activeObjectives = new List<Objective>();
        if (currentMission != null &&
            currentMission.Objective.Count > 0 &&
            currentMission.CurrentIndex < currentMission.Objective.Count)
        {
            var currentSet = currentMission.Objective[currentMission.CurrentIndex];
            foreach (var objective in currentSet.Objectives)
            {
                activeObjectives.Add(objective);
            }
        }
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

        DrawObjectiveList(spriteBatch, bottomAnchorPosition);
    }

    private void DrawObjectiveList(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        var currentList = showingAvailableMissions ? availableMissions : activeMissions;

        if (iconTexture == null || currentMission == null || currentList.Count == 0)
            return;

        if (!showingAvailableMissions && (activeObjectives == null || activeObjectives.Count == 0))
            return;

        int panelObjectCount;
        if (showingAvailableMissions)
        {
            panelObjectCount = 1;
        }
        else
        {
            panelObjectCount = activeObjectives.Count;
        }

        int totalHeight = PanelHeight + (PanelHeight * panelObjectCount);

        int posX = Main.screenWidth - TitlePanelWidth - 220;
        int posY = 386;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;
        nextButtonTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Dialogue/ArrowForward").Value;
        prevButtonTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Dialogue/ArrowBack").Value;

        Rectangle panelRect = new Rectangle(posX, posY, TitlePanelWidth, totalHeight);

        Vector2 titlePos = new Vector2(posX, posY + PanelHeight / (float)Math.PI);

        Vector2 iconPos = new Vector2(posX + 16, posY + PanelHeight / (float)Math.PI + 10);
        spriteBatch.Draw(
            iconTexture,
            iconPos,
            null,
            Color.White * alpha,
            0f,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            0.7f,
            SpriteEffects.None,
            0f
        );

        string missionTitle;
        if (showingAvailableMissions)
        {
            missionTitle = "Job Opportunities";
        }
        else
        {
            missionTitle = currentMission.Name;
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            missionTitle,
            titlePos.X + 22,
            titlePos.Y,
            Color.White * alpha,
            Color.Black * alpha,
            Vector2.Zero,
            1f
        );

        bool hasMultipleMissions = currentList.Count > 1;
        bool hasOtherMissionType = (showingAvailableMissions && activeMissions.Count > 0) ||
                                 (!showingAvailableMissions && availableMissions.Count > 0);

        if (hasOtherMissionType)
        {
            Vector2 toggleButtonPos = new Vector2(posX + TitlePanelWidth - ButtonSize * 3 - 10, posY + PanelHeight / (float)Math.PI + 10);
            Rectangle toggleButtonRect = new Rectangle((int)toggleButtonPos.X, (int)toggleButtonPos.Y, ButtonSize, ButtonSize);

            Texture2D toggleTexture = iconTexture;

            spriteBatch.Draw(
                toggleTexture,
                toggleButtonPos,
                null,
                Color.White * alpha,
                0f,
                new Vector2(toggleTexture.Width / 2, toggleTexture.Height / 2),
                0.5f,
                SpriteEffects.None,
                0f
            );

            if (Main.mouseLeft && Main.mouseLeftRelease && alpha > 0.9f && toggleButtonRect.Contains(Main.MouseScreen.ToPoint()))
            {
                showingAvailableMissions = !showingAvailableMissions;
                LoadMissions();
                UpdateActiveObjectives();
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }

        if (hasMultipleMissions)
        {
            Vector2 prevButtonPos = new Vector2(posX + TitlePanelWidth - ButtonSize * 2 - 5, posY + PanelHeight * (float)Math.PI + 20);
            Rectangle prevButtonRect = new Rectangle((int)prevButtonPos.X - 8, (int)prevButtonPos.Y - 6, ButtonSize, 16);

            spriteBatch.Draw(
                prevButtonTexture,
                prevButtonPos,
                null,
                Color.White * alpha,
                0f,
                new Vector2(prevButtonTexture.Width / 2, prevButtonTexture.Height / 2),
                0.6f,
                SpriteEffects.None,
                0f
            );

            Vector2 nextButtonPos = new Vector2(posX + TitlePanelWidth - ButtonSize - 5, posY + PanelHeight * (float)Math.PI + 20);
            Rectangle nextButtonRect = new Rectangle((int)nextButtonPos.X - 8, (int)nextButtonPos.Y - 6, ButtonSize, 16);

            spriteBatch.Draw(
                nextButtonTexture,
                nextButtonPos,
                null,
                Color.White * alpha,
                0f,
                new Vector2(nextButtonTexture.Width / 2, nextButtonTexture.Height / 2),
                0.6f,
                SpriteEffects.None,
                0f
            );

            if (Main.mouseLeft && Main.mouseLeftRelease && alpha > 0.9f)
            {
                if (prevButtonRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    CycleToPreviousMission();
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
                else if (nextButtonRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    CycleToNextMission();
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
            }
        }

        int yOffset = PanelHeight;

        if (showingAvailableMissions)
        {
            string employer = "Unknown";
            if (currentMission.Employer > 0)
            {
                employer = $"{currentMission.Name} | {currentMission.Employer}";
            }

            Vector2 textPos = new Vector2(posX + TextPadding, posY + yOffset + PanelHeight / (float)Math.PI);

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                employer,
                textPos.X,
                textPos.Y,
                Color.Gold * alpha,
                Color.Black * alpha,
                Vector2.Zero,
                0.8f
            );
        }
        else
        {
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

        if (currentList.Count > 1)
        {
            string countPrefix = showingAvailableMissions ? "Job " : "Mission ";
            string missionCounter = $"{currentMissionIndex + 1}/{currentList.Count}";
            Vector2 counterPos = new Vector2(posX + TitlePanelWidth - 35, posY + PanelHeight * 4.2f);

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                missionCounter,
                counterPos.X,
                counterPos.Y,
                Color.White * alpha,
                Color.Black * alpha,
                Vector2.Zero,
                0.7f
            );
        }

        if ((availableMissions.Count > 0 && activeMissions.Count > 0))
        {
            string viewType = showingAvailableMissions ? "Opprotunities" : "Active Missions";
            Vector2 viewTypePos = new Vector2(posX + 10, posY + totalHeight - 20);

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                viewType,
                viewTypePos.X,
                viewTypePos.Y,
                Color.White * alpha,
                Color.Black * alpha,
                Vector2.Zero,
                0.6f
            );
        }
    }

    private void CycleToNextMission()
    {
        var currentList = showingAvailableMissions ? availableMissions : activeMissions;
        if (currentList.Count <= 1)
            return;

        currentMissionIndex = (currentMissionIndex + 1) % currentList.Count;
        currentMission = currentList[currentMissionIndex];
        UpdateActiveObjectives();
    }

    private void CycleToPreviousMission()
    {
        var currentList = showingAvailableMissions ? availableMissions : activeMissions;
        if (currentList.Count <= 1)
            return;

        currentMissionIndex = (currentMissionIndex - 1 + currentList.Count) % currentList.Count;
        currentMission = currentList[currentMissionIndex];
        UpdateActiveObjectives();
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

            if (Main.GameUpdateCount % 60 == 0)
            {
                int oldMissionId = currentMission.ID;
                LoadMissions();

                if (!activeMissions.Contains(currentMission) && activeMissions.Count > 0)
                {
                    currentMission = activeMissions[0];
                    UpdateActiveObjectives();
                }
            }
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