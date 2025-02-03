using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using static ReverieMusic.ReverieMusic;

using Reverie.Core.Dialogue;
using Reverie.Core.Mechanics;
using Reverie.Core.Graphics;
using Terraria.ID;

namespace Reverie.Cutscenes
{
    public class IntroCutscene : CutsceneSystem
    {
        private const float CUTSCENE_DURATION = 36f;
        private float _elapsedTime;

        public override void Start()
        {
            base.Start();
            SetMusic(MusicID.OtherworldlyUnderworld); //MusicLoader.GetMusicSlot(Instance, $"{Assets.Music}Resurgence")
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
            Player player = Main.LocalPlayer;

            if (!IsPlaying) return;
            Main.time = 49800;
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            FadeAlpha = MathHelper.Lerp(0.85f, 0f, _elapsedTime / CUTSCENE_DURATION);   
        }

        public override bool IsFinished()
            => _elapsedTime >= CUTSCENE_DURATION;
        
        public override void End()
        {
            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.Reawakening_Opening, true);
            EnablePlayerMovement();
            Player player = Main.LocalPlayer;
            base.End();
        }
    }
}