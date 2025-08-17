using Microsoft.Xna.Framework;
using ReLogic.Content;
using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using Reverie.Core.Cinematics.Music;
using Reverie.Core.Loaders;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.Cutscenes;

public class OpeningCutscene : Cutscene
{
    private const float SCENE1_DURATION = 12f;
    private const float SCENE2_DURATION = 7f;
    private const float FADE_IN_DURATION = 3f;
    private const float LOGO_FADE_DELAY = 2f;
    private const float LOGO_FADE_DURATION = 2f;
    private const float LOGO_FADE_OUT_DELAY = 6f;
    private const float LOGO_FADE_OUT_DURATION = 2f;
    private const float STAR_FADE_START = 8f;
    private const float STAR_FADE_DURATION = 4f;

    private float logoAlpha = 0f;
    private float logoScale = 1.35f;
    private float logoRotation = 0f;
    private Texture2D logoTexture;
    private float starFieldAlpha = 1f;

    private Vector2 spawnTilePosition;
    private Vector2 topLeftPosition;
    private Vector2 topRightPosition;
    private bool cameraPathStarted = false;
    private bool impactStarted = false;
    private float horizontalPanDistance = 2000f;

    private List<CutsceneStar> cutsceneStars = [];
    private List<CutsceneObject> screenObj = [];
    private Dictionary<CutsceneStar, Vector2> starVelocities = [];
    private float spaceDriftSpeed = 0.32f;

    // Player fall variables
    private bool playerFallStarted = false;
    private bool playerFalling = false;
    private bool impactOccurred = false;
    private Vector2 originalPlayerPos;
    private float fallHeight = 6500f;
    private float impactTimer = 0f;
    private bool wasOnGround = false;

    public override void Start()
    {
        spawnTilePosition = new Vector2(Main.spawnTileX * 16f + 8f, Main.spawnTileY * 16f + 8f);

        var topY = 40f * 16f;
        topLeftPosition = new Vector2(spawnTilePosition.X - horizontalPanDistance / 2f, topY);
        topRightPosition = new Vector2(spawnTilePosition.X + horizontalPanDistance / 2f, topY);

        EnableLetterbox = true;
        base.Start();
        SetMusic(MusicLoader.GetMusicSlot($"{CUTSCENE_MUSIC_DIRECTORY}DawnofReverie"), MusicFadeMode.Instant);
    }

    protected override void OnCutsceneStart()
    {
        FadeAlpha = 1f;
        FadeColor = Color.Black;
        logoTexture = ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}Logo_Outline", AssetRequestMode.ImmediateLoad).Value;

        StartCameraPath();

        ControlsOFF();
        InvisON();
        Main.shimmerAlpha = 1f;

        InitializeStars();

        // Store original player state
        var player = Main.LocalPlayer;
        originalPlayerPos = player.position;
        wasOnGround = player.velocity.Y == 0f && Collision.SolidCollision(player.position, player.width, player.height + 1);
    }

    private void StartCameraPath()
    {
        var waypoints = new List<Vector2>
        {
            topLeftPosition,
            topRightPosition,
            spawnTilePosition
        };

        var durations = new List<int>
        {
            (int)(SCENE1_DURATION * 60f),
            (int)(SCENE2_DURATION * 60f)
        };

        CameraSystem.CreateCameraPath(waypoints, durations);
        cameraPathStarted = true;
    }

    protected override void OnCutsceneUpdate(GameTime gameTime)
    {
        if (ElapsedSeconds < SCENE1_DURATION)
        {
            StarfieldScene();
        }
        else
        {
            FallingScene(gameTime);
        }

        UpdateStars();
        UpdatePlayerFall();
    }

    private void StarfieldScene()
    {
        if (ElapsedSeconds < FADE_IN_DURATION)
        {
            FadeIn(FADE_IN_DURATION * 60f);
        }

        if (ElapsedSeconds >= LOGO_FADE_DELAY && ElapsedSeconds < LOGO_FADE_OUT_DELAY)
        {
            var logoProgress = Math.Min((ElapsedSeconds - LOGO_FADE_DELAY) / LOGO_FADE_DURATION, 1f);
            logoAlpha = MathHelper.SmoothStep(0f, 1f, logoProgress);
        }
        else if (ElapsedSeconds >= LOGO_FADE_OUT_DELAY)
        {
            var fadeOutProgress = Math.Min((ElapsedSeconds - LOGO_FADE_OUT_DELAY) / LOGO_FADE_OUT_DURATION, 1f);
            logoAlpha = MathHelper.SmoothStep(1f, 0f, fadeOutProgress);
        }

        if (ElapsedSeconds >= STAR_FADE_START)
        {
            float fadeProgress = (ElapsedSeconds - STAR_FADE_START) / STAR_FADE_DURATION;
            starFieldAlpha = Math.Max(0f, 1f - fadeProgress);
        }

        Main.shimmerAlpha = 1f;
    }

    private void FallingScene(GameTime gameTime)
    {
        if (!playerFallStarted)
        {
            InvisOFF();
            StartPlayerFall();
            playerFallStarted = true;
        }

        float scene2Progress = (ElapsedSeconds - SCENE1_DURATION) / SCENE2_DURATION;
        Main.shimmerAlpha = 1f - (scene2Progress * 0.8f);

        if (impactOccurred)
        {
            impactTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            Main.shimmerAlpha = 0f;

            if (impactTimer > 0.5f)
            {
                FadeOut(3f * 60f, 0f, Color.Black);
            }
        }
    }

    private void StartPlayerFall()
    {
        var player = Main.LocalPlayer;

        var fallStartPos = new Vector2(
            spawnTilePosition.X + Main.rand.Next(-100, 100),
            spawnTilePosition.Y - fallHeight
        );

        player.position = fallStartPos - new Vector2(player.width / 2f, player.height);

        FallDamageOFF();
        playerFalling = true;
    }

    private void UpdatePlayerFall()
    {
        if (!playerFalling || impactOccurred) return;

        var player = Main.LocalPlayer;

        // Add rotation while falling
        player.fullRotation += 0.2f;
        player.fullRotationOrigin = new Vector2(player.width / 2f, player.height / 2f);

        bool isOnGround = player.velocity.Y == 0f && Collision.SolidCollision(player.position, player.width, player.height + 1);

        if (!wasOnGround && isOnGround && player.velocity.Y >= 0f)
        {
            TriggerImpact();
        }

        wasOnGround = isOnGround;
    }

    private void TriggerImpact()
    {
        var player = Main.LocalPlayer;

        player.fullRotation = 0f;
        player.fullRotationOrigin = Vector2.Zero;

        CameraSystem.shake = 25;
        SoundEngine.PlaySound(SoundID.Item14);

        impactOccurred = true;
        playerFalling = false;
        impactTimer = 0f;
        FallDamageOFF();
    }

    private void InitializeStars()
    {
        cutsceneStars.Clear();
        screenObj.Clear();
        starVelocities.Clear();

        var starCount = Main.rand.Next(60, 100);
        for (var i = 0; i < starCount; i++)
        {
            CreateNewStar(true);
        }

        for (var i = 0; i < Main.rand.Next(3, 6); i++)
        {
            CreateObjects();
        }
    }

    private void CreateNewStar(bool initialSetup = false)
    {
        var star = new CutsceneStar
        {
            position = new Vector2(
                Main.rand.Next(0, Main.screenWidth),
                Main.rand.Next(0, Main.screenHeight)
            ),
            rotation = Main.rand.NextFloat(0, MathHelper.TwoPi),
            scale = Main.rand.NextFloat(0.3f, 0.8f),
            type = Main.rand.Next(0, 4),
            twinkle = Main.rand.NextFloat(0.6f, 1f),
            twinkleSpeed = Main.rand.NextFloat(0.001f, 0.005f),
            rotationSpeed = Main.rand.NextFloat(-0.0001f, 0.0001f),
            fadeIn = initialSetup ? 0f : 0.5f
        };

        if (Main.rand.NextBool())
            star.twinkleSpeed *= -1f;

        // Stars drift rightward to match horizontal camera pan
        starVelocities[star] = new Vector2(
            spaceDriftSpeed * (0.5f + star.scale * 0.3f),
            Main.rand.NextFloat(-0.1f, 0.1f)
        );

        cutsceneStars.Add(star);
    }

    private void CreateObjects()
    {
        var spaceObj = new CutsceneObject
        {
            position = new Vector2(
                Main.rand.Next(-100, Main.screenWidth + 100),
                Main.rand.Next(100, Main.screenHeight - 100)
            ),
            velocity = new Vector2(
                spaceDriftSpeed * 0.6f,
                Main.rand.NextFloat(-0.05f, 0.05f)
            ),
            rotation = Main.rand.NextFloat(0, MathHelper.TwoPi),
            scale = Main.rand.NextFloat(0.4f, 0.7f),
            type = Main.rand.Next(4),
            alpha = 0f
        };

        screenObj.Add(spaceObj);
    }

    private void UpdateStars()
    {
        // Update stars (only during Scene 1)
        if (ElapsedSeconds < SCENE1_DURATION && starFieldAlpha > 0f)
        {
            for (var i = cutsceneStars.Count - 1; i >= 0; i--)
            {
                var star = cutsceneStars[i];
                star.Update();
                star.position += starVelocities[star];

                // Remove stars that go off-screen right (matching camera pan direction)
                if (star.position.X > Main.screenWidth + 50 || star.position.Y < -50 || star.position.Y > Main.screenHeight + 50)
                {
                    cutsceneStars.RemoveAt(i);
                    starVelocities.Remove(star);
                    CreateNewStar();
                }
            }

            // Update easter eggs
            for (var i = screenObj.Count - 1; i >= 0; i--)
            {
                var easterEgg = screenObj[i];
                easterEgg.Update();

                if (easterEgg.position.X > Main.screenWidth + 100)
                {
                    screenObj.RemoveAt(i);
                    CreateObjects();
                }
            }

            // Occasionally create new stars
            if (Main.rand.NextBool(120))
            {
                CreateNewStar();
            }
        }
    }

    protected override void DrawCutsceneContent(SpriteBatch spriteBatch)
    {
        base.DrawCutsceneContent(spriteBatch);

        if (starFieldAlpha > 0f && ElapsedSeconds < SCENE1_DURATION)
        {
            DrawStarField(spriteBatch);
        }
    }

    private void DrawStarField(SpriteBatch spriteBatch)
    {
        var glowTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;
        var starTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star").Value;
        var colorStart = new Color(143, 244, 255);
        var colorEnd = Color.White;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap,
                         DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        // Draw stars
        foreach (var star in cutsceneStars)
        {
            var brightness = 0.5f + star.twinkle * 0.5f;
            var colorLerp = star.type / 4f;
            var baseColor = Color.Lerp(colorStart, colorEnd, colorLerp);
            var alpha = star.fadeIn > 0f ? 1f - star.fadeIn : 1f;
            var starColor = baseColor * brightness * alpha * starFieldAlpha;
            var bloomScale = star.scale * (0.8f + star.twinkle * 0.4f);

            spriteBatch.Draw(glowTexture, star.position, null, starColor * 0.6f, 0f,
                           new Vector2(glowTexture.Width / 2, glowTexture.Height / 2),
                           bloomScale * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.Draw(starTexture, star.position, null, starColor, star.rotation,
                           new Vector2(starTexture.Width / 2, starTexture.Height / 2),
                           bloomScale * 0.5f, SpriteEffects.None, 0f);
        }

        // Draw easter eggs
        foreach (var easterEgg in screenObj)
        {
            var texture = easterEgg.GetTexture();

            spriteBatch.Draw(glowTexture, easterEgg.position, null,
                           Color.White * 0.5f * easterEgg.alpha * starFieldAlpha,
                           0f, new Vector2(glowTexture.Width / 2, glowTexture.Height / 2),
                           easterEgg.scale * 2f, SpriteEffects.None, 0f);

            spriteBatch.Draw(texture, easterEgg.position, null,
                           Color.White * easterEgg.alpha * starFieldAlpha,
                           easterEgg.rotation, new Vector2(texture.Width / 2, texture.Height / 2),
                           easterEgg.scale, SpriteEffects.None, 0f);
        }

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap,
                         DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
    }



    protected override void OnCutsceneEnd()
    {
        base.OnCutsceneEnd();
        Main.shimmerAlpha = 0f;

        CameraSystem.UnlockCamera();
        CameraSystem.ReturnCamera(1);
        CameraSystem.Reset();
        ControlsON();
        InvisOFF();
        FallDamageON();

        // Reset player rotation
        var player = Main.LocalPlayer;
        player.fullRotation = 0f;
        player.fullRotationOrigin = Vector2.Zero;

        cutsceneStars.Clear();
        screenObj.Clear();
        starVelocities.Clear();
    }

    public override bool IsFinished()
    {
        return impactOccurred && impactTimer >= 2f;
    }
}

public class CutsceneStar
{
    public Vector2 position;
    public float rotation;
    public float scale;
    public int type;
    public float twinkle;
    public float twinkleSpeed;
    public float rotationSpeed;
    public float fadeIn;

    public void Update()
    {
        rotation += rotationSpeed;
        twinkle += twinkleSpeed;
        twinkle = MathHelper.Clamp(twinkle, 0f, 1f);

        if (fadeIn > 0f)
            fadeIn -= 0.02f;
    }
}

public class CutsceneObject
{
    public Vector2 position;
    public Vector2 velocity;
    public float rotation;
    public float scale;
    public float alpha;
    public int type;

    public void Update()
    {
        position += velocity;
        rotation += 0.005f;

        if (alpha < 1f)
            alpha += 0.01f;
    }

    public Texture2D GetTexture()
    {
        return type switch
        {
            0 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}Rock_01").Value,
            1 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}Rock_02").Value,
            2 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}Rock_03").Value,
            3 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostTree").Value,
            _ => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostMartian").Value
        };
    }
}