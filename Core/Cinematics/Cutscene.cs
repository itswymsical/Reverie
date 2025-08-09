using Terraria.GameContent;
using Reverie.Common.Systems;
using System.Linq;
using Reverie.Core.Cinematics.Music;

namespace Reverie.Core.Cinematics;

public abstract class Cutscene
{
    public bool IsPlaying { get; set; }
    protected bool IsUIHidden { get; set; }

    public float FadeAlpha { get; set; }
    public Color FadeColor { get; set; } = Color.Black;

    public static bool DisableInputs { get; set; }
    public static bool NoFallDamage { get; set; }
    public static bool IsPlayerVisible { get; set; } = true;

    public bool EnableLetterbox { get; set; } = false;

    public float LetterboxHeightPercentage { get; set; } = 0.07f;
    public Color LetterboxColor { get; set; } = Color.Black;
    public EaseFunction LetterboxEasing { get; set; } = EaseFunction.EaseQuadOut;
    public int LetterboxAnimationDuration { get; set; } = 60;

    private int? _currentMusicID = null;

    protected float ElapsedSeconds { get; set; }

    private bool _isSkipping = false;
    private int _skipHoldTime = 0;
    private int _skipAnimationFrame = 0;
    private int _skipAnimationTimer = 0;
    private bool _skipFadeOutStarted = false;
    private float _skipFadeOutDuration = 1f;
    private float _skipFadeOutTimer = 0f;

    protected bool CanSkip { get; set; } = true;
    protected int SkipHoldDuration { get; set; } = 120;
    protected int SkipAnimationFrameRate { get; set; } = 5;

    private Texture2D _skipIcon = TextureAssets.LoadingSunflower?.Value;
    private int _skipIconTotalFrames = 19;

    /// <summary>
    /// Starts the cutscene with default parameters
    /// </summary>
    public virtual void Start()
    {
        try
        {
            IsPlaying = true;
            IsUIHidden = false;
            ElapsedSeconds = 0f;
            _isSkipping = false;
            _skipHoldTime = 0;
            _skipAnimationFrame = 0;
            _skipAnimationTimer = 0;
            _skipFadeOutStarted = false;
            _skipFadeOutTimer = 0f;

            if (EnableLetterbox)
            {
                Letterbox.HeightPercentage = LetterboxHeightPercentage;
                Letterbox.LetterboxColor = LetterboxColor;
                Letterbox.EasingFunction = LetterboxEasing;
                Letterbox.AnimationDurationFrames = LetterboxAnimationDuration;

                Letterbox.Show();
            }

            OnCutsceneStart();
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error("Error starting cutscene: " + ex.Message);
        }
    }

    /// <summary>
    /// Starts the cutscene with custom letterbox configuration
    /// </summary>
    /// <param name="letterboxHeight">Height percentage (0.0-0.5)</param>
    /// <param name="color">Color of the letterbox</param>
    /// <param name="easing">Easing function for the animation</param>
    /// <param name="duration">Animation duration in frames</param>
    public virtual void Start(float letterboxHeight, Color? color = null, EaseFunction easing = null, int? duration = null)
    {
        EnableLetterbox = true;
        LetterboxHeightPercentage = MathHelper.Clamp(letterboxHeight, 0.01f, 0.5f);

        if (color.HasValue)
            LetterboxColor = color.Value;

        if (easing != null)
            LetterboxEasing = easing;

        if (duration.HasValue)
            LetterboxAnimationDuration = duration.Value;

        Start();
    }

    protected virtual void OnCutsceneStart() { }

    public virtual void Update(GameTime gameTime)
    {
        try
        {
            if (!IsPlaying) return;

            ElapsedSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (EnableLetterbox)
            {
                Letterbox.Update();
            }

            // Update music fade handler
            MusicFadeHandler.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (CanSkip && !_skipFadeOutStarted)
            {
                if (ReverieSystem.SkipCutsceneKeybind.Current)
                {
                    _skipHoldTime++;
                    _isSkipping = true;
                    _skipAnimationTimer++;
                    if (_skipAnimationTimer >= SkipAnimationFrameRate)
                    {
                        _skipAnimationTimer = 0;
                        _skipAnimationFrame++;
                        if (_skipAnimationFrame >= _skipIconTotalFrames)
                        {
                            _skipAnimationFrame = 0;
                        }
                    }

                    if (_skipHoldTime >= SkipHoldDuration)
                    {
                        OnSkipTriggered();
                    }
                }
                else
                {
                    _isSkipping = false;
                    _skipHoldTime = 0;
                    _skipAnimationFrame = 0;
                    _skipAnimationTimer = 0;
                }
            }

            if (_skipFadeOutStarted)
            {
                _skipFadeOutTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float fadeProgress = _skipFadeOutTimer / _skipFadeOutDuration;
                FadeAlpha = Math.Min(fadeProgress, 1f);

                if (fadeProgress >= 1f)
                {
                    End();
                    return;
                }
            }

            OnCutsceneUpdate(gameTime);

            if (!_skipFadeOutStarted && IsFinished())
            {
                End();
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error("Error updating cutscene: " + ex.Message);
        }
    }

    protected virtual void OnSkipTriggered()
    {
        _skipFadeOutStarted = true;
        FadeColor = Color.Black;
    }

    protected virtual void OnCutsceneUpdate(GameTime gameTime) { }

    /// <summary>
    /// Sets the background music for the cutscene
    /// </summary>
    /// <param name="musicID">Music ID to play, or null to keep current music</param>
    /// <param name="fadeMode">How the music should transition</param>
    /// <param name="fadeTime">Duration of fade in seconds (for FadeIn/FadeOut modes)</param>
    public void SetMusic(int? musicID, MusicFadeMode fadeMode = MusicFadeMode.CrossFade, float fadeTime = 1f)
    {
        if (musicID.HasValue)
        {
            _currentMusicID = musicID.Value;
            MusicFadeHandler.SetMusic(musicID.Value, fadeMode, fadeTime);
        }
    }

    /// <summary>
    /// Draws the cutscene content, use DrawCutsceneContent(SpriteBatch) if your not changing the default drawing behavior.
    /// </summary>
    /// <param name="spriteBatch"></param>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (!IsPlaying) return;

        DrawFade(spriteBatch);

        if (UsesLetterbox())
        {
            Letterbox.Draw(spriteBatch);
        }

        DrawCutsceneContent(spriteBatch);

        if ((ElapsedSeconds >= 5f || _isSkipping) && !_skipFadeOutStarted)
        {
            DrawSkipIndicator(spriteBatch);
        }
    }

    public virtual void DrawSkipIndicator(SpriteBatch spriteBatch)
    {
        // Only show the animated icon when actually skipping
        if (_isSkipping && _skipIcon != null)
        {
            var frameWidth = _skipIcon.Width;
            var frameHeight = _skipIcon.Height / _skipIconTotalFrames;
            Rectangle sourceRectangle = new Rectangle(0, _skipAnimationFrame * frameHeight, frameWidth, frameHeight);
            Vector2 iconPosition = new Vector2(Main.screenWidth - frameWidth - 10, Main.screenHeight - frameHeight - 10);

            spriteBatch.Draw(_skipIcon, iconPosition, sourceRectangle, Color.White);
        }

        string keybindName = ReverieSystem.SkipCutsceneKeybind.GetAssignedKeys().FirstOrDefault() ?? "[None]";

        string skipText = _isSkipping
            ? $"Hold [{keybindName}] to skip..."
            : $"Hold [{keybindName}] to skip";

        var font = FontAssets.MouseText.Value;
        Vector2 textSize = font.MeasureString(skipText);

        Vector2 textPosition;
        if (_isSkipping && _skipIcon != null)
        {
            var frameWidth = _skipIcon.Width;
            var frameHeight = _skipIcon.Height / _skipIconTotalFrames;
            Vector2 iconPosition = new Vector2(Main.screenWidth - frameWidth - 10, Main.screenHeight - frameHeight - 10);
            textPosition = new Vector2(
                iconPosition.X - textSize.X - 10,
                iconPosition.Y + (frameHeight - textSize.Y) / 2
            );
            Rectangle sourceRectangle = new Rectangle(0, _skipAnimationFrame * frameHeight, frameWidth, frameHeight);

            spriteBatch.Draw(_skipIcon, iconPosition, sourceRectangle, Color.White);
        }
        else
        {
            textPosition = new Vector2(
                Main.screenWidth - textSize.X - 10,
                Main.screenHeight - textSize.Y - 10
            );
        }

        Utils.DrawBorderString(spriteBatch, skipText, textPosition, Color.White);

    }
    protected virtual bool UsesLetterbox() => EnableLetterbox;

    protected virtual void DrawCutsceneContent(SpriteBatch spriteBatch) { }

    public abstract bool IsFinished();

    public virtual void End()
    {
        try
        {
            ControlsON();
            IsPlaying = false;

            if (EnableLetterbox)
            {
                Letterbox.Hide();
            }

            if (_currentMusicID.HasValue)
            {
                MusicFadeHandler.RestorePreviousMusic();
                _currentMusicID = null;
            }

            OnCutsceneEnd();
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error("Error ending cutscene: " + ex.Message);
        }
    }

    protected virtual void OnCutsceneEnd() { }

    protected static void FallDamageOFF() => NoFallDamage = true;
    protected static void FallDamageON() => NoFallDamage = false;

    protected static void InvisOFF() => IsPlayerVisible = true;
    protected static void InvisON() => IsPlayerVisible = false;

    protected static void ControlsOFF() => DisableInputs = true;
    protected static void ControlsON() => DisableInputs = false;

    protected void DrawFade(SpriteBatch spriteBatch)
    {
        if (FadeAlpha <= 0f) return;

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

    protected bool TimedFade(float targetAlpha, float duration, float delay = 0f, Color? color = null, EaseFunction easing = null)
    {
        if (color.HasValue)
        {
            FadeColor = color.Value;
        }

        if (ElapsedSeconds < delay)
        {
            return false;
        }

        float elapsedSinceDelay = ElapsedSeconds - delay;
        float progress = Math.Min(elapsedSinceDelay / duration, 1f);

        if (easing != null)
        {
            progress = easing.Ease(progress);
        }
        else
        {
            progress = EaseFunction.Linear.Ease(progress);
        }

        FadeAlpha = MathHelper.Lerp(FadeAlpha, targetAlpha, progress);

        return progress >= 1f;
    }

    protected bool FadeIn(float duration, float delay = 0f, Color? color = null, EaseFunction easing = null)
    {
        if (ElapsedSeconds <= delay && FadeAlpha == 0f)
        {
            FadeAlpha = 1f;
        }

        return TimedFade(0f, duration, delay, color, easing);
    }

    protected bool FadeOut(float duration, float delay = 0f, Color? color = null, EaseFunction easing = null)
    {
        return TimedFade(1f, duration, delay, color, easing);
    }

    protected Rectangle GetSafeDrawingArea()
    {
        return Letterbox.GetSafeArea();
    }
}