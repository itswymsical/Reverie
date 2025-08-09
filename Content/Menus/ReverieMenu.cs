using ReLogic.Content;
using Reverie.Content.Biomes.Canopy;
using Reverie.Core.Loaders;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.Graphics.Effects;

namespace Reverie.Content.Menus;

public partial class ReverieMenu : ModMenu
{
    private List<Star> menuStars;
    private List<EasterEggObject> easterEggObjects;

    private bool starsInitialized = false;

    private float spaceDriftSpeed = 0.05f;
    private List<Vector2> clickPositions = [];
    private List<float> clickTimes = [];
    private const float CLICK_LIFETIME = 60f;
    private const float BURST_RADIUS = 120f;
    private const float BURST_FORCE = 3f;
    private const float COLLISION_THRESHOLD = 8f;

    private Dictionary<Star, Vector2> starVelocities = [];

    private const float EASTER_EGG_CHANCE = 0.05f;
    private const float EASTER_EGG_ROTATION_SPEED = 0.005f;
    private const float EASTER_EGG_DRIFT_SPEED = 0.2f;

    public override void Load()
    {
        menuStars = [];
        clickPositions = [];
        clickTimes = [];
        starVelocities = [];
        easterEggObjects = [];
    }

    public override void Unload()
    {
        menuStars.Clear();
        easterEggObjects.Clear();
        clickPositions.Clear();
        clickTimes.Clear();
        starVelocities.Clear();
        starsInitialized = false;
    }

    public override int Music => MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}Meditation");

    public override string DisplayName => "Reverie";

    public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}Logo_Outline");

    public override Asset<Texture2D> SunTexture => null;

    public override Asset<Texture2D> MoonTexture => null;
    public static bool InMenu => MenuLoader.CurrentMenu == ModContent.GetInstance<ReverieMenu>() && Main.gameMenu;

    public override void OnSelected()
    {
        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ReverieMenu") with { Volume = 0.65f});
    }

    private void InitializeStars()
    {
        menuStars.Clear();
        easterEggObjects.Clear();

        var starCount = Main.rand.Next(70, 100);
        for (var i = 0; i < starCount; i++)
        {
            CreateNewStar(true);
        }

        TryCreateEasterEgg(new Vector2(Main.rand.Next(Main.screenWidth), Main.rand.Next(Main.screenHeight)));

        starsInitialized = true;
    }

    private bool TryCreateEasterEgg(Vector2 position)
    {
        if (easterEggObjects.Count >= 3)
            return false;

        if (Main.rand.NextFloat() > EASTER_EGG_CHANCE && position.X < 0)
            return false;

        var velocity = new Vector2(
            -EASTER_EGG_DRIFT_SPEED * Main.rand.NextFloat(0.9f, 1.3f),
            Main.rand.NextFloat(-0.01f, 0.01f)
        );

        var scale = Main.rand.NextFloat(0.3f, 0.7f);

        easterEggObjects.Add(new EasterEggObject(position, velocity, scale));
        return true;
    }

    private void CreateNewStar(bool initialSetup = false)
    {
        if (!initialSetup && Main.rand.NextFloat() < EASTER_EGG_CHANCE)
        {
            // Try to create an easter egg at the right edge of the screen
            if (TryCreateEasterEgg(new Vector2(Main.screenWidth + 20, Main.rand.Next(100, Main.screenHeight - 100))))
                return;
        }

        var star = new Star();

        star.position = new Vector2(
            Main.rand.Next(0, Main.screenWidth),
            Main.rand.Next(0, Main.screenHeight)
        );

        star.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
        star.scale = Main.rand.NextFloat(0.2f, 0.72f);
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
            -spaceDriftSpeed * (0.5f + star.scale),
            0f
        );

        menuStars.Add(star);
    }

    private void CreateStardustBurst(Vector2 position, float scale, Color color)
    {
        // Create a burst of dust particles at the collision point
        var dustCount = Main.rand.Next(8, 16);
        for (var i = 0; i < dustCount; i++)
        {
            var dustVelocity = new Vector2(
                Main.rand.NextFloat(-2f, 2f),
                Main.rand.NextFloat(-2f, 2f)
            );

            var dust = Dust.NewDustPerfect(
                position,
                DustID.AncientLight,
                dustVelocity,
                0,
                color,
                scale * Main.rand.NextFloat(1f, 1.7f)
            );

            dust.noGravity = true;
            dust.fadeIn = 0.1f;
            dust.noLight = false;
        }
    }

    private float galaxyTime = 0f;

    public override void Update(bool isOnTitleScreen)
    {
        base.Update(isOnTitleScreen);

        if (!InMenu)
        {
            // If we were initialized but now we're not in the menu, clean up resources
            if (starsInitialized)
            {
                menuStars.Clear();
                easterEggObjects.Clear();
                clickPositions.Clear();
                clickTimes.Clear();
                starVelocities.Clear();
                starsInitialized = false;
            }
            return;
        }
        if (InMenu)
        {
            galaxyTime += 1f / 60f; // Smooth increment only when in menu
            if (galaxyTime > 1000f) galaxyTime -= 1000f; // Keep it bounded
        }

        if (Main.mouseLeft && Main.mouseLeftRelease)
        {
            var clickPosition = new Vector2(Main.mouseX, Main.mouseY);
            clickPositions.Add(clickPosition);
            clickTimes.Add(0f);
        }

        // Update click lifetimes and remove old clicks
        for (var i = clickTimes.Count - 1; i >= 0; i--)
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

        drawColor = Color.White;
        logoScale = 1.4f;
        logoRotation = 0f;
        logoDrawCenter = logoDrawCenter + new Vector2(0, 20);

        if (!InMenu || !Main.gameMenu)
        {
            // If we're not in the menu, clear resources to save memory
            if (starsInitialized)
            {
                menuStars.Clear();
                easterEggObjects.Clear();
                clickPositions.Clear();
                clickTimes.Clear();
                starVelocities.Clear();
                starsInitialized = false;
            }
            return true; // Skip all the drawing but allow the logo to be drawn normally
        }

        if (!starsInitialized)
            InitializeStars();

        spriteBatch.Draw(
            ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Space").Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            drawColor
        );

        var glowTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;
        var starTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star").Value;

        // scren glow
        spriteBatch.Draw(
            glowTexture,
            logoDrawCenter,
            null,
            drawColor * 0.02f,
            0f,
            new Vector2(glowTexture.Width / 2, glowTexture.Height / 2),
            new Vector2(Logo.Width(), Logo.Height()),
            SpriteEffects.None,
            0f
        );
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        var colorStart = new Color(143, 244, 255);
        var colorEnd = Color.White;

        for (var i = easterEggObjects.Count - 1; i >= 0; i--)
        {
            var easterEgg = easterEggObjects[i];

            easterEgg.Update();

            var texture = easterEgg.GetTexture();

            spriteBatch.Draw(
                glowTexture,
                easterEgg.Position,
                null,
                Color.White * 0.5f * easterEgg.Alpha,
                0f,
                new Vector2(glowTexture.Width / 2, glowTexture.Height / 2),
                easterEgg.Scale * 2f,
                SpriteEffects.None,
                0f
            );

            spriteBatch.Draw(
                texture,
                easterEgg.Position,
                null,
                Color.White * easterEgg.Alpha,
                easterEgg.Rotation,
                new Vector2(texture.Width / 2, texture.Height / 2),
                easterEgg.Scale,
                SpriteEffects.None,
                0f
            );

            // Remove if off-screen
            if (easterEgg.Position.X < -50 ||
                easterEgg.Position.Y < -50 ||
                easterEgg.Position.Y > Main.screenHeight + 50)
            {
                easterEggObjects.RemoveAt(i);
            }
        }

        // Process any click bursts
        for (var i = 0; i < clickPositions.Count; i++)
        {
            var clickPos = clickPositions[i];
            var clickAge = clickTimes[i];

            var burstScale = clickAge / 15f * 3f;
            var alpha = 1f - clickAge / CLICK_LIFETIME;
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

            if (clickAge < 2f)
            {
                foreach (var star in menuStars)
                {
                    if (star.falling)
                        continue;

                    // Calculate distance to star
                    var distance = Vector2.Distance(clickPos, star.position);

                    // Apply force if within burst radius
                    if (distance < BURST_RADIUS)
                    {
                        // Direction from click to star
                        var direction = star.position - clickPos;
                        if (direction != Vector2.Zero)
                            direction.Normalize();

                        // Force diminishes with distance
                        var force = BURST_FORCE * (1f - distance / BURST_RADIUS);

                        // Apply burst force to star's velocity
                        starVelocities[star] += direction * force;
                    }
                }
            }
        }

        // Check for star collisions
        // We need to use traditional for loops (not foreach) because we might modify the collection
        for (var i = 0; i < menuStars.Count; i++)
        {
            var star1 = menuStars[i];

            // Skip falling stars for collision
            if (star1.falling)
                continue;

            // Compare with other stars
            for (var j = i + 1; j < menuStars.Count; j++)
            {
                var star2 = menuStars[j];

                // Skip falling stars for collision
                if (star2.falling)
                    continue;

                // Calculate distance between stars
                var distance = Vector2.Distance(star1.position, star2.position);
                var collisionSize = (star1.scale + star2.scale) * COLLISION_THRESHOLD;

                // If stars are close enough, create collision effect
                if (distance < collisionSize)
                {
                    // Calculate collision point (midpoint between stars)
                    var collisionPoint = (star1.position + star2.position) * 0.5f;

                    // Calculate average color for the dust burst
                    var colorLerp1 = star1.type / 4f + (int)star1.position.X * (int)star1.position.Y % 10 / 10f;
                    var colorLerp2 = star2.type / 4f + (int)star2.position.X * (int)star2.position.Y % 10 / 10f;
                    colorLerp1 = colorLerp1 % 1f;
                    colorLerp2 = colorLerp2 % 1f;

                    var starColor1 = Color.Lerp(colorStart, colorEnd, colorLerp1 * (Main.GameUpdateCount / 60f));
                    var starColor2 = Color.Lerp(colorStart, colorEnd, colorLerp2 * (Main.GameUpdateCount / 60f));
                    var burstColor = Color.Lerp(starColor1, starColor2, 0.5f * (Main.GameUpdateCount / 60f));

                    // Create stardust burst
                    CreateStardustBurst(collisionPoint, (star1.scale + star2.scale) * 0.5f, burstColor);

                    // Push stars apart
                    if (starVelocities.ContainsKey(star1) && starVelocities.ContainsKey(star2))
                    {
                        var pushDir1 = star1.position - star2.position;
                        if (pushDir1 != Vector2.Zero)
                            pushDir1.Normalize();

                        var pushDir2 = -pushDir1;

                        starVelocities[star1] += pushDir1 * 1.1f;
                        starVelocities[star2] += pushDir2 * 1.1f;
                    }
                }
            }
        }

        // Move and draw all stars
        for (var i = menuStars.Count - 1; i >= 0; i--)
        {
            var star = menuStars[i];

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
            var brightness = 0.5f + star.twinkle * 0.5f;
            var colorLerp = star.type / 4f + (int)star.position.X * (int)star.position.Y % 10 / 10f;
            colorLerp = colorLerp % 1f;
            var baseColor = Color.Lerp(colorStart, colorEnd, colorLerp);

            var alpha = 1f;
            if (star.fadeIn > 0f)
                alpha = 1f - star.fadeIn;

            var starColor = baseColor * brightness * alpha;
            var bloomScale = star.scale * (0.8f + star.twinkle * 0.4f);

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

                CreateNewStar();
            }
        }

        if (Main.rand.NextBool(50))
        {
            var starIndex = Main.rand.Next(menuStars.Count);
            menuStars[starIndex].Fall();

            if (starVelocities.ContainsKey(menuStars[starIndex]))
                starVelocities.Remove(menuStars[starIndex]);

            var star = menuStars[starIndex];
            star.rotationSpeed = 0.1f;
            star.rotation = 0.01f;
            star.fallSpeed.Y = Main.rand.Next(100, 201) * 0.001f;
            star.fallSpeed.X = Main.rand.Next(-100, 101) * 0.001f;
        }

        if (Main.rand.NextBool(160))
        {
            CreateNewStar();
        }

        var logoRenderPos = new Vector2(logoDrawCenter.X / 1.35f, logoDrawCenter.Y * -0.25f);
        var logoWidth = Logo.Width() * logoScale;
        var logoHeight = Logo.Height() * logoScale;
        var dotOffset = new Vector2(
            logoWidth * 0.825f,
            logoHeight * 0.39f
        );
        var galaxyWorldPos = logoRenderPos + dotOffset;
        var galaxyShaderPos = new Vector2(
            (galaxyWorldPos.X - Main.screenWidth * 0.5f) / (Main.screenWidth * 0.5f),
            (galaxyWorldPos.Y - Main.screenHeight * 0.5f) / (Main.screenHeight * 0.5f)
        );

        spriteBatch.Draw(Logo.Value, new Vector2(logoDrawCenter.X / 1.35f, logoDrawCenter.Y * -0.25f),
                         null, Color.White, logoRotation, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
        spriteBatch.End();

        var effect = ShaderLoader.GetShader("GalaxyShader").Value;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap,
                          DepthStencilState.None, Main.Rasterizer, effect, Main.UIScaleMatrix);

        if (effect != null)
        {
            effect.Parameters["uTime"]?.SetValue((float)(Main.timeForVisualEffects * 0.0015f));
            effect.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["uSourceRect"]?.SetValue(new Vector4(0, 0, Main.screenWidth, Main.screenHeight));
            effect.Parameters["uIntensity"]?.SetValue(.85f);

            effect.Parameters["uImage0"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}RibbonTrail", AssetRequestMode.ImmediateLoad).Value);
            effect.Parameters["uImage1"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}StormTrail", AssetRequestMode.ImmediateLoad).Value);

            effect.Parameters["uCenter"]?.SetValue(galaxyShaderPos);
            effect.Parameters["uScale"]?.SetValue(3.5f);

            effect.Parameters["uRotation"]?.SetValue((float)(Main.timeForVisualEffects * 0.001f));
            effect.Parameters["uArmCount"]?.SetValue(4.0f);

            effect.Parameters["uColor"]?.SetValue(new Vector4(0.8f, 0.4f, 1.5f, .85f));
        }

        var pixel = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Wyrmscape").Value;
        spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

        var pixel2 = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value;
        spriteBatch.Draw(pixel2, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap,
                          DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix); 
        return false;
    }
}