using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static ReverieMusic.ReverieMusic;
using Reverie.Core.Dialogue;
using Reverie.Core.Mechanics;
using Reverie.Core.Graphics;
using Reverie.Core;
using Reverie.Content.Terraria.NPCs.WorldNPCs;
using Reverie.Core.Missions;
using Reverie.Common.Players;
using Microsoft.Xna.Framework.Graphics;

namespace Reverie.Cutscenes
{
    public class EyeAppearanceCutscene : CutsceneSystem, IDrawCutscene
    {
        private const float CUTSCENE_DURATION = 35f;
        private float _elapsedTime;
        private NPC _eyeNPC;

        // State tracking
        private enum CutsceneState
        {
            FadeIn,
            EyeAppear,
            ZoomToEye,
            Dialogue,
            EyeStare,
            EyeDisappear
        }

        private CutsceneState _currentState = CutsceneState.FadeIn;
        private float _stateTimer;

        public override void Start()
        {
            base.Start();
            SetMusic(MusicLoader.GetMusicSlot(Instance, $"{Assets.Music}Resurgence"));

            _elapsedTime = 0f;
            _stateTimer = 0f;
            FadeColor = Color.Black;
            FadeAlpha = 1f;
            IsUIHidden = false;

            Vector2 spawnPosition = Main.LocalPlayer.Center - new Vector2(0, Main.screenHeight / 2);
            _eyeNPC = NPC.NewNPCDirect(default,
                (int)spawnPosition.X,
                (int)spawnPosition.Y,
                ModContent.NPCType<EOC_Cutscene>());

            DisablePlayerMovement();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!IsPlaying) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _elapsedTime += deltaTime;
            _stateTimer += deltaTime;

            UpdateState(deltaTime);
        }
        public void CustomDraw(SpriteBatch spriteBatch)
        {
            DialogueManager.Instance.StartDialogue(
                          NPCDataManager.Default,
                          DialogueID.Reawakening_EyeSequence,
                          true);
        }
        private void UpdateState(float deltaTime)
        {
            switch (_currentState)
            {
                case CutsceneState.FadeIn:
                    FadeAlpha = MathHelper.Lerp(1f, 0f, _stateTimer / 2f);
                    if (_stateTimer >= 2f)
                    {
                        _currentState = CutsceneState.EyeAppear;
                        _stateTimer = 0f;
                    }
                    break;

                case CutsceneState.EyeAppear:
                    if (_stateTimer >= 1f)
                    {
                        CameraSystem.MoveCameraOut(60, _eyeNPC.Center);
                        _currentState = CutsceneState.ZoomToEye;
                        _stateTimer = 0f;
                    }
                    break;

                case CutsceneState.ZoomToEye:
                    if (_stateTimer <= 2f)
                    {
                        ZoomHandler.SetZoomAnimation(1.5f, 60);
                    }
                    if (_stateTimer >= 3f)
                    {
                        _currentState = CutsceneState.Dialogue;
                        _stateTimer = 0f;
                    }
                    break;

                case CutsceneState.Dialogue:
                    // Check if dialogue is NOT active AND we've started it (prevents skipping)
                    if (_stateTimer >= 0.5f)
                    {
                        _currentState = CutsceneState.EyeStare;
                        _stateTimer = 0f;
                    }
                    break;

                case CutsceneState.EyeStare:
                    if (_stateTimer >= 2f)
                    {
                        _currentState = CutsceneState.EyeDisappear;
                        _stateTimer = 0f;
                    }
                    break;

                case CutsceneState.EyeDisappear:
                    FadeAlpha = MathHelper.Lerp(0f, 1f, _stateTimer / 2f);
                    if (_stateTimer >= 2f)
                    {
                        IsPlaying = false;
                    }
                    break;
            }
        }

        public override bool IsFinished()
            => !IsPlaying;

        public override void End()
        {
            // Clean up the Eye NPC
            if (_eyeNPC != null && _eyeNPC.active)
            {
                _eyeNPC.active = false;
            }

            // Return camera to normal
            CameraSystem.ReturnCamera(60);
            ZoomHandler.SetZoomAnimation(1f, 60);

            // Start the next dialogue sequence
            DialogueManager.Instance.StartDialogue(
                NPCDataManager.GuideData,
                DialogueID.Reawakening_TrainingSequence,
                true);

            MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            Mission Reawakening = player.GetMission(MissionID.Reawakening);

            if (Reawakening != null && Reawakening.CurrentSetIndex == 1)
                Reawakening.UpdateProgress(0);

            EnablePlayerMovement();
            base.End();
        }
    }
}