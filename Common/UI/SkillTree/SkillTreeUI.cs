using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Reverie.Common.Players;
using Reverie.Common.Tiles;
using Reverie.Core.Skills;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace Reverie.Common.UI.SkillTree
{
    public abstract class SkillTreeUI : UIElement
    {
        protected UIPanel mainPanel;
        protected UIText skillPoints;
        protected List<StackableSkillButton> skillButtons;
        protected int availableSkillPoints;

        protected ExperiencePlayer expPlayer;
        protected SkillPlayer skillPlayer;

        protected UIImageButton[] skillTreeTabButtons;
        protected UIElement[] skillTreeContents;
        protected UIText skillTreeTitleText;
        protected int currentSkillTreeTab = 0;
        protected string[] skillTreeNames = ["Excavation", "Combat", "Defense"];

        protected UIPanel tabPanel;
        protected const float TAB_WIDTH = 40f;
        protected const float TAB_HEIGHT = 30f;
        protected const float TAB_SPACING = 10f;


        public Color colorBG { get; set; } = new Color(73, 94, 171, 135);

        public SkillTreeUI()
        {
            availableSkillPoints = 0;
            skillButtons = new List<StackableSkillButton>();
            mainPanel = new UIPanel();
            skillPoints = new UIText("Skill Points: 0");
        }

        public override void OnInitialize()
        {
            mainPanel.Width.Set(250, 0f);
            mainPanel.Height.Set(10, 1f);
            mainPanel.Left.Set(20, 0f);
            mainPanel.Top.Set(0, 0f);
            mainPanel.BackgroundColor = colorBG;

            skillPoints.Width.Set(50, 0f);
            skillPoints.Height.Set(30, 0f);
            skillPoints.Left.Set(-250, 1f);
            skillPoints.Top.Set(350, 0f);
            skillPoints.TextColor = Color.White;
            skillPoints.ShadowColor = Color.Black;

            mainPanel.Append(skillPoints);
            Append(mainPanel);

            InitializeSkillTreeTabs();
        }

        protected void InitializeSkillTreeTabs()
        {
            tabPanel = new UIPanel();
            tabPanel.Width.Set(TAB_WIDTH, 0f);
            tabPanel.Height.Set(-20, 1f); // Full height minus some padding
            tabPanel.Left.Set(-TAB_WIDTH - 5, 1f); // Position on the right side
            tabPanel.Top.Set(10, 0f);
            tabPanel.BackgroundColor = new Color(50, 58, 119, 225); // Slightly transparent
            mainPanel.Append(tabPanel);

            skillTreeTabButtons = new UIImageButton[skillTreeNames.Length];
            for (int i = 0; i < skillTreeNames.Length; i++)
            {
                skillTreeTabButtons[i] = new UIImageButton(Request<Texture2D>($"{Assets.UI.SkillTree}" + skillTreeNames[i]));
                skillTreeTabButtons[i].Left.Set(-2, 0f);
                skillTreeTabButtons[i].Top.Set(i * (TAB_HEIGHT + TAB_SPACING), 0f);
                skillTreeTabButtons[i].Width.Set(TAB_WIDTH, 0f);
                skillTreeTabButtons[i].Height.Set(TAB_HEIGHT, 0f);
                int index = i;
                skillTreeTabButtons[i].OnLeftClick += (evt, element) => SwitchSkillTreeTab(index);
                tabPanel.Append(skillTreeTabButtons[i]);
            }

            skillTreeTitleText = new UIText(skillTreeNames[0]);
            skillTreeTitleText.Left.Set(10, 0f);
            skillTreeTitleText.Top.Set(10, 0f);
            mainPanel.Append(skillTreeTitleText);

            skillTreeContents = new UIElement[skillTreeNames.Length];
            for (int i = 0; i < skillTreeNames.Length; i++)
            {
                skillTreeContents[i] = new UIElement();
                skillTreeContents[i].Width.Set(-TAB_WIDTH - 30, 1f); // Adjust width to account for tab panel
                skillTreeContents[i].Height.Set(-60, 1f);
                skillTreeContents[i].Left.Set(10, 0f);
                skillTreeContents[i].Top.Set(40, 0f);
                mainPanel.Append(skillTreeContents[i]);
            }

            InitializeSkillTrees();
            SwitchSkillTreeTab(0);
        }

        protected void SwitchSkillTreeTab(int tabIndex)
        {
            currentSkillTreeTab = tabIndex;
            skillTreeTitleText.SetText($"{skillTreeNames[tabIndex]}");
            for (int i = 0; i < skillTreeContents.Length; i++)
            {
                if (i == tabIndex)
                {
                    skillTreeContents[i].Top.Set(40, 0f);
                }
                else
                {
                    skillTreeContents[i].Top.Set(5000, 0f); // Move off-screen
                }
            }

            // Highlight the selected tab
            for (int i = 0; i < skillTreeTabButtons.Length; i++)
            {
                skillTreeTabButtons[i].SetVisibility(1f, i == tabIndex ? 0.8f : 0.5f);
            }
        }

        protected abstract void InitializeSkillTrees();

        public void UpdateSkillPoints(ExperiencePlayer expPlayer, SkillPlayer skillPlayer)
        {
            this.expPlayer = expPlayer;
            this.skillPlayer = skillPlayer;
            availableSkillPoints = expPlayer.skillPoints;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            skillPoints.SetText($"Skill Points: {availableSkillPoints}");
            Recalculate();
        }

        public void CreateSkillButton(float x, float y, StackableSkillButton parentSkill, int skillId)
        {
            Skill skill = SkillList.GetSkillById(skillId);
            if (skill == null) return;

            StackableSkillButton button = new StackableSkillButton(
                Request<Texture2D>($"{Assets.UI.SkillTree}{ skill.Name.Replace(" ", "")}"),
                parentSkill,
                skill
            );

            button.Left.Set(x, 0f);
            button.Top.Set(y, 0f);

            button.OnLeftClick += (evt, element) =>
            {
                if (skillPlayer != null && button.CanAddStack())
                {
                    if (expPlayer.TryUseSkillPoint())
                    {
                        skillPlayer.AddSkillStack(skillId);
                        button.AddStack();
                        availableSkillPoints--;
                        SoundEngine.PlaySound(new SoundStyle($"{Assets.SFX.Directory}SkillUnlock")
                        {
                            Volume = 0.75f,
                            PitchVariance = 0f,
                            MaxInstances = 3,
                        },
                        skillPlayer.Player.position);
                    }
                }
                else
                {
                    SoundEngine.PlaySound(new SoundStyle($"{Assets.SFX.Directory}SkillLocked")
                    {
                        Volume = 0.75f,
                        PitchVariance = 0f,
                        MaxInstances = 3,
                    },
                    skillPlayer.Player.position);
                }
            };
            skillButtons.Add(button);
            mainPanel.Append(button);
        }
    }
    public class StackableSkillButton : UIImageButton
    {
        public int SkillId { get; private set; }
        public int CurrentStack { get; private set; }
        public bool UnlockedSkill { get; private set; }
        public int MaxStack { get; private set; }
        public StackableSkillButton ParentSkill { get; private set; }
        public string SkillName { get; private set; }
        public string SkillDescription { get; private set; }

        private Texture2D[] borderTextures;
        private Texture2D lockTexture;
        private bool isHovering;
        private Vector2 hoverPosition;

        private Skill _skill;
        public StackableSkillButton(Asset<Texture2D> texture, StackableSkillButton parentSkill, Skill skill)
          : base(texture)
        {
            SkillId = skill.ID;
            ParentSkill = parentSkill;
            _skill = skill;
            CurrentStack = 0;
            UnlockedSkill = false;
            MaxStack = skill.MaxStack;
            SkillName = skill.Name;
            SkillDescription = skill.Description;

            Width.Set(50, 0f);
            Height.Set(50, 0f);

            borderTextures =
            [
            Request<Texture2D>($"{Assets.UI.SkillTree}EmptySkill").Value,
            Request<Texture2D>($"{Assets.UI.SkillTree}EmptySkill").Value,
            Request<Texture2D>($"{Assets.UI.SkillTree}EmptySkill").Value,
            Request<Texture2D>($"{Assets.UI.SkillTree}EmptySkill").Value,
            Request<Texture2D>($"{Assets.UI.SkillTree}EmptySkill").Value
            ];
            lockTexture = Request<Texture2D>($"{Assets.UI.SkillTree}EmptySkill").Value;
        }

        public void SetInitialStack(int stack)
        {
            CurrentStack = Math.Min(stack, MaxStack);
            // You might want to add some visual update here if needed
        }
        public void CheckIfUnlocked(bool unlocked)
        {
            UnlockedSkill = unlocked;
        }
        public bool CanAddStack()
        {
            bool parentCondition = ParentSkill == null || ParentSkill.CurrentStack > 0;
            bool stackCondition = CurrentStack < MaxStack;
            return parentCondition && stackCondition;
        }

        public void AddStack()
        {
            if (CanAddStack())
            {
                CurrentStack++;
            }
        }

        private int GetBorderIndex()
        {
            if (CurrentStack == 0) return -1;
            float progress = (float)CurrentStack / MaxStack;
            return Math.Min((int)(progress * borderTextures.Length), borderTextures.Length - 1);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            CalculatedStyle dimensions = GetDimensions();

            // Draw border if unlocked
            int borderIndex = GetBorderIndex();
            if (borderIndex >= 0)
            {
                spriteBatch.Draw(borderTextures[borderIndex], dimensions.ToRectangle(), Color.White);
            }

            // Draw lock icon if locked
            if (CurrentStack == 0)
            {
                Vector2 lockPosition = new(dimensions.X + dimensions.Width / 2 - lockTexture.Width / 2,
                                                   dimensions.Y + dimensions.Height / 2 - lockTexture.Height / 2);
                spriteBatch.Draw(lockTexture, lockPosition, Color.White);
            }
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            isHovering = true;
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            isHovering = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (isHovering)
            {
                DrawTooltip(spriteBatch);
            }
        }

        private void DrawTooltip(SpriteBatch spriteBatch)
        {
            string tooltipText = GetTooltipText();
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(tooltipText);

            // Calculate tooltip position using Main.MouseScreen
            Vector2 position = Main.MouseScreen + new Vector2(16, 16);

            // Ensure tooltip doesn't go off-screen
            if (position.X + textSize.X > Main.screenWidth)
                position.X = Main.screenWidth - textSize.X - 4;
            if (position.Y + textSize.Y > Main.screenHeight)
                position.Y = Main.screenHeight - textSize.Y - 4;

            Rectangle tooltipRect = new((int)position.X - 4, (int)position.Y - 4, (int)textSize.X + 8, (int)textSize.Y + 8);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, tooltipRect, Color.Black * 0.8f);

            spriteBatch.DrawString(FontAssets.MouseText.Value, tooltipText, position, Color.White);
        }

        private string GetTooltipText()
        {
            string type = _skill.IsAdvancement ? "[ADVANCEMENT]" : _skill.IsAugment ? "[AUGMENTATION]" : "";
            string status = $"[{CurrentStack}/{MaxStack}]";
            string effect = $"[Current]: {_skill.GetEffectForStack(CurrentStack):P0}";
            string nextLevel = CanAddStack()
                ? $"Next Upgrade: {_skill.GetEffectForStack(CurrentStack + 1):P0}"
                : "[Maxed Out]";
            string unlockInfo = CanAddStack()
                ? "Upgrade"
                : (CurrentStack == 0 ? "[Locked, requires at least 1 stack of the parent skill(s)]" : "");

            return $"{SkillName} {type} {status}\n{SkillDescription}\n{effect}\n{nextLevel}\n{unlockInfo}";
        }
    }
}