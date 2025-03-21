using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Terraria.Audio;

using Reverie.Core.Dialogue;
using Reverie.Common.Systems;
using Reverie.Content.Projectiles;

namespace Reverie.Core.Cinematics.Cutscenes;

public class FallingStarCutscene : Cutscene
{
    #region Types
    private enum CutscenePhase
    {
        OpeningCredits,
        FadeIn,
        GuideScene,
        PlayerDescent,
        Impact,
        Finish
    }

    private readonly struct CreditEntry
    {
        public string Label { get; init; }
        public string Value { get; init; }
        public float ScreenWidthPercent { get; init; }
    }

    private readonly struct PhaseData
    {
        public float Duration { get; init; }
        public Action<float> Handler { get; init; }
        public Action OnComplete { get; init; }
    }
    #endregion

    #region Constants
    private static class Timings
    {
        public const float FADE_DURATION = 8f;
        public const float OPENING_CREDITS_DURATION = 24f;
        public const float GUIDE_SCENE_DURATION = 7f;
        public const float DESCENT_DURATION = 3f;
        public const float IMPACT_HOLD_DURATION = 5f;

        public static class Credits
        {
            public const float LOGO_FADE_IN_DURATION = 2f;
            public const float LOGO_HOLD_DURATION = 4f;
            public const float LOGO_FADE_OUT_DURATION = 2f;
            public const float TEXT_FADE_IN_DURATION = 2f;
            public const float TEXT_HOLD_DURATION = 12f;
            public const float TEXT_FADE_OUT_DURATION = 2f;
            public const float TEXT_START_DELAY = 8f;
        }
    }

    private static class Positions
    {
        public static readonly Vector2 GUIDE_OFFSET = new(-185, 0);
        public static readonly Vector2 PLAYER_START_OFFSET = new(-150, -Main.screenHeight * 1.3f);
        public static readonly Vector2 LOGO_POSITION = new(Main.screenWidth / 2f, Main.screenHeight * 0.1f);
        public static readonly Vector2 LEFT_CREDITS_POSITION = new(Main.screenWidth * 0.25f, Main.screenHeight * 0.4f);
        public static readonly Vector2 RIGHT_CREDITS_POSITION = new(Main.screenWidth * 0.75f, Main.screenHeight * 0.4f);
    }

    private static readonly CreditEntry[] LeftColumnCredits =
    {
        new() { Label = "Written By:", Value = "wymsical, ElectroManiac", ScreenWidthPercent = 0.25f },
        new() { Label = "Composer:", Value = "wymsical", ScreenWidthPercent = 0.25f },
        new() { Label = "Lead Artist:", Value = "ElectroManiac", ScreenWidthPercent = 0.25f },
        new() { Label = "Programmers:", Value = "wymsical, naka", ScreenWidthPercent = 0.25f }
    };

    private static readonly CreditEntry[] RightColumnCredits =
    {
        new() { Label = "Artists:", Value = ".sweetberries, Crystal_zone,", ScreenWidthPercent = 0.75f },
        new() { Label = "", Value = "Dominick, RAWTHORN", ScreenWidthPercent = 0.75f },
        new() { Label = "Special Thanks:", Value = "HugeKraken, naka,", ScreenWidthPercent = 0.75f },
        new() { Label = "", Value = "grae", ScreenWidthPercent = 0.75f }
    };
    #endregion

    #region Fields
    private readonly Dictionary<CutscenePhase, PhaseData> phaseHandlers;
    private float _elapsedTime;
    private CutscenePhase _currentPhase;
    private NPC _guide;
    private Vector2 _playerStartPosition;
    private Vector2 _playerTargetPosition;
    private bool _dialoguePlayed;
    private bool _impactTriggered;
    private bool _creditsStarted;
    private float _logoAlpha;
    private float _textAlpha;
    #endregion

    #region Initialization
    public FallingStarCutscene()
    {
        phaseHandlers = new Dictionary<CutscenePhase, PhaseData>
        {
            [CutscenePhase.OpeningCredits] = new PhaseData
            {
                Duration = Timings.OPENING_CREDITS_DURATION,
                Handler = HandleOpeningCredits,
                OnComplete = () => TransitionToPhase(CutscenePhase.GuideScene)
            },
            [CutscenePhase.GuideScene] = new PhaseData
            {
                Duration = Timings.GUIDE_SCENE_DURATION,
                Handler = HandleGuideScene,
                OnComplete = () => TransitionToPhase(CutscenePhase.PlayerDescent)
            },
            [CutscenePhase.PlayerDescent] = new PhaseData
            {
                Duration = Timings.DESCENT_DURATION,
                Handler = HandlePlayerDescent,
                OnComplete = () => TransitionToPhase(CutscenePhase.Impact)
            },
            [CutscenePhase.Impact] = new PhaseData
            {
                Duration = Timings.IMPACT_HOLD_DURATION,
                Handler = HandleImpact,
                OnComplete = () => TransitionToPhase(CutscenePhase.Finish)
            }
        };
    }

    public override void Start()
    {
        try
        {
            base.Start();
            InitializeState();
            SetupScene();
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error starting FallingStarCutscene: {ex}");
            End();
        }
    }

    private void InitializeState()
    {
        _elapsedTime = 0f;
        _currentPhase = CutscenePhase.OpeningCredits;
        _dialoguePlayed = false;
        _impactTriggered = false;
        _creditsStarted = false;
        _logoAlpha = 0f;
        _textAlpha = 0f;
    }

    private void SetupScene()
    {
        FadeColor = Color.Black;
        FadeAlpha = 1f;
        DisableFallDamage();
        EnableInvisibility();
        SetMusic(MusicLoader.GetMusicSlot(Instance, "Assets/Music/DawnofReverie"));
        InitializeGuide();
        SetupPositions();
    }

    private void InitializeGuide()
    {
        _guide = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
        if (_guide == null)
        {
            var guideIndex = NPC.NewNPC(default, Main.spawnTileX * 16, Main.spawnTileY * 16, NPCID.Guide);
            _guide = Main.npc[guideIndex];
        }
    }

    private void SetupPositions()
    {
        _playerStartPosition = _guide.position + Positions.PLAYER_START_OFFSET;
        _playerTargetPosition = _guide.position + Positions.GUIDE_OFFSET;
    }
    #endregion

    #region Phase Handlers
    private void HandleOpeningCredits(float phaseTime)
    {
        FadeAlpha = 1f - phaseTime / Timings.FADE_DURATION;

        if (phaseTime <= Timings.OPENING_CREDITS_DURATION / 1.4f)
            Main.SceneMetrics.ShimmerMonolithState = 1;
        if (phaseTime >= Timings.OPENING_CREDITS_DURATION / 1.4f)
            FadeAlpha = 0f + phaseTime / 12f;

        if (!_creditsStarted)
        {
            StartCreditsCamera();
            _creditsStarted = true;
        }
    }

    private void HandleGuideScene(float phaseTime)
    {
        FadeAlpha = 1f - phaseTime / Timings.FADE_DURATION;

        if (phaseTime >= Timings.GUIDE_SCENE_DURATION * 0.5f && !_dialoguePlayed)
        {
            StartGuideDialogue();
            _dialoguePlayed = true;
        }

        if (phaseTime >= Timings.GUIDE_SCENE_DURATION)
        {
            PreparePlayerDescent();
        }
    }

    private void HandlePlayerDescent(float phaseTime)
    {
        var descentProgress = phaseTime / Timings.DESCENT_DURATION;
        HandleDescentMovement(descentProgress);

        if (Main.LocalPlayer.TouchedTiles.Any())
        {
            HandleLandingImpact();
        }
    }

    private void HandleImpact(float phaseTime)
    {
        var fadeProgress = phaseTime / Timings.IMPACT_HOLD_DURATION;
        FadeAlpha -= fadeProgress / 16;
    }
    #endregion

    #region Helper Methods
    private static async void PlaySoundWithDelay()
    {
        try
        {
            for (var i = 0; i < 10; i++)
            {
                SoundEngine.PlaySound(SoundID.Item9);
                await Task.Delay(500);
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in PlaySoundWithDelay: {ex}");
        }
    }

    private void UpdateCreditsAlpha()
    {
        if (_currentPhase != CutscenePhase.OpeningCredits) return;

        UpdateLogoAlpha();
        UpdateTextAlpha();
    }

    private void UpdateLogoAlpha()
    {
        if (_elapsedTime <= Timings.Credits.LOGO_FADE_IN_DURATION)
        {
            _logoAlpha = _elapsedTime / Timings.Credits.LOGO_FADE_IN_DURATION;
        }
        else if (_elapsedTime <= Timings.Credits.LOGO_FADE_IN_DURATION + Timings.Credits.LOGO_HOLD_DURATION)
        {
            _logoAlpha = 1f;
        }
        else if (_elapsedTime <= Timings.Credits.LOGO_FADE_IN_DURATION + Timings.Credits.LOGO_HOLD_DURATION + Timings.Credits.LOGO_FADE_OUT_DURATION)
        {
            _logoAlpha = 1f - (_elapsedTime - (Timings.Credits.LOGO_FADE_IN_DURATION + Timings.Credits.LOGO_HOLD_DURATION)) / Timings.Credits.LOGO_FADE_OUT_DURATION;
        }
        else
        {
            _logoAlpha = 0f;
        }
    }

    private void UpdateTextAlpha()
    {
        float textTime = _elapsedTime - Timings.Credits.TEXT_START_DELAY;
        if (textTime <= 0)
        {
            _textAlpha = 0f;
        }
        else if (textTime <= Timings.Credits.TEXT_FADE_IN_DURATION)
        {
            _textAlpha = textTime / Timings.Credits.TEXT_FADE_IN_DURATION;
        }
        else if (textTime <= Timings.Credits.TEXT_FADE_IN_DURATION + Timings.Credits.TEXT_HOLD_DURATION)
        {
            _textAlpha = 1f;
        }
        else if (textTime <= Timings.Credits.TEXT_FADE_IN_DURATION + Timings.Credits.TEXT_HOLD_DURATION + Timings.Credits.TEXT_FADE_OUT_DURATION)
        {
            _textAlpha = 1f - (textTime - (Timings.Credits.TEXT_FADE_IN_DURATION + Timings.Credits.TEXT_HOLD_DURATION)) / Timings.Credits.TEXT_FADE_OUT_DURATION;
        }
        else
        {
            _textAlpha = 0f;
        }
    }

    private void DrawCreditsLine(SpriteBatch spriteBatch, CreditEntry entry, int index)
    {
        float yOffset = index * 60f;
        Vector2 basePosition = new(Main.screenWidth * entry.ScreenWidthPercent, Main.screenHeight * 0.4f);
        Vector2 labelPos = basePosition + new Vector2(0, yOffset);
        Vector2 valuePos = labelPos + new Vector2(0, 25f);

        Utils.DrawBorderString(spriteBatch, entry.Label, labelPos, Color.Gold * _textAlpha, 1.2f);
        Utils.DrawBorderString(spriteBatch, entry.Value, valuePos, Color.White * _textAlpha, 0.9f);
    }

    private void StartCreditsCamera()
    {
        Vector2 startPosition = _guide.Center - new Vector2(0, Main.screenHeight * 2.2f);
        CameraSystem.DoPanAnimationOffset(
            (int)(Timings.OPENING_CREDITS_DURATION * 60),
            startPosition,
            _guide.Center);
    }

    private void StartGuideDialogue()
    {
        EnableInvisibility();
        PlaySoundWithDelay();
        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_Cutscene, true);
    }

    private void PreparePlayerDescent()
    {
        DisableInvisibility();
        Main.LocalPlayer.Center = _playerStartPosition;
    }

    private void HandleDescentMovement(float progress)
    {
        DisableFallDamage();
        Main.LocalPlayer.position = Vector2.Lerp(_playerStartPosition, _playerTargetPosition, progress * 1.2f);
        Main.LocalPlayer.fullRotation += 1.4f * progress * 1.12f;
        FadeAlpha = progress / 16;
        CameraSystem.MoveCameraOut(0, Main.LocalPlayer.Center);
    }

    private void HandleLandingImpact()
    {
        if (!_impactTriggered)
        {
            TriggerImpactEffects();
            _impactTriggered = true;
        }
        TransitionToPhase(CutscenePhase.Impact);
        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_Intro, true);
    }

    private void TriggerImpactEffects()
    {
        Projectile.NewProjectile(null, Main.LocalPlayer.Center, Vector2.Zero,
            ModContent.ProjectileType<ExplosiveLanding>(), 0, 0f, Main.myPlayer);
        FadeColor = Color.White;
        FadeAlpha = 1f;
        CameraSystem.shake = 30;
    }

    private void TransitionToPhase(CutscenePhase newPhase)
    {
        Instance.Logger.Debug($"Transitioning from {_currentPhase} to {newPhase}");
        _currentPhase = newPhase;
        _elapsedTime = 0f;

        // Execute any specific setup for the new phase
        switch (newPhase)
        {
            case CutscenePhase.GuideScene:
                CameraSystem.Reset();
                CameraSystem.DoPanAnimationOffset(
                    (int)(Timings.GUIDE_SCENE_DURATION * 60),
                    _guide.Center - new Vector2(0, -200),
                    _guide.Center);
                break;
        }
    }
    #endregion

    #region Core Functionality
    public override void Update(GameTime gameTime)
    {
        try
        {
            base.Update(gameTime);

            if (!IsPlaying) return;

            DisablePlayerMovement();
            UpdateCutscene(gameTime);
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in Update: {ex}");
            End();
        }
    }

    private void UpdateCutscene(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _elapsedTime += deltaTime;

        UpdateCreditsAlpha();

        if (phaseHandlers.TryGetValue(_currentPhase, out var phaseData))
        {
            phaseData.Handler(_elapsedTime);

            if (_elapsedTime >= phaseData.Duration)
            {
                phaseData.OnComplete?.Invoke();
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        try
        {
            base.Draw(spriteBatch);

            if (_currentPhase != CutscenePhase.OpeningCredits) return;

            DrawLogo(spriteBatch);
            DrawCredits(spriteBatch);
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in Draw: {ex}");
        }
    }

    private void DrawLogo(SpriteBatch spriteBatch)
    {
        if (_logoAlpha <= 0f) return;

        Texture2D logoTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/Logo").Value;
        Vector2 logoOrigin = new Vector2(logoTexture.Width / 2f, logoTexture.Height / 2f);

        spriteBatch.Draw(
            logoTexture,
            Positions.LOGO_POSITION,
            null,
            Color.White * _logoAlpha,
            0f,
            logoOrigin,
            1f,
            SpriteEffects.None,
            0f);
    }

    private void DrawCredits(SpriteBatch spriteBatch)
    {
        if (_textAlpha <= 0f) return;

        for (int i = 0; i < LeftColumnCredits.Length; i++)
        {
            DrawCreditsLine(spriteBatch, LeftColumnCredits[i], i);
        }

        for (int i = 0; i < RightColumnCredits.Length; i++)
        {
            DrawCreditsLine(spriteBatch, RightColumnCredits[i], i);
        }
    }
    #endregion

    #region State Management
    public override bool IsFinished() => _currentPhase == CutscenePhase.Finish;

    public override void End()
    {
        try
        {
            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SettlingIn);
            EnableFallDamage();
            EnablePlayerMovement();
            CameraSystem.Reset();
            base.End();
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error ending cutscene: {ex}");
        }
    }
    #endregion
}