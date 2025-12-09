using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using Reverie.Core.Cinematics.Music;
using Reverie.Core.Dialogue;
using Reverie.Core.Loaders;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.Cutscenes;

public class ArrivalCutscene : Cutscene
{
    #region Constants
    private const float SCENE1_DURATION = 3.5f; // SpaceView
    private const float SCENE2_DURATION = 6f; // The Fall
    private const float SCENE3_DURATION = 4f; // Impact
    private const float SCENE4_DURATION = 3f; // Guide Reaction
    private const float SCENE5_DURATION = 1f; // Approach

    private const float LOGO_FADE_IN = 1f;
    private const float LOGO_HOLD = 1.5f;
    private const float LOGO_FADE_OUT = 0.5f;

    private const float FALL_HEIGHT = 6000f;
    private const float GUIDE_DISTANCE = 150f;
    #endregion

    #region State
    private enum CutscenePhase
    {
        SpaceView,
        TheFall,
        Impact,
        GuideReaction,
        Approach,
        End
    }

    private CutscenePhase currentPhase = CutscenePhase.SpaceView;
    private float phaseTimer = 0f;
    #endregion

    #region Positions
    private Vector2 spawnPoint;
    private Vector2 spaceViewPos;
    private Vector2 impactPoint;
    private Vector2 guideStartPos;
    private NPC guide;
    #endregion

    #region Visual State
    private Texture2D logoTexture;
    private float logoAlpha = 0f;
    private float starAlpha = 0f;
    private Vector2 starScreenPos;
    private bool hasPlayedImpactSound = false;
    #endregion

    public override void Start()
    {
        // Calculate positions
        spawnPoint = new Vector2(Main.spawnTileX * 16f + 8f, Main.spawnTileY * 16f + 8f);
        impactPoint = spawnPoint;
        spaceViewPos = impactPoint - new Vector2(0, FALL_HEIGHT);
        guideStartPos = impactPoint + new Vector2(GUIDE_DISTANCE, 0);

        EnableLetterbox = true;
        base.Start();

        SetMusic(
            MusicLoader.GetMusicSlot($"{CUTSCENE_MUSIC_DIRECTORY}DawnofReverie"),
            MusicFadeMode.FadeIn,
            1f
        );
    }

    protected override void OnCutsceneStart()
    {
        // Load assets
        logoTexture = ModContent.Request<Texture2D>(
            $"{LOGO_DIRECTORY}Logo_Outline",
            AssetRequestMode.ImmediateLoad
        ).Value;

        InitializeStarfield();

        // Lock player
        ControlsOFF();
        FallDamageOFF();
        InvisON();
        FreezePlayer();

        // Position Guide
        guide = GetNPC(NPCID.Guide);
        if (guide != null)
        {
            SetNPCPosition(guide, guideStartPos);
            guide.ai[0] = 0; // Reset AI state
        }

        // Camera to space view
        CameraSystem.LockCamera(spaceViewPos);

        // Start with black screen
        FadeAlpha = 1f;
        FadeColor = Color.Black;
    }

    protected override void OnCutsceneUpdate(GameTime gameTime)
    {
        phaseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (currentPhase)
        {
            case CutscenePhase.SpaceView:
                UpdateSpaceView();
                UpdateStarfield();
                break;
            case CutscenePhase.TheFall:
                UpdateFall();
                break;
            case CutscenePhase.Impact:
                UpdateImpact();
                break;
            case CutscenePhase.GuideReaction:
                UpdateGuideReaction();
                break;
            case CutscenePhase.Approach:
                UpdateApproach();
                break;
            case CutscenePhase.End:
                UpdateEnd();
                break;
        }
    }

    #region Phase Updates
    private void UpdateSpaceView()
    {
        // Fade in from black
        if (phaseTimer < LOGO_FADE_IN)
        {
            FadeAlpha = 1f - (phaseTimer / LOGO_FADE_IN);
        }
        else
        {
            FadeAlpha = 0f;
        }

        // Logo animation
        if (phaseTimer < LOGO_FADE_IN)
        {
            logoAlpha = phaseTimer / LOGO_FADE_IN;
        }
        else if (phaseTimer < LOGO_FADE_IN + LOGO_HOLD)
        {
            logoAlpha = 1f;
        }
        else if (phaseTimer < SCENE1_DURATION)
        {
            float fadeOutProgress = (phaseTimer - LOGO_FADE_IN - LOGO_HOLD) / LOGO_FADE_OUT;
            logoAlpha = 1f - fadeOutProgress;
        }

        // Transition to fall
        if (phaseTimer >= SCENE1_DURATION)
        {
            currentPhase = CutscenePhase.TheFall;
            phaseTimer = 0f;
            logoAlpha = 0f;
        }
    }

    private void UpdateFall()
    {
        // Camera pans from space to ground
        float progress = phaseTimer / SCENE2_DURATION;
        float easedProgress = EaseFunction.EaseQuadIn.Ease(progress);

        Vector2 cameraPos = Vector2.Lerp(spaceViewPos, impactPoint, easedProgress);
        CameraSystem.LockCamera(cameraPos);

        // Star gets brighter as it falls
        starAlpha = Math.Min(progress * 2f, 1f);

        // Calculate star screen position (it stays centered)
        starScreenPos = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);

        // Transition to impact
        if (phaseTimer >= SCENE2_DURATION)
        {
            currentPhase = CutscenePhase.Impact;
            phaseTimer = 0f;
        }
    }

    private void UpdateImpact()
    {
        // Lock camera on impact point
        CameraSystem.LockCamera(impactPoint);

        // Flash effect
        if (phaseTimer < 0.1f)
        {
            FadeAlpha = 1f;
            FadeColor = Color.White;
        }
        else if (phaseTimer < 0.3f)
        {
            FadeAlpha = 1f - ((phaseTimer - 0.1f) / 0.2f);
            FadeColor = Color.White;
        }
        else
        {
            FadeAlpha = 0f;
        }

        // Screen shake
        if (!hasPlayedImpactSound)
        {
            CameraSystem.shake = 35;
            SoundEngine.PlaySound(SoundID.Item14);
            hasPlayedImpactSound = true;

            // Position player at impact (still invisible)
            SetPlayerPosition(impactPoint - Main.LocalPlayer.Size / 2);

            // Knock Guide over (slapstick)
            if (guide != null)
            {
                guide.velocity = new Vector2(-3f, -8f); // Knocked back
            }
        }

        starAlpha = 0f; // Star disappears

        // Transition to Guide reaction
        if (phaseTimer >= SCENE3_DURATION)
        {
            currentPhase = CutscenePhase.GuideReaction;
            phaseTimer = 0f;
        }
    }

    private void UpdateGuideReaction()
    {
        // Camera pans to show crater and Guide
        float progress = phaseTimer / SCENE4_DURATION;
        Vector2 midPoint = (impactPoint + guideStartPos) / 2f;
        CameraSystem.LockCamera(midPoint);

        // Guide gets up animation (let physics handle it naturally)
        if (guide != null && phaseTimer > 1f)
        {
            // Guide stands up and looks at crater
            guide.velocity *= 0.9f; // Slow down
            guide.spriteDirection = impactPoint.X < guide.Center.X ? -1 : 1;
        }

        // Make player visible in crater
        if (phaseTimer > 1.5f)
        {
            InvisOFF();
        }

        // Transition to approach
        if (phaseTimer >= SCENE4_DURATION)
        {
            currentPhase = CutscenePhase.Approach;
            phaseTimer = 0f;
        }
    }

    private void UpdateApproach()
    {
        // Guide walks toward player
        if (guide != null)
        {
            float walkProgress = Math.Min(phaseTimer / SCENE5_DURATION, 1f);
            Vector2 targetPos = impactPoint + new Vector2(50, 0); // Stop near player

            guide.Center = Vector2.Lerp(
                guideStartPos,
                targetPos,
                EaseFunction.EaseQuadOut.Ease(walkProgress)
            );
            guide.spriteDirection = -1; // Face player
        }

        // Camera follows Guide slightly
        Vector2 followPos = Vector2.Lerp(impactPoint, guide.Center, 0.3f);
        CameraSystem.LockCamera(followPos);

        // Fade to black at end
        if (phaseTimer > SCENE5_DURATION - 1f)
        {
            float fadeProgress = (phaseTimer - (SCENE5_DURATION - 1f)) / 1f;
            FadeAlpha = fadeProgress;
            FadeColor = Color.Black;
        }

        // Transition to end
        if (phaseTimer >= SCENE5_DURATION)
        {
            currentPhase = CutscenePhase.End;
            phaseTimer = 0f;
        }
    }

    private void UpdateEnd()
    {
        // Just wait for IsFinished to trigger
    }
    #endregion

    protected override void DrawCutsceneContent(SpriteBatch spriteBatch)
    {
        switch (currentPhase)
        {
            case CutscenePhase.SpaceView:
                DrawSpaceView(spriteBatch);
                break;
            case CutscenePhase.TheFall:
                DrawFallingStar(spriteBatch);
                break;
        }
    }

    private void DrawSpaceView(SpriteBatch spriteBatch)
    {
        // Draw simple starfield background
        DrawStarfield(spriteBatch);

        // Draw logo with credits
        if (logoAlpha > 0f)
        {
            DrawLogoWithGalaxy(spriteBatch);
        }
    }

    private void DrawLogoWithGalaxy(SpriteBatch spriteBatch)
    {
        // Position the logo
        var logoDrawCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 4f);
        var logoRenderPos = new Vector2(
            logoDrawCenter.X - (logoTexture.Width * 1.35f) / 2f,
            logoDrawCenter.Y
        );
        var logoWidth = logoTexture.Width * 1.35f;
        var logoHeight = logoTexture.Height * 1.35f;

        // Calculate galaxy dot position (the spinning effect center)
        var dotOffset = new Vector2(logoWidth * 0.82f, logoHeight * 0.39f);
        var galaxyWorldPos = logoRenderPos + dotOffset;
        var galaxyShaderPos = new Vector2(
            (galaxyWorldPos.X - Main.screenWidth * 0.5f) / (Main.screenWidth * 0.5f),
            (galaxyWorldPos.Y - Main.screenHeight * 0.5f) / (Main.screenHeight * 0.5f)
        );

        // Draw main logo
        spriteBatch.Draw(
            logoTexture,
            logoRenderPos,
            null,
            Color.White * logoAlpha,
            0f,
            Vector2.Zero,
            1.35f,
            SpriteEffects.None,
            0f
        );

        // Switch to additive blend for galaxy effect
        spriteBatch.End();

        var effect = ShaderLoader.GetShader("GalaxyShader").Value;
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Additive,
            SamplerState.PointClamp,
            DepthStencilState.None,
            Main.Rasterizer,
            effect,
            Main.UIScaleMatrix
        );

        // Configure galaxy shader
        if (effect != null)
        {
            effect.Parameters["uTime"]?.SetValue((float)(Main.timeForVisualEffects * 0.0015f));
            effect.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["uSourceRect"]?.SetValue(new Vector4(0, 0, Main.screenWidth, Main.screenHeight));
            effect.Parameters["uIntensity"]?.SetValue(0.85f * logoAlpha);

            effect.Parameters["uImage0"]?.SetValue(
                ModContent.Request<Texture2D>($"{VFX_DIRECTORY}RibbonTrail", AssetRequestMode.ImmediateLoad).Value
            );
            effect.Parameters["uImage1"]?.SetValue(
                ModContent.Request<Texture2D>($"{VFX_DIRECTORY}StormTrail", AssetRequestMode.ImmediateLoad).Value
            );

            effect.Parameters["uCenter"]?.SetValue(galaxyShaderPos);
            effect.Parameters["uScale"]?.SetValue(3.5f);

            effect.Parameters["uRotation"]?.SetValue((float)(Main.timeForVisualEffects * 0.001f));
            effect.Parameters["uArmCount"]?.SetValue(4.0f);

            effect.Parameters["uColor"]?.SetValue(new Vector4(0.8f, 0.4f, 1.5f, 0.85f));
        }

        // Draw Perlin texture (background swirl)
        var perlinSpiral = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Perlin").Value;
        spriteBatch.Draw(
            perlinSpiral,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White * logoAlpha
        );

        // Draw Energy Trail texture (additional effect layer)
        var pixelTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value;
        spriteBatch.Draw(
            pixelTexture,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White * logoAlpha
        );

        // Return to normal blend
        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            Main.Rasterizer,
            null,
            Main.UIScaleMatrix
        );
    }

    #region Starfield System
    private class Star
    {
        public Vector2 Position;
        public float Scale;
        public float Twinkle;
        public float TwinkleSpeed;
        public float Rotation;
        public float RotationSpeed;

        public void Update()
        {
            Twinkle += TwinkleSpeed;
            if (Twinkle > 1f || Twinkle < 0f)
                TwinkleSpeed *= -1f; // Bounce twinkle

            Rotation += RotationSpeed;
        }
    }

    private List<Star> stars = new();
    private const int STAR_COUNT = 80;

    private void InitializeStarfield()
    {
        stars.Clear();

        for (int i = 0; i < STAR_COUNT; i++)
        {
            stars.Add(new Star
            {
                Position = new Vector2(
                    Main.rand.Next(0, Main.screenWidth),
                    Main.rand.Next(0, Main.screenHeight)
                ),
                Scale = Main.rand.NextFloat(0.3f, 1f),
                Twinkle = Main.rand.NextFloat(0.5f, 1f),
                TwinkleSpeed = Main.rand.NextFloat(0.002f, 0.008f) * (Main.rand.NextBool() ? 1 : -1),
                Rotation = Main.rand.NextFloat(0, MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.01f, 0.01f)
            });
        }
    }

    private void UpdateStarfield()
    {
        foreach (var star in stars)
        {
            star.Update();
        }
    }

    private void DrawStarfield(SpriteBatch spriteBatch)
    {
        Texture2D glowTex = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;
        Texture2D starTex = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star").Value;

        // End normal blend, start additive for glowy stars
        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Additive,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            Main.Rasterizer,
            null,
            Main.UIScaleMatrix
        );

        foreach (var star in stars)
        {
            float brightness = 0.5f + (star.Twinkle * 0.5f);
            Color starColor = Color.Lerp(
                new Color(143, 244, 255), // Blue-white
                Color.White,
                star.Scale
            ) * brightness;

            // Glow layer
            spriteBatch.Draw(
                glowTex,
                star.Position,
                null,
                starColor * 0.5f,
                0f,
                new Vector2(glowTex.Width / 2f, glowTex.Height / 2f),
                star.Scale * 0.8f,
                SpriteEffects.None,
                0f
            );

            // Star core
            spriteBatch.Draw(
                starTex,
                star.Position,
                null,
                starColor,
                star.Rotation,
                new Vector2(starTex.Width / 2f, starTex.Height / 2f),
                star.Scale * 0.4f,
                SpriteEffects.None,
                0f
            );
        }

        // Return to normal blend
        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            Main.Rasterizer,
            null,
            Main.UIScaleMatrix
        );
    }
    #endregion

    private void DrawFallingStar(SpriteBatch spriteBatch)
    {
        if (starAlpha <= 0f) return;

        // Draw simple glow effect for falling star
        Texture2D glowTex = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}placeholderguy").Value;

        float glowSize = 50f + (starAlpha * 100f); // Gets bigger as it falls

        spriteBatch.Draw(
            glowTex,
            starScreenPos,
            null,
            Color.Orange * starAlpha,
            0f,
            new Vector2(glowTex.Width / 2f, glowTex.Height / 2f),
            glowSize / glowTex.Width,
            SpriteEffects.None,
            0f
        );

        // Core
        spriteBatch.Draw(
            glowTex,
            starScreenPos,
            null,
            Color.White * starAlpha,
            0f,
            new Vector2(glowTex.Width / 2f, glowTex.Height / 2f),
            (glowSize * 0.3f) / glowTex.Width,
            SpriteEffects.None,
            0f
        );
    }

    public override bool IsFinished()
    {
        return currentPhase == CutscenePhase.End;
    }

    public override void End()
    {
        // Clean up
        CameraSystem.UnlockCamera();
        ControlsON();
        FallDamageON();
        InvisOFF();

        // Reset player
        var player = Main.LocalPlayer;
        player.fullRotation = 0f;
        player.fullRotationOrigin = Vector2.Zero;
        player.velocity = Vector2.Zero;

        // Start dialogue
        DialogueManager.Instance.StartDialogue("JourneysBegin.Crash", 4);

        base.End();
    }
}