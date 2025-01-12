using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Reverie.Core.Missions;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using System.Linq;

namespace Reverie.Common.UI.MissionUI
{
    public class MissionCompleteNotification(Mission mission) : IInGameNotification
    {
        private const float DISPLAY_TIME = 5f * 60f;
        private const float ANIMATION_TIME = 60f;
        private const float FADEOUT_TIME = 30f;

        public bool ShouldBeRemoved => timeLeft <= 0;
        private float timeLeft = DISPLAY_TIME;
        private readonly Mission completedMission = mission;

        private float AnimationProgress => Math.Clamp((DISPLAY_TIME - timeLeft) / ANIMATION_TIME, 0f, 1f);

        private float FadeoutProgress
        {
            get
            {
                if (timeLeft > FADEOUT_TIME) return 1f;
                return timeLeft / FADEOUT_TIME;
            }
        }

        private float TextScale
        {
            get
            {
                float introScale = MathHelper.SmoothStep(0.1f, 1f, AnimationProgress);
                return introScale * MathHelper.Lerp(0.5f, 1f, FadeoutProgress);
            }
        }

        private float SunburstScale
        {
            get
            {
                float introScale = MathHelper.SmoothStep(0.1f, 0.6f, AnimationProgress);
                return introScale * MathHelper.Lerp(0.3f, 1f, FadeoutProgress);
            }
        }

        private float SunburstRotation => timeLeft * 0.01f;

        private Color TextColor
        {
            get
            {
                Color baseColor = Color.Gold;
                Color glowColor = Color.White;
                Color currentColor = Color.Lerp(glowColor, baseColor, AnimationProgress);
                return currentColor * FadeoutProgress;
            }
        }

        public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
        {
            if (timeLeft <= 0) return;

            float fadeoutOffset = MathHelper.Lerp(30f, 0f, FadeoutProgress);
            Vector2 screenCenter = new(Main.screenWidth / 2f, Main.screenHeight / 3f - fadeoutOffset);
            Texture2D sunburstTexture = ModContent.Request<Texture2D>($"{Assets.VFX.Directory}Sunburst").Value;

            float rotation = SunburstRotation;
            Vector2 origin = new(sunburstTexture.Width / 2f);
            spriteBatch.Draw(
                sunburstTexture,
                screenCenter,
                null,
                TextColor * 0.2f * FadeoutProgress,
                rotation,
                origin,
                SunburstScale,
                SpriteEffects.None,
                0f
            );

            string title = "Mission Complete!";
            float titleScale = TextScale * 1.05f;
            Vector2 titleSize = FontAssets.DeathText.Value.MeasureString(title) * titleScale;
            Vector2 titlePos = screenCenter - titleSize / 2f;

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.DeathText.Value,
                title,
                titlePos.X,
                titlePos.Y,
                TextColor,
                Color.Black * FadeoutProgress,
                Vector2.Zero,
                titleScale
            );

            string missionName = completedMission.MissionData.Name;
            float nameScale = TextScale * 1f;
            Vector2 nameSize = FontAssets.MouseText.Value.MeasureString(missionName) * nameScale;
            Vector2 namePos = screenCenter + new Vector2(-nameSize.X / 2f, titleSize.Y / 2f);

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                missionName,
                namePos.X,
                namePos.Y,
                TextColor,
                Color.Black * FadeoutProgress,
                Vector2.Zero,
                nameScale
            );

            string rewards = $"Rewards: {string.Join(", ", completedMission.MissionData.Rewards.Select(r => $"{r.stack} {r.Name}"))}";
            float rewardScale = TextScale * 0.6f;
            Vector2 rewardSize = FontAssets.MouseText.Value.MeasureString(rewards) * rewardScale;
            Vector2 rewardPos = namePos + new Vector2(-nameSize.X / 2f, nameSize.Y + 10);

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                rewards,
                rewardPos.X,
                rewardPos.Y,
                Color.White * FadeoutProgress,
                Color.Black * FadeoutProgress,
                Vector2.Zero,
                rewardScale
            );
        }

        public void Update()
        {
            if (timeLeft == DISPLAY_TIME)
                SoundEngine.PlaySound(SoundID.AchievementComplete);

            timeLeft--;
            timeLeft = Math.Max(0, timeLeft);
        }
        public void PushAnchor(ref Vector2 positionAnchorBottom) { }
    }
}