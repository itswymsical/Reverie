using Reverie.Common.Players;

using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Common.UI.LevelSystem
{
    public class LevelUpNotification : IInGameNotification
    {
        private const float DISPLAY_TIME = 8f * 60f;
        private const float ANIMATION_TIME = 90f;
        private const float FADEOUT_TIME = 60f;

        public bool ShouldBeRemoved => timeLeft <= 0;
        private float timeLeft = DISPLAY_TIME;

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
                var introScale = MathHelper.SmoothStep(0.1f, 1f, AnimationProgress);
                return introScale * MathHelper.Lerp(0.3f, 1f, FadeoutProgress);
            }
        }

        private float SunburstScale
        {
            get
            {
                var introScale = MathHelper.SmoothStep(0.1f, 0.6f, AnimationProgress);
                return introScale * MathHelper.Lerp(0.3f, 1.3f, FadeoutProgress);
            }
        }

        private float SunburstRotation => timeLeft * 0.01f;

        private Color TextColor
        {
            get
            {
                var baseColor = Color.LightGoldenrodYellow;
                var glowColor = Color.White;
                var currentColor = Color.Lerp(glowColor, baseColor, AnimationProgress);
                return currentColor * FadeoutProgress;
            }
        }
        ExperiencePlayer player = Main.LocalPlayer.GetModPlayer<ExperiencePlayer>();
        public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
        {
            if (timeLeft <= 0) return;

            var fadeoutOffset = MathHelper.Lerp(30f, 0f, FadeoutProgress);
            Vector2 screenCenter = new(Main.screenWidth / 2f, Main.screenHeight / 4f - fadeoutOffset);
            var sunburstTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Sunburst").Value;

            var rotation = SunburstRotation;
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

            var levelLabel = $"Level {player.playerLevel}";
            var levelLabelScale = TextScale * 1.2f;
            var levelLabelSize = FontAssets.DeathText.Value.MeasureString(levelLabel) * levelLabelScale;
            var levelLabelPos = screenCenter - levelLabelSize / 2.2f;

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.DeathText.Value,
                levelLabel,
                levelLabelPos.X,
                levelLabelPos.Y + 16,
                TextColor,
                Color.DarkGray * FadeoutProgress,
                Vector2.Zero,
                levelLabelScale
            );

            var rankingTitle = $"[Title]"; //todo: make ranking title system and make it class based
            var titleScale = TextScale * 0.9f;
            var titleSize = FontAssets.MouseText.Value.MeasureString(rankingTitle) * titleScale;
            var titlePos = screenCenter + new Vector2(-titleSize.X / 2f, levelLabelSize.Y / 2f);

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                rankingTitle,
                titlePos.X,
                titlePos.Y - 8,
                TextColor * FadeoutProgress,
                Color.Black * FadeoutProgress,
                Vector2.Zero,
                titleScale
            );

            var rewards = $"Skill Points Available: {player.skillPoints}";
            var rewardScale = TextScale * 0.7f;
            var rewardSize = FontAssets.MouseText.Value.MeasureString(rewards) * rewardScale;
            var rewardPos = screenCenter + new Vector2(-rewardSize.X / 2f, titleSize.Y / 2f);

            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                rewards,
                rewardPos.X,
                rewardPos.Y + 48,
                TextColor * FadeoutProgress,
                Color.Black * FadeoutProgress,
                Vector2.Zero,
                rewardScale
            );
        }

        public void Update()
        {
            if (timeLeft == DISPLAY_TIME)
                SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}MissionComplete"));

            timeLeft--;
            timeLeft = Math.Max(0, timeLeft);
        }
        public void PushAnchor(ref Vector2 positionAnchorBottom) { }
    }
}