using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static ReverieMusic.ReverieMusic;
using Reverie.Core.Dialogue;
using Reverie.Core.Mechanics;
using Reverie.Core.Graphics;
using Terraria.ID;
using System.Linq;

namespace Reverie.Cutscenes
{
    public class IntroCutscene : CutsceneSystem
    {
        private enum CutscenePhase
        {
            Setup,
            GuideScene,
            Rumble,
            PlayerDive,
            Impact,
            Finish
        }

        private const float GUIDE_DURATION = 10f;
        private const float RUMBLE_DURATION = 3f;
        private const float DIVE_DURATION = 7f;
        private const float IMPACT_DURATION = 4.5f;

        private float _elapsedTime;
        private CutscenePhase _currentPhase;
        private NPC _guide;
        private Vector2 _guidePosition;
        private Vector2 _playerStartPosition;

        public override void Start()
        {
            base.Start();

            // Setup initial conditions
            _elapsedTime = 0f;
            _currentPhase = CutscenePhase.Setup;
            FadeColor = Color.Black;
            FadeAlpha = 0f;

            // Set time to noon
            Main.time = 27000;

            // Find or spawn Guide NPC
            _guide = Main.npc.FirstOrDefault(n => n.type == NPCID.Guide);
            if (_guide == null)
            {
                int guideIndex = NPC.NewNPC(default, Main.spawnTileX * 16, Main.spawnTileY * 16, NPCID.Guide);
                _guide = Main.npc[guideIndex];
            }

            // Position Guide and Player
            _guidePosition = _guide.Center;
            _playerStartPosition = _guidePosition + new Vector2(0, -Main.screenHeight * 1.5f);
            Main.LocalPlayer.Center = _playerStartPosition;

            // Move camera to guide immediately
            CameraSystem.MoveCameraOut(1, _guidePosition);
        }
        private bool init = false;
        private void UpdatePhase(float deltaTime)
        {
            switch (_currentPhase)
            {
                case CutscenePhase.Setup:
                    _currentPhase = CutscenePhase.GuideScene;
                    break;

                case CutscenePhase.GuideScene:
                    if (_elapsedTime >= GUIDE_DURATION)
                    {
                        _currentPhase = CutscenePhase.Rumble;
                        _elapsedTime = 0f;
                    }
                    break;

                case CutscenePhase.Rumble:
                    // Increase shake intensity over time
                    CameraSystem.shake = (int)MathHelper.Lerp(0, 15, _elapsedTime / RUMBLE_DURATION);

                    if (_elapsedTime >= RUMBLE_DURATION)
                    {
                        _currentPhase = CutscenePhase.PlayerDive;
                        _elapsedTime = 0f;

                        // Pan to player position
                        CameraSystem.DoPanAnimation(
                            duration: (int)(DIVE_DURATION * 60f),
                            target: _playerStartPosition,
                            secondaryTarget: _guidePosition,
                            easeIn: Vector2.Lerp  // Use linear interpolation for faster camera movement
                        );
                    }
                    break;

                case CutscenePhase.PlayerDive:
                    float diveProgress = _elapsedTime / DIVE_DURATION;
                    Main.LocalPlayer.Center = Vector2.Lerp(_playerStartPosition, _guidePosition, diveProgress);

                    if (_elapsedTime >= DIVE_DURATION)
                    {
                        _currentPhase = CutscenePhase.Impact;
                        _elapsedTime = 0f;
                        FadeColor = Color.White;
                        CameraSystem.shake = 30; // Big shake on impact
                    }
                    break;

                case CutscenePhase.Impact:
                    FadeAlpha = 1f;
                    if (!init)
                    {
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_Intro, true);
                        init = true;
                    }
                    
                    if (_elapsedTime >= IMPACT_DURATION)
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

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _elapsedTime += deltaTime;

            UpdatePhase(deltaTime);
        }

        public override bool IsFinished() => _currentPhase == CutscenePhase.Finish;

        public override void End()
        {
            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SettlingIn, true);
            EnablePlayerMovement();
            CameraSystem.Reset(); // Reset any remaining camera effects
            base.End();
        }
    }
}