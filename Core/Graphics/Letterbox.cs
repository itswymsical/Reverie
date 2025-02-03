using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;

namespace Reverie.Core.Graphics
{
    public static class Letterbox
    {
        public static int LetterboxHeight { get; private set; }

        private const int ANIMATION_DURATION = 60;
        private static int _currentFrame;
        private static bool _isAnimating;
        private static bool _isShowing;
        private static float _heightMultiplier = 0.05f;

        public static void Show()
        {
            if (!_isShowing)
            {
                _isShowing = true;
                _isAnimating = true;
                _currentFrame = 0;
            }
        }

        public static void Hide()
        {
            if (_isShowing)
            {
                _isShowing = false;
                _isAnimating = true;
                _currentFrame = 0;
            }
        }

        public static void Update()
        {
            if (!_isAnimating) return;

            _currentFrame++;
            if (_currentFrame >= ANIMATION_DURATION)
            {
                _isAnimating = false;
            }

            float progress = _isShowing ?
                (float)_currentFrame / ANIMATION_DURATION :
                1f - ((float)_currentFrame / ANIMATION_DURATION);

            progress = (float)Math.Sin(progress * MathHelper.PiOver2);

            LetterboxHeight = (int)(Main.screenHeight * _heightMultiplier * progress);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (LetterboxHeight <= 0) return;

            var color = Color.Black * (LetterboxHeight / (Main.screenHeight * _heightMultiplier));

            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle(0, 0, Main.screenWidth, LetterboxHeight),
                color
            );

            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle(0, Main.screenHeight - LetterboxHeight, Main.screenWidth, LetterboxHeight),
                color
            );
        }

        public static bool IsActive => LetterboxHeight > 0;
    }
}