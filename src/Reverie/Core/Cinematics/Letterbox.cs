using Terraria.GameContent;

namespace Reverie.Core.Cinematics;

public static class Letterbox
{
    public static int LetterboxHeight { get; private set; }

    private const int ANIMATION_DURATION = 60;
    private static int currentFrame;
    private static bool isAnimating;
    private static bool isShowing;
    private static float heightMultiplier = 0.05f;

    public static void Show()
    {
        if (!isShowing)
        {
            isShowing = true;
            isAnimating = true;
            currentFrame = 0;
        }
    }

    public static void Hide()
    {
        if (isShowing)
        {
            isShowing = false;
            isAnimating = true;
            currentFrame = 0;
        }
    }

    public static void Update()
    {
        if (!isAnimating) return;

        currentFrame++;
        if (currentFrame >= ANIMATION_DURATION)
        {
            isAnimating = false;
        }

        var progress = isShowing ?
            (float)currentFrame / ANIMATION_DURATION :
            1f - (float)currentFrame / ANIMATION_DURATION;

        progress = (float)Math.Sin(progress * MathHelper.PiOver2);

        LetterboxHeight = (int)(Main.screenHeight * heightMultiplier * progress);
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        if (LetterboxHeight <= 0) return;

        var color = Color.Black * (LetterboxHeight / (Main.screenHeight * heightMultiplier));

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