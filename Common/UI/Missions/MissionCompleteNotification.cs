using Reverie.Core.Missions;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Common.UI.Missions;

public class MissionCompleteNotification(Mission mission) : IInGameNotification
{
    private const float DISPLAY_TIME = 8f * 60f;
    private const float ANIMATION_TIME = 90f;
    private const float FADEOUT_TIME = 60f;

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
            var introScale = MathHelper.SmoothStep(0.1f, 1f, AnimationProgress);
            return introScale * MathHelper.Lerp(0.5f, 1f, FadeoutProgress);
        }
    }

    private float SunburstScale
    {
        get
        {
            var introScale = MathHelper.SmoothStep(0.1f, 0.6f, AnimationProgress);
            return introScale * MathHelper.Lerp(0.3f, 1f, FadeoutProgress);
        }
    }

    private float SunburstRotation => timeLeft * 0.01f;

    private Color TextColor
    {
        get
        {
            var baseColor = Color.Gold;
            var glowColor = Color.White;
            var currentColor = Color.Lerp(glowColor, baseColor, AnimationProgress);
            return currentColor * FadeoutProgress;
        }
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (timeLeft <= 0) return;

        var fadeoutOffset = MathHelper.Lerp(30f, 0f, FadeoutProgress);
        Vector2 screenCenter = new(Main.screenWidth / 2f, Main.screenHeight / 3f - fadeoutOffset);
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

        var title = "Mission Complete!";
        var titleScale = TextScale * 1.05f;
        var titleSize = FontAssets.DeathText.Value.MeasureString(title) * titleScale;
        var titlePos = screenCenter - titleSize / 2f;

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

        var missionName = completedMission.Name;
        var nameScale = TextScale * 1f;
        var nameSize = FontAssets.MouseText.Value.MeasureString(missionName) * nameScale;
        var namePos = screenCenter + new Vector2(-nameSize.X / 2f, titleSize.Y / 2f);

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

        var rewards = $"Rewards: {string.Join(", ", completedMission.Rewards.Select(r => $"{r.stack} {r.Name}"))}";
        var rewardScale = TextScale * 0.6f;
        var rewardSize = FontAssets.MouseText.Value.MeasureString(rewards) * rewardScale;
        var rewardPos = namePos + new Vector2(-32, rewardSize.Y + 24);

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
            SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}MissionComplete"));

        timeLeft--;
        timeLeft = Math.Max(0, timeLeft);
    }
    public void PushAnchor(ref Vector2 positionAnchorBottom) { }
}
