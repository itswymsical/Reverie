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
        //if (showingAvailableMissions && isHoveringMission && currentMission != null)
        //{
        //    DrawMissionDetailPanel(spriteBatch, bottomAnchorPosition);
        //}
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
            var npcName = Lang.GetNPCNameValue(currentMission.ProviderNPC);
            if (currentMission.ProviderNPC > 0)
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

            Rectangle missionEntryRect = new Rectangle(
                posX,
                posY + yOffset,
                TitlePanelWidth,
                PanelHeight
            );

            isHoveringMission = missionEntryRect.Contains(Main.MouseScreen.ToPoint()) 
                && !isHoveringToggleButton &&
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
            Vector2 counterPos = new Vector2(posX + TitlePanelWidth, posY + PanelHeight * 4.2f);

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