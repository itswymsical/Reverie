using System.Collections.Generic;
using System.Linq;

using Terraria.UI;
using Terraria.Audio;
using Terraria.GameContent;

using Reverie.Core.Missions;
using Reverie.Utilities;
using System.Reflection;

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
    private Texture2D prevTexture;
    private Texture2D nextTexture;
    private Texture2D toggleTexture;

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
    private bool wasInventoryOpen = false;

    private Dictionary<string, float> completedObjectiveFade = new Dictionary<string, float>();
    public bool ShouldBeRemoved => false;

    public MissionNotification(Mission mission)
    {
        currentMission = mission;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;
        prevTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Dialogue/ArrowForward").Value;
        nextTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Dialogue/ArrowBack").Value;

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
        if (currentMission != null && currentMission.Objective.Count > 0)
        {
            // Check if the current objective set is completed
            bool isCurrentSetCompleted = currentMission.CurrentIndex < currentMission.Objective.Count &&
                                        currentMission.Objective[currentMission.CurrentIndex].IsCompleted;

            // If the current set is completed but we have more sets, advance to the next set
            if (isCurrentSetCompleted && currentMission.CurrentIndex < currentMission.Objective.Count - 1)
            {
                currentMission.CurrentIndex++;

                // Notify mission update to ensure progress is saved
                Main.LocalPlayer.GetModPlayer<MissionPlayer>().NotifyMissionUpdate(currentMission);

                // No fade data to clear
            }

            // Get current objective set
            if (currentMission.CurrentIndex < currentMission.Objective.Count)
            {
                var currentSet = currentMission.Objective[currentMission.CurrentIndex];

                // Add all objectives from the current set
                foreach (var objective in currentSet.Objectives)
                {
                    // Add all objectives from the current set
                    activeObjectives.Add(objective);
                }
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

        int posX = Main.screenWidth - TitlePanelWidth - 230;
        int posY = 356;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;
        prevTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/Prev").Value;
        nextTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/Next").Value;
        toggleTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/CycleMenu").Value;
        Rectangle panelRect = new Rectangle(posX, posY, TitlePanelWidth, totalHeight);

        Vector2 titlePos = new Vector2(posX, posY + PanelHeight / (float)Math.PI);
        var hoverOffset = (float)Math.Sin(Main.GameUpdateCount * 0.7f * 0.1f) * 1f;
        Vector2 iconPos = new Vector2(posX + 16, posY + PanelHeight / 1.6f - hoverOffset);
        spriteBatch.Draw(
            iconTexture,
            iconPos,
            null,
            Color.White * alpha,
            0f,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            0.67f,
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
            titlePos.X + 26,
            titlePos.Y,
            Color.White * alpha,
            Color.Black * alpha,
            Vector2.Zero,
            1.05f
        );

        bool hasMultipleMissions = currentList.Count > 1;
        bool hasOtherMissionType = (showingAvailableMissions && activeMissions.Count > 0) ||
                                 (!showingAvailableMissions && availableMissions.Count > 0);

        if (hasOtherMissionType)
        {
            Vector2 toggleButtonPos = new Vector2(posX + TitlePanelWidth + 16, posY + PanelHeight / (float)Math.PI + 10.5f);
            Rectangle toggleButtonRect = new Rectangle((int)toggleButtonPos.X - 8, (int)toggleButtonPos.Y - 8, 20, 20);

            spriteBatch.Draw(
                toggleTexture,
                toggleButtonPos,
                null,
                Color.White * alpha,
                0f,
                new Vector2(toggleTexture.Width / 2, toggleTexture.Height / 2),
                0.8f,
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
                nextTexture,
                prevButtonPos,
                null,
                Color.White * alpha,
                0f,
                new Vector2(nextTexture.Width / 2, nextTexture.Height / 2),
                0.6f,
                SpriteEffects.None,
                0f
            );

            Vector2 nextButtonPos = new Vector2(posX + TitlePanelWidth - ButtonSize - 5, posY + PanelHeight * (float)Math.PI + 20);
            Rectangle nextButtonRect = new Rectangle((int)nextButtonPos.X - 8, (int)nextButtonPos.Y - 6, ButtonSize, 16);

            spriteBatch.Draw(
                prevTexture,
                nextButtonPos,
                null,
                Color.White * alpha,
                0f,
                new Vector2(prevTexture.Width / 2, prevTexture.Height / 2),
                0.6f,
                SpriteEffects.None,
                0f
            );

            if (Main.mouseLeft && Main.mouseLeftRelease && alpha > 0.9f)
            {
                if (prevButtonRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    CycleToPreviousMission();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                else if (nextButtonRect.Contains(Main.MouseScreen.ToPoint()))
                {
                    CycleToNextMission();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }
        }

        int yOffset = PanelHeight;

        if (showingAvailableMissions)
        {
            string employer = "Unknown";
            var npcName = Lang.GetNPCNameValue(currentMission.Employer);
            if (currentMission.Employer > 0)
            {
                employer = $"{currentMission.Name} [{npcName}]";
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
                    FontAssets.ItemStack.Value,
                    objectiveText,
                    textPos.X,
                    textPos.Y,
                    objective.IsCompleted ? new Color(150, 255, 150) * alpha : Color.White * alpha,
                    Color.Black * alpha,
                    Vector2.Zero,
                    0.75f
                );

                yOffset += PanelHeight - 13;
            }
        }

        if (currentList.Count > 1)
        {
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
            string viewType = showingAvailableMissions ? "Available Missions" : "Active Missions";
            Vector2 viewTypePos = new Vector2(posX + 10, posY + totalHeight - 80);

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

        // No fade data to clear
        UpdateActiveObjectives();
    }

    private void CycleToPreviousMission()
    {
        var currentList = showingAvailableMissions ? availableMissions : activeMissions;
        if (currentList.Count <= 1)
            return;

        currentMissionIndex = (currentMissionIndex - 1 + currentList.Count) % currentList.Count;
        currentMission = currentList[currentMissionIndex];

        // Clear objective fade data when switching missions
        completedObjectiveFade.Clear();
        UpdateActiveObjectives();
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom)
    {
        // No implementation needed
    }

    public void Update()
    {
        bool isInventoryOpen = Main.playerInventory;

        // Handle fade in/out animations based on inventory state
        if (isInventoryOpen != wasInventoryOpen)
        {
            // Inventory state changed
            if (isInventoryOpen)
            {
                // Inventory just opened
                isFadingOut = false;

                // Refresh mission list when inventory opens
                LoadMissions();
                UpdateActiveObjectives();

                // Play sound only when inventory opens
                if ((showingAvailableMissions && availableMissions.Count > 0) ||
                    (!showingAvailableMissions && activeMissions.Count > 0))
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }
            else
            {
                // Inventory just closed
                isFadingOut = true;
                fadeoutProgress = 0f;
            }

            wasInventoryOpen = isInventoryOpen;
        }

        if (isInventoryOpen)
        {
            isFadingOut = false;
            fadeInProgress += FADE_IN_SPEED;
            fadeInProgress = Math.Min(fadeInProgress, 1.0f);

            LoadMissions();
            UpdateActiveObjectives();
        }

        if (isFadingOut)
        {
            fadeoutProgress += FADE_OUT_SPEED;
            fadeoutProgress = Math.Min(fadeoutProgress, 1.0f);
        }

        // No objective-specific fade values to update

        // If all objectives in the current set have faded out, check if we need to move to the next set
        if (!showingAvailableMissions &&
            currentMission != null &&
            currentMission.CurrentIndex < currentMission.Objective.Count &&
            currentMission.Objective[currentMission.CurrentIndex].IsCompleted)
        {
            UpdateActiveObjectives();
        }
    }

    public void StartFadeOut()
    {
        isFadingOut = true;
        fadeoutProgress = 0f;
    }
}