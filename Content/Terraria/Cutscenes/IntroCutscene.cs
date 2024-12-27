using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using static ReverieMusic.ReverieMusic;

using Reverie.Core.Dialogue;
using Reverie.Core.Mechanics;
using Reverie.Core.Cutscenes;

namespace Reverie.Cutscenes
{
    public class IntroCutscene : CutsceneSystem
    {
        private const float CUTSCENE_DURATION = 18f;
        private float _elapsedTime;

        public override void Start()
        {
            base.Start();
            SetMusic(MusicLoader.GetMusicSlot(Instance, $"{Assets.Music}Resurgence"));
            _elapsedTime = 0f;

            FadeColor = Color.Black;
            FadeAlpha = 1f;

            Vector2 startPosition = Main.LocalPlayer.Center - new Vector2(0, Main.screenHeight);
            CameraSystem.DoPanAnimation((int)(CUTSCENE_DURATION * 60.5f), startPosition, Main.LocalPlayer.Center, useOffsetOrigin: true);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            DisablePlayerMovement();
            if (!IsPlaying) return;

            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            FadeAlpha = MathHelper.Lerp(0.85f, 0f, _elapsedTime / CUTSCENE_DURATION);   
        }

        public override bool IsFinished()
            => _elapsedTime >= CUTSCENE_DURATION;
        
        public override void End()
        {
            DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.WakingUpToTheGuideYapping);
            EnablePlayerMovement();
            base.End();
        }
    }
}