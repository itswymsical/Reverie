using Reverie.Core.Dialogue;
using Terraria.Audio;
using System.Linq;
using System.Threading.Tasks;
using Reverie.Common.Systems;
using Reverie.Utilities.Extensions;
using Reverie.Content.Projectiles;

namespace Reverie.Core.Cinematics.Cutscenes;

public class FallingStarCutscene : Cutscene
{
    private enum CutscenePhase
    {
        FadeIn,
        GuideScene,
        PlayerDescent,
        Impact,
        Finish
    }

    private const float FADE_DURATION = 8f;
    private const float GUIDE_SCENE_DURATION = 7f;
    private const float DESCENT_DURATION = 3f;
    private const float IMPACT_HOLD_DURATION = 5f;

    private float _elapsedTime;
    private CutscenePhase _currentPhase;
    private NPC _guide;
    private Vector2 _playerStartPosition;
    private Vector2 _playerTargetPosition;
    private bool _dialoguePlayed;
    private bool _impactTriggered;
    public override void Start()
    {
        base.Start();

        _elapsedTime = 0f;
        _currentPhase = CutscenePhase.FadeIn;
        _dialoguePlayed = false;
        _impactTriggered = false;

        FadeColor = Color.Black;
        FadeAlpha = 1f;
        DisableFallDamage();
        EnableInvisibility();
        SetMusic(MusicID.WindyDay);
        _guide = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
        if (_guide == null)
        {
            var guideIndex = NPC.NewNPC(default, Main.spawnTileX * 16, Main.spawnTileY * 16, NPCID.Guide);
            _guide = Main.npc[guideIndex];
        }

        _playerStartPosition = _guide.position + new Vector2(-150, -Main.screenHeight * 1.3f);
        _playerTargetPosition = _guide.position + new Vector2(-175, 0);
    }

    private static async void PlaySoundWithDelay()
    {
        for (var i = 0; i < 10; i++)
        {
            SoundEngine.PlaySound(SoundID.Item9);
            await Task.Delay(500);
        }
    }

    private void UpdatePhase()
    {
        CameraSystem.DoPanAnimation((int)17f * 60, _guide.Center - new Vector2(0, -200), _guide.Center);

        switch (_currentPhase)
        {
            case CutscenePhase.FadeIn:
                FadeAlpha = 1f - _elapsedTime / FADE_DURATION;

                _guide.velocity = Vector2.Zero;
                _guide.ForceBubbleChatState();

                if (_elapsedTime >= FADE_DURATION)
                {
                    _currentPhase = CutscenePhase.GuideScene;
                    _elapsedTime = 0f;
                }
                break;

            case CutscenePhase.GuideScene:
                if (_elapsedTime >= GUIDE_SCENE_DURATION * 0.5f && !_dialoguePlayed)
                {
                    EnableInvisibility();
                    PlaySoundWithDelay();
                    DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_Cutscene, true);
                    _dialoguePlayed = true;
                    CameraSystem.shake = 3;
                }

                if (_elapsedTime >= GUIDE_SCENE_DURATION)
                {
                    DisableInvisibility();

                    _currentPhase = CutscenePhase.PlayerDescent;
                    _elapsedTime = 0f;
                    Main.LocalPlayer.Center = _playerStartPosition;
                }
                break;

            case CutscenePhase.PlayerDescent:
                var descentProgress = _elapsedTime / DESCENT_DURATION;
                DisableFallDamage();

                Main.LocalPlayer.position = Vector2.Lerp(_playerStartPosition, _playerTargetPosition, descentProgress * 1.2f);
                Main.LocalPlayer.fullRotation += 1.4f * descentProgress * 1.12f;
                FadeAlpha = descentProgress / 16;

                CameraSystem.MoveCameraOut(0, Main.LocalPlayer.Center);

                if (Main.LocalPlayer.TouchedTiles.Any())
                {
                    if (!_impactTriggered)
                    {
                        var proj = Projectile.NewProjectile(null, Main.LocalPlayer.Center, Vector2.Zero,
                            ModContent.ProjectileType<ExplosiveLanding>(), 0, 0f, Main.myPlayer);
                        _impactTriggered = true;
                        FadeColor = Color.White;
                        FadeAlpha = 1f;
                        CameraSystem.shake = 30;
                    }
                    _currentPhase = CutscenePhase.Impact;
                    _elapsedTime = 0f;
                    DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_Intro, true);
                }
                break;

            case CutscenePhase.Impact:
                var fadeProgress = _elapsedTime / IMPACT_HOLD_DURATION;
                FadeAlpha -= fadeProgress / 16;

                if (_elapsedTime >= IMPACT_HOLD_DURATION)
                {
                    _currentPhase = CutscenePhase.Finish;
                }
                break;
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        DisablePlayerMovement();

        if (!IsPlaying) return;

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _elapsedTime += deltaTime;

        UpdatePhase();
    }

    public override bool IsFinished()
    {
        return _currentPhase == CutscenePhase.Finish;
    }

    public override void End()
    {
        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SettlingIn);

        EnableFallDamage();

        EnablePlayerMovement();

        CameraSystem.Reset();

        base.End();
    }
}
