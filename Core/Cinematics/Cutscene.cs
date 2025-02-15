using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.UI;
using System.Linq;

namespace Reverie.Core.Cinematics;

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