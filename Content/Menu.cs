﻿using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;

namespace Reverie.Content;

public class ReverieMenu : ModMenu
{
    private List<Star> menuStars;
    private bool starsInitialized = false;

    private float spaceDriftSpeed = 0.05f;
    private List<Vector2> clickPositions = new();
    private List<float> clickTimes = new();
    private const float CLICK_LIFETIME = 60f;
    private const float BURST_RADIUS = 120f;
    private const float BURST_FORCE = 3f;
    private const float COLLISION_THRESHOLD = 8f;

    private Dictionary<Star, Vector2> starVelocities = new();

    public override void Load()
    {
        menuStars = [];
        clickPositions = [];
        clickTimes = [];
        starVelocities = new();
    }

    public override int Music => MusicID.Space /*MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}Resurgence")*/;

    public override string DisplayName => "Reverie";

    public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}Logo");

    public override Asset<Texture2D> SunTexture => null;

    public override Asset<Texture2D> MoonTexture => null;
    public static bool InMenu => MenuLoader.CurrentMenu == ModContent.GetInstance<ReverieMenu>() && Main.gameMenu;

    public override void OnSelected()
    {
        SoundEngine.PlaySound(SoundID.DD2_BookStaffCast);
    }

    private void InitializeStars()
    {
        menuStars.Clear();

        int starCount = Main.rand.Next(130, 200);
        for (int i = 0; i < starCount; i++)
        {
            CreateNewStar(true);
        }

        starsInitialized = true;
    }

    private void CreateStardustBurst(Vector2 position, float scale, Color color)
    {
        // Create a burst of dust particles at the collision point
        int dustCount = Main.rand.Next(8, 16);
        for (int i = 0; i < dustCount; i++)
        {
            // Calculate random velocity for the dust particle
            Vector2 dustVelocity = new Vector2(
                Main.rand.NextFloat(-2f, 2f),
                Main.rand.NextFloat(-2f, 2f)
            );

            // Create the dust particle
            Dust dust = Dust.NewDustPerfect(
                position,
                DustID.AncientLight, // Use AncientLight for the stardust effect
                dustVelocity,
                0, // Alpha
                color, // Match the star's color
                scale * Main.rand.NextFloat(0.5f, 1.0f) // Vary the size
            );

            // Configure dust properties
            dust.noGravity = true;
            dust.fadeIn = 0.1f;
            dust.noLight = false;
        }
    }

    private void CreateNewStar(bool initialSetup = false)
    {
        // Create a new Star instance
        Star star = new Star();

        // Position the star randomly across the entire screen
        // (We now use the same positioning logic for both initial and replacement stars)
        star.position = new Vector2(
            Main.rand.Next(0, Main.screenWidth),
            Main.rand.Next(0, Main.screenHeight)
        );

        // Set random rotation
        star.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);

        // Vary star sizes
        star.scale = Main.rand.NextFloat(0.2f, 0.72f);
        if (Main.rand.NextBool(20))
            star.scale *= 0.8f;

        // Set star type (affects appearance)
        star.type = Main.rand.Next(0, 4);

        // Configure twinkling properties
        star.twinkle = Main.rand.NextFloat(0.6f, 1f);
        star.twinkleSpeed = Main.rand.NextFloat(0.0005f, 0.004f);

        // Some stars twinkle faster
        if (Main.rand.NextBool(5))
            star.twinkleSpeed *= 2f;

        // Randomize twinkle direction
        if (Main.rand.NextBool())
            star.twinkleSpeed *= -1f;

        // Configure rotation speed
        star.rotationSpeed = Main.rand.NextFloat(0.000001f, 0.00001f);

        // Randomize rotation direction
        if (Main.rand.NextBool())
            star.rotationSpeed *= -0.05f;

        // Set fade-in effect - always fade in new stars
        // (No special case for initial setup anymore)
        star.fadeIn = 0.5f;

        // Initialize velocity in the dictionary
        // Even for randomly positioned stars, we still want the leftward drift
        starVelocities[star] = new Vector2(
            -spaceDriftSpeed * (0.5f + star.scale),
            0f
        );

        // Add the star to our collection
        menuStars.Add(star);
    }

    public override void Update(bool isOnTitleScreen)
    {
        base.Update(isOnTitleScreen);

        // Check for mouse clicks
        if (Main.mouseLeft && Main.mouseLeftRelease)
        {
            Vector2 clickPosition = new Vector2(Main.mouseX, Main.mouseY);
            clickPositions.Add(clickPosition);
            clickTimes.Add(0f);
        }

        // Update click lifetimes and remove old clicks
        for (int i = clickTimes.Count - 1; i >= 0; i--)
        {
            clickTimes[i]++;
            if (clickTimes[i] >= CLICK_LIFETIME)
            {
                clickTimes.RemoveAt(i);
                clickPositions.RemoveAt(i);
            }
        }
    }

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
    {
        if (!starsInitialized)
            InitializeStars();

        drawColor = Color.White;
        spriteBatch.Draw(
            ModContent.Request<Texture2D>($"{VFX_DIRECTORY}SpaceOverlay").Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            drawColor
        );

        Texture2D glowTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;
        Texture2D starTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star").Value;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        Color colorStart = new Color(143, 244, 255);
        Color colorEnd = Color.White;
        // Process any click bursts
        for (int i = 0; i < clickPositions.Count; i++)
        {
            Vector2 clickPos = clickPositions[i];
            float clickAge = clickTimes[i];

            // Draw click burst effect
            float burstScale = (clickAge / 15f) * 3f;
            float alpha = 1f - (clickAge / CLICK_LIFETIME);
            spriteBatch.Draw(
                glowTexture,
                clickPos,
                null,
                new Color(179, 255, 243) * alpha * 0.5f,
                0f,
                new Vector2(glowTexture.Width / 2, glowTexture.Height / 2),
                burstScale,
                SpriteEffects.None,
                0f
            );

            // Only apply force to stars when the click is new
            if (clickAge < 2f)
            {
                foreach (Star star in menuStars)
                {
                    if (star.falling)
                        continue;

                    // Calculate distance to star
                    float distance = Vector2.Distance(clickPos, star.position);

                    // Apply force if within burst radius
                    if (distance < BURST_RADIUS)
                    {
                        // Direction from click to star
                        Vector2 direction = star.position - clickPos;
                        if (direction != Vector2.Zero)
                            direction.Normalize();

                        // Force diminishes with distance
                        float force = BURST_FORCE * (1f - (distance / BURST_RADIUS));

                        // Apply burst force to star's velocity
                        starVelocities[star] += direction * force;
                    }
                }
            }
        }

        // Check for star collisions
        // We need to use traditional for loops (not foreach) because we might modify the collection
        for (int i = 0; i < menuStars.Count; i++)
        {
            Star star1 = menuStars[i];

            // Skip falling stars for collision
            if (star1.falling)
                continue;

            // Compare with other stars
            for (int j = i + 1; j < menuStars.Count; j++)
            {
                Star star2 = menuStars[j];

                // Skip falling stars for collision
                if (star2.falling)
                    continue;

                // Calculate distance between stars
                float distance = Vector2.Distance(star1.position, star2.position);
                float collisionSize = (star1.scale + star2.scale) * COLLISION_THRESHOLD;

                // If stars are close enough, create collision effect
                if (distance < collisionSize)
                {
                    // Calculate collision point (midpoint between stars)
                    Vector2 collisionPoint = (star1.position + star2.position) * 0.5f;

                    // Calculate average color for the dust burst
                    float colorLerp1 = (star1.type / 4f) + (((int)star1.position.X * (int)star1.position.Y) % 10) / 10f;
                    float colorLerp2 = (star2.type / 4f) + (((int)star2.position.X * (int)star2.position.Y) % 10) / 10f;
                    colorLerp1 = colorLerp1 % 1f;
                    colorLerp2 = colorLerp2 % 1f;

                    Color starColor1 = Color.Lerp(colorStart, colorEnd, colorLerp1 * (Main.GameUpdateCount / 60f));
                    Color starColor2 = Color.Lerp(colorStart, colorEnd, colorLerp2 * (Main.GameUpdateCount / 60f));
                    Color burstColor = Color.Lerp(starColor1, starColor2, 0.5f * (Main.GameUpdateCount / 60f));

                    // Create stardust burst
                    CreateStardustBurst(collisionPoint, (star1.scale + star2.scale) * 0.5f, burstColor);

                    // Push stars apart
                    if (starVelocities.ContainsKey(star1) && starVelocities.ContainsKey(star2))
                    {
                        Vector2 pushDir1 = star1.position - star2.position;
                        if (pushDir1 != Vector2.Zero)
                            pushDir1.Normalize();

                        Vector2 pushDir2 = -pushDir1;

                        starVelocities[star1] += pushDir1 * 1.1f;
                        starVelocities[star2] += pushDir2 * 1.1f;
                    }
                }
            }
        }

        // Move and draw all stars
        for (int i = menuStars.Count - 1; i >= 0; i--)
        {
            Star star = menuStars[i];

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
                    new Vector2(-spaceDriftSpeed * (0.5f + star.scale), 0f),
                    0.01f
                );
            }

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

            // Remove stars that have moved off-screen
            if (star.hidden ||
                star.position.Y > Main.screenHeight + 100 ||
                star.position.Y < -100 ||
                star.position.X < -50)
            {
                menuStars.RemoveAt(i);

                // Remove from velocity dictionary to avoid memory leaks
                if (starVelocities.ContainsKey(star))
                    starVelocities.Remove(star);

                // Create a new star
                CreateNewStar();
            }
        }

        // Create shooting stars occasionally
        if (Main.rand.NextBool(50))
        {
            int starIndex = Main.rand.Next(menuStars.Count);
            menuStars[starIndex].Fall();

            // Remove from velocity dictionary when a star starts falling
            if (starVelocities.ContainsKey(menuStars[starIndex]))
                starVelocities.Remove(menuStars[starIndex]);

            // Customize falling speed
            Star star = menuStars[starIndex];
            star.rotationSpeed = 0.1f;
            star.rotation = 0.01f;
            star.fallSpeed.Y = (float)Main.rand.Next(100, 201) * 0.001f;
            star.fallSpeed.X = (float)Main.rand.Next(-100, 101) * 0.001f;
        }

        // Occasionally add a new star to maintain density
        if (Main.rand.NextBool(160))
        {
            CreateNewStar();
        }

        // End this SpriteBatch and restore normal blending for the logo
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        return true;
    }
}