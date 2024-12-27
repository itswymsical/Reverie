using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Reverie.Core.Missions;
using System.Linq;
using Terraria.ModLoader;

namespace Reverie.Common.UI.MissionUI
{
    public class MissionStatusIndicator(Mission mission) : IInGameNotification
    {
        public bool ShouldBeRemoved => timeLeft <= 0;
        private int timeLeft = 5 * 60;
        private readonly Mission completedMission = mission;
        private readonly float panelWidth = 420f;
        private readonly Texture2D bloomTexture = ModContent.Request<Texture2D>($"{Assets.VFX.Directory}Vignette").Value;

        private float Scale
        {
            get
            {
                if (timeLeft < 30)
                    return MathHelper.Lerp(0f, 1f, timeLeft / 30f);
                if (timeLeft > 285)
                    return MathHelper.Lerp(1f, 0f, (timeLeft - 285) / 15f);
                return 1f;
            }
        }

        private float Opacity
        {
            get
            {
                if (Scale <= 0.5f)
                    return 0f;
                return (Scale - 0.5f) / 0.5f;
            }
        }

        private float TopTextScale
        {
            get
            {
                float progress = (300f - timeLeft) / 30f;
                return MathHelper.Clamp(progress * 1.5f, 0f, 1f);
            }
        }

        private float TopTextOpacity
        {
            get
            {
                float progress = (300f - timeLeft) / 15f;
                return MathHelper.Clamp(progress, 0f, 1f);
            }
        }

        public void Update()
        {
            if (timeLeft == 5 * 60)
            {
                SoundEngine.PlaySound(SoundID.AchievementComplete, Main.LocalPlayer.position);
            }
            timeLeft--;
            if (timeLeft < 0)
            {
                timeLeft = 0;
            }
        }

        public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
        {
            if (Opacity <= 0f)
                return;

            DrawTopCenterText(spriteBatch);
            DrawBottomPanel(spriteBatch, bottomAnchorPosition);
        }

        private void DrawTopCenterText(SpriteBatch spriteBatch)
        {
            string title = "Mission Complete!";
            string subtitle = completedMission.MissionData.Name;

            Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 4f);

            // Draw bloom texture
            float bloomScale = TopTextScale * 1.5f;
            Vector2 bloomOrigin = new Vector2(bloomTexture.Width / 2f, bloomTexture.Height / 2f);
            spriteBatch.Draw(bloomTexture, screenCenter, null, Color.White * TopTextOpacity * 0.5f, 0f, bloomOrigin, bloomScale, SpriteEffects.None, 0f);

            // Draw main title
            Vector2 titleSize = FontAssets.DeathText.Value.MeasureString(title) * TopTextScale;
            Vector2 titlePosition = screenCenter - new Vector2(titleSize.X / 2f, titleSize.Y);
            Utils.DrawBorderString(spriteBatch, title, titlePosition, Color.Gold * TopTextOpacity, TopTextScale, 0.5f, 0.4f);

            // Draw subtitle
            float subtitleScale = TopTextScale * 0.6f;
            Vector2 subtitleSize = FontAssets.MouseText.Value.MeasureString(subtitle) * subtitleScale;
            Vector2 subtitlePosition = screenCenter + new Vector2(-subtitleSize.X / 2f, 10f);
            Utils.DrawBorderString(spriteBatch, subtitle, subtitlePosition, Color.White * TopTextOpacity, subtitleScale, 0.5f, 0.4f);
        }

        private void DrawBottomPanel(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
        {
            Player player = Main.LocalPlayer;
            string rewards = string.Join(", ", completedMission.MissionData.Rewards.Select(r => $"{r.stack} {r.Name}"));
            float effectiveScale = Scale * 1.1f;
            float panelHeight = new Vector2(35f, 20f).Y * 2f * effectiveScale;
            Vector2 panelSize = new Vector2(panelWidth + 18, panelHeight);
            Vector2 panelPosition = bottomAnchorPosition + new Vector2(0f, (15f - panelSize.Y * 20f) * 0.5f);
            Rectangle panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);
            Color colorText = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor / 5, Main.mouseTextColor);

            Utils.DrawInvBG(spriteBatch, panelRectangle, default);

            // Draw the first reward item icon
            if (completedMission.MissionData.Rewards.Count > 0)
            {
                var firstReward = completedMission.MissionData.Rewards[0];
                var rewardTexture = TextureAssets.Item[firstReward.type];
                spriteBatch.Draw(rewardTexture.Value, panelPosition + new Vector2(-30f + panelWidth / 3, -52f + panelSize.Y), Color.White * Opacity);
            }

            Utils.DrawBorderString(spriteBatch, "Mission Complete!", panelPosition + new Vector2(panelWidth / 4, -44f + panelSize.Y), colorText * Opacity, effectiveScale * 0.9f, 1f, 0.4f);
            Utils.DrawBorderString(spriteBatch, $"Rewards: {rewards}", panelPosition + new Vector2(panelWidth / 4, -24f + panelSize.Y), colorText * Opacity, effectiveScale * 0.7f, 1f, 0.4f);
        }

        public void PushAnchor(ref Vector2 positionAnchorBottom) => positionAnchorBottom.Y -= 50f * Opacity;
    }
}