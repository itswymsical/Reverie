using Terraria.GameContent;
using Reverie.Core.Animation;
using Reverie.Common.Systems;
using System.Linq;

namespace Reverie.Core.Cinematics;

public abstract class Cutscene
{
    protected bool IsPlaying { get; set; }
    protected bool IsUIHidden { get; set; }

    public float FadeAlpha { get; set; }
    public Color FadeColor { get; set; } = Color.Black;

    public static bool DisableInputs { get; set; }
    public static bool NoFallDamage { get; set; }
    public static bool IsPlayerVisible { get; set; } = true;

    public float LetterboxHeightPercentage { get; set; } = 0.1f;
    public Color LetterboxColor { get; set; } = Color.Black;
    public EaseFunction LetterboxEasing { get; set; } = EaseFunction.EaseQuadOut;
    public int LetterboxAnimationDuration { get; set; } = 60;

    private int? _currentMusicID = null;
    private int _previousMusicBox = -1;

    protected float ElapsedTime { get; set; }

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

    private Texture2D _skipIcon;
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
            ElapsedTime = 0f;
            _isSkipping = false;
            _skipHoldTime = 0;
            _skipAnimationFrame = 0;
            _skipAnimationTimer = 0;
            _skipFadeOutStarted = false;
            _skipFadeOutTimer = 0f;

            Letterbox.HeightPercentage = LetterboxHeightPercentage;
            Letterbox.LetterboxColor = LetterboxColor;
            Letterbox.EasingFunction = LetterboxEasing;
            Letterbox.AnimationDurationFrames = LetterboxAnimationDuration;

            Letterbox.Show();

            SetMusic(_currentMusicID);

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

            ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            Letterbox.Update();

            if (_currentMusicID.HasValue)
            {
                Main.musicBox2 = _currentMusicID.Value;
            }

            if (CanSkip && !_skipFadeOutStarted)
            {
                if (ReverieSystem.SkipCutsceneKeybind.Current)
                {
                    _skipHoldTime++;
                    _isSkipping = true;
                    // Animate loading icon
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

                    // Check if skip is complete
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

            // Handle skip fade out
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

    /// <summary>
    /// Called when skip is triggered
    /// </summary>
    protected virtual void OnSkipTriggered()
    {

        _skipFadeOutStarted = true;
        FadeColor = Color.Black;
      
    }

    /// <summary>
    /// Called every frame to update cutscene logic
    /// </summary>
    /// <param name="gameTime">Game time information</param>
    protected virtual void OnCutsceneUpdate(GameTime gameTime) { }

    /// <summary>
    /// Sets the background music for the cutscene
    /// </summary>
    /// <param name="musicID">Music ID to play, or null to keep current music</param>
    public void SetMusic(int? musicID)
    {
        if (musicID.HasValue)
        {
            _previousMusicBox = Main.musicBox2;
            _currentMusicID = musicID.Value;
            Main.musicBox2 = musicID.Value;
        }
    }

    /// <summary>
    /// Draws the cutscene elements
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for drawing</param>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (!IsPlaying) return;

        DrawFade(spriteBatch);

        if (UsesLetterbox())
        {
            Letterbox.DrawCinematic(spriteBatch);
        }
        else
        {
            Letterbox.Draw(spriteBatch);
        }

        DrawCutsceneContent(spriteBatch);

        // Draw skip indicator
        if (_isSkipping && !_skipFadeOutStarted)
        {
            DrawSkipIndicator(spriteBatch);
        }
    }

    /// <summary>
    /// Draws the skip indicator
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for drawing</param>
    public virtual void DrawSkipIndicator(SpriteBatch spriteBatch)
    {
        if (_skipIcon == null)
        {
            _skipIcon = TextureAssets.LoadingSunflower?.Value;
            if (_skipIcon == null) return;
        }

        var frameWidth = _skipIcon.Width;
        var frameHeight = _skipIcon.Height / _skipIconTotalFrames;
        Rectangle sourceRectangle = new Rectangle(0, _skipAnimationFrame * frameHeight, frameWidth, frameHeight);
        Vector2 iconPosition = new Vector2(Main.screenWidth - frameWidth - 10, Main.screenHeight - frameHeight - 10);

        spriteBatch.Draw(_skipIcon, iconPosition, sourceRectangle, Color.White);

        string keybindName = ReverieSystem.SkipCutsceneKeybind.GetAssignedKeys().FirstOrDefault() ?? "[None]";
        string skipText = $"Hold [{keybindName}] to skip...";

        var font = FontAssets.MouseText.Value;
        Vector2 textSize = font.MeasureString(skipText);
        Vector2 textPosition = new Vector2(
            iconPosition.X - textSize.X - 10,
            iconPosition.Y + (frameHeight - textSize.Y) / 2
        );

        Utils.DrawBorderString(spriteBatch, skipText, textPosition, Color.White);
    }

    /// <summary>
    /// Whether to use the cinematic letterbox effect
    /// </summary>
    /// <returns>True to use cinematic letterbox, false for standard</returns>
    protected virtual bool UsesLetterbox() => false;

    /// <summary>
    /// Draws the cutscene-specific elements
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for drawing</param>
    protected virtual void DrawCutsceneContent(SpriteBatch spriteBatch) { }

    /// <summary>
    /// Determines if the cutscene has finished playing
    /// </summary>
    /// <returns>True if finished, false otherwise</returns>
    public abstract bool IsFinished();

    /// <summary>
    /// Ends the cutscene and restores normal gameplay
    /// </summary>
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

            OnCutsceneEnd();
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error("Error ending cutscene: " + ex.Message);
        }
    }

    /// <summary>
    /// Called when the cutscene ends, for cleanup
    /// </summary>
    protected virtual void OnCutsceneEnd() { }

    protected static void DisableFallDamage() => NoFallDamage = true;
    protected static void EnableFallDamage() => NoFallDamage = false;

    protected static void DisableInvisibility() => IsPlayerVisible = true;
    protected static void EnableInvisibility() => IsPlayerVisible = false;

    protected static void DisablePlayerMovement() => DisableInputs = true;
    protected static void EnablePlayerMovement() => DisableInputs = false;

    /// <summary>
    /// Draws a full-screen fade effect
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch for drawing</param>
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

    /// <summary>
    /// Creates a timed fade effect
    /// </summary>
    /// <param name="targetAlpha">Target alpha value (0.0-1.0)</param>
    /// <param name="duration">Duration in seconds</param>
    /// <param name="delay">Delay before starting in seconds</param>
    /// <param name="color">Fade color (defaults to black)</param>
    /// <param name="easing">Easing function to use</param>
    /// <returns>True when complete, false while in progress</returns>
    protected bool TimedFade(float targetAlpha, float duration, float delay = 0f, Color? color = null, EaseFunction easing = null)
    {
        if (color.HasValue)
        {
            FadeColor = color.Value;
        }

        if (ElapsedTime < delay)
        {
            return false;
        }

        float elapsedSinceDelay = ElapsedTime - delay;
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

    /// <summary>
    /// Fades in from black (or specified color)
    /// </summary>
    /// <param name="duration">Duration in seconds</param>
    /// <param name="delay">Delay before starting in seconds</param>
    /// <param name="color">Fade color (defaults to black)</param>
    /// <param name="easing">Easing function to use</param>
    /// <returns>True when complete, false while in progress</returns>
    protected bool FadeIn(float duration, float delay = 0f, Color? color = null, EaseFunction easing = null)
    {
        // Start from full opacity if just starting
        if (ElapsedTime <= delay && FadeAlpha == 0f)
        {
            FadeAlpha = 1f;
        }

        return TimedFade(0f, duration, delay, color, easing);
    }

    /// <summary>
    /// Fades out to black (or specified color)
    /// </summary>
    /// <param name="duration">Duration in seconds</param>
    /// <param name="delay">Delay before starting in seconds</param>
    /// <param name="color">Fade color (defaults to black)</param>
    /// <param name="easing">Easing function to use</param>
    /// <returns>True when complete, false while in progress</returns>
    protected bool FadeOut(float duration, float delay = 0f, Color? color = null, EaseFunction easing = null)
    {
        return TimedFade(1f, duration, delay, color, easing);
    }

    /// <summary>
    /// Gets the safe drawing area between letterboxes
    /// </summary>
    /// <returns>Rectangle representing the visible area</returns>
    protected Rectangle GetSafeDrawingArea()
    {
        return Letterbox.GetSafeArea();
    }
}