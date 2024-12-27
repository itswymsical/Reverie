using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework;
using Reverie.Helpers;
using Reverie.Common.Players;
using Reverie.Common.UI.SkillTree;
using Reverie.Core.Missions;

namespace Reverie.Common.UI
{
    public class MirrorNetUI : UIState
    {
        private UIElement area;

        private UIImage mirrorButton;
        private UIImage mainPanel;

        private UIImageButton missionButton;
        private UIImageButton statsButton;
        private UIImageButton skillsButton;

        private UIElement[] tabContents;
        private UIImageButton[] tabButtons;
        private readonly string[] tabNames = ["Missions", "Stats", "Skills"];

        private UIImageButton closeButton;
        private UIText titleText;

        private readonly string hoverText = "Check MirrorNet";
        private readonly int[] countsAsMagicMirror =
            [ItemID.MagicMirror, ItemID.IceMirror, ItemID.CellPhone, ItemID.Shellphone]; //Player will require a mirror for the UI to pop up

        public override void OnInitialize()
        {
            #region Mirror Button
            mirrorButton = new UIImage(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}MirrorButton"));
            mirrorButton.Width.Set(28, 0f);
            mirrorButton.Height.Set(28, 0f);
            mirrorButton.Left.Set(250f, 0f);
            mirrorButton.Top.Set(250f, 0f);
            mirrorButton.OnLeftClick += ToggleUI;
            mirrorButton.OnMouseOver += (evt, element) => isHovering = true;
            mirrorButton.OnMouseOut += (evt, element) => isHovering = false;
            Append(mirrorButton);
            #endregion

            area = new() { VAlign = 0.425f };
            area.HAlign = area.VAlign;
            area.Width.Set(282, 0f);
            area.Height.Set(298, 0f);

            mainPanel = new UIImage(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Mirror_Panel"));
            mainPanel.Width.Set(area.Width.Pixels, 0f);
            mainPanel.Height.Set(area.Height.Pixels, 0f);
            mainPanel.HAlign = area.VAlign;
            mainPanel.VAlign = area.VAlign;
            area.Append(mainPanel);

            titleText = new UIText("Missions");
            titleText.Left.Set(area.Width.Pixels / 1.75f, 0f);
            titleText.Top.Set(10, 0f);
            mainPanel.Append(titleText);

            #region TAB CONTENT

            tabButtons = new UIImageButton[3];
            for (int i = 0; i < 3; i++)
            {
                tabButtons[i] = new UIImageButton(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Mirror_Tab_Left"));
                tabButtons[i].Left.Set(area.Left.Pixels, 0f);
                tabButtons[i].Top.Set((area.Top.Pixels + 5.5f) + i * 34, 0f);
                tabButtons[i].Width.Set(32, 0f);
                tabButtons[i].Height.Set(32, 0f);
                int index = i;
                tabButtons[i].OnLeftClick += (evt, element) => SwitchTab(index);
                area.Append(tabButtons[i]);
            }

            tabContents = new UIElement[3];
            tabContents[0] = new MissionListUI();
            tabContents[1] = new StatsUI();
            tabContents[2] = new SkillTreeManager();


            for (int i = 0; i < 3; i++)
            {
                tabContents[i].Width.Set(area.Width.Pixels, 1f);
                tabContents[i].Height.Set(area.Height.Pixels, 1f);
                tabContents[i].Left.Set(area.Left.Pixels, 0f);
                tabContents[i].Top.Set(area.Top.Pixels, 0f);
                tabContents[i].OnInitialize();

                mainPanel.Append(tabContents[i]);
            }
            #endregion

            closeButton = new UIImageButton(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}BackButton"));
            closeButton.Width.Set(30, 0f);
            closeButton.Height.Set(closeButton.Width.Pixels, 0f);
            closeButton.Left.Set(area.Width.Pixels - (closeButton.Width.Pixels + 4), 0f);
            closeButton.Top.Set(area.Top.Pixels + 4, 0f);
            closeButton.OnLeftClick += CloseUI;

            SwitchTab(0);
            area.Append(closeButton);
            Append(area);
        }

        private bool isVisible = false;
        private bool isHovering = false;

        private void ToggleUI(UIMouseEvent evt, UIElement listeningElement)
        {
            isVisible = !isVisible;
            if (isVisible)
            {
                Append(area);
                DisableOtherUI();
            }
            else
            {
                RemoveChild(area);
            }
        }

        public void OpenUI()
        {
            if (!isVisible)
            {
                isVisible = true;
                Append(area);
                DisableOtherUI();
            }
        }

        private void CloseUI(UIMouseEvent evt, UIElement listeningElement)
        {
            isVisible = false;
            RemoveChild(area);
        }

        private static void DisableOtherUI()
        {
            Main.playerInventory = false;
            Main.InGameUI.SetState(null);
        }

        public void UpdatePlayers(ExperiencePlayer expPlayer, SkillPlayer skillPlayer, MissionPlayer missionPlayer)
        {
            for (int i = 0; i < tabContents.Length; i++)
            {
                if (tabContents[i] is SkillTreeUI skillTreeUI)
                {
                    skillTreeUI.UpdateSkillPoints(expPlayer, skillPlayer);
                }
                if (tabContents[i] is MissionBoardUI missionBoardUI)
                {
                    missionBoardUI.SetMissionPlayer(missionPlayer);
                }
                else if (tabContents[i] is StatsUI statsUI)
                {
                    statsUI.UpdateStats(expPlayer.Player);
                }
            }
        }

        private void SwitchTab(int tabIndex)
        {
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
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.playerInventory && Main.LocalPlayer.HasInInventory(countsAsMagicMirror))
            {
                mirrorButton.Left.Set(570f, 0f);
                mirrorButton.Top.Set(279f, 0f);
            }
            else
            {
                mirrorButton.Left.Set(-100f, 0f);
                mirrorButton.Top.Set(-100f, 0f);
            }
            if (isHovering)
            {
                CalculatedStyle dimensions = mirrorButton.GetDimensions();
                spriteBatch.Draw(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}MirrorButton_Hover").Value, new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height), Color.White);

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

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            mirrorButton.Draw(spriteBatch);

            if (isVisible)
            {
                area.Draw(spriteBatch);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (isVisible && mainPanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (mirrorButton.ContainsPoint(Main.MouseScreen))
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

    }

    public class MissionListUI : UIElement
    {
        private UIImageButton sortButton;
        private UIImage missionPanel;
        private int currentSortMode = 0;
        private readonly string[] sortModes = ["Active", "Available", "Complete"];

        private List<Mission> currentlyDisplayedMissions = [];
        private MissionPlayer missionPlayer;
        public override void OnInitialize()
        {
            Width.Set(282, 0f);
            Height.Set(298, 0f);

            missionPanel = new UIImage(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Mirror_MissionList"));
            missionPanel.Width.Set(GetDimensions().Width, 0f);
            missionPanel.Height.Set(GetDimensions().Height, 0f);
            Append(missionPanel);

            sortButton = new UIImageButton(ModContent.Request<Texture2D>($"{Assets.UI.MagicMirror}Mirror_MissionList_SortButton"));
            sortButton.Width.Set(22, 0f);
            sortButton.Height.Set(22, 0f);
            sortButton.Left.Set(Left.Pixels + 40, 0f);
            sortButton.Top.Set(20, 0f);
            sortButton.OnLeftClick += CycleSortMode;
            missionPanel.Append(sortButton);



            RefreshMissionList();
        }

        private void CycleSortMode(UIMouseEvent evt, UIElement listeningElement)
        {
            currentSortMode = (currentSortMode + 1) % sortModes.Length;
            RefreshMissionList();
        }

        private void RefreshMissionList()
        {
            currentlyDisplayedMissions.Clear();
            if (missionPlayer != null)
            {
                switch (currentSortMode)
                {
                    case 0:
                        currentlyDisplayedMissions.AddRange(missionPlayer.GetActiveMissions());
                        break;
                    case 1:
                        currentlyDisplayedMissions.AddRange(missionPlayer.GetActiveMissions());
                        break;
                    case 2:
                        currentlyDisplayedMissions.AddRange(missionPlayer.GetCompletedMissions());
                        break;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            Vector2 sortTextPos = GetDimensions().Position() + new Vector2(68, 22);
            ChatManager.DrawColorCodedStringWithShadow(
                spriteBatch,
                FontAssets.MouseText.Value,
                sortModes[currentSortMode],
                sortTextPos,
                Color.White,
                shadowColor: Color.Black,
                0f,
                Vector2.Zero,
                Vector2.One
            );

            Vector2 missionEntryStart = GetDimensions().Position() + new Vector2(68, 30);
            float yOffset = 0;

            foreach (var mission in currentlyDisplayedMissions)
            {
                if (yOffset + 40 > Height.Pixels - 20) break; // Stop if we're about to overflow

                DrawMissionEntry(spriteBatch, mission, missionEntryStart + new Vector2(0, yOffset));
                yOffset += 40;
            }
        }

        private void DrawMissionEntry(SpriteBatch spriteBatch, Mission mission, Vector2 position)
        {
            // Draw mission name
            ChatManager.DrawColorCodedString(
                spriteBatch,
                FontAssets.MouseText.Value,
                mission.MissionData.Name,
                position,
                Color.White,
                0f,
                Vector2.Zero,
                Vector2.One * 0.9f
            );

            // Draw mission description/progress
            ChatManager.DrawColorCodedString(
                spriteBatch,
                FontAssets.MouseText.Value,
                mission.MissionData.Description,
                position + new Vector2(0, 20),
                Color.Gray,
                0f,
                Vector2.Zero,
                Vector2.One * 0.8f
            );
        }
    }

    public class StatsUI : UIElement
    {
        private UIText levelText;
        private UIText experienceText;
        private UIText damageText;
        private UIText deathsText;
        private UIText bossKillsText;

        private ExperiencePlayer expPlayer;

        public override void OnInitialize()
        {
            Width.Set(282, 0f);
            Height.Set(298, 0f);

            levelText = new UIText("Level: 0");
            levelText.Top.Set(10, 0f);
            levelText.Left.Set(10, 0f);
            Append(levelText);

            // We need to append and draw the player portait

            experienceText = new UIText("XP: 0 / 0");
            experienceText.Top.Set(40, 0f);
            experienceText.Left.Set(10, 0f);
            Append(experienceText);

            damageText = new UIText("Total Damage: 0");
            damageText.Top.Set(70, 0f);
            damageText.Left.Set(10, 0f);
            Append(damageText);

            deathsText = new UIText("Deaths: 0");
            deathsText.Top.Set(100, 0f);
            deathsText.Left.Set(10, 0f);
            Append(deathsText);

            bossKillsText = new UIText("Boss Kills: 0");
            bossKillsText.Top.Set(130, 0f);
            bossKillsText.Left.Set(10, 0f);
            Append(bossKillsText);
        }

        public void UpdateStats(Player player)
        {
            expPlayer = player.GetModPlayer<ExperiencePlayer>();
            var mPlayer = player.GetModPlayer<MissionPlayer>();

            levelText.SetText($"Level: {expPlayer.experienceLevel}");
            experienceText.SetText($"EXP: {expPlayer.experienceValue} / {ExperiencePlayer.GetNextExperienceThreshold(expPlayer.experienceLevel)}");
            deathsText.SetText($"Deaths: {player.numberOfDeathsPVE}");
            bossKillsText.SetText($"Missions Completed: {mPlayer.missionDict.Values.Where(m => m.Progress == MissionProgress.Completed).Count()}");

        }
    }
}