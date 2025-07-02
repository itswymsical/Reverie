using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using Reverie.Core.Cinematics.Music;
using Terraria.Audio;

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
    private bool panStarted = false;
    private Texture2D logoTexture;

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

        base.Start();
        SetMusic(MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}DawnofReverie"), MusicFadeMode.Instant);
    }

    protected override void OnCutsceneStart()
    {
        FadeAlpha = 1f;
        FadeColor = Color.Black;
        logoTexture = ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}Logo").Value;

        var startPos = originalPlayerPosition - new Vector2(0, panDistance);
        CameraSystem.MoveCameraOut(1, startPos);
        DisablePlayerMovement();

        EnableInvisibility();
    }

    protected override void OnCutsceneUpdate(GameTime gameTime)
    {
        FadeIn(fadeInDuration);

        if (!panStarted && ElapsedTime >= panStartDelay)
        {
            CameraSystem.ReturnCamera(panDuration);
            panStarted = true;
        }

        if (ElapsedTime >= logoFadeDelay && ElapsedTime < logoFadeOutDelay)
        {
            var logoProgress = Math.Min((ElapsedTime - logoFadeDelay) / logoFadeDuration, 1f);
            logoAlpha = MathHelper.SmoothStep(0f, 1f, logoProgress);
        }
        else if (ElapsedTime >= logoFadeOutDelay)
        {
            var fadeOutProgress = Math.Min((ElapsedTime - logoFadeOutDelay) / logoFadeOutDuration, 1f);
            logoAlpha = MathHelper.SmoothStep(1f, 0f, fadeOutProgress);
        }

        var logoFadeEndTime = logoFadeOutDelay + logoFadeOutDuration;
        if (!fallSequenceStarted && ElapsedTime >= logoFadeEndTime + fallStartDelay)
        {
            StartFallSequence();
            fallSequenceStarted = true;
        }

        if (fallSequenceStarted)
        {
            fallSequenceTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateFallSequence();
        }
    }

    private void StartFallSequence()
    {

        var fallStartPosition = new Vector2(originalPlayerPosition.X, originalPlayerPosition.Y - fallHeight);
        Main.LocalPlayer.position = fallStartPosition - new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height);
        Main.LocalPlayer.velocity = Vector2.Zero;

        DisableInvisibility();

        EnableFallDamage();

        playerFalling = true;
    }

    private void UpdateFallSequence()
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
            DisableFallDamage();
            CameraSystem.shake = 35;
            SoundEngine.PlaySound(SoundID.Item14);
            playerFalling = false;
        }
    }

    protected override void DrawCutsceneContent(SpriteBatch spriteBatch)
    {
        if (logoTexture != null && logoAlpha > 0f)
        {
            var logoSize = new Vector2(logoTexture.Width, logoTexture.Height);
            var logoPosition = new Vector2(
                (Main.screenWidth - logoSize.X) / 2.25f, 0f);
            spriteBatch.Draw(
                logoTexture,
                logoPosition,
                null,
                Color.White * logoAlpha,
                0f,
                Vector2.Zero,
                1.25f,
                SpriteEffects.None,
                0f
            );
        }
    }

    public override bool IsFinished()
    {
        if (impactOccurred && fallSequenceTimer >= 2f)
        {
            return true;
        }
        return ElapsedTime >= logoFadeOutDelay + logoFadeOutDuration + 10f;
    }

    protected override void OnCutsceneEnd()
    {
        CameraSystem.UnlockCamera();
        CameraSystem.ReturnCamera(1);

        EnablePlayerMovement();

        DisableInvisibility();
        DisableFallDamage();

        if (!impactOccurred)
        {
            Main.LocalPlayer.position = originalPlayerPosition - new Vector2(Main.LocalPlayer.width / 2f, Main.LocalPlayer.height);
            Main.LocalPlayer.velocity = Vector2.Zero;
        }

        MusicFadeHandler.RestorePreviousMusic(MusicFadeMode.FadeOut, 9.5f);
    }
}