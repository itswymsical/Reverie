using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content;

public class ReverieMenu : ModMenu
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

    private class EasterEggObject
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float Scale;
        public float Alpha;
        public int Type;

        public EasterEggObject(Vector2 position, Vector2 velocity, float scale)
        {
            Position = position;
            Velocity = velocity;
            Rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
            Scale = scale;

            Alpha = 0f;
            Type = Main.rand.Next(7);
            if (Type == 6)
            {
                Scale = 0.2f;
            }
        }

        public void Update()
        {
            Position += Velocity;

            Rotation += EASTER_EGG_ROTATION_SPEED;
            if (Rotation > MathHelper.TwoPi)
                Rotation -= MathHelper.TwoPi;

            if (Alpha < 1f)
                Alpha += 0.01f;
        }

        public Texture2D GetTexture()
        {
            return Type switch
            {
                0 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostMartian").Value,
                1 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostMeteorHead").Value,
                2 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}SpaceDolphin").Value,
                3 => TextureAssets.Item[ItemID.FirstFractal].Value,
                4 => TextureAssets.Item[ItemID.SDMG].Value,
                5 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostTree").Value,
                6 => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}DeadEye").Value,

                _ => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}LostMartian").Value
            };
        }
    }

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
        easterEggObjects.Clear();

        int starCount = Main.rand.Next(130, 200);
        for (int i = 0; i < starCount; i++)
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

        Vector2 velocity = new Vector2(
            -EASTER_EGG_DRIFT_SPEED * Main.rand.NextFloat(0.9f, 1.3f),
            Main.rand.NextFloat(-0.01f, 0.01f)
        );

        float scale = Main.rand.NextFloat(0.3f, 0.7f);

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

        Star star = new Star();

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
        int dustCount = Main.rand.Next(8, 16);
        for (int i = 0; i < dustCount; i++)
        {
            Vector2 dustVelocity = new Vector2(
                Main.rand.NextFloat(-2f, 2f),
                Main.rand.NextFloat(-2f, 2f)
            );

            Dust dust = Dust.NewDustPerfect(
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
        drawColor = Color.White;
        logoScale = 1.12f;
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
            ModContent.Request<Texture2D>($"{VFX_DIRECTORY}SpaceOverlay").Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            drawColor
        );

        Texture2D glowTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;
        Texture2D starTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star").Value;

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

        Color colorStart = new Color(143, 244, 255);
        Color colorEnd = Color.White;

        for (int i = easterEggObjects.Count - 1; i >= 0; i--)
        {
            EasterEggObject easterEgg = easterEggObjects[i];

            easterEgg.Update();

            Texture2D texture = easterEgg.GetTexture();

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
        for (int i = 0; i < clickPositions.Count; i++)
        {
            Vector2 clickPos = clickPositions[i];
            float clickAge = clickTimes[i];

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

            Star star = menuStars[starIndex];
            star.rotationSpeed = 0.1f;
            star.rotation = 0.01f;
            star.fallSpeed.Y = (float)Main.rand.Next(100, 201) * 0.001f;
            star.fallSpeed.X = (float)Main.rand.Next(-100, 101) * 0.001f;
        }

        if (Main.rand.NextBool(160))
        {
            CreateNewStar();
        }

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        return true;
    }
}