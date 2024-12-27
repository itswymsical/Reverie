using Reverie.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Reverie.Common.Configs;

namespace Reverie.Common.UI
{
    internal class ExperienceMeter : UIState
    {
        private UIText text;
        private UIText level;
        private UIElement area;
        private UIImage barFrame;

        public override void OnInitialize()
        {

            area = new UIElement();
            area.Left.Set(-area.Width.Pixels - 472, 1f);
            area.Top.Set(14, 0f);
            area.Width.Set(182, 0f);
            area.Height.Set(60, 0f);

            barFrame = new UIImage(ModContent.Request<Texture2D>($"{Assets.UI.ExperienceMeter}XP_Panel_Empty"));
            barFrame.Left.Set(22, 0f);
            barFrame.Top.Set(0, 0f);
            barFrame.Width.Set(142, 0f);
            barFrame.Height.Set(40, 0f);

            text = new UIText("0/0", 0.8f);
            text.Width.Set(142, 0f);
            text.Height.Set(40, 0f);
            text.Top.Set(-8, 0f);
            text.Left.Set(17, 0f);
            area.Append(text);

            level = new UIText("0", 0.8f);
            level.Width.Set(142, 0f);
            level.Height.Set(40, 0f);
            level.Top.Set(9.65f, 0f);
            level.Left.Set(78f, 0f);         

            area.Append(barFrame);
            area.Append(level);
            Append(area);
        }

        public override void Draw(SpriteBatch spriteBatch) => base.Draw(spriteBatch);    

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            var modPlayer = Main.LocalPlayer.GetModPlayer<ExperiencePlayer>();
            var config = ModContent.GetInstance<ExperienceMeterConfig>();
            float xpPercentage = (float)modPlayer.experienceValue / ExperiencePlayer.GetNextExperienceThreshold(modPlayer.experienceLevel);
            xpPercentage = MathHelper.Clamp(xpPercentage, 0f, 1f);

            Rectangle hitbox = barFrame.GetInnerDimensions().ToRectangle();

            // Background and star overlay draws
            spriteBatch.Draw(
                ModContent.Request<Texture2D>($"{Assets.UI.ExperienceMeter}XP_Fill_Empty").Value,
                hitbox,
                config.BarColor
            );

            spriteBatch.Draw(
                ModContent.Request<Texture2D>($"{Assets.UI.ExperienceMeter}XP_Star").Value,
                hitbox,
                config.BarColor
            );

            // Precise XP fill calculation
            int fillWidth = (int)(hitbox.Width * xpPercentage - 25f);
            for (int i = 0; i < fillWidth; i++)
            {
                spriteBatch.Draw(
                    ModContent.Request<Texture2D>($"{Assets.UI.ExperienceMeter}XP_Fill").Value,
                    new Rectangle(hitbox.X + i, hitbox.Y + hitbox.Height - 28, 1, 12),
                    config.BarColor
                );
            }
        }

        public override void Update(GameTime gameTime)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<ExperiencePlayer>();
            text.SetText($"Experience: {modPlayer.experienceValue} / {ExperiencePlayer.GetNextExperienceThreshold(modPlayer.experienceLevel)}", 0.6f, false);
            level.SetText($"{modPlayer.experienceLevel}", 0.8f, false);

            base.Update(gameTime);
        }
    }

    [Autoload(Side = ModSide.Client)]
    internal class ExperienceUISystem : ModSystem
    {
        private UserInterface ExperienceMeterUserInterface;
        private ExperienceMeter ExperienceMeter;

        public override void Load()
        {
            ExperienceMeter = new();
            ExperienceMeterUserInterface = new();
            ExperienceMeterUserInterface.SetState(ExperienceMeter);
        }

        public override void UpdateUI(GameTime gameTime) => ExperienceMeterUserInterface?.Update(gameTime);
        
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "ReverieMod: Experience Meter",
                    delegate
                    {
                        ExperienceMeterUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}