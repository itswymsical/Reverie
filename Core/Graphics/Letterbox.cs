using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
                _currentFrame = ANIMATION_DURATION;
            }

            float progress = _isShowing ?
                (float)_currentFrame / ANIMATION_DURATION :
                1f - ((float)_currentFrame / ANIMATION_DURATION);

            LetterboxHeight = (int)(Main.screenHeight * 0.075f * progress);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (LetterboxHeight <= 0) return;

            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle(0, 0, Main.screenWidth, LetterboxHeight),
                Color.Black
            );

            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle(0, Main.screenHeight - LetterboxHeight, Main.screenWidth, LetterboxHeight),
                Color.Black
            );
        }

        public static bool IsActive => LetterboxHeight > 0;
    }
}