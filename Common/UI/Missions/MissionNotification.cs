using Reverie.Core.Missions;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

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
    private Texture2D nextTexture;
    private Texture2D prevTexture;
    private Texture2D toggleTexture;
    private Texture2D missionIconTexture;

    private const string EMPTY_CHECKBOX = "☐";
    private const string CHECKED_CHECKBOX = "☑";

    private const int TitlePanelWidth = 180;
    private const int PanelHeight = 34;
    private const int TextPadding = 10;
    private const int ButtonSize = 16;

    private const int DetailPanelWidth = 300;
    private const int DetailPanelPadding = 15;

    private List<Mission> activeMissions;
    private List<Mission> availableMissions;
    private int currentMissionIndex = 0;
    private bool showingAvailableMissions = false;
    private bool wasInventoryOpen = false;

    private bool isHoveringMission = false;
    private bool isHoveringToggleButton = false;
    private bool isHoveringPrevButton = false;
    private bool isHoveringNextButton = false;
    private float hoverFadeIn = 0f;
    private const float HOVER_FADE_SPEED = 0.1f;
    private bool clicked = false;
    private Vector2 iconPos;

    private Dictionary<string, float> completedObjectiveFade = new Dictionary<string, float>();
    public bool ShouldBeRemoved => false;

    public MissionNotification(Mission mission)
    {
        currentMission = mission;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;
        nextTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Dialogue/ArrowForward").Value;
        prevTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Dialogue/ArrowBack").Value;
        missionIconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;

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
            bool isCurrentSetCompleted = currentMission.CurrentIndex < currentMission.Objective.Count &&
                                        currentMission.Objective[currentMission.CurrentIndex].IsCompleted;

            if (isCurrentSetCompleted && currentMission.CurrentIndex < currentMission.Objective.Count - 1)
            {
                currentMission.CurrentIndex++;

                Main.LocalPlayer.GetModPlayer<MissionPlayer>().NotifyMissionUpdate(currentMission);
            }

            if (currentMission.CurrentIndex < currentMission.Objective.Count)
            {
                var currentSet = currentMission.Objective[currentMission.CurrentIndex];

                foreach (var objective in currentSet.Objectives)
                {
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

        if (showingAvailableMissions && isHoveringMission && currentMission != null)
        {
            DrawMissionDetailPanel(spriteBatch);
        }
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

        int posX = Main.screenWidth - TitlePanelWidth - 240;
        int posY = 356;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable").Value;
        nextTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/Next").Value;
        prevTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/Prev").Value;
        toggleTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/CycleMenu").Value;
        Rectangle panelRect = new Rectangle(posX, posY, TitlePanelWidth, totalHeight);

        Vector2 titlePos = new Vector2(posX, posY + PanelHeight / (float)Math.PI);
        var hoverOffset = (float)Math.Sin(Main.GameUpdateCount * 0.7f * 0.1f) * 1f;
        iconPos = new Vector2(posX + 16, posY + PanelHeight / 1.6f - hoverOffset);
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
            titlePos.Y + 3,
            Color.White * alpha,
            Color.Black * alpha,
            Vector2.Zero,
            0.9f
        );

        bool hasMultipleMissions = currentList.Count > 1;
        bool hasOtherMissionType = (showingAvailableMissions && activeMissions.Count > 0) ||
                                 (!showingAvailableMissions && availableMissions.Count > 0);

        if (hasOtherMissionType)
        {
            Vector2 toggleButtonPos = new Vector2(posX + TitlePanelWidth + 16, posY + PanelHeight / (float)Math.PI + 10.5f);
            Rectangle toggleButtonRect = new Rectangle(
                (int)toggleButtonPos.X - toggleTexture.Width / 2,
                (int)toggleButtonPos.Y - toggleTexture.Height / 2,
                toggleTexture.Width,
                toggleTexture.Height
            );

            isHoveringToggleButton = toggleButtonRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;

            spriteBatch.Draw(
                toggleTexture,
                toggleButtonPos,
                null,
                isHoveringToggleButton ? Color.White * alpha : Color.White * alpha * 0.8f,
                0f,
                new Vector2(toggleTexture.Width / 2, toggleTexture.Height / 2),
                isHoveringToggleButton ? 0.85f : 0.8f,
                SpriteEffects.None,
                0f
            );

            if (isHoveringToggleButton && Main.mouseLeft && Main.mouseLeftRelease && alpha > 0.9f)
            {
                Main.mouseLeftRelease = false;
                showingAvailableMissions = !showingAvailableMissions;
                LoadMissions();
                UpdateActiveObjectives();
                SoundEngine.PlaySound(SoundID.MenuTick);

                // Reset hover states
                isHoveringToggleButton = false;
                isHoveringPrevButton = false;
                isHoveringNextButton = false;
                isHoveringMission = false;
            }

            if (isHoveringToggleButton)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        if (hasMultipleMissions)
        {
            Vector2 prevButtonPos = new Vector2(posX + TitlePanelWidth / 1.01f, posY + PanelHeight * (float)Math.PI + 20);
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

            Vector2 nextButtonPos = new Vector2(posX + TitlePanelWidth / 0.92f, posY + PanelHeight * (float)Math.PI + 20);
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

            isHoveringNextButton = nextButtonRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;
            isHoveringPrevButton = prevButtonRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;

            spriteBatch.Draw(
                nextTexture,
                prevButtonPos,
                null,
                isHoveringPrevButton ? Color.White * alpha : Color.White * alpha * 0.8f,
                0f,
                new Vector2(nextTexture.Width / 2, nextTexture.Height / 2),
                isHoveringPrevButton ? 0.65f : 0.6f,
                SpriteEffects.None,
                0f
            );

            spriteBatch.Draw(
                prevTexture,
                nextButtonPos,
                null,
                isHoveringNextButton ? Color.White * alpha : Color.White * alpha * 0.8f,
                0f,
                new Vector2(prevTexture.Width / 2, prevTexture.Height / 2),
                isHoveringNextButton ? 0.65f : 0.6f,
                SpriteEffects.None,
                0f
            );

            // Handle clicks on navigation buttons
            if (Main.mouseLeft && Main.mouseLeftRelease && alpha > 0.9f)
            {
                if (isHoveringPrevButton)
                {
                    Main.mouseLeftRelease = false;
                    CycleToPreviousMission();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                else if (isHoveringNextButton)
                {
                    Main.mouseLeftRelease = false;
                    CycleToNextMission();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }

            if (isHoveringPrevButton || isHoveringNextButton)
            {
                Main.LocalPlayer.mouseInterface = true;
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

            // Check if mouse is hovering over the mission entry
            Rectangle missionEntryRect = new Rectangle(
                posX,
                posY + yOffset,
                TitlePanelWidth,
                PanelHeight
            );

            isHoveringMission = missionEntryRect.Contains(Main.MouseScreen.ToPoint()) &&
                                 !isHoveringToggleButton &&
                                 !isHoveringPrevButton &&
                                 !isHoveringNextButton &&
                                 PlayerInput.IgnoreMouseInterface == false;

            if (isHoveringMission)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
        else
        {
            isHoveringMission = false;

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
    }

    private void DrawMissionDetailPanel(SpriteBatch spriteBatch)
    {
        if (currentMission == null)
            return;

        int lineCount = 3;
        if (currentMission.Objective.Count > 0)
        {
            lineCount += currentMission.Objective[0].Objectives.Count;
        }

        int panelHeight = 20 + (lineCount * 20) + DetailPanelPadding * 2;

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

        // Update hover fade in/out effect
        hoverFadeIn = MathHelper.Lerp(hoverFadeIn, 1f, HOVER_FADE_SPEED);
        if (!isHoveringMission && hoverFadeIn < 0.05f)
        {
            hoverFadeIn = 0f;
            return;
        }

        // Check if mouse is hovering over detail panel
        bool isHoveringDetailPanel = panelRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;
        if (isHoveringDetailPanel)
        {
            Main.LocalPlayer.mouseInterface = true;
        }

        Color panelColor = new Color(0, 0, 0, (int)(200 * hoverFadeIn));

        // Draw panel background
        Utils.DrawInvBG(spriteBatch, panelRect, panelColor);

        int textY = panelRect.Y + DetailPanelPadding;

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            currentMission.Name,
            panelRect.X + DetailPanelPadding,
            textY,
            new Color(192, 151, 83, (int)(255 * hoverFadeIn)), // Gold
            Color.Black * hoverFadeIn,
            Vector2.Zero,
            1f
        );
        textY += 25;

        string employerName = "Unknown";
        if (currentMission.Employer > 0)
        {
            employerName = Lang.GetNPCNameValue(currentMission.Employer);
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
            currentMission.Description,
            panelRect.X + DetailPanelPadding,
            textY,
            Color.White * hoverFadeIn,
            Color.Black * hoverFadeIn,
            Vector2.Zero,
            0.8f
        );
        textY += 30;

        if (currentMission.Rewards.Count > 0 || currentMission.Experience > 0)
        {
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                "Rewards:",
                panelRect.X + DetailPanelPadding,
                textY,
                Color.White * hoverFadeIn,
                Color.Black * hoverFadeIn,
                Vector2.Zero,
                0.8f
            );
            textY += 20;

            int rewardX = panelRect.X + DetailPanelPadding;
            foreach (var reward in currentMission.Rewards)
            {
                if (reward.type <= 0)
                    continue;

                spriteBatch.Draw(
                    TextureAssets.Item[reward.type].Value,
                    new Vector2(rewardX, textY),
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
                        textY + 6,
                        Color.White * hoverFadeIn,
                        Color.Black * hoverFadeIn,
                        Vector2.Zero,
                        0.8f
                    );
                }

                rewardX += 40;
            }

            if (currentMission.Experience > 0)
            {
                Utils.DrawBorderStringFourWay(
                    spriteBatch,
                    FontAssets.MouseText.Value,
                    $"{currentMission.Experience} XP",
                    rewardX,
                    textY,
                    new Color(73, 213, 255, (int)(255 * hoverFadeIn)), // Light blue for XP
                    Color.Black * hoverFadeIn,
                    Vector2.Zero,
                    0.8f
                );
            }

            textY += 30;
        }

        // Add a button-like region for accepting the mission
        Rectangle acceptButtonRect = new Rectangle(
            panelRect.X + DetailPanelWidth / 2 - 60,
            panelRect.Y + panelHeight - DetailPanelPadding - 20,
            120,
            25
        );

        bool isHoveringAcceptButton = acceptButtonRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;

        // Draw a highlight for the accept button when hovering
        if (isHoveringAcceptButton)
        {
            Utils.DrawInvBG(spriteBatch, acceptButtonRect, new Color(255, 255, 100, 50));
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            "Click to accept",
            panelRect.X + DetailPanelWidth / 2,
            panelRect.Y + panelHeight - DetailPanelPadding - 6,
            isHoveringAcceptButton ? Color.Yellow * hoverFadeIn : Color.Yellow * hoverFadeIn * 0.8f,
            Color.Black * hoverFadeIn,
            new Vector2(0.5f, 0f),
            0.8f
        );

        // Handle click on accept button
        if (isHoveringAcceptButton && Main.mouseLeft && Main.mouseLeftRelease && !clicked)
        {
            Main.mouseLeftRelease = false;
            HandleMissionAccept();
        }
    }

    private void HandleMissionAccept()
    {
        SoundEngine.PlaySound(SoundID.MenuOpen);

        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        missionPlayer.StartMission(currentMission.ID);

        clicked = true;

        // Refresh mission lists after accepting a mission
        LoadMissions();
        UpdateActiveObjectives();
    }

    private void CycleToNextMission()
    {
        var currentList = showingAvailableMissions ? availableMissions : activeMissions;
        if (currentList.Count <= 1)
            return;

        currentMissionIndex = (currentMissionIndex + 1) % currentList.Count;
        currentMission = currentList[currentMissionIndex];

        // Reset hover and click state when changing missions
        hoverFadeIn = 0f;
        clicked = false;

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

        // Reset hover and click state when changing missions
        hoverFadeIn = 0f;
        clicked = false;

        // Clear objective fade data when switching missions
        completedObjectiveFade.Clear();
        UpdateActiveObjectives();
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom) { }

    public void Update()
    {
        bool isInventoryOpen = Main.playerInventory;

        if (isInventoryOpen != wasInventoryOpen)
        {
            if (isInventoryOpen)
            {
                isFadingOut = false;

                LoadMissions();
                UpdateActiveObjectives();

                clicked = false;

                if ((showingAvailableMissions && availableMissions.Count > 0) ||
                    (!showingAvailableMissions && activeMissions.Count > 0))
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }
            else
            {
                isFadingOut = true;
                fadeoutProgress = 0f;

                isHoveringMission = false;
                isHoveringToggleButton = false;
                isHoveringPrevButton = false;
                isHoveringNextButton = false;
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

            hoverFadeIn = 0f;
        }

        if (!isHoveringMission && hoverFadeIn > 0)
        {
            hoverFadeIn = MathHelper.Lerp(hoverFadeIn, 0f, HOVER_FADE_SPEED);
        }

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