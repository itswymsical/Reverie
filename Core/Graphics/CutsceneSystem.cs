using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.UI;
using System.Linq;

namespace Reverie.Core.Graphics
{
    public abstract class Cutscene
    {
        protected bool IsPlaying { get; set; }
        protected bool IsUIHidden { get; set; }

        public float FadeAlpha { get; set; }
        public Color FadeColor { get; set; }

        public static bool DisableMoment { get; set; }
        public static bool NoFallDamage { get; set; }
        public static bool IsPlayerVisible { get; set; } = true;


        private int? _currentMusicID = null;
        private int _previousMusicBox = -1;

        public virtual void Start()
        {
            try
            {
                IsPlaying = true;
                IsUIHidden = false;
                Letterbox.Show();
                SetMusic(_currentMusicID);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error("Error starting cutscene: " + ex.Message);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            try
            {
                if (!IsPlaying) return;
                Letterbox.Update();
                if (_currentMusicID.HasValue)
                {
                    Main.musicBox2 = _currentMusicID.Value;
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error("Error updating cutscene: " + ex.Message);
            }
        }
        public void SetMusic(int? musicID)
        {
            if (musicID.HasValue)
            {
                _previousMusicBox = Main.musicBox2;
                _currentMusicID = musicID.Value;
                Main.musicBox2 = musicID.Value;
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!IsPlaying) return;

            DrawFade(spriteBatch);
            Letterbox.Draw(spriteBatch);
            DrawCutsceneContent(spriteBatch);
        }

        /// <summary>
        /// Draws the cutscene elements. Called every frame while the cutscene is active.
        /// </summary>
        protected virtual void DrawCutsceneContent(SpriteBatch spriteBatch) { }

        public abstract bool IsFinished();

        public virtual void End()
        {
            try
            {
                EnablePlayerMovement();
                IsPlaying = false;
                Letterbox.Hide();
                if (_currentMusicID.HasValue)
                {
                    Main.musicBox2 = _previousMusicBox;
                    _currentMusicID = null;
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error("Error ending cutscene: " + ex.Message);
            }
        }

        protected static void DisableFallDamage() => NoFallDamage = true;
        protected static void EnableFallDamage() => NoFallDamage = false;

        protected static void DisableInvisibility() => IsPlayerVisible = true;
        protected static void EnableInvisibility() => IsPlayerVisible = false;

        protected static void DisablePlayerMovement() => DisableMoment = true; 
        protected static void EnablePlayerMovement() => DisableMoment = false;
        

        protected void DrawFade(SpriteBatch spriteBatch)
        {
            if (FadeAlpha > 0f)
            {
                spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    null,
                    FadeColor * FadeAlpha,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
    public class CutsceneLoader : ModSystem
    {
        public static Cutscene CurrentCutscene { get; private set; }

        private static readonly string[] UILayersToHide =
        [
            "Vanilla: Inventory",
            "Vanilla: Hotbar",
            "Vanilla: Resource Bars",
            "Vanilla: Map / Minimap",
            "Vanilla: Info Accessories Bar",
            "Vanilla: Builder Accessories Bar",
            "Vanilla: Settings Button",
            "Vanilla: Mouse Over",
            "Vanilla: Radial Hotbars",
            "Vanilla: Player Chat"
        ];

        public override void Load()
        {
            On_Main.DrawInterface += DrawCutscene;
            On_Main.DoDraw += UpdateCutscene;
        }

        public override void Unload()
        {
            On_Main.DrawInterface -= DrawCutscene;
            On_Main.DoDraw -= UpdateCutscene;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (CurrentCutscene != null)
            {
                foreach (var layer in layers)
                {
                    if (UILayersToHide.Contains(layer.Name))
                    {
                        layer.Active = false;
                    }
                    else if (layer.Name == "Vanilla: NPC / Sign Dialog" ||
                             layer.Name == "Vanilla: Achievement Complete Popups")
                    {
                        layer.Active = true;
                    }
                }
            }
        }

        private void UpdateCutscene(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);

            if (CurrentCutscene != null)
            {
                CurrentCutscene.Update(gameTime);
                if (CurrentCutscene.IsFinished())
                {
                    CurrentCutscene.End();
                    CurrentCutscene = null;
                }
            }
        }

        private void DrawCutscene(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            try
            {
                if (CurrentCutscene != null)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.LinearClamp,
                        DepthStencilState.None,
                        Main.Rasterizer,
                        null,
                        Main.UIScaleMatrix);

                    if (CurrentCutscene is IDrawCutscene customDrawCutscene)
                    {
                        customDrawCutscene.CustomDraw(Main.spriteBatch);
                    }
                    else
                    {
                        CurrentCutscene.Draw(Main.spriteBatch);
                    }

                    Main.spriteBatch.End();
                }

                orig(self, gameTime);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in DrawCutscene: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Starts playing a new cutscene.
        /// </summary>
        /// <param name="cutscene">The cutscene to play.</param>
        public static void PlayCutscene(Cutscene cutscene)
        {
            try
            {
                if (CurrentCutscene != null)
                {
                    ModContent.GetInstance<Reverie>().Logger.Warn("Attempting to start a new cutscene while one is already in progress. Ending the current cutscene.");
                    CurrentCutscene.End();
                }

                CurrentCutscene = cutscene;
                CurrentCutscene.Start();
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error("Error playing cutscene: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Interface for cutscenes that need custom drawing logic.
    /// </summary>
    public interface IDrawCutscene
    {
        void CustomDraw(SpriteBatch spriteBatch);
    }
}