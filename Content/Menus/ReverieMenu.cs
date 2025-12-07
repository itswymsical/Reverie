using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Reverie.Core.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.Menus
{
    public class ReverieMenu : ModMenu
    {

        private Texture2D backgroundTexture = ModContent.Request<Texture2D>(ICON_DIRECTORY + "SkyGradient", AssetRequestMode.ImmediateLoad).Value;
        private Texture2D moon = ModContent.Request<Texture2D>(ICON_DIRECTORY + "Moon", AssetRequestMode.ImmediateLoad).Value;
        private Texture2D foreground = ModContent.Request<Texture2D>(ICON_DIRECTORY + "LightTrail", AssetRequestMode.ImmediateLoad).Value;
        private Texture2D ocean = ModContent.Request<Texture2D>(ICON_DIRECTORY + "Ocean", AssetRequestMode.ImmediateLoad).Value;

        private List<Star> menuStars;
        private List<CloudObject> menuClouds;

        private bool objsInitialized = false;

        private float cloudDriftSpeed = 0.05f;
        private float starDriftSpeed = 0.03f;
        private Dictionary<Star, Vector2> starVelocities = [];

        private class CloudObject
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Alpha;
            public int Type;

            public CloudObject(Vector2 position, Vector2 velocity, float scale)
            {
                Position = position;
                Velocity = velocity;
                Scale = scale;
                Alpha = 0f;
                Type = Main.rand.Next(0, 22);
            }

            public void Update()
            {
                Position += Velocity;

                if (Alpha < 0.85f)
                    Alpha += 0.01f;
            }
        }

        public override void Load()
        {
            menuStars = [];
            menuClouds = [];
            starVelocities = [];
        }

        public override void Unload()
        {
            menuStars.Clear();
            menuClouds.Clear();
            starVelocities.Clear();
        }

        public override int Music => MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}Meditation");

        public override string DisplayName => "Daydream";

        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>($"{LOGO_DIRECTORY}Logo_Outline");

        public override Asset<Texture2D> SunTexture => null;

        public override Asset<Texture2D> MoonTexture => null;
        public static bool InMenu => MenuLoader.CurrentMenu == ModContent.GetInstance<ReverieMenu>() && Main.gameMenu;

        public override void OnSelected()
        {
            SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ReverieMenu") with { Volume = 0.65f });
        }

        private void InitializeMenuObjects()
        {
            menuStars.Clear();
            menuClouds.Clear();

            var starCount = Main.rand.Next(70, 100);
            for (var i = 0; i < starCount; i++)
            {
                CreateStar();
            }

            var cloudCount = Main.rand.Next(10, 30);
            for (var i = 0; i < cloudCount; i++)
            {
                CreateCloud();
            }

            objsInitialized = true;
        }

        private void CreateStar()
        {
            var star = new Star
            {
                position = new Vector2(
                    Main.rand.Next(0, Main.screenWidth),
                    Main.rand.Next(0, Main.screenHeight)
                ),

                rotation = Main.rand.NextFloat(0, MathHelper.TwoPi),
                scale = Main.rand.NextFloat(0.2f, 0.72f)
            };
            if (Main.rand.NextBool(20))
                star.scale *= 0.3f;

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
                -starDriftSpeed * (0.5f + star.scale),
                0f
            );

            menuStars.Add(star);
        }

        private bool CreateCloud()
        {
            var position = new Vector2(Main.rand.Next(Main.screenWidth), Main.rand.Next(Main.screenHeight));

            if (menuClouds.Count >= 30)
                return false;

            if (Main.rand.NextFloat() > 0.05f && position.X < 0)
                return false;

            var velocity = new Vector2(
                -cloudDriftSpeed * Main.rand.NextFloat(0.9f, 1.3f),
                Main.rand.NextFloat(-0.01f, 0.01f)
            );

            var scale = Main.rand.NextFloat(0.3f, 0.7f);

            menuClouds.Add(new CloudObject(position, velocity, scale));
            return true;
        }

        public override void Update(bool isOnTitleScreen)
        {
            base.Update(isOnTitleScreen);

            if (!InMenu)
            {
                // If we were initialized but now we're not in the menu, clean up resources
                if (objsInitialized)
                {
                    menuStars.Clear();
                    menuClouds.Clear();
                    starVelocities.Clear();
                    objsInitialized = false;
                }
                return;
            }
        }

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            float time = (float)Main.timeForVisualEffects * 0.005f;

            float mediumPan = time * 0.5f;

            mediumPan = mediumPan % 1f;

            drawColor = Color.White;
            logoScale = 1.4f;
            logoRotation = 0f;
            logoDrawCenter = logoDrawCenter + new Vector2(0, 20);

            if (!InMenu || !Main.gameMenu)
            {
                if (objsInitialized)
                {
                    menuStars.Clear();
                    menuClouds.Clear();
                    starVelocities.Clear();
                    objsInitialized = false;
                }
                return true; // Skip all the drawing but allow the logo to be drawn normally
            }

            if (!objsInitialized)
                InitializeMenuObjects();

            var screenRect = new Rectangle(0, 0, Main.screenWidth + 1, Main.screenHeight);
            spriteBatch.Draw(backgroundTexture, screenRect, drawColor);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            MakeStars(spriteBatch);
            MakeClouds(spriteBatch);
            spriteBatch.Draw(Main.Assets.Request<Texture2D>("Images/Background_283").Value, screenRect, drawColor * 0.75f);

            spriteBatch.Draw(foreground, screenRect, drawColor * 0.95f); // make time based opacity  with "breathing" later

            #region Galaxy Logo
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
            #endregion

            return false;
        }

        private void MakeStars(SpriteBatch spriteBatch)
        {
            var glowTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;
            var starTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star").Value;

            var colorStart = new Color(143, 244, 255);
            var colorEnd = Color.White;

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
                        new Vector2(-starDriftSpeed * (0.5f + star.scale), 0f),
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

                    CreateStar();
                }
            }

            //if (Main.rand.NextBool(50))
            //{
            //    var starIndex = Main.rand.Next(menuStars.Count);
            //    menuStars[starIndex].Fall();

            //    if (starVelocities.ContainsKey(menuStars[starIndex]))
            //        starVelocities.Remove(menuStars[starIndex]);

            //    var star = menuStars[starIndex];
            //    star.rotationSpeed = 0.1f;
            //    star.rotation = 0.01f;
            //    star.fallSpeed.Y = Main.rand.Next(100, 201) * 0.001f;
            //    star.fallSpeed.X = Main.rand.Next(-100, 101) * 0.001f;
            //}

            if (Main.rand.NextBool(160))
            {
                CreateStar();
            }
        }

        private void MakeClouds(SpriteBatch spriteBatch)
        {
            for (var i = menuClouds.Count - 1; i >= 0; i--)
            {
                var easterEgg = menuClouds[i];

                easterEgg.Update();

                var texture = TextureAssets.Cloud[easterEgg.Type].Value;


                spriteBatch.Draw(
                    texture,
                    easterEgg.Position,
                    null,
                    Color.White * easterEgg.Alpha,
                    0,
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
                    menuClouds.RemoveAt(i);
                    CreateCloud();

                }

                if (Main.rand.NextBool(160))
                {
                    CreateCloud();
                }
            }
        }
    }
}