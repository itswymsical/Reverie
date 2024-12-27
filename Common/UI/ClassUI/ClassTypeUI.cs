using Reverie.Common.Players;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Reverie.Common.Systems;
using System.Collections.Generic;
using Reverie.Core.Missions;

namespace Reverie.Common.UI.ClassUI
{
    internal class ClassSelectionUI : UIState
    {
        private UIElement backgroundPanel;
        private List<ClassColumn> classColumns;

        private class ClassColumn : UIElement
        {
            public UIImageButton Button { get; private set; }
            public UIText ClassName { get; private set; }

            public ClassColumn(string texturePath, string className)
            {
                Asset<Texture2D> buttonTexture = ModContent.Request<Texture2D>(texturePath);
                Button = new UIImageButton(buttonTexture);
                Button.Width.Set(112, 0f);
                Button.Height.Set(106, 0f);
                Button.Top.Set(0, 0f);
                Button.HAlign = 0.5f;
                Append(Button);

                ClassName = new UIText(className, 0.8f);
                ClassName.Top.Set(78, 0f);
                ClassName.HAlign = 0.57f;
                Append(ClassName);

                Width.Set(100, 0f);
                Height.Set(100, 0f);
            }
        }

        public override void OnInitialize()
        {
            backgroundPanel = new();
            backgroundPanel.SetPadding(10);
            backgroundPanel.Left.Set(0, 0.2f);
            backgroundPanel.Top.Set(0, 0.2f);
            backgroundPanel.Width.Set(0, 0.6f);
            backgroundPanel.Height.Set(0, 0.6f);
            Append(backgroundPanel);

            classColumns =
            [
                new ClassColumn($"{Assets.UI.ClassSelection}Class_Frame", "Warrior"),
                new ClassColumn($"{Assets.UI.ClassSelection}Class_Frame", "Mage"),
                new ClassColumn($"{Assets.UI.ClassSelection}Class_Frame", "Marksman"),
                new ClassColumn($"{Assets.UI.ClassSelection}Class_Frame", "Conjurer")
            ];
            MissionPlayer missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            Mission Reawakening = missionPlayer.GetMission(MissionID.Reawakening);

            float columnSpacing = 1f / (classColumns.Count + 1);
            for (int i = 0; i < classColumns.Count; i++)
            {
                ClassColumn column = classColumns[i];
                column.Left.Set(0, columnSpacing * (i + 1) - 0.05f);
                column.Top.Set(0, 0.3f);
                backgroundPanel.Append(column);

                column.Button.OnLeftClick += (evt, element) => {
                    SetPlayerClass(column.ClassName.Text);
                    ModContent.GetInstance<ReverieUISystem>().ClassInterface.SetState(null);
                    Main.NewText($"You have selected the {column.ClassName.Text} path!", Color.Yellow);
                    if (Reawakening.CurrentSetIndex == 1)
                        Reawakening.UpdateProgress(0);
                };
            }

            UIText title = new("Select Path", 1.2f);
            title.HAlign = 0.5f;
            title.Top.Set(20, 0f);
            backgroundPanel.Append(title);
        }

        private void SetPlayerClass(string className)
        {
            ReveriePlayer player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();

            switch (className)
            {
                case "Warrior":
                    player.pathWarrior = true;
                    break;
                case "Mage":
                    player.pathMage = true;
                    break;
                case "Marksman":
                    player.pathMarksman = true;
                    break;
                case "Conjurer":
                    player.pathConjurer = true;
                    break;
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}