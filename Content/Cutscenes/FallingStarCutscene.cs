using Reverie.Core.Animation;
using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;

namespace Reverie.Content.Cutscenes;

public class FallingStarCutscene : Cutscene
{
    #region Constants, Fields, and Properties
    private readonly float cutsceneDuration = 25.5f; // Total duration in seconds
    private readonly float fadeInDuration = 5f;
    private readonly float stableSceneDuration = 15f; // Extended to allow complete star travel
    private readonly float fadeOutDuration = 4f;

    private readonly List<Star> stars = [];
    private readonly Dictionary<Star, Vector2> starVelocities = [];
    private bool starsInitialized = false;

    private const float SPACE_DRIFT_SPEED = 0.05f;
    private const int INITIAL_STAR_COUNT = 150;

    private Texture2D silhouetteTexture;
    private Vector2 silhouettePosition;
    private Vector2 silhouetteVelocity;
    private float silhouetteScale = 0.04f;
    private float silhouetteAlpha = 0f;
    private float silhouetteRotation = 0f;
    private bool silhouetteActive = false;
    private float silhouetteDelay = 5.5f;
    private float silhouetteDuration = 12f;
    private float silhouetteProgress = 0f;
    private float baseRotationSpeed = 0.03f;
    private float[] trailRotationOffsets;

    private Vector2 silhouetteStartPos;
    private Vector2 silhouetteEndPos;

    private const float SHAKE_START_THRESHOLD = 0.5f;
    private const float MAX_SHAKE_INTENSITY = 14f;
    private const int TRAIL_SEGMENTS = 30;

    private Texture2D glowTexture;
    private Texture2D silglowTexture;
    private Texture2D starTexture;
    private Texture2D spaceOverlayTexture;
    #endregion

    #region Setup
    public FallingStarCutscene()
    {
        DisablePlayerMovement();
        LetterboxHeightPercentage = 0.12f;
        LetterboxColor = Color.Black;
        LetterboxEasing = EaseFunction.EaseQuadOut;
        LetterboxAnimationDuration = 60;

        trailRotationOffsets = new float[TRAIL_SEGMENTS];

        CanSkip = true;
        SkipHoldDuration = 180;
    }

    public bool skipped = false;
    protected override void OnSkipTriggered()
    {
        base.OnSkipTriggered();
        skipped = true;
        CameraSystem.shake = 0;

        silhouetteActive = false;
    }

    protected override bool UsesLetterbox() => true;

    private void InitializeStars()
    {
        stars.Clear();
        starVelocities.Clear();

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

        starVelocities[star] = new Vector2(
            -SPACE_DRIFT_SPEED * (0.5f + star.scale),
            0f
        );

        stars.Add(star);
    }

    protected override void OnCutsceneStart()
    {
        glowTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        silglowTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star09", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        starTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        spaceOverlayTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}SpaceOverlay", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        silhouetteTexture = ModContent.Request<Texture2D>($"{CUTSCENE_TEXTURE_DIRECTORY}PlayerSilhouette", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        FadeColor = Color.Black;
        FadeAlpha = 1f;

        if (!starsInitialized)
        {
            InitializeStars();
        }

        silhouetteStartPos = new Vector2(-100, 100);
        silhouetteEndPos = new Vector2(Main.screenWidth + 200, Main.screenHeight + 100);

        silhouettePosition = silhouetteStartPos;
        silhouetteVelocity = Vector2.Zero;
        silhouetteScale = 0.1f;
        silhouetteAlpha = 0f;
        silhouetteRotation = MathHelper.PiOver4;
        silhouetteActive = false;
        silhouetteProgress = 0f;

        for (int i = 0; i < TRAIL_SEGMENTS; i++)
        {
            trailRotationOffsets[i] = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
        }

        DisablePlayerMovement();
    }
    #endregion

    private void UpdateSilhouette()
    {
        if (!silhouetteActive && ElapsedTime >= silhouetteDelay && ElapsedTime < fadeInDuration + stableSceneDuration)
        {
            silhouettePosition = silhouetteStartPos;

            silhouetteVelocity = (silhouetteEndPos - silhouettePosition) / (silhouetteDuration * 60);

            silhouetteActive = true;
            silhouetteScale = 0.1f;
            silhouetteAlpha = 0.8f;

            for (int i = 0; i < TRAIL_SEGMENTS; i++)
            {
                trailRotationOffsets[i] = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
            }
        }

        if (silhouetteActive)
        {
            silhouettePosition += silhouetteVelocity / 1.34f;

            float totalDistance = Vector2.Distance(silhouetteStartPos, silhouetteEndPos);
            float currentDistance = Vector2.Distance(silhouetteStartPos, silhouettePosition);
            silhouetteProgress = MathHelper.Clamp(currentDistance / totalDistance, 0f, 1f);

            silhouetteScale = MathHelper.Lerp(0.1f, 0.6f, silhouetteProgress);

            silhouetteAlpha = MathHelper.Clamp(0.8f + (silhouetteProgress * 0.2f), 0f, 1f);

            silhouetteRotation += baseRotationSpeed * (1 + silhouetteProgress);
            if (silhouetteRotation > MathHelper.TwoPi)
                silhouetteRotation -= MathHelper.TwoPi;

            if (silhouetteProgress >= SHAKE_START_THRESHOLD)
            {
                float shakeProgress = (silhouetteProgress - SHAKE_START_THRESHOLD) / (1f - SHAKE_START_THRESHOLD);
                float intensity = (float)Math.Pow(shakeProgress * 1.5f, 2) * MAX_SHAKE_INTENSITY;

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

    protected override void OnCutsceneUpdate(GameTime gameTime)
    {
        if (ElapsedTime < fadeInDuration)
        {
            FadeAlpha = 1f - (ElapsedTime / fadeInDuration);
        }
        else if (ElapsedTime > fadeInDuration + stableSceneDuration)
        {
            float fadeOutElapsed = ElapsedTime - (fadeInDuration + stableSceneDuration);
            FadeAlpha = Math.Min(fadeOutElapsed / fadeOutDuration, 1f);
        }
        else
        {
            FadeAlpha = 0f;
        }

        // exact same code from the ModMenu lol
        for (int i = stars.Count - 1; i >= 0; i--)
        {
            Star star = stars[i];

            star.Update();

            if (!star.falling)
            {
                star.position += starVelocities[star];

                starVelocities[star] = Vector2.Lerp(
                    starVelocities[star],
                    new Vector2(-SPACE_DRIFT_SPEED * (0.5f + star.scale), 0f),
                    0.01f
                );
            }

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

        if (Main.rand.NextBool(50))
        {
            if (stars.Count > 0)
            {
                int starIndex = Main.rand.Next(stars.Count);
                stars[starIndex].Fall();

                if (starVelocities.ContainsKey(stars[starIndex]))
                    starVelocities.Remove(stars[starIndex]);

                Star star = stars[starIndex];
                star.rotationSpeed = 0.1f;
                star.rotation = 0.01f;
                star.fallSpeed.Y = (float)Main.rand.Next(100, 201) * 0.001f;
                star.fallSpeed.X = (float)Main.rand.Next(-100, 101) * 0.001f;
            }
        }

        if (Main.rand.NextBool(160))
        {
            CreateNewStar();
        }

        UpdateSilhouette();
    }

    #region Rendering

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsPlaying) return;

        DrawCutsceneContent(spriteBatch);

        DrawFade(spriteBatch);

        if (UsesLetterbox())
        {
            Letterbox.DrawCinematic(spriteBatch);
        }
        else
        {
            Letterbox.Draw(spriteBatch);
        }

        DrawSkipIndicator(spriteBatch);
    }

    protected override void DrawCutsceneContent(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            spaceOverlayTexture,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            new Color(8, 8, 8)
        );

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        Color colorStart = new Color(143, 244, 255);
        Color colorEnd = Color.White;

        foreach (Star star in stars)
        {
            if (star.hidden)
                continue;

            float brightness = 0.5f + (star.twinkle * 0.5f);
            float colorLerp = (star.type / 4f) + (((int)star.position.X * (int)star.position.Y) % 10) / 10f;
            colorLerp = colorLerp % 1f;
            Color baseColor = Color.Lerp(colorStart, colorEnd, colorLerp);

            float alpha = 1f;
            if (star.fadeIn > 0f)
                alpha = 1f - star.fadeIn;

            Color starColor = baseColor * brightness * alpha;
            float bloomScale = star.scale * (0.8f + (star.twinkle * 0.4f));

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
            Color silhouetteGlowColor = new Color(143, 244, 255) * silhouetteAlpha;

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

            Vector2 trailDir = -Vector2.Normalize(silhouetteVelocity);

            for (int i = 1; i <= TRAIL_SEGMENTS; i++)
            {
                float segmentProgress = i / (float)TRAIL_SEGMENTS;
                float trailDistance = i * 12f;
                Vector2 trailPos = silhouettePosition + (trailDir * trailDistance);

                float pulseEffect = (float)Math.Sin(ElapsedTime * 5f + i * 0.5f) * 0.1f + 0.9f;
                float trailAlpha = silhouetteAlpha * (1f - segmentProgress) * pulseEffect;

                float trailScale = silhouetteScale * (1f - segmentProgress * 0.8f) * 1.25f;

                float segmentRotation = silhouetteRotation +
                                       trailRotationOffsets[i - 1] +
                                       (float)Math.Sin(ElapsedTime * (3f - segmentProgress * 2f) + i * 0.3f) * 0.5f;

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

            spriteBatch.Draw(
                silglowTexture,
                silhouettePosition,
                null,
                new Color(170, 240, 255) * silhouetteAlpha * 0.9f,
                silhouetteRotation * 0.7f,
                new Vector2(silglowTexture.Width / 2, silglowTexture.Height / 2),
                mainStarScale * 0.4f,
                SpriteEffects.None,
                0f
            );
        }

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
    }
    #endregion

    public override bool IsFinished()
    {
        return skipped || ElapsedTime >= cutsceneDuration;
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