using Reverie.Common.Systems.Camera;
using Reverie.Core.Animation;
using Reverie.Core.Cinematics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;

namespace Reverie.Content.Cutscenes;

public class FallingStarCutscene : Cutscene
{
    // Configuration
    private readonly float cutsceneDuration = 25.5f; // Total duration in seconds
    private readonly float fadeInDuration = 5f;
    private readonly float stableSceneDuration = 15f; // Extended to allow complete star travel
    private readonly float fadeOutDuration = 4f;

    // Star management
    private readonly List<Star> stars = [];
    private readonly Dictionary<Star, Vector2> starVelocities = [];
    private bool starsInitialized = false;

    // Star properties
    private const float SPACE_DRIFT_SPEED = 0.05f;
    private const int INITIAL_STAR_COUNT = 150;

    // Silhouette properties
    private Texture2D silhouetteTexture;
    private Vector2 silhouettePosition;
    private Vector2 silhouetteVelocity;
    private float silhouetteScale = 0.04f;
    private float silhouetteAlpha = 0f;
    private float silhouetteRotation = 0f;
    private bool silhouetteActive = false;
    private float silhouetteDelay = 5.5f; // Start after fade-in is complete
    private float silhouetteDuration = 12f;
    private float silhouetteProgress = 0f;
    private float baseRotationSpeed = 0.03f; // Base rotation speed for the silhouette
    private float[] trailRotationOffsets; // Array to store trail segment rotation offsets

    private Vector2 silhouetteStartPos;
    private Vector2 silhouetteEndPos;

    private const float SHAKE_START_THRESHOLD = 0.5f;
    private const float MAX_SHAKE_INTENSITY = 14f;
    private const int TRAIL_SEGMENTS = 30; // Increased number of trail segments

    private Texture2D glowTexture;
    private Texture2D silglowTexture;
    private Texture2D starTexture;
    private Texture2D spaceOverlayTexture;

    public FallingStarCutscene()
    {
        DisablePlayerMovement();
        LetterboxHeightPercentage = 0.12f;
        LetterboxColor = Color.Black;
        LetterboxEasing = EaseFunction.EaseQuadOut;
        LetterboxAnimationDuration = 60;

        // Initialize rotation offsets for trail segments
        trailRotationOffsets = new float[TRAIL_SEGMENTS];
    }

    protected override void OnCutsceneStart()
    {
        // Load textures
        glowTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        silglowTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star09", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        starTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        spaceOverlayTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}SpaceOverlay", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        silhouetteTexture = ModContent.Request<Texture2D>($"{CUTSCENE_TEXTURE_DIRECTORY}PlayerSilhouette", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        // Initialize fade to black
        FadeColor = Color.Black;
        FadeAlpha = 1f;

        // Initialize stars
        if (!starsInitialized)
        {
            InitializeStars();
        }

        // Set up silhouette trajectory
        silhouetteStartPos = new Vector2(-100, 100); // Start off-screen to the left
        silhouetteEndPos = new Vector2(Main.screenWidth + 200, Main.screenHeight + 100); // End well off-screen

        // Initialize silhouette
        silhouettePosition = silhouetteStartPos;
        silhouetteVelocity = Vector2.Zero;
        silhouetteScale = 0.1f;
        silhouetteAlpha = 0f;
        silhouetteRotation = MathHelper.PiOver4; // 45 degree angle
        silhouetteActive = false;
        silhouetteProgress = 0f;

        // Initialize random rotation offsets for trail segments
        for (int i = 0; i < TRAIL_SEGMENTS; i++)
        {
            trailRotationOffsets[i] = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
        }

        // Disable player movement during cutscene
        DisablePlayerMovement();
    }

    private void InitializeStars()
    {
        stars.Clear();
        starVelocities.Clear();

        // Create initial stars
        for (int i = 0; i < INITIAL_STAR_COUNT; i++)
        {
            CreateNewStar();
        }

        starsInitialized = true;
    }

    private void CreateNewStar()
    {
        Star star = new Star
        {
            position = new Vector2(
                Main.rand.Next(0, Main.screenWidth),
                Main.rand.Next(0, Main.screenHeight)
            ),

            rotation = Main.rand.NextFloat(0, MathHelper.TwoPi),
            scale = Main.rand.NextFloat(0.2f, 0.72f)
        };

        if (Main.rand.NextBool(20))
            star.scale *= 0.8f;

        star.type = Main.rand.Next(0, 4);
        star.twinkle = Main.rand.NextFloat(0.6f, 1f);
        star.twinkleSpeed = Main.rand.NextFloat(0.0005f, 0.004f);

        if (Main.rand.NextBool(5))
            star.twinkleSpeed *= 2f;

        if (Main.rand.NextBool())
            star.twinkleSpeed *= -1f;

        star.rotationSpeed = Main.rand.NextFloat(0.000001f, 0.00001f);

        if (Main.rand.NextBool())
            star.rotationSpeed *= -0.05f;

        star.fadeIn = 0.5f;

        // Set initial velocity
        starVelocities[star] = new Vector2(
            -SPACE_DRIFT_SPEED * (0.5f + star.scale),
            0f
        );

        stars.Add(star);
    }

    protected override void OnCutsceneUpdate(GameTime gameTime)
    {
        // Handle fading phases
        if (ElapsedTime < fadeInDuration)
        {
            // Fade in
            FadeAlpha = 1f - (ElapsedTime / fadeInDuration);
        }
        else if (ElapsedTime > fadeInDuration + stableSceneDuration)
        {
            // Fade out
            float fadeOutElapsed = ElapsedTime - (fadeInDuration + stableSceneDuration);
            FadeAlpha = Math.Min(fadeOutElapsed / fadeOutDuration, 1f);
        }
        else
        {
            // During stable phase
            FadeAlpha = 0f;
        }

        // Update all stars
        for (int i = stars.Count - 1; i >= 0; i--)
        {
            Star star = stars[i];

            // Update star properties
            star.Update();

            // Apply velocity if not a falling star
            if (!star.falling)
            {
                // Apply velocity 
                star.position += starVelocities[star];

                // Gradually return to the default left drift
                starVelocities[star] = Vector2.Lerp(
                    starVelocities[star],
                    new Vector2(-SPACE_DRIFT_SPEED * (0.5f + star.scale), 0f),
                    0.01f
                );
            }

            // Remove stars that have moved off-screen
            if (star.hidden ||
                star.position.Y > Main.screenHeight + 100 ||
                star.position.Y < -100 ||
                star.position.X < -50)
            {
                stars.RemoveAt(i);

                // Remove from velocity dictionary to avoid memory leaks
                if (starVelocities.ContainsKey(star))
                    starVelocities.Remove(star);

                CreateNewStar();
            }
        }

        // Create shooting stars occasionally
        if (Main.rand.NextBool(50))
        {
            if (stars.Count > 0)
            {
                int starIndex = Main.rand.Next(stars.Count);
                stars[starIndex].Fall();

                // Remove from velocity dictionary when a star starts falling
                if (starVelocities.ContainsKey(stars[starIndex]))
                    starVelocities.Remove(stars[starIndex]);

                Star star = stars[starIndex];
                star.rotationSpeed = 0.1f;
                star.rotation = 0.01f;
                star.fallSpeed.Y = (float)Main.rand.Next(100, 201) * 0.001f;
                star.fallSpeed.X = (float)Main.rand.Next(-100, 101) * 0.001f;
            }
        }

        // Add new stars occasionally
        if (Main.rand.NextBool(160))
        {
            CreateNewStar();
        }

        // Update silhouette
        UpdateSilhouette();
    }

    private void UpdateSilhouette()
    {
        // Check if it's time to activate the silhouette
        if (!silhouetteActive && ElapsedTime >= silhouetteDelay && ElapsedTime < fadeInDuration + stableSceneDuration)
        {
            // Start the silhouette from off-screen
            silhouettePosition = silhouetteStartPos;

            // Calculate velocity to reach endpoint in the desired time
            silhouetteVelocity = (silhouetteEndPos - silhouettePosition) / (silhouetteDuration * 60); // 60 FPS assumed

            silhouetteActive = true;
            silhouetteScale = 0.1f; // Starting scale
            silhouetteAlpha = 0.8f; // Make it clearly visible from the start

            // Initialize random rotation speeds for trail segments
            for (int i = 0; i < TRAIL_SEGMENTS; i++)
            {
                // Generate varied rotation speeds
                trailRotationOffsets[i] = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
            }
        }

        // Update silhouette if active
        if (silhouetteActive)
        {
            // Update position
            silhouettePosition += silhouetteVelocity / 1.34f;

            // Calculate progress (0 to 1) across the screen
            float totalDistance = Vector2.Distance(silhouetteStartPos, silhouetteEndPos);
            float currentDistance = Vector2.Distance(silhouetteStartPos, silhouettePosition);
            silhouetteProgress = MathHelper.Clamp(currentDistance / totalDistance, 0f, 1f);

            // Scale and alpha based on progress - increase scale as it moves
            silhouetteScale = MathHelper.Lerp(0.1f, 0.6f, silhouetteProgress);

            // Ensure alpha stays high for visibility
            silhouetteAlpha = MathHelper.Clamp(0.8f + (silhouetteProgress * 0.2f), 0f, 1f);

            // Update main silhouette rotation - create a spinning effect
            silhouetteRotation += baseRotationSpeed * (1 + silhouetteProgress); // Rotation speed increases with progress
            if (silhouetteRotation > MathHelper.TwoPi)
                silhouetteRotation -= MathHelper.TwoPi;

            // Apply camera shake when reaching threshold
            if (silhouetteProgress >= SHAKE_START_THRESHOLD)
            {
                // Calculate shake intensity - exponential increase
                float shakeProgress = (silhouetteProgress - SHAKE_START_THRESHOLD) / (1f - SHAKE_START_THRESHOLD);
                float intensity = (float)Math.Pow(shakeProgress * 1.5f, 2) * MAX_SHAKE_INTENSITY;

                // Apply shake
                CameraSystem.shake = (int)intensity;
            }

            // Only deactivate when it's truly off-screen or when fading out starts
            bool isOffScreen = silhouettePosition.X > Main.screenWidth + 100 &&
                               silhouettePosition.Y > Main.screenHeight + 100;

            if (isOffScreen || ElapsedTime >= fadeInDuration + stableSceneDuration)
            {
                silhouetteActive = false;
            }
        }
    }

    // Override the base Draw method to control drawing order
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsPlaying) return;

        // First draw the content
        DrawCutsceneContent(spriteBatch);

        // Then draw the fade effect
        DrawFade(spriteBatch);

        // Finally draw letterbox
        if (UsesLetterbox())
        {
            Letterbox.DrawCinematic(spriteBatch);
        }
        else
        {
            Letterbox.Draw(spriteBatch);
        }
    }

    protected override void DrawCutsceneContent(SpriteBatch spriteBatch)
    {
        // Draw the space background
        spriteBatch.Draw(
            spaceOverlayTexture,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            new Color(8, 8, 8)
        );

        // End the current SpriteBatch and begin a new one with additive blending for the glow effects
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        Color colorStart = new Color(143, 244, 255);
        Color colorEnd = Color.White;

        // Draw all stars
        foreach (Star star in stars)
        {
            // Skip hidden stars
            if (star.hidden)
                continue;

            // Calculate and apply color/alpha
            float brightness = 0.5f + (star.twinkle * 0.5f);
            float colorLerp = (star.type / 4f) + (((int)star.position.X * (int)star.position.Y) % 10) / 10f;
            colorLerp = colorLerp % 1f;
            Color baseColor = Color.Lerp(colorStart, colorEnd, colorLerp);

            float alpha = 1f;
            if (star.fadeIn > 0f)
                alpha = 1f - star.fadeIn;

            Color starColor = baseColor * brightness * alpha;
            float bloomScale = star.scale * (0.8f + (star.twinkle * 0.4f));

            // Draw glow
            spriteBatch.Draw(
                glowTexture,
                star.position,
                null,
                starColor * 0.6f,
                0f,
                new Vector2(glowTexture.Width / 2, glowTexture.Height / 2),
                bloomScale * 1.1f,
                SpriteEffects.None,
                0f
            );

            // Draw star
            spriteBatch.Draw(
                starTexture,
                star.position,
                null,
                starColor,
                star.rotation,
                new Vector2(starTexture.Width / 2, starTexture.Height / 2),
                bloomScale * 0.5f,
                SpriteEffects.None,
                0f
            );
        }

        if (silhouetteActive)
        {
            // Color for the main star and trail
            Color silhouetteGlowColor = new Color(143, 244, 255) * silhouetteAlpha;

            // MAIN STAR/COMET HEAD
            // Draw the main star effect - this is your bright central point
            float mainStarScale = silhouetteScale * 1.4f;
            spriteBatch.Draw(
                silglowTexture,
                silhouettePosition,
                null,
                silhouetteGlowColor,
                silhouetteRotation,
                new Vector2(silglowTexture.Width / 2, silglowTexture.Height / 2),
                mainStarScale,
                SpriteEffects.None,
                0f
            );

            // TRAIL
            // Draw a trail/comet effect with dynamic rotation for each segment
            Vector2 trailDir = -Vector2.Normalize(silhouetteVelocity);

            for (int i = 1; i <= TRAIL_SEGMENTS; i++)
            {
                float segmentProgress = i / (float)TRAIL_SEGMENTS; // 0 to 1 progress along trail
                float trailDistance = i * 12f; // Slightly closer trail segments
                Vector2 trailPos = silhouettePosition + (trailDir * trailDistance);

                // Create a pulsing/varying alpha effect
                float pulseEffect = (float)Math.Sin(ElapsedTime * 5f + i * 0.5f) * 0.1f + 0.9f;
                float trailAlpha = silhouetteAlpha * (1f - segmentProgress) * pulseEffect;

                // Scale decreases along the trail
                float trailScale = silhouetteScale * (1f - segmentProgress * 0.8f) * 1.25f;

                // Each segment has its own rotation that changes over time
                float segmentRotation = silhouetteRotation +
                                       trailRotationOffsets[i - 1] +
                                       (float)Math.Sin(ElapsedTime * (3f - segmentProgress * 2f) + i * 0.3f) * 0.5f;

                // Color variation along trail - from blue-white to more blue
                Color trailColor = Color.Lerp(
                    silhouetteGlowColor,
                    new Color(40, 120, 255) * 0.3f,
                    segmentProgress
                );

                spriteBatch.Draw(
                    silglowTexture,
                    trailPos,
                    null,
                    trailColor * trailAlpha,
                    segmentRotation,
                    new Vector2(silglowTexture.Width / 2, silglowTexture.Height / 2),
                    trailScale,
                    SpriteEffects.None,
                    0f
                );
            }

            // SILHOUETTE
            // We're not going to draw the actual silhouette texture since it's showing up incorrectly
            // Instead, we'll enhance the comet effect with additional glow layers

            // Extra glow layer for better visibility
            spriteBatch.Draw(
                silglowTexture,
                silhouettePosition,
                null,
                Color.Black * silhouetteAlpha * 0.4f,
                silhouetteRotation,
                new Vector2(silglowTexture.Width / 2, silglowTexture.Height / 2),
                mainStarScale * 0.7f,
                SpriteEffects.None,
                0f
            );

            // Add a small inner core with a different color for visual interest
            spriteBatch.Draw(
                silglowTexture,
                silhouettePosition,
                null,
                new Color(170, 240, 255) * silhouetteAlpha * 0.9f,
                silhouetteRotation * 0.7f, // Slightly different rotation
                new Vector2(silglowTexture.Width / 2, silglowTexture.Height / 2),
                mainStarScale * 0.4f,
                SpriteEffects.None,
                0f
            );
        }

        // End the additive blending batch and begin a new one with alpha blending for other content
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
    }

    protected override bool UsesLetterbox() => true;

    public override bool IsFinished()
    {
        return ElapsedTime >= cutsceneDuration;
    }

    protected override void OnCutsceneEnd()
    {
        stars.Clear();
        starVelocities.Clear();
        starsInitialized = false;
        silhouetteActive = false;

        EnablePlayerMovement();
    }
}