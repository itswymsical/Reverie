using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Reverie.Common.Players;
using Reverie.Common.UI.SkillTree;
using Reverie.Core.Missions;
using Reverie.Core.Skills;
using Reverie.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Reverie.Common.UI
{
    public class MagicMirrorUI : UIState
    {
        private UIImage buttonTexture;
        private UIPanel mainPanel;
        private UIImageButton[] tabButtons;
        private UIText titleText;
        private UIElement[] tabContents;
        private UIPanel leftPanel;
        private UIImageButton backButton;

        private int currentTab = 0;
        private readonly int[] mirrors = [ItemID.MagicMirror, ItemID.IceMirror, ItemID.CellPhone, ItemID.Shellphone];
        private readonly string[] tabNames = [ "Missions", "Player Stats", "Skill Trees" ];
        private readonly string hoverText = "Open Magic Mirror";

        private bool isVisible = false;
        private bool isHovering = false;
        public override void OnInitialize()
        {
            buttonTexture = new UIImage(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Icon_Mirror"));
            buttonTexture.Width.Set(28, 0f);
            buttonTexture.Height.Set(28, 0f);
            buttonTexture.Left.Set(250f, 0f);
            buttonTexture.Top.Set(250f, 0f);
            buttonTexture.OnLeftClick += ToggleUI;
            buttonTexture.OnMouseOver += (evt, element) => isHovering = true;
            buttonTexture.OnMouseOut += (evt, element) => isHovering = false;
            Append(buttonTexture);

            // Main panel
            mainPanel = new UIPanel(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Panel_BG"), ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Panel"), 12, 4);
            mainPanel.SetPadding(10);
            mainPanel.Width.Set(800, 0f);
            mainPanel.Height.Set(500, 0f);
            mainPanel.HAlign = 0.5f;
            mainPanel.VAlign = 0.5f;
            mainPanel.BackgroundColor = new Color(50, 58, 119, 245);

            // Tab buttons
            tabButtons = new UIImageButton[3];
            for (int i = 0; i < 3; i++)
            {
                tabButtons[i] = new UIImageButton(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Tab"));
                tabButtons[i].Left.Set(10 + i * 40, 0f);
                tabButtons[i].Top.Set(10, 0f);
                tabButtons[i].Width.Set(32, 0f);
                tabButtons[i].Height.Set(32, 0f);
                int index = i;
                tabButtons[i].OnLeftClick += (evt, element) => SwitchTab(index);
                mainPanel.Append(tabButtons[i]);
            }

            // Title text
            titleText = new UIText("Missions");
            titleText.Left.Set(140, 0f);
            titleText.Top.Set(20, 0f);
            mainPanel.Append(titleText);

            // Left panel
            leftPanel = new UIPanel(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Panel_BG"), ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Panel"), 12, 4);
            leftPanel.Width.Set(250, 0f);
            leftPanel.Height.Set(-60, 1f);
            leftPanel.Left.Set(10, 0f);
            leftPanel.Top.Set(50, 0f);
            leftPanel.BackgroundColor = new Color(50, 58, 119, 245);
            mainPanel.Append(leftPanel);

            // Tab contents
            tabContents = new UIElement[3];
            tabContents[0] = new MissionBoardUI();
            tabContents[1] = new StatsUI();
            tabContents[2] = new SkillTreeManager();

            for (int i = 0; i < 3; i++)
            {
                tabContents[i].Width.Set(leftPanel.Width.Pixels, 1f);
                tabContents[i].Height.Set(leftPanel.Height.Pixels, 1f);
                tabContents[i].Left.Set(leftPanel.Left.Pixels, 0f);
                tabContents[i].Top.Set(leftPanel.Top.Pixels, 0f);
                tabContents[i].OnInitialize(); // Add this line to initialize each UI element

                mainPanel.Append(tabContents[i]);
            }

            SwitchTab(0);

            backButton = new UIImageButton(TextureAssets.MapDeath);
            backButton.Width.Set(32, 0f);
            backButton.Height.Set(32, 0f);
            backButton.Left.Set(740f, 0f);
            backButton.Top.Set(10f, 0f);
            backButton.OnLeftClick += CloseUI;
            mainPanel.Append(backButton);
        }

        private void ToggleUI(UIMouseEvent evt, UIElement listeningElement)
        {
            isVisible = !isVisible;
            if (isVisible)
            {
                Append(mainPanel);
                DisableOtherUI();
            }
            else
            {
                RemoveChild(mainPanel);
                EnableOtherUI();
            }
        }
        public void OpenUI()
        {
            if (!isVisible)
            {
                isVisible = true;
                Append(mainPanel);
                DisableOtherUI();
            }
        }

        private void CloseUI(UIMouseEvent evt, UIElement listeningElement)
        {
            isVisible = false;
            RemoveChild(mainPanel);
            EnableOtherUI();
        }

        private static void DisableOtherUI()
        {
            Main.playerInventory = false;
            Main.InGameUI.SetState(null);
            // Add any other UI elements that need to be disabled
        }

        private static void EnableOtherUI()
        {
            // Re-enable other UI elements if needed
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.playerInventory && Main.LocalPlayer.HasInInventory(mirrors))
            {
                buttonTexture.Left.Set(570f, 0f);
                buttonTexture.Top.Set(279f, 0f);
            }
            else
            {
                buttonTexture.Left.Set(-100f, 0f);
                buttonTexture.Top.Set(-100f, 0f);
            }
            if (isHovering)
            {
                CalculatedStyle dimensions = buttonTexture.GetDimensions();
                spriteBatch.Draw(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Icon_Mirror_Hover").Value, new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height), Color.White);

                Vector2 mousePosition = new(Main.mouseX, Main.mouseY);
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(hoverText);
                Vector2 textPosition = mousePosition + new Vector2(16, 16);

                if (textPosition.X + textSize.X > Main.screenWidth)
                    textPosition.X = Main.screenWidth - textSize.X - 4;
                if (textPosition.Y + textSize.Y > Main.screenHeight)
                    textPosition.Y = Main.screenHeight - textSize.Y - 4;
                
                ChatManager.DrawColorCodedStringShadow(spriteBatch, (DynamicSpriteFont)FontAssets.MouseText, hoverText, textPosition, Color.Black, 0f, default, Vector2.One * 1f);
                ChatManager.DrawColorCodedString(spriteBatch, (DynamicSpriteFont)FontAssets.MouseText, hoverText, textPosition, Color.White, 0f, default, Vector2.One * 1f);
            }
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (isVisible && mainPanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (buttonTexture.ContainsPoint(Main.MouseScreen))
            {
                isHovering = true;
                Main.LocalPlayer.mouseInterface = true;
            }
            else
            {
                isHovering = false;
            }
            if (Main.playerInventory)
            {
                isVisible = false;
                RemoveChild(mainPanel);
            }
        }

        private void SwitchTab(int tabIndex)
        {
            currentTab = tabIndex;
            titleText.SetText(tabNames[tabIndex]);
            for (int i = 0; i < 3; i++)
            {
                if (i == tabIndex)
                {
                    tabContents[i].Recalculate();
                    tabContents[i].Top.Set(50, 0f);
                }
                else
                {
                    tabContents[i].Top.Set(5000, 0f); // Move off-screen instead of removing
                }
            }
            UpdateLeftPanel();
        }

        private void UpdateLeftPanel()
        {
            leftPanel.RemoveAllChildren();
            switch (currentTab)
            {
                case 0: // Missions
                        // Add mission list or summary here
                    break;
                case 1: // Player Stats
                        // Add player stats summary here
                    break;
                case 2: // Skill Trees
                        // Add skill tree overview or navigation here
                    break;
            }
        }

        public void UpdatePlayers(ExperiencePlayer expPlayer, SkillPlayer skillPlayer, MissionPlayer missionPlayer)
        {
            for (int i = 0; i < tabContents.Length; i++)
            {
                if (tabContents[i] is SkillTreeUI skillTreeUI)
                {
                    skillTreeUI.UpdateSkillPoints(expPlayer, skillPlayer);
                }
                else if (tabContents[i] is MissionBoardUI missionBoardUI)
                {
                    missionBoardUI.SetMissionPlayer(missionPlayer);
                }
                else if (tabContents[i] is StatsUI statsUI)
                {
                    statsUI.UpdateStats(expPlayer.Player);
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            buttonTexture.Draw(spriteBatch);

            if (isVisible)
            {
                mainPanel.Draw(spriteBatch);
            }
        }
    }

    public class MissionBoardUI : UIElement
    {
        private UIList missionList;
        private UIScrollbar scrollbar;
        private MissionPlayer missionPlayer;
        private UIText descriptionText;
        private UIPanel sortPanel;
        private readonly string[] sortOptions = ["Available", "Active", "Completed"];
        private int selectedSort = 0;

        public MissionBoardUI() => OnInitialize();

        public override void OnInitialize()
        {
            // Initialize missionList if it's null
            missionList ??= [];
            missionList.SetPadding(8);
            missionList.Width.Set(0, 1f);
            missionList.Height.Set(300, 1f);
            Append(missionList);

            scrollbar = new UIScrollbar();
            scrollbar.SetView(100f, 1000f);
            scrollbar.Height.Set(-80, 1f);
            scrollbar.Top.Set(40, 0f);
            scrollbar.Left.Set(-20, 1f);
            Append(scrollbar);

            missionList.SetScrollbar(scrollbar);

            descriptionText = new UIText("", 0.8f);
            descriptionText.Top.Set(-220, 1f);
            descriptionText.Left.Set(258, 0f);
            descriptionText.Width.Set(100, 0f);
            descriptionText.Height.Set(100, 0f);
            descriptionText.TextColor = Color.White;
            Append(descriptionText);

            sortPanel = new UIPanel();
            sortPanel.Width.Set(0, 1f);
            sortPanel.Height.Set(30, 0f);

            for (int i = 0; i < sortOptions.Length; i++)
            {
                int index = i;
                UITextPanel<string> sortButton = new UITextPanel<string>(sortOptions[i], 0.7f);
                sortButton.Width.Set(100, 0f);
                sortButton.Height.Set(30, 0f);
                sortButton.Left.Set(i * 105, 0f);
                sortButton.OnLeftClick += (evt, element) => SelectSort(index);
                sortButton.BackgroundColor = (i == selectedSort) ? new Color(46, 60, 119) : new Color(73, 94, 171);
                sortPanel.Append(sortButton);
            }

            Append(sortPanel);
        }

        public void RefreshMissionList()
        {
            if (missionList == null)
            {
                OnInitialize();
            }

            missionList.Clear();

            if (missionPlayer != null)
            {
                IEnumerable<Mission> missionsToShow = selectedSort switch
                {
                    0 => missionPlayer.GetAvailableMissions(),
                    1 => missionPlayer.GetActiveMissions(),
                    2 => missionPlayer.GetCompletedMissions(),
                    _ => [],
                };
                AddMissionGroup("", missionsToShow, AcceptMission);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (missionPlayer == null || missionPlayer.Player != Main.LocalPlayer)
            {
                missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            }

            RefreshMissionList();
        }

        private void AddMissionGroup(string groupTitle, IEnumerable<Mission> missions, System.Action<Mission> buttonAction)
        {
            var missionList = missions.ToList();
            if (missionList.Count > 0)
            {
                foreach (var mission in missionList)
                {
                    var missionPanel = new UIPanel(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Panel_BG"), 
                        ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Panel"), 12, 4);
                    missionPanel.SetPadding(8);
                    missionPanel.Width.Set(0, 0.6f);
                    missionPanel.Height.Set(50, 0f);

                    var nameText = new UIText(mission.MissionData.Name, 0.7f);
                    nameText.Top.Set(4, 0f);
                    missionPanel.Append(nameText);

                    if (mission.Progress == MissionProgress.Inactive)
                    {
                        string buttonText = "Accept";
                        var actionButton = new UITextPanel<string>(buttonText, 0.66f);
                        actionButton.Width.Set(80, 0f);
                        actionButton.Height.Set(28, 0f);
                        actionButton.Left.Set(-76, 1f);
                        actionButton.Top.Set(-4, 0f);
                        actionButton.OnLeftClick += (evt, element) => buttonAction(mission);
                        actionButton.TextColor = (mission.Progress == MissionProgress.Inactive) ? Color.Yellow : Color.White;
                        missionPanel.Append(actionButton);
                    }

                    missionPanel.OnMouseOver += (evt, element) => ShowDescription(mission);
                    missionPanel.OnMouseOut += (evt, element) => HideDescription();

                    this.missionList.Add(missionPanel);
                }
            }
        }

        private void ShowDescription(Mission mission)
        {
            string description = $"{mission.MissionData.Description}\n\nRewards:";

            if (mission.MissionData.XPReward > 0)
            {
                description += $"\n- {mission.MissionData.XPReward} Exp.";
            }

            foreach (var reward in mission.MissionData.Rewards)
            {
                description += $"\n- [i:{reward.type}] {reward.stack}x {reward.Name}";
            }

            descriptionText.SetText(description);
        }

        private void HideDescription()
        {
            descriptionText.SetText("");
        }

        private void AcceptMission(Mission mission)
        {
            if (mission.Progress == MissionProgress.Inactive)
            {
                missionPlayer.StartMission(mission.ID);
                Main.NewText($"Accepted mission: {mission.MissionData.Name}!", Color.Green);
                RefreshMissionList();
            }
        }

        private void SelectSort(int index)
        {
            selectedSort = index;

            foreach (var child in sortPanel.Children)
            {
                if (child is UITextPanel<string> sortButton)
                {
                    int buttonIndex = sortPanel.Children.ToList().IndexOf(sortButton);
                    sortButton.BackgroundColor = (buttonIndex == selectedSort) ? new Color(46, 60, 119) : new Color(73, 94, 171);
                }
            }
            RefreshMissionList();
        }

        public void SetMissionPlayer(MissionPlayer player) => missionPlayer = player;
    }

    public class UIProgressBar : UIElement
    {
        private float progress;
        private Color barColor = Color.Green;

        public void SetProgress(float value)
        {
            progress = Math.Clamp(value, 0f, 1f);
        }

        public void SetColor(Color color)
        {
            barColor = color;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // Draw background
            Rectangle dimensions = GetDimensions().ToRectangle();
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, dimensions, Color.Gray * 0.5f);

            // Draw progress
            int progressWidth = (int)(dimensions.Width * progress);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(dimensions.X, dimensions.Y, progressWidth, dimensions.Height),
                barColor);
        }
    }
    public class SkillTreeManager : SkillTreeUI
    {
        protected override void InitializeSkillTrees()
        {
            InitializeExcavationSkillTree(skillTreeContents[0]);
            InitializeCombatSkillTree(skillTreeContents[1]);
            InitializeCraftingSkillTree(skillTreeContents[2]);
        }

        private void InitializeExcavationSkillTree(UIElement container)
        {
            colorBG = new Color(123, 123, 123, 135);
            CreateSkillButton(90, 0, null, SkillList.IDs.WithHaste);
            CreateSkillButton(90, 60, skillButtons[0], SkillList.IDs.Fortune);
            CreateSkillButton(20, 60, skillButtons[1], SkillList.IDs.YearnForTheMines);
        }

        private void InitializeCombatSkillTree(UIElement container)
        {
            colorBG = new Color(123, 88, 123, 135);
            CreateSkillButton(90, 0, null, SkillList.IDs.WithHaste);
            CreateSkillButton(90, 60, skillButtons[0], SkillList.IDs.Fortune);
            CreateSkillButton(20, 60, skillButtons[1], SkillList.IDs.YearnForTheMines);
        }

        private void InitializeCraftingSkillTree(UIElement container)
        {
            colorBG = new Color(75, 123, 223, 135);
            CreateSkillButton(90, 0, null, SkillList.IDs.WithHaste);
            CreateSkillButton(90, 60, skillButtons[0], SkillList.IDs.Fortune);
            CreateSkillButton(20, 60, skillButtons[1], SkillList.IDs.YearnForTheMines);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateSkillButtons();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            if (mainPanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        public void UpdateSkillButtons()
        {
            if (Main.LocalPlayer?.active == true && Main.LocalPlayer.GetModPlayer<SkillPlayer>() is SkillPlayer skillPlayer)
            {
                foreach (var button in skillButtons)
                {
                    Skill skill = SkillList.GetSkillById(button.SkillId);
                    if (skill != null)
                    {
                        int currentStack = skillPlayer.GetSkillStack(skill.ID);
                        button.SetInitialStack(currentStack);

                    }
                    else
                    {
                        Main.NewText($"[DEBUG] Skill not found for button with ID {button.SkillId}", Color.Red);
                    }
                }
            }
        }
    }

    public class ProgressUI : UIElement { }
}