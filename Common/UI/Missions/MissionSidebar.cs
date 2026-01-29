using Reverie.Core.Missions;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace Reverie.Common.UI.Missions;

public class MissionSidebar
{
    #region Constants
    private const float FADE_IN_SPEED = 0.015f;
    private const float FADE_OUT_SPEED = 0.015f;
    private const float HOVER_FADE_SPEED = 0.1f;

    private const string EMPTY_CHECKBOX = "☐";
    private const string CHECKED_CHECKBOX = "☑";

    private const int PANEL_WIDTH = 180;
    private const int PANEL_HEIGHT = 34;
    private const int TEXT_PADDING = 10;
    private const int MISSIONS_PER_PAGE = 5;

    private const float PANEL_X_OFFSET = 2.65f;
    private const float ICON_Y_DIVISOR = 1.6f;
    private const float TITLE_Y_DIVISOR = (float)Math.PI;
    private const float BUTTON_Y_MULTIPLIER = (float)Math.PI;
    private const float COUNTER_Y_MULTIPLIER = 4.2f;
    private const float ICON_SCALE = 0.67f;
    private const float HOVER_AMPLITUDE = 1f;
    private const float HOVER_FREQUENCY = 0.07f;
    private const int OBJECTIVE_Y_SPACING = 12;
    private const int AVAILABLE_MISSION_Y_SPACING = 22;
    #endregion

    #region Fields
    private bool isFadingOut;
    private float fadeoutProgress;
    private float fadeInProgress;
    private float alpha;
    private float hoverFadeIn;

    private Mission currentMission;
    private List<Objective> activeObjectives;
    private List<Mission> activeMissions;
    private List<Mission> availableMissions;

    private int currentMissionIndex;
    private int currentAvailablePage;
    private bool showingAvailableMissions;
    private bool wasInventoryOpen;
    private bool isPinned;

    private readonly Dictionary<string, float> objFadeFinish = [];

    private bool isHoveringMission;
    private bool isHoveringToggleButton;
    private bool isHoveringPrevButton;
    private bool isHoveringNextButton;
    private bool isHoveringPinButton;

    private Vector2 iconPos;

    private Texture2D iconTexture;
    private Texture2D nextTexture;
    private Texture2D prevTexture;
    private Texture2D toggleTexture;
    private Texture2D pinTexture;
    #endregion

    #region Properties
    public bool ShouldBeRemoved => false;
    #endregion

    #region Constructor
    public MissionSidebar(Mission mission)
    {
        currentMission = mission;
        LoadTextures();
        LoadMissions();
        UpdateActiveObjectives();
        ResetFadeStates();
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private void LoadTextures()
    {
        iconTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/MissionAvailable", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        nextTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/Next", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        prevTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/Prev", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        toggleTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/CycleMenu", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        pinTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/UI/Missions/Pin", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
    }

    private void ResetFadeStates()
    {
        fadeInProgress = 0f;
        fadeoutProgress = 0f;
        isFadingOut = false;
    }
    #endregion

    #region Mission Management
    private void LoadMissions()
    {
        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

        // Get sideline missions
        var sidelineActive = missionPlayer.ActiveMissions();
        var sidelineAvailable = missionPlayer.AvailableMissions();

        // Get mainline missions
        var mainlineActive = MissionWorld.Instance.GetAllMissions()
            .Where(m => m.Progress == MissionProgress.Ongoing);
        var mainlineAvailable = MissionWorld.Instance.GetAllMissions()
            .Where(m => m.Status == MissionStatus.Unlocked && m.Progress != MissionProgress.Ongoing);

        // Combine both
        activeMissions = [.. sidelineActive.Concat(mainlineActive)];
        availableMissions = [.. sidelineAvailable.Concat(mainlineAvailable)];

        HandleMissionListToggling();

        if (!showingAvailableMissions)
            SetupActiveMissionIndex();
        else
            SetupAvailableMissionPage();
    }

    private void HandleMissionListToggling()
    {
        bool shouldToggleToAvailable = showingAvailableMissions && availableMissions.Count == 0 && activeMissions.Count > 0;
        bool shouldToggleToActive = !showingAvailableMissions && activeMissions.Count == 0 && availableMissions.Count > 0;

        if (shouldToggleToAvailable || shouldToggleToActive)
            showingAvailableMissions = !showingAvailableMissions;
    }

    private void SetupActiveMissionIndex()
    {
        if (activeMissions.Count == 0) return;

        if (currentMission == null || !activeMissions.Contains(currentMission))
        {
            currentMission = activeMissions[0];
            currentMissionIndex = 0;
        }
        else
        {
            currentMissionIndex = activeMissions.FindIndex(m => m.ID == currentMission.ID);
            if (currentMissionIndex < 0 && activeMissions.Count > 0)
                currentMissionIndex = 0;
        }
    }

    private void SetupAvailableMissionPage()
    {
        int maxPages = CalculateMaxPages(availableMissions.Count);
        currentAvailablePage = Math.Clamp(currentAvailablePage, 0, maxPages - 1);

        if (availableMissions.Count > 0 && (currentMission == null || !availableMissions.Contains(currentMission)))
            currentMission = availableMissions[0];
    }

    private void UpdateActiveObjectives()
    {
        activeObjectives = new List<Objective>();

        if (currentMission == null || currentMission.ObjectiveList.Count == 0 || showingAvailableMissions)
            return;

        HandleObjectiveProgression();
        PopulateVisibleObjectives();
    }

    private void HandleObjectiveProgression()
    {
        bool isCurrentSetCompleted = currentMission.CurrentList < currentMission.ObjectiveList.Count &&
                                   currentMission.ObjectiveList[currentMission.CurrentList].IsCompleted;

        if (isCurrentSetCompleted && currentMission.CurrentList < currentMission.ObjectiveList.Count - 1)
        {
            currentMission.CurrentList++;
            Main.LocalPlayer.GetModPlayer<MissionPlayer>().NotifyMissionUpdate(currentMission);
        }
    }

    private void PopulateVisibleObjectives()
    {
        if (currentMission.CurrentList >= currentMission.ObjectiveList.Count) return;

        var currentSet = currentMission.ObjectiveList[currentMission.CurrentList];
        foreach (var objective in currentSet.Objective)
        {
            if (objective.ShouldBeVisible(currentMission))
                activeObjectives.Add(objective);
        }
    }
    #endregion

    #region Navigation
    private void CycleToNextMission()
    {
        if (activeMissions.Count <= 1) return;

        currentMissionIndex = (currentMissionIndex + 1) % activeMissions.Count;
        currentMission = activeMissions[currentMissionIndex];
        ResetHoverState();
        UpdateActiveObjectives();
    }

    private void CycleToPreviousMission()
    {
        if (activeMissions.Count <= 1) return;

        currentMissionIndex = (currentMissionIndex - 1 + activeMissions.Count) % activeMissions.Count;
        currentMission = activeMissions[currentMissionIndex];
        ResetHoverState();
        objFadeFinish.Clear();
        UpdateActiveObjectives();
    }

    private void NavigateToNextPage()
    {
        int maxPages = CalculateMaxPages(availableMissions.Count);
        currentAvailablePage = (currentAvailablePage + 1) % maxPages;
    }

    private void NavigateToPreviousPage()
    {
        int maxPages = CalculateMaxPages(availableMissions.Count);
        currentAvailablePage = (currentAvailablePage - 1 + maxPages) % maxPages;
    }

    private int CalculateMaxPages(int itemCount) => Math.Max(1, (int)Math.Ceiling(itemCount / (float)MISSIONS_PER_PAGE));

    private void ResetHoverState()
    {
        hoverFadeIn = 0f;
        isHoveringMission = false;
        isHoveringToggleButton = false;
        isHoveringPrevButton = false;
        isHoveringNextButton = false;
        isHoveringPinButton = false;
    }
    #endregion

    #region Drawing
    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        alpha = isFadingOut ? 1.0f - fadeoutProgress : fadeInProgress;
        alpha = MathHelper.Clamp(alpha, 0f, 1f);
        DrawObjectiveList(spriteBatch, bottomAnchorPosition);
    }

    private void DrawObjectiveList(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        var currentList = showingAvailableMissions ? availableMissions : activeMissions;

        if (!ShouldDrawPanel(currentList)) return;

        var layout = CalculateLayout(currentList);
        DrawIcon(spriteBatch, layout);
        DrawTitle(spriteBatch, layout);
        DrawPinButton(spriteBatch, layout);
        DrawToggle(spriteBatch, layout);
        DrawNavigationArrows(spriteBatch, layout, currentList);
        DrawContent(spriteBatch, layout, currentList);
        DrawTooltips(spriteBatch);
    }

    private bool ShouldDrawPanel(List<Mission> currentList)
    {
        if (iconTexture == null || currentList.Count == 0) return false;
        if (!showingAvailableMissions && (activeObjectives == null || activeObjectives.Count == 0)) return false;
        return true;
    }

    private PanelLayout CalculateLayout(List<Mission> currentList)
    {
        int panelObjectCount = showingAvailableMissions
            ? CalculateVisibleMissions()
            : activeObjectives.Count;

        int totalHeight = PANEL_HEIGHT + (PANEL_HEIGHT * panelObjectCount);
        int posX = Main.screenWidth - (int)(PANEL_WIDTH * PANEL_X_OFFSET);
        int posY = Main.screenHeight / 3 - PANEL_HEIGHT;

        return new PanelLayout
        {
            PanelRect = new Rectangle(posX, posY, PANEL_WIDTH, totalHeight),
            PosX = posX,
            PosY = posY
        };
    }

    private int CalculateVisibleMissions()
    {
        int startIndex = currentAvailablePage * MISSIONS_PER_PAGE;
        return Math.Min(startIndex + MISSIONS_PER_PAGE, availableMissions.Count) - startIndex;
    }

    private void DrawIcon(SpriteBatch spriteBatch, PanelLayout layout)
    {
        var hoverOffset = (float)Math.Sin(Main.GameUpdateCount * HOVER_FREQUENCY) * HOVER_AMPLITUDE;
        iconPos = new Vector2(layout.PosX, layout.PosY + PANEL_HEIGHT / ICON_Y_DIVISOR - hoverOffset);

        spriteBatch.Draw(
            iconTexture,
            iconPos,
            null,
            Color.White * alpha,
            0f,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            ICON_SCALE,
            SpriteEffects.None,
            0f
        );
    }

    private void DrawTitle(SpriteBatch spriteBatch, PanelLayout layout)
    {
        string title = showingAvailableMissions ? "Available Missions" : currentMission.Name;
        Vector2 titlePos = new Vector2(layout.PosX - 16, layout.PosY + PANEL_HEIGHT / TITLE_Y_DIVISOR);

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            title,
            titlePos.X + 26,
            titlePos.Y + 3,
            Color.White * alpha,
            Color.Black * alpha,
            Vector2.Zero,
            0.9f
        );
    }

    private void DrawPinButton(SpriteBatch spriteBatch, PanelLayout layout)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, default, Main.Rasterizer);

        Vector2 pinPos = new Vector2(
            layout.PosX - 32,
            layout.PosY + PANEL_HEIGHT / TITLE_Y_DIVISOR + 10.5f
        );

        Rectangle pinRect = new Rectangle(
            (int)pinPos.X - pinTexture.Width / 2,
            (int)pinPos.Y - pinTexture.Height / 2,
            pinTexture.Width,
            pinTexture.Height
        );

        isHoveringPinButton = pinRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;

        Color pinColor = isPinned ? Color.Gold : Color.White;
        if (isHoveringPinButton) pinColor = Color.LightGray;

        spriteBatch.Draw(
            pinTexture,
            pinPos,
            null,
            Color.White * alpha * (isHoveringPinButton ? 1f : 0.8f),
            isPinned ? MathHelper.ToRadians(-45f) : 0f,
            new Vector2(pinTexture.Width / 2, pinTexture.Height / 2),
            isHoveringPinButton ? 1.15f : 1f,
            SpriteEffects.FlipHorizontally,
            0f
        );

        if (isHoveringPinButton && Main.mouseLeft && Main.mouseLeftRelease && alpha > 0.9f)
        {
            Main.mouseLeftRelease = false;
            isPinned = !isPinned;
            SoundEngine.PlaySound(SoundID.MenuTick);
            isHoveringPinButton = false;
        }

        if (isHoveringPinButton)
        {
            Main.LocalPlayer.mouseInterface = true;
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, default, default, default);
    }

    private void DrawToggle(SpriteBatch spriteBatch, PanelLayout layout)
    {
        bool hasOtherMissionType = (showingAvailableMissions && activeMissions.Count > 0) ||
                                 (!showingAvailableMissions && availableMissions.Count > 0);

        if (!hasOtherMissionType) return;

        string title = showingAvailableMissions ? "Available Missions" : currentMission.Name;
        Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(title) * 0.9f;

        Vector2 togglePos = new Vector2(
            layout.PosX + 26 + titleSize.X + 8,
            layout.PosY + PANEL_HEIGHT / TITLE_Y_DIVISOR + 10.5f
        );

        Rectangle toggleRect = new Rectangle(
            (int)togglePos.X - toggleTexture.Width / 2,
            (int)togglePos.Y - toggleTexture.Height / 2,
            toggleTexture.Width,
            toggleTexture.Height
        );

        isHoveringToggleButton = toggleRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;

        spriteBatch.Draw(
            toggleTexture,
            togglePos,
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

    private bool ShouldProcessClick(bool isHovering)
    {
        return isHovering && Main.mouseLeft && Main.mouseLeftRelease && alpha > 0.9f;
    }

    private void DrawNavigationArrows(SpriteBatch spriteBatch, PanelLayout layout, List<Mission> currentList)
    {
        bool shouldShowNavigation = showingAvailableMissions
            ? CalculateMaxPages(availableMissions.Count) > 1
            : activeMissions.Count > 1;

        if (!shouldShowNavigation) return;

        var (prevPos, nextPos) = CalculateNavigationPositions(layout);
        var prevRect = CreateButtonRect(prevPos, prevTexture);
        var nextRect = CreateButtonRect(nextPos, nextTexture);

        isHoveringPrevButton = IsHoveringButton(prevRect);
        isHoveringNextButton = IsHoveringButton(nextRect);

        DrawButton(spriteBatch, prevTexture, prevPos, isHoveringPrevButton);
        DrawButton(spriteBatch, nextTexture, nextPos, isHoveringNextButton);

        if (isHoveringPrevButton || isHoveringNextButton)
        {
            Main.LocalPlayer.mouseInterface = true;
        }

        HandleNavigationClicks();
        DrawCounter(spriteBatch, layout, currentList);
    }

    private (Vector2 prev, Vector2 next) CalculateNavigationPositions(PanelLayout layout)
    {
        Vector2 prevPos = new Vector2(
            layout.PosX + PANEL_WIDTH / 1.25f,
            layout.PosY + PANEL_HEIGHT * BUTTON_Y_MULTIPLIER + 20
        );
        Vector2 nextPos = new Vector2(
            layout.PosX + PANEL_WIDTH / 1.1f,
            layout.PosY + PANEL_HEIGHT * BUTTON_Y_MULTIPLIER + 20
        );

        return (prevPos, nextPos);
    }

    private void DrawButton(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, bool isHovering)
    {
        float scale = isHovering ? 0.65f : 0.6f;
        Color color = isHovering ? Color.White * alpha : Color.White * alpha * 0.8f;

        spriteBatch.Draw(
            texture,
            position,
            null,
            color,
            0f,
            new Vector2(texture.Width / 2, texture.Height / 2),
            scale,
            SpriteEffects.None,
            0f
        );
    }

    private Rectangle CreateButtonRect(Vector2 position, Texture2D texture)
    {
        return new Rectangle(
            (int)position.X - texture.Width / 2,
            (int)position.Y - texture.Height / 2,
            texture.Width,
            texture.Height
        );
    }

    private bool IsHoveringButton(Rectangle buttonRect)
    {
        return buttonRect.Contains(Main.MouseScreen.ToPoint()) && PlayerInput.IgnoreMouseInterface == false;
    }

    private void DrawCounter(SpriteBatch spriteBatch, PanelLayout layout, List<Mission> currentList)
    {
        string counterText = showingAvailableMissions
            ? $"Page {currentAvailablePage + 1}/{CalculateMaxPages(availableMissions.Count)}"
            : $"{currentMissionIndex + 1}/{activeMissions.Count}";

        Vector2 counterPos = new Vector2(
            (layout.PosX / 1.05f) + (showingAvailableMissions ? PANEL_WIDTH / 2 : PANEL_WIDTH),
            (layout.PosY / 1.095f) + PANEL_HEIGHT * COUNTER_Y_MULTIPLIER
        );

        Vector2 origin = showingAvailableMissions ? Vector2.UnitX * 0.5f : Vector2.Zero;

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            counterText,
            counterPos.X,
            counterPos.Y,
            Color.White * alpha,
            Color.Black * alpha,
            origin,
            0.7f
        );
    }

    private void DrawContent(SpriteBatch spriteBatch, PanelLayout layout, List<Mission> currentList)
    {
        if (showingAvailableMissions)
            DrawAvailableMissions(spriteBatch, layout);
        else
            DrawActiveObjectives(spriteBatch, layout);
    }

    private void DrawAvailableMissions(SpriteBatch spriteBatch, PanelLayout layout)
    {
        int startIndex = currentAvailablePage * MISSIONS_PER_PAGE;
        int endIndex = Math.Min(startIndex + MISSIONS_PER_PAGE, availableMissions.Count);
        int yOffset = PANEL_HEIGHT;

        for (int i = startIndex; i < endIndex; i++)
        {
            var mission = availableMissions[i];
            string employer = GetMissionDisplayText(mission);
            Vector2 textPos = new Vector2(layout.PosX + TEXT_PADDING, layout.PosY + yOffset + PANEL_HEIGHT / TITLE_Y_DIVISOR);

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

            yOffset += AVAILABLE_MISSION_Y_SPACING;
        }
    }

    private string GetMissionDisplayText(Mission mission)
    {
        if (mission.ProviderNPC <= 0) return "Unknown";

        var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
        return $"{mission.Name} - {npcName}";
    }

    private void DrawActiveObjectives(SpriteBatch spriteBatch, PanelLayout layout)
    {
        int yOffset = PANEL_HEIGHT;

        foreach (var objective in activeObjectives)
        {
            string objectiveText = FormatObjectiveText(objective);
            Vector2 textPos = new Vector2(layout.PosX + TEXT_PADDING, layout.PosY + yOffset + PANEL_HEIGHT / TITLE_Y_DIVISOR);
            Color textColor = objective.IsCompleted ? new Color(150, 255, 150) * alpha : Color.White * alpha;

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.ItemStack.Value,
                objectiveText,
                textPos.X,
                textPos.Y,
                textColor,
                Color.Black * alpha,
                Vector2.Zero,
                0.85f
            );

            yOffset += PANEL_HEIGHT - OBJECTIVE_Y_SPACING;
        }
    }

    private string FormatObjectiveText(Objective objective)
    {
        string status = objective.IsCompleted ? CHECKED_CHECKBOX : EMPTY_CHECKBOX;

        if (objective.RequiredCount > 1)
            return $"{status} {objective.Description} [{objective.Count}/{objective.RequiredCount}]";

        return $"{status} {objective.Description}";
    }

    private void DrawTooltips(SpriteBatch spriteBatch)
    {
        if (alpha == 0) return;

        string hoverText = GetHoverText();
        if (!string.IsNullOrEmpty(hoverText))
            DrawHoverText(spriteBatch, hoverText);
    }

    private string GetHoverText()
    {
        if (isHoveringPinButton)
            return isPinned ? "Unpin Sidebar" : "Pin Sidebar";

        bool hasOtherMissionType = (showingAvailableMissions && activeMissions.Count > 0) ||
                                 (!showingAvailableMissions && availableMissions.Count > 0);

        if (isHoveringToggleButton && hasOtherMissionType)
            return showingAvailableMissions ? "See Active Missions" : "See Available Missions";

        if (isHoveringPrevButton)
            return showingAvailableMissions ? "Previous Page" : "Previous Mission";

        if (isHoveringNextButton)
            return showingAvailableMissions ? "Next Page" : "Next Mission";

        return string.Empty;
    }

    private void DrawHoverText(SpriteBatch spriteBatch, string text)
    {
        Vector2 mousePos = Main.MouseScreen;
        Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);

        Vector2 textPos = new Vector2(
            mousePos.X - textSize.X / 2,
            mousePos.Y - textSize.Y - 10
        );

        textPos.X = MathHelper.Clamp(textPos.X, 10, Main.screenWidth - textSize.X - 10);
        textPos.Y = MathHelper.Clamp(textPos.Y, 10, Main.screenHeight - textSize.Y - 10);

        Rectangle hoverBg = new Rectangle(
            (int)textPos.X - 16,
            (int)textPos.Y - 10,
            (int)textSize.X + 8,
            (int)textSize.Y + 8
        );

        DrawUtils.DrawPanel(spriteBatch, hoverBg, new Color(63, 65, 161) * 0.8f * alpha);

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            text,
            textPos.X,
            textPos.Y,
            Color.White * alpha,
            Color.Black * alpha,
            Vector2.Zero,
            0.8f
        );
    }
    #endregion

    #region Input Handling
    private void HandleToggleClick()
    {
        if (!ShouldProcessClick(isHoveringToggleButton)) return;

        Main.mouseLeftRelease = false;
        Main.LocalPlayer.mouseInterface = true;

        showingAvailableMissions = !showingAvailableMissions;
        LoadMissions();
        UpdateActiveObjectives();
        SoundEngine.PlaySound(SoundID.MenuTick);
        ResetHoverState();
    }

    private void HandleNavigationClicks()
    {
        if (ShouldProcessClick(isHoveringPrevButton))
        {
            Main.mouseLeftRelease = false;
            Main.LocalPlayer.mouseInterface = true;

            if (showingAvailableMissions) NavigateToPreviousPage();
            else CycleToPreviousMission();
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        else if (ShouldProcessClick(isHoveringNextButton))
        {
            Main.mouseLeftRelease = false;
            Main.LocalPlayer.mouseInterface = true;

            if (showingAvailableMissions) NavigateToNextPage();
            else CycleToNextMission();
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }

    private void HandleInput()
    {
        if (alpha <= 0.9f || PlayerInput.IgnoreMouseInterface) return;

        HandleToggleClick();
        HandleNavigationClicks();
    }
    #endregion

    #region Update Logic
    public void Update()
    {
        bool isInventoryOpen = Main.playerInventory;
        bool shouldBeVisible = isInventoryOpen || isPinned;

        if (isInventoryOpen != wasInventoryOpen)
        {
            if (shouldBeVisible)
            {
                isFadingOut = false;
                fadeInProgress = 0f;

                LoadMissions();
                UpdateActiveObjectives();

                if ((showingAvailableMissions && availableMissions.Count > 0) ||
                    (!showingAvailableMissions && activeMissions.Count > 0))
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }
            else if (!isPinned)
            {
                isFadingOut = true;
                fadeoutProgress = 0f;

                isHoveringMission = false;
                isHoveringToggleButton = false;
                isHoveringPrevButton = false;
                isHoveringNextButton = false;
                isHoveringPinButton = false;
            }

            wasInventoryOpen = isInventoryOpen;
        }

        if (shouldBeVisible && !isFadingOut)
        {
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
            currentMission.CurrentList < currentMission.ObjectiveList.Count &&
            currentMission.ObjectiveList[currentMission.CurrentList].IsCompleted)
        {
            UpdateActiveObjectives();
        }
    }

    private void HandleInventoryToggle()
    {
        bool isInventoryOpen = Main.playerInventory;

        if (isInventoryOpen == wasInventoryOpen) return;

        if (isInventoryOpen)
        {
            isFadingOut = false;
            LoadMissions();
            UpdateActiveObjectives();

            if (HasMissionsToShow())
                SoundEngine.PlaySound(SoundID.MenuTick);
        }
        else
        {
            isFadingOut = true;
            fadeoutProgress = 0f;
            ResetHoverState();
        }

        wasInventoryOpen = isInventoryOpen;
    }

    private bool HasMissionsToShow()
    {
        return (showingAvailableMissions && availableMissions.Count > 0) ||
               (!showingAvailableMissions && activeMissions.Count > 0);
    }

    private void UpdateFadeStates()
    {
        bool isInventoryOpen = Main.playerInventory;

        if (isInventoryOpen)
        {
            isFadingOut = false;
            fadeInProgress = Math.Min(fadeInProgress + FADE_IN_SPEED, 1.0f);
            LoadMissions();
            UpdateActiveObjectives();
        }

        if (isFadingOut)
        {
            fadeoutProgress = Math.Min(fadeoutProgress + FADE_OUT_SPEED, 1.0f);
            hoverFadeIn = 0f;
        }

        if (!isHoveringMission && hoverFadeIn > 0)
            hoverFadeIn = MathHelper.Lerp(hoverFadeIn, 0f, HOVER_FADE_SPEED);
    }

    private void UpdateObjectiveProgression()
    {
        if (showingAvailableMissions || currentMission == null) return;

        if (currentMission.CurrentList < currentMission.ObjectiveList.Count &&
            currentMission.ObjectiveList[currentMission.CurrentList].IsCompleted)
        {
            UpdateActiveObjectives();
        }
    }

    public void StartFadeOut()
    {
        isFadingOut = true;
        fadeoutProgress = 0f;
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom) { }
    #endregion

    #region Helper Structures
    private struct PanelLayout
    {
        public Rectangle PanelRect;
        public int PosX;
        public int PosY;
    }
    #endregion
}

public class MissionSidebarManager : ModSystem
{
    public static MissionSidebarManager Instance { get; set; }
    public MissionSidebarManager() => Instance = this;

    private MissionSidebar currentNotification;

    public override void Unload()
    {
        if (!Main.dedServ)
        {
            Instance = null;
        }
        currentNotification = null;
    }

    public void SetNotification(MissionSidebar notification)
    {
        currentNotification = notification;
    }

    public void ClearNotification()
    {
        currentNotification = null;
    }

    public override void PostUpdateEverything()
    {
        currentNotification?.Update();
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mapIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Map / Minimap"));
        if (mapIndex != -1)
        {
            layers.Insert(mapIndex + 1, new LegacyGameInterfaceLayer(
                "Reverie: Mission Notification",
                delegate {
                    currentNotification?.DrawInGame(Main.spriteBatch, Vector2.Zero);
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}