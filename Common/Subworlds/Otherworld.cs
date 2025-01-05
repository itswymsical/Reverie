using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Common.Systems.Subworlds
{
    public abstract class Otherworld : Subworld
    {
        private int currentFrame;
        private double frameTime;
        private const int totalFrames = 19;
        private const double timePerFrame = 1;
        private readonly float fadeInDuration = 90f;
        private double elapsedTime;

        private string currentTip;
        private double tipTimer;
        private const double tipDuration = 5.0;

        public virtual Texture2D LoadingScreen { get; protected set; }
        public virtual Texture2D OtherworldTitle { get; protected set; }
        public virtual Texture2D LoadingIcon { get; protected set; }

        public override void SetStaticDefaults()
        {
            LoadingScreen = (Texture2D)ModContent.Request<Texture2D>(
                $"{nameof(Reverie)}" +
                $"/{Assets.Backgrounds}" +
                $"TransitionBG");
            OtherworldTitle = TextureAssets.Logo.Value;
            LoadingIcon = TextureAssets.LoadingSunflower.Value;
        }

        public override void DrawSetup(GameTime gameTime)
        {
            PlayerInput.SetZoom_UI();
            
            Main.instance.GraphicsDevice.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            DrawMenu(gameTime);
            
            Main.DrawCursor(Main.DrawThickCursor());
            Main.spriteBatch.End();
        }

        public override void DrawMenu(GameTime gameTime)
        {
            float opacity = MathHelper.Clamp((float)(elapsedTime / fadeInDuration), 0f, 1f);

            elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            frameTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (frameTime >= timePerFrame)
            {
                frameTime -= timePerFrame;
                currentFrame = (currentFrame + 1) % totalFrames;
            }
            // Draw background
            if (LoadingScreen != null)
            {
                Main.spriteBatch.Draw(LoadingScreen, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * opacity);
            }

            // Draw title
            if (OtherworldTitle != null)
            {
                Vector2 nameTexturePosition = new(10, 10);
                Main.spriteBatch.Draw(OtherworldTitle, nameTexturePosition, Color.White * opacity);
            }

            // Draw loading icon
            if (LoadingIcon != null)
            {
                int frameWidth = LoadingIcon.Width;
                int frameHeight = LoadingIcon.Height / totalFrames;
                Rectangle sourceRectangle = new(0, currentFrame * frameHeight, frameWidth, frameHeight);
                Vector2 iconPosition = new(Main.screenWidth - LoadingIcon.Width - 10, Main.screenHeight - frameHeight - 10);
                Main.spriteBatch.Draw(LoadingIcon, iconPosition, sourceRectangle, Color.White * opacity);
            }

            UpdateTip(gameTime);
            string tipsText = currentTip;

            Vector2 tipsSize = FontAssets.MouseText.Value.MeasureString(tipsText);
            Vector2 tipsPosition = new(Main.screenWidth / 2, Main.screenHeight - 40);

            Color tipsColor = Color.White * opacity;
            Color tipsShadowColor = Color.Black * opacity;

            // Draw shadow
            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                tipsText,
                tipsPosition.X,
                tipsPosition.Y,
                tipsShadowColor,
                tipsShadowColor,
                tipsSize / 2f
            );

            // Draw text
            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                tipsText,
                tipsPosition.X - 1,
                tipsPosition.Y - 1,
                tipsColor,
                tipsShadowColor,
                tipsSize / 2f
            );
        }
        private void UpdateTip(GameTime gameTime)
        {
            tipTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (tipTimer >= tipDuration)
            {
                currentTip = GetRandomTip();
                tipTimer = 0;
            }
        }
        private string GetRandomTip()
        {
            return Tips[Main.rand.Next(Tips.Length)];
        }
        private readonly string[] Tips =
        [
            "Please be patient, these subworlds may take up to 3 minutes to generate."
        ];
    }
}