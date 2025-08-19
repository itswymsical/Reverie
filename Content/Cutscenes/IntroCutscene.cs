using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Reverie.Common.Systems;
using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using Reverie.Core.Cinematics.Music;
using Reverie.Core.Dialogue;
using Reverie.Core.Loaders;
using Reverie.Core.Missions;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Skies;

namespace Reverie.Content.Cutscenes;

public class IntroCutscene : Cutscene
{
    private float fadeInDuration = 7 * 60f;
    private float panStartDelay = 0f;
    private int panDuration = 420;
    private float panDistance = 730f;
    private float logoFadeDelay = 0.5f;
    private float logoFadeDuration = 3f;
    private float logoFadeOutDelay = 5f;
    private float logoFadeOutDuration = 2f;
    private float logoAlpha = 0f;
    private float logoScale = 1.35f;
    private float logoRotation = 0f;
    private Texture2D logoTexture;

    private bool panStarted = false;

    private Vector2 originalPlayerPosition;
    private float fallStartDelay = 0.5f;
    private float fallHeight = 1000f;
    private bool fallSequenceStarted = false;
    private bool playerFalling = false;
    private bool impactOccurred = false;
    private float fallSequenceTimer = 0f;

    public override void Start()
    {
        originalPlayerPosition = new Vector2(Main.spawnTileX * 16f + 8f, Main.spawnTileY * 16f + 8f);

        EnableLetterbox = true; // Add this line
        base.Start();
        SetMusic(MusicLoader.GetMusicSlot($"{CUTSCENE_MUSIC_DIRECTORY}DawnofReverie"), MusicFadeMode.Instant);
    }

    protected override void OnCutsceneStart()
    {
        FadeAlpha = 1f;
        FadeColor = Color.Black;
        logoTexture = ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}Logo_Outline", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        var startPos = originalPlayerPosition - new Vector2(0, panDistance);
        CameraSystem.MoveCameraOut(1, startPos);
        ControlsOFF();
        InvisON();
    }

    protected override void OnCutsceneUpdate(GameTime gameTime)
    {
        FadeIn(fadeInDuration);

        if (!panStarted && ElapsedSeconds >= panStartDelay)
        {
            CameraSystem.ReturnCamera(panDuration);
            panStarted = true;
        }

        if (ElapsedSeconds >= logoFadeDelay && ElapsedSeconds < logoFadeOutDelay)
        {
            var logoProgress = Math.Min((ElapsedSeconds - logoFadeDelay) / logoFadeDuration, 1f);
            logoAlpha = MathHelper.SmoothStep(0f, 1f, logoProgress);
        }
        else if (ElapsedSeconds >= logoFadeOutDelay)
        {
            var fadeOutProgress = Math.Min((ElapsedSeconds - logoFadeOutDelay) / logoFadeOutDuration, 1f);
            logoAlpha = MathHelper.SmoothStep(1f, 0f, fadeOutProgress);
        }

        var logoFadeEndTime = logoFadeOutDelay + logoFadeOutDuration;
        if (!fallSequenceStarted && ElapsedSeconds >= logoFadeEndTime + fallStartDelay)
        {
            StartFalling();
            fallSequenceStarted = true;
        }

        if (fallSequenceStarted)
        {
            fallSequenceTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateFalling();
        }
    }

    private void StartFalling()
    {

        var fallStartPosition = new Vector2(originalPlayerPosition.X, originalPlayerPosition.Y - fallHeight);
        Main.LocalPlayer.position = fallStartPosition - new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height);
        Main.LocalPlayer.velocity = Vector2.Zero;

        InvisOFF();

        FallDamageOFF();

        playerFalling = true;
    }

    private void UpdateFalling()
    {
        CameraSystem.LockCamera(new Vector2(originalPlayerPosition.X, originalPlayerPosition.Y - 28));

        if (playerFalling && !impactOccurred)
        {
            Main.LocalPlayer.fullRotation += 0.35f;
            Main.LocalPlayer.fullRotationOrigin = new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height / 2f);
        }

        if (!playerFalling || impactOccurred) return;

        if (Main.LocalPlayer.Center.Y >= originalPlayerPosition.Y - 50f && Main.LocalPlayer.velocity.Y > 0)
        {
            Main.LocalPlayer.fullRotation = 0f;
            Main.LocalPlayer.fullRotationOrigin = Vector2.Zero;

            impactOccurred = true;
            FallDamageOFF();
            CameraSystem.shake = 35;
            SoundEngine.PlaySound(SoundID.Item14);
            playerFalling = false;
        }
    }

    protected override void DrawCutsceneContent(SpriteBatch spriteBatch)
    {
        if (logoTexture != null && logoAlpha > 0f)
        {
            DrawLogo(spriteBatch);
        }
    }

    private void DrawLogo(SpriteBatch spriteBatch)
    {
        var logoDrawCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 14f);
        var logoRenderPos = new Vector2(logoDrawCenter.X / 1.35f, logoDrawCenter.Y);
        var logoWidth = logoTexture.Width * logoScale;
        var logoHeight = logoTexture.Height * logoScale;

        var dotOffset = new Vector2(logoWidth * 0.82f, logoHeight * 0.39f);
        var galaxyWorldPos = logoRenderPos + dotOffset;
        var galaxyShaderPos = new Vector2(
            (galaxyWorldPos.X - Main.screenWidth * 0.5f) / (Main.screenWidth * 0.5f),
            (galaxyWorldPos.Y - Main.screenHeight * 0.5f) / (Main.screenHeight * 0.5f)
        );

        spriteBatch.Draw(logoTexture, logoRenderPos, null, Color.White * logoAlpha,
                       logoRotation, Vector2.Zero, logoScale, SpriteEffects.None, 0f);

        spriteBatch.End();

        var effect = ShaderLoader.GetShader("GalaxyShader").Value;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                        DepthStencilState.None, Main.Rasterizer, effect, Main.UIScaleMatrix);

        if (effect != null)
        {
            effect.Parameters["uTime"]?.SetValue((float)(Main.timeForVisualEffects * 0.0015f));
            effect.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["uSourceRect"]?.SetValue(new Vector4(0, 0, Main.screenWidth, Main.screenHeight));
            effect.Parameters["uIntensity"]?.SetValue(.85f * logoAlpha);

            effect.Parameters["uImage0"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}RibbonTrail", AssetRequestMode.ImmediateLoad).Value);
            effect.Parameters["uImage1"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}StormTrail", AssetRequestMode.ImmediateLoad).Value);

            effect.Parameters["uCenter"]?.SetValue(galaxyShaderPos);
            effect.Parameters["uScale"]?.SetValue(3.5f);

            effect.Parameters["uRotation"]?.SetValue((float)(Main.timeForVisualEffects * 0.001f));
            effect.Parameters["uArmCount"]?.SetValue(4.0f);

            effect.Parameters["uColor"]?.SetValue(new Vector4(0.8f, 0.4f, 1.5f, .85f));
        }

        var perlinSpiral = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Perlin").Value;
        spriteBatch.Draw(perlinSpiral, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                       Color.White * logoAlpha);

        var pixelTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value;
        spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                       Color.White * logoAlpha);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap,
                        DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
    }

    public override bool IsFinished()
    {
        if (impactOccurred && fallSequenceTimer >= 2f)
        {
            return true;
        }
        return ElapsedSeconds >= logoFadeOutDelay + logoFadeOutDuration + 10f;
    }

    protected override void OnCutsceneEnd()
    {
        CameraSystem.UnlockCamera();
        CameraSystem.ReturnCamera(1);
        DownedSystem.initialCutscene = true;

        ControlsON();

        InvisOFF();
        FallDamageON();

        if (!impactOccurred)
        {
            Main.LocalPlayer.position = originalPlayerPosition - new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height);
            Main.LocalPlayer.velocity = Vector2.Zero;
        }

        DialogueManager.Instance.StartDialogue("JourneysBegin.Crash", 4, zoomIn: false, letterbox: true);
    }
}
