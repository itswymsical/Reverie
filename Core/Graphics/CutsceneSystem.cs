using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Reverie.Core.Graphics
{
    public abstract class CutsceneSystem
    {
        protected bool IsPlaying { get; set; }
        protected bool IsUIHidden { get; set; }

        public float FadeAlpha { get; set; }
        public Color FadeColor { get; set; }

        protected bool IsPlayerMovementDisabled { get; private set; }
        protected bool IsPlayerVisible { get; private set; }

        private int? _currentMusicID = null;
        private int _previousMusicBox = -1;
        public static bool DisableMoment { get; set; }
        public virtual void Start()
        {
            try
            {
                IsPlaying = true;
                IsPlayerVisible = true;
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

        protected void DisablePlayerMovement()
        {
            DisableMoment = true;
        }
        protected void EnablePlayerMovement()
        {
            DisableMoment = false;
        }

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
        public static CutsceneSystem CurrentCutscene { get; private set; }

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
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

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
                else
                {
                    if (orig == null)
                    {
                        ModContent.GetInstance<Reverie>().Logger.Error("Original DrawInterface method is null");
                        return;
                    }
                    if (self == null)
                    {
                        ModContent.GetInstance<Reverie>().Logger.Error("Main instance is null");
                        return;
                    }
                    if (gameTime == null)
                    {
                        ModContent.GetInstance<Reverie>().Logger.Error("GameTime is null");
                        return;
                    }
                    orig(self, gameTime);
                }
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
        public static void PlayCutscene(CutsceneSystem cutscene)
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