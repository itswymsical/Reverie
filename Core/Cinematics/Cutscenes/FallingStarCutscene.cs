using System.Linq;
using System.Threading.Tasks;

using Terraria.Audio;
using Terraria.GameContent;

using Reverie.Core.Animation;
using Reverie.Core.Dialogue;

using Reverie.Common.Systems.Camera;

namespace Reverie.Core.Cinematics.Cutscenes
{
    public class FallingStarCutscene : Cutscene
    {
        private enum Phase { OpeningCredits, GuideScene, PlayerDescent, Impact, Finish }

        private struct CreditEntry { public string Label; public string Value; public float X; }

        private static class Timing
        {
            public const float FADE = 8f;
            public const float CREDITS = 24f;
            public const float GUIDE = 7f;
            public const float DESCENT = 3f;
            public const float IMPACT = 5f;

            public const float LOGO_IN = 4f;
            public const float LOGO_HOLD = 2f;
            public const float LOGO_OUT = 2f;
            public const float TEXT_IN = 2f;
            public const float TEXT_HOLD = 12f;
            public const float TEXT_OUT = 2f;
            public const float TEXT_DELAY = 8f;
        }

        private Phase _phase = Phase.OpeningCredits;
        private NPC _guide;
        private Vector2 _playerStartPos;
        private Vector2 _playerTargetPos;
        private float _logoAlpha;
        private float _textAlpha;
        private bool _dialoguePlayed;
        private bool _impactTriggered;
        private bool _creditsStarted;

        private readonly CreditEntry[] _leftCredits = new[] {
            new CreditEntry { Label = "Written By:", Value = "wymsical, ElectroManiac", X = 0.125f },
            new CreditEntry { Label = "Composers:", Value = "wymsical", X = 0.125f },
            new CreditEntry { Label = "Lead Artist:", Value = "ElectroManiac", X = 0.125f },
            new CreditEntry { Label = "Programmers:", Value = "wymsical, naka", X = 0.125f }
        };

        private readonly CreditEntry[] _rightCredits = new[] {
            new CreditEntry { Label = "Artists:", Value = ".sweetberries, Crystal_zone,", X = 0.65f },
            new CreditEntry { Label = "", Value = "Dominick, RAWTHORN", X = 0.65f },
            new CreditEntry { Label = "Special Thanks:", Value = "HugeKraken, naka,", X = 0.65f },
            new CreditEntry { Label = "", Value = "grae", X = 0.65f }
        };

        private readonly Vector2 _guideOffset = new(-185, 0);
        private readonly Vector2 _playerOffset = new(-150, -Main.screenHeight * 1.3f);
        private readonly Vector2 _logoPos = new(Main.screenWidth / 2.15f, Main.screenHeight * 0.1f);

        public FallingStarCutscene()
        {
            LetterboxHeightPercentage = 0.12f;
            LetterboxColor = Color.Black;
            LetterboxEasing = EaseFunction.EaseQuadOut;
        }

        public override void Start()
        {
            base.Start();

            FadeColor = Color.Black;
            FadeAlpha = 1f;
            DisableFallDamage();
            EnableInvisibility();
            SetMusic(MusicLoader.GetMusicSlot(ModContent.GetInstance<Reverie>(), "Assets/Music/DawnofReverie"));

            _guide = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
            if (_guide == null)
            {
                var index = NPC.NewNPC(default, Main.spawnTileX * 16, Main.spawnTileY * 16, NPCID.Guide);
                _guide = Main.npc[index];
            }

            _playerStartPos = _guide.position + _playerOffset;
            _playerTargetPos = _guide.position + _guideOffset;
        }

        protected override void OnCutsceneUpdate(GameTime gameTime)
        {
            DisablePlayerMovement();

            UpdateCreditsAlpha();

            switch (_phase)
            {
                case Phase.OpeningCredits:
                    HandleOpeningCredits();
                    break;
                case Phase.GuideScene:
                    HandleGuideScene();
                    break;
                case Phase.PlayerDescent:
                    HandlePlayerDescent();
                    break;
                case Phase.Impact:
                    HandleImpact();
                    break;
            }
        }

        private void HandleOpeningCredits()
        {
            FadeAlpha = 1f - ElapsedTime / Timing.FADE;

            if (ElapsedTime <= Timing.CREDITS / 1.4f)
                Main.SceneMetrics.ShimmerMonolithState = 1;

            if (ElapsedTime >= Timing.CREDITS / 1.4f)
                FadeAlpha = 0f + ElapsedTime / 12f;

            if (!_creditsStarted)
            {
                Vector2 startPosition = _guide.Center - new Vector2(0, Main.screenHeight * 2.2f);
                CameraSystem.DoPanAnimationOffset(
                    (int)(Timing.CREDITS * 60), startPosition, _guide.Center);
                _creditsStarted = true;
            }

            if (ElapsedTime >= Timing.CREDITS)
            {
                _phase = Phase.GuideScene;
                ElapsedTime = 0f;
                CameraSystem.Reset();
                CameraSystem.DoPanAnimationOffset(
                    (int)(Timing.GUIDE * 60),
                    _guide.Center - new Vector2(0, -200),
                    _guide.Center);
            }
        }

        private void HandleGuideScene()
        {
            FadeAlpha = 1f - ElapsedTime / Timing.FADE;

            if (ElapsedTime >= Timing.GUIDE * 0.5f && !_dialoguePlayed)
            {
                EnableInvisibility();
                PlaySoundWithDelay();
                DialogueManager.Instance.StartDialogueByKey(
                NPCDataManager.GuideData,
                DialogueKeys.CrashLanding.Cutscene,
                lineCount: 1,
                zoomIn: true);
                _dialoguePlayed = true;
            }

            if (ElapsedTime >= Timing.GUIDE)
            {
                _phase = Phase.PlayerDescent;
                ElapsedTime = 0f;
                DisableInvisibility();
                Main.LocalPlayer.Center = _playerStartPos;
            }
        }

        private void HandlePlayerDescent()
        {
            float progress = ElapsedTime / Timing.DESCENT;

            DisableFallDamage();
            Main.LocalPlayer.position = Vector2.Lerp(_playerStartPos, _playerTargetPos, progress * 1.2f);
            Main.LocalPlayer.fullRotation += 1.4f * progress * 1.12f;
            FadeAlpha = progress / 16;
            CameraSystem.MoveCameraOut(0, Main.LocalPlayer.Center);

            if (Main.LocalPlayer.TouchedTiles.Any())
            {
                if (!_impactTriggered)
                {
                    // Impact effects
                    FadeColor = Color.White;
                    FadeAlpha = 1f;
                    CameraSystem.shake = 30;
                    _impactTriggered = true;
                }

                _phase = Phase.Impact;
                ElapsedTime = 0f;
                DialogueManager.Instance.StartDialogueByKey(
                NPCDataManager.GuideData,
                DialogueKeys.CrashLanding.Intro,
                lineCount: 1,
                zoomIn: true);
            }

            if (ElapsedTime >= Timing.DESCENT)
            {
                _phase = Phase.Impact;
                ElapsedTime = 0f;
            }
        }

        private void HandleImpact()
        {
            float progress = ElapsedTime / Timing.IMPACT;
            FadeAlpha -= progress / 16;

            if (ElapsedTime >= Timing.IMPACT)
            {
                _phase = Phase.Finish;
            }
        }

        private void UpdateCreditsAlpha()
        {
            if (_phase != Phase.OpeningCredits) return;

            if (ElapsedTime <= Timing.LOGO_IN)
            {
                _logoAlpha = ElapsedTime / Timing.LOGO_IN;
            }
            else if (ElapsedTime <= Timing.LOGO_IN + Timing.LOGO_HOLD)
            {
                _logoAlpha = 1f;
            }
            else if (ElapsedTime <= Timing.LOGO_IN + Timing.LOGO_HOLD + Timing.LOGO_OUT)
            {
                _logoAlpha = 1f - (ElapsedTime - (Timing.LOGO_IN + Timing.LOGO_HOLD)) / Timing.LOGO_OUT;
            }
            else
            {
                _logoAlpha = 0f;
            }

            float textTime = ElapsedTime - Timing.TEXT_DELAY;
            if (textTime <= 0)
            {
                _textAlpha = 0f;
            }
            else if (textTime <= Timing.TEXT_IN)
            {
                _textAlpha = textTime / Timing.TEXT_IN;
            }
            else if (textTime <= Timing.TEXT_IN + Timing.TEXT_HOLD)
            {
                _textAlpha = 1f;
            }
            else if (textTime <= Timing.TEXT_IN + Timing.TEXT_HOLD + Timing.TEXT_OUT)
            {
                _textAlpha = 1f - (textTime - (Timing.TEXT_IN + Timing.TEXT_HOLD)) / Timing.TEXT_OUT;
            }
            else
            {
                _textAlpha = 0f;
            }
        }

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
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in sound playback: {ex}");
            }
        }

        protected override void DrawCutsceneContent(SpriteBatch spriteBatch)
        {
            // Only draw credits during opening phase
            if (_phase != Phase.OpeningCredits) return;

            // Draw logo if visible
            if (_logoAlpha > 0f)
            {
                Texture2D logo = TextureAssets.Logo.Value;
                Vector2 origin = new(logo.Width / 2f, logo.Height / 2f);

                spriteBatch.Draw(
                    logo,
                    _logoPos,
                    null,
                    Color.White * _logoAlpha,
                    0f,
                    origin,
                    1f,
                    SpriteEffects.None,
                    0f);
            }

            // Draw credits if visible
            if (_textAlpha > 0f)
            {
                // Draw left column
                for (int i = 0; i < _leftCredits.Length; i++)
                {
                    float y = i * 60f;
                    Vector2 base1 = new(Main.screenWidth * _leftCredits[i].X, Main.screenHeight * 0.4f);
                    Vector2 labelPos = base1 + new Vector2(0, y);
                    Vector2 valuePos = labelPos + new Vector2(0, 25f);

                    Utils.DrawBorderString(spriteBatch, _leftCredits[i].Label, labelPos, Color.Gold * _textAlpha, 1.2f);
                    Utils.DrawBorderString(spriteBatch, _leftCredits[i].Value, valuePos, Color.White * _textAlpha, 0.9f);
                }

                // Draw right column
                for (int i = 0; i < _rightCredits.Length; i++)
                {
                    float y = i * 60f;
                    Vector2 base1 = new(Main.screenWidth * _rightCredits[i].X, Main.screenHeight * 0.4f);
                    Vector2 labelPos = base1 + new Vector2(0, y);
                    Vector2 valuePos = labelPos + new Vector2(0, 25f);

                    Utils.DrawBorderString(spriteBatch, _rightCredits[i].Label, labelPos, Color.Gold * _textAlpha, 1.2f);
                    Utils.DrawBorderString(spriteBatch, _rightCredits[i].Value, valuePos, Color.White * _textAlpha, 0.9f);
                }
            }
        }

        public override bool IsFinished() => _phase == Phase.Finish;

        public override void End()
        {
            try
            {
                DialogueManager.Instance.StartDialogueByKey(
                NPCDataManager.GuideData,
                DialogueKeys.CrashLanding.SettlingIn,
                lineCount: 5,
                zoomIn: true);
                EnableFallDamage();
                EnablePlayerMovement();
                CameraSystem.Reset();
                base.End();
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error ending cutscene: {ex}");
            }
        }

        protected override bool UsesCinematicLetterbox() => true;
    }
}