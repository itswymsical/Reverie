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

    private readonly Texture2D iconTexture;
    private readonly Texture2D nextTexture;
    private readonly Texture2D prevTexture;
    private readonly Texture2D toggleTexture;

    private const string EMPTY_CHECKBOX = "☐";
    private const string CHECKED_CHECKBOX = "☑";

    private const int TITLE_PANEL_WIDTH = 180;
    private const int PANEL_HEIGHT = 34;
    private const int TEXT_PADDING = 10;
    private const int BUTTON_SIZE = 16;
    private const int MISSIONS_PER_PAGE = 5; // Maximum missions to display per page

    private List<Mission> activeMissions;
    private List<Mission> availableMissions;
    private int currentMissionIndex = 0;
    private int currentAvailablePage = 0; // Track the current page of available missions
    private bool showingAvailableMissions = false;
    private bool wasInventoryOpen = false;

    private bool isHoveringMission = false;
    private bool isHoveringToggleButton = false;
    private bool isHoveringPrevButton = false;
    private bool isHoveringNextButton = false;
    private float hoverFadeIn = 0f;
    private const float HOVER_FADE_SPEED = 0.1f;
    private Vector2 iconPos;

    private readonly Dictionary<string, float> objFadeFinish = [];
    public bool ShouldBeRemoved => false;

    public MissionNotification(Mission mission)
    {
        currentMission = mission;

        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        nextTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/Next", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        prevTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/Prev", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        toggleTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/CycleMenu", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

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

        if (!showingAvailableMissions)
        {
            var currentList = activeMissions;

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
        else
        {
            // For available missions, we just need to ensure the page is valid
            int maxPages = (int)Math.Ceiling(availableMissions.Count / (float)MISSIONS_PER_PAGE);
            if (maxPages == 0)
                maxPages = 1;

            currentAvailablePage = Math.Clamp(currentAvailablePage, 0, maxPages - 1);

            // If we have available missions and currentMission isn't set, set it to the first mission
            if (availableMissions.Count > 0 && (currentMission == null || !availableMissions.Contains(currentMission)))
            {
                currentMission = availableMissions[0];
            }
        }
    }

    private void UpdateActiveObjectives()
    {
        activeObjectives = new List<Objective>();
        if (currentMission != null && currentMission.Objective.Count > 0 && !showingAvailableMissions)
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
                    if (objective.ShouldBeVisible(currentMission))
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

        if (iconTexture == null || currentList.Count == 0)
            return;

        if (!showingAvailableMissions && (activeObjectives == null || activeObjectives.Count == 0))
            return;

        int panelObjectCount;
        int totalHeight;

        if (showingAvailableMissions)
        {
            // Calculate missions to show on current page
            int startIndex = currentAvailablePage * MISSIONS_PER_PAGE;
            int endIndex = Math.Min(startIndex + MISSIONS_PER_PAGE, availableMissions.Count) - 1;
            panelObjectCount = endIndex - startIndex + 1;
            totalHeight = PANEL_HEIGHT + (PANEL_HEIGHT * panelObjectCount);
        }
        else
        {
            panelObjectCount = activeObjectives.Count;
            totalHeight = PANEL_HEIGHT + (PANEL_HEIGHT * panelObjectCount);
        }

        int posX = Main.screenWidth - TITLE_PANEL_WIDTH - 240;
        int posY = 356;

        Rectangle panelRect = new Rectangle(posX, posY, TITLE_PANEL_WIDTH, totalHeight);

        Vector2 titlePos = new Vector2(posX, posY + PANEL_HEIGHT / (float)Math.PI);
        var hoverOffset = (float)Math.Sin(Main.GameUpdateCount * 0.7f * 0.1f) * 1f;
        iconPos = new Vector2(posX + 16, posY + PANEL_HEIGHT / 1.6f - hoverOffset);
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
            missionTitle = "Available Missions";
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

        bool hasOtherMissionType = (showingAvailableMissions && activeMissions.Count > 0) ||
                                 (!showingAvailableMissions && availableMissions.Count > 0);

        if (hasOtherMissionType)
        {
            Vector2 toggleButtonPos = new Vector2(posX + TITLE_PANEL_WIDTH + 16, posY + PANEL_HEIGHT / (float)Math.PI + 10.5f);
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

        // Navigation buttons
        if (showingAvailableMissions)
        {
            // Pagination for available missions
            int maxPages = (int)Math.Ceiling(availableMissions.Count / (float)MISSIONS_PER_PAGE);
            if (maxPages > 1)
            {
                // Draw pagination buttons
                Vector2 prevButtonPos = new Vector2(posX + TITLE_PANEL_WIDTH / 1.01f, posY + PANEL_HEIGHT * (float)Math.PI + 20);
                Rectangle prevButtonRect = new Rectangle((int)prevButtonPos.X - 8, (int)prevButtonPos.Y - 6, BUTTON_SIZE, 16);

                Vector2 nextButtonPos = new Vector2(posX + TITLE_PANEL_WIDTH / 0.92f, posY + PANEL_HEIGHT * (float)Math.PI + 20);
                Rectangle nextButtonRect = new Rectangle((int)nextButtonPos.X - 8, (int)nextButtonPos.Y - 6, BUTTON_SIZE, 16);

                isHoveringNextButton = nextButtonRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;
                isHoveringPrevButton = prevButtonRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;

                spriteBatch.Draw(
                    prevTexture,
                    prevButtonPos,
                    null,
                    isHoveringPrevButton ? Color.White * alpha : Color.White * alpha * 0.8f,
                    0f,
                    new Vector2(prevTexture.Width / 2, prevTexture.Height / 2),
                    isHoveringPrevButton ? 0.65f : 0.6f,
                    SpriteEffects.None,
                    0f
                );

                spriteBatch.Draw(
                    nextTexture,
                    nextButtonPos,
                    null,
                    isHoveringNextButton ? Color.White * alpha : Color.White * alpha * 0.8f,
                    0f,
                    new Vector2(nextTexture.Width / 2, nextTexture.Height / 2),
                    isHoveringNextButton ? 0.65f : 0.6f,
                    SpriteEffects.None,
                    0f
                );

                if (Main.mouseLeft && Main.mouseLeftRelease && alpha > 0.9f)
                {
                    if (isHoveringPrevButton)
                    {
                        Main.mouseLeftRelease = false;
                        NavigateToPreviousPage();
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                    else if (isHoveringNextButton)
                    {
                        Main.mouseLeftRelease = false;
                        NavigateToNextPage();
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                }

                if (isHoveringPrevButton || isHoveringNextButton)
                {
                    Main.LocalPlayer.mouseInterface = true;
                }

                // Draw page counter
                string pageCounter = $"Page {currentAvailablePage + 1}/{maxPages}";
                Vector2 counterPos = new Vector2(posX + TITLE_PANEL_WIDTH / 2, posY + PANEL_HEIGHT * 4.2f);

                Utils.DrawBorderStringFourWay(
                    spriteBatch,
                    FontAssets.MouseText.Value,
                    pageCounter,
                    counterPos.X,
                    counterPos.Y,
                    Color.White * alpha,
                    Color.Black * alpha,
                    Vector2.UnitX * 0.5f, // Center the text
                    0.7f
                );
            }
        }
        else if (activeMissions.Count > 1)
        {
            // Existing mission cycling buttons for active missions
            Vector2 prevButtonPos = new Vector2(posX + TITLE_PANEL_WIDTH / 1.01f, posY + PANEL_HEIGHT * (float)Math.PI + 20);
            Rectangle prevButtonRect = new Rectangle((int)prevButtonPos.X - 8, (int)prevButtonPos.Y - 6, BUTTON_SIZE, 16);

            Vector2 nextButtonPos = new Vector2(posX + TITLE_PANEL_WIDTH / 0.92f, posY + PANEL_HEIGHT * (float)Math.PI + 20);
            Rectangle nextButtonRect = new Rectangle((int)nextButtonPos.X - 8, (int)nextButtonPos.Y - 6, BUTTON_SIZE, 16);

            isHoveringNextButton = nextButtonRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;
            isHoveringPrevButton = prevButtonRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;

            spriteBatch.Draw(
                prevTexture,
                prevButtonPos,
                null,
                isHoveringPrevButton ? Color.White * alpha : Color.White * alpha * 0.8f,
                0f,
                new Vector2(prevTexture.Width / 2, prevTexture.Height / 2),
                isHoveringPrevButton ? 0.65f : 0.6f,
                SpriteEffects.None,
                0f
            );

            spriteBatch.Draw(
                nextTexture,
                nextButtonPos,
                null,
                isHoveringNextButton ? Color.White * alpha : Color.White * alpha * 0.8f,
                0f,
                new Vector2(nextTexture.Width / 2, nextTexture.Height / 2),
                isHoveringNextButton ? 0.65f : 0.6f,
                SpriteEffects.None,
                0f
            );

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

            // Draw mission counter
            string missionCounter = $"{currentMissionIndex + 1}/{activeMissions.Count}";
            Vector2 counterPos = new Vector2(posX + TITLE_PANEL_WIDTH, posY + PANEL_HEIGHT * 4.2f);

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

        int yOffset = PANEL_HEIGHT;

        if (showingAvailableMissions)
        {
            // Calculate missions to show on current page
            int startIndex = currentAvailablePage * MISSIONS_PER_PAGE;
            int endIndex = Math.Min(startIndex + MISSIONS_PER_PAGE, availableMissions.Count);

            // Draw each available mission as a list
            for (int i = startIndex; i < endIndex; i++)
            {
                var mission = availableMissions[i];

                string employer = "Unknown";
                var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
                if (mission.ProviderNPC > 0)
                {
                    employer = $"{mission.Name} - {npcName}";
                }

                Vector2 textPos = new Vector2(posX + TEXT_PADDING, posY + yOffset + PANEL_HEIGHT / (float)Math.PI);

                Rectangle missionEntryRect = new Rectangle(
                    posX,
                    posY + yOffset,
                    TITLE_PANEL_WIDTH,
                    PANEL_HEIGHT
                );

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

                yOffset += 22;
            }
        }
        else
        {
            // Original code for showing active mission objectives
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

                Vector2 textPos = new Vector2(posX + TEXT_PADDING, posY + yOffset + PANEL_HEIGHT / (float)Math.PI);

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

                yOffset += PANEL_HEIGHT - 13;
            }
        }
    }

    private void CycleToNextMission()
    {
        var currentList = activeMissions;
        if (currentList.Count <= 1)
            return;

        currentMissionIndex = (currentMissionIndex + 1) % currentList.Count;
        currentMission = currentList[currentMissionIndex];

        hoverFadeIn = 0f;

        UpdateActiveObjectives();
    }

    private void CycleToPreviousMission()
    {
        var currentList = activeMissions;
        if (currentList.Count <= 1)
            return;

        currentMissionIndex = (currentMissionIndex - 1 + currentList.Count) % currentList.Count;
        currentMission = currentList[currentMissionIndex];

        // Reset hover and click state when changing missions
        hoverFadeIn = 0f;
        // Clear objective fade data when switching missions
        objFadeFinish.Clear();
        UpdateActiveObjectives();
    }

    private void NavigateToNextPage()
    {
        int maxPages = (int)Math.Ceiling(availableMissions.Count / (float)MISSIONS_PER_PAGE);
        currentAvailablePage = (currentAvailablePage + 1) % maxPages;
    }

    private void NavigateToPreviousPage()
    {
        int maxPages = (int)Math.Ceiling(availableMissions.Count / (float)MISSIONS_PER_PAGE);
        currentAvailablePage = (currentAvailablePage - 1 + maxPages) % maxPages;
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