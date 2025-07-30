using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Core.Cinematics;

public static class Letterbox
{
    public static int LetterboxHeight { get; private set; }
    public static float HeightPercentage { get; set; } = 0.07f;
    public static Color LetterboxColor { get; set; } = Color.Black;
    public static EaseFunction EasingFunction { get; set; } = EaseFunction.EaseQuadOut;
    public static int AnimationDurationFrames { get; set; } = 60;

    public static int BorderSize { get; set; } = 20;
    public static float BorderIntensity { get; set; } = 0.5f;

    private static int currentFrame;
    private static bool isAnimating;
    private static bool isShowing;
    private static float targetOpacity = 1f;
    private static Texture2D pixelTexture;

    /// <summary>
    /// Initialize the letterbox with a reference to a 1x1 pixel texture
    /// </summary>
    /// <param name="pixelTex">Reference to a 1x1 white pixel texture</param>
    public static void Initialize(Texture2D pixelTex)
    {
        pixelTexture = pixelTex;
    }

    /// <summary>
    /// Show the letterbox with optional parameters
    /// </summary>
    /// <param name="duration">Optional override for animation duration in frames</param>
    /// <param name="heightPercentage">Optional override for height as percentage of screen</param>
    /// <param name="opacity">Optional override for max opacity (0.0-1.0)</param>
    /// <param name="easing">Optional easing function to use</param>
    /// <param name="borderSize">Optional override for gradient border size</param>
    /// <param name="borderIntensity">Optional override for gradient border intensity</param>
    public static void Show(int? duration = null, float? heightPercentage = null, float? opacity = null,
        EaseFunction easing = null, int? borderSize = null, float? borderIntensity = null)
    {
        if (isShowing && !isAnimating)
            return;

        // Apply optional overrides if provided
        if (duration.HasValue)
            AnimationDurationFrames = duration.Value;

        if (heightPercentage.HasValue)
            HeightPercentage = MathHelper.Clamp(heightPercentage.Value, 0.01f, 0.5f);

        if (opacity.HasValue)
            targetOpacity = MathHelper.Clamp(opacity.Value, 0f, 1f);

        if (easing != null)
            EasingFunction = easing;

        if (borderSize.HasValue)
            BorderSize = Math.Max(0, borderSize.Value);

        if (borderIntensity.HasValue)
            BorderIntensity = MathHelper.Clamp(borderIntensity.Value, 0f, 1f);

        isShowing = true;
        isAnimating = true;
        currentFrame = 0;
    }

    /// <summary>
    /// Hide the letterbox with optional parameters
    /// </summary>
    /// <param name="duration">Optional override for animation duration in frames</param>
    /// <param name="easing">Optional easing function to use</param>
    public static void Hide(int? duration = null, EaseFunction easing = null)
    {
        if (!isShowing && !isAnimating)
            return;

        if (duration.HasValue)
            AnimationDurationFrames = duration.Value;

        if (easing != null)
            EasingFunction = easing;

        isShowing = false;
        isAnimating = true;
        currentFrame = 0;
    }

    /// <summary>
    /// Toggle the letterbox state
    /// </summary>
    public static void Toggle()
    {
        if (isShowing)
            Hide();
        else
            Show();
    }

    /// <summary>
    /// Update the letterbox state - call this in your game's Update method
    /// </summary>
    public static void Update()
    {
        if (!isAnimating)
            return;

        currentFrame++;

        if (currentFrame >= AnimationDurationFrames)
        {
            isAnimating = false;
            // Ensure we're at exactly the target state after animation completes
            LetterboxHeight = isShowing ?
                (int)(Main.screenHeight * HeightPercentage) :
                0;
            return;
        }

        // Calculate animation progress (0.0 to 1.0)
        var progress = (float)currentFrame / AnimationDurationFrames;

        // Apply progress based on whether we're showing or hiding
        var easedProgress = isShowing ?
            EasingFunction.Ease(progress) :
            1f - EasingFunction.Ease(progress);

        // Calculate letterbox height
        LetterboxHeight = (int)(Main.screenHeight * HeightPercentage * easedProgress);
    }

    /// <summary>
    /// Draw the letterbox with cinematic gradient borders
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing</param>
    public static void Draw(SpriteBatch spriteBatch)
    {
        if (LetterboxHeight <= 0)
            return;

        // Ensure we have a texture to draw
        var textureToDraw = pixelTexture ?? TextureAssets.MagicPixel.Value;

        // Calculate alpha based on current height vs target height
        var alpha = LetterboxHeight / (Main.screenHeight * HeightPercentage) * targetOpacity;
        var color = LetterboxColor * alpha;

        // Draw main letterbox bars
        // Top letterbox
        spriteBatch.Draw(
            textureToDraw,
            new Rectangle(0, 0, Main.screenWidth, LetterboxHeight),
            color
        );

        // Bottom letterbox
        spriteBatch.Draw(
            textureToDraw,
            new Rectangle(0, Main.screenHeight - LetterboxHeight, Main.screenWidth, LetterboxHeight),
            color
        );

        // Draw gradient borders if enabled
        if (BorderSize > 0)
        {
            DrawGradientBorders(spriteBatch, textureToDraw, alpha);
        }
    }
    /// <summary>
    /// Draw the letterbox with custom height percentage, for dialogue
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing</param>
    public static void Draw(SpriteBatch spriteBatch, float heightPercent = 0.07f)
    {
        if (LetterboxHeight <= 0)
            return;

        // Ensure we have a texture to draw
        var textureToDraw = pixelTexture ?? TextureAssets.MagicPixel.Value;

        // Calculate alpha based on current height vs target height
        var alpha = LetterboxHeight / (Main.screenHeight * heightPercent) * targetOpacity;
        var color = LetterboxColor * alpha;

        // Top letterbox
        spriteBatch.Draw(
            textureToDraw,
            new Rectangle(0, 0, Main.screenWidth, LetterboxHeight),
            color
        );

        // Bottom letterbox
        spriteBatch.Draw(
            textureToDraw,
            new Rectangle(0, Main.screenHeight - LetterboxHeight, Main.screenWidth, LetterboxHeight),
            color
        );

        if (BorderSize > 0)
        {
            DrawGradientBorders(spriteBatch, textureToDraw, alpha);
        }
    }

    /// <summary>
    /// Draw the gradient border effects
    /// </summary>
    private static void DrawGradientBorders(SpriteBatch spriteBatch, Texture2D texture, float baseAlpha)
    {
        for (var i = 0; i < BorderSize; i++)
        {
            var gradientProgress = 1f - (float)i / BorderSize;
            var alpha = gradientProgress * BorderIntensity * baseAlpha;
            var gradientColor = LetterboxColor * alpha;

            spriteBatch.Draw(
                texture,
                new Rectangle(0, LetterboxHeight + i, Main.screenWidth, 1),
                gradientColor
            );
        }

        for (var i = 0; i < BorderSize; i++)
        {
            var gradientProgress = 1f - (float)i / BorderSize;
            var alpha = gradientProgress * BorderIntensity * baseAlpha;
            var gradientColor = LetterboxColor * alpha;

            spriteBatch.Draw(
                texture,
                new Rectangle(0, Main.screenHeight - LetterboxHeight - i - 1, Main.screenWidth, 1),
                gradientColor
            );
        }
    }

    /// <summary>
    /// for special cases, you can draw a letterbox with custom gradient parameters
    /// </summary>
    /// <param name="borderSize">Custom border size in pixels</param>
    /// <param name="borderIntensity">Custom border intensity (0.0-1.0)</param>
    public static void DrawWithCustomGradient(SpriteBatch spriteBatch, int borderSize, float borderIntensity)
    {
        if (LetterboxHeight <= 0)
            return;

        var textureToDraw = pixelTexture ?? TextureAssets.MagicPixel.Value;
        var alpha = LetterboxHeight / (Main.screenHeight * HeightPercentage) * targetOpacity;
        var color = LetterboxColor * alpha;

        // Draw main letterbox bars
        spriteBatch.Draw(
            textureToDraw,
            new Rectangle(0, 0, Main.screenWidth, LetterboxHeight),
            color
        );
        spriteBatch.Draw(
            textureToDraw,
            new Rectangle(0, Main.screenHeight - LetterboxHeight, Main.screenWidth, LetterboxHeight),
            color
        );

        // Draw custom gradient borders
        if (borderSize > 0)
        {
            borderIntensity = MathHelper.Clamp(borderIntensity, 0f, 1f);

            for (var i = 0; i < borderSize; i++)
            {
                var gradientProgress = 1f - (float)i / borderSize;
                var gradientAlpha = gradientProgress * borderIntensity * alpha;
                var gradientColor = LetterboxColor * gradientAlpha;

                // Top gradient
                spriteBatch.Draw(
                    textureToDraw,
                    new Rectangle(0, LetterboxHeight + i, Main.screenWidth, 1),
                    gradientColor
                );

                // Bottom gradient
                spriteBatch.Draw(
                    textureToDraw,
                    new Rectangle(0, Main.screenHeight - LetterboxHeight - i - 1, Main.screenWidth, 1),
                    gradientColor
                );
            }
        }
    }

    /// <summary>
    /// Get the current safe area rectangle that avoids the letterbox
    /// </summary>
    /// <returns>Rectangle representing the visible area between letterboxes</returns>
    public static Rectangle GetSafeArea()
    {
        return new Rectangle(
            0,
            LetterboxHeight,
            Main.screenWidth,
            Main.screenHeight - LetterboxHeight * 2
        );
    }

    /// <summary>
    /// Check if the letterbox is currently visible
    /// </summary>
    public static bool IsActive => LetterboxHeight > 0;

    /// <summary>
    /// Check if the letterbox is currently animating
    /// </summary>
    public static bool IsAnimating => isAnimating;

    /// <summary>
    /// Get the progress of the animation (0.0 to 1.0)
    /// </summary>
    public static float AnimationProgress => isAnimating ? (float)currentFrame / AnimationDurationFrames : isShowing ? 1f : 0f;
}

public class LetterboxSystem : ModSystem
{
    private static readonly string[] UILayersToHide =
    [
        "Vanilla: Inventory",
        "Vanilla: Hotbar",
        "Vanilla: Resource Bars",
        "Vanilla: Map / Minimap",
        "Vanilla: Info Accessories Bar",
        "Vanilla: Builder Accessories Bar",
        "Vanilla: Mouse Over",
        "Vanilla: Radial Hotbars",
        "Vanilla: Player Chat",
        "Vanilla: Laser Ruler",
        "Vanilla: Gamepad Lock On",
        "Vanilla: Tile Grid Option",
        "Reverie: Experience Meter"
    ];

    public override void PostSetupContent()
    {
        Letterbox.Initialize(TextureAssets.MagicPixel.Value);
    }

    public override void PostUpdateEverything()
    {
        Letterbox.Update();
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Letterbox.IsActive)
        {
            foreach (var layer in layers)
            {
                if (UILayersToHide.Contains(layer.Name))
                {
                    layer.Active = false;
                }
            }
        }
    }
}