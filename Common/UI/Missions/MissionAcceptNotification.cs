using Reverie.Core.Loaders;
using Reverie.Core.Missions;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Common.UI.Missions;

public class MissionAcceptNotification(Mission mission) : IInGameNotification
{
    private const float DISPLAY_TIME = 5.5f * 60f;
    private const float ANIMATION_TIME = 60f;
    private const float FADEOUT_TIME = 30f;

    public bool ShouldBeRemoved => timeLeft <= 0;
    private float timeLeft = DISPLAY_TIME;
    private readonly Mission acceptedMission = mission;

    private float AnimationProgress => Math.Clamp((DISPLAY_TIME - timeLeft) / ANIMATION_TIME, 0f, 1f);

    private float FadeoutProgress
    {
        get
        {
            if (timeLeft > FADEOUT_TIME) return 1f;
            return timeLeft / FADEOUT_TIME;
        }
    }

    // Unified scale for all elements - starts small, hits normal size
    private float UnifiedScale => MathHelper.SmoothStep(0.15f, 1f, AnimationProgress);

    private float ElementOpacity => FadeoutProgress; // Simple fadeout at the end

    private Color TextColor
    {
        get
        {
            // Keep white throughout animation
            var whiteColor = Color.White;
            return whiteColor * ElementOpacity;
        }
    }

    private float SunburstRotation => timeLeft * 0.005f;

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (timeLeft <= 0) return;

        var fadeoutOffset = MathHelper.Lerp(30f, 0f, FadeoutProgress);
        Vector2 screenCenter = new(Main.screenWidth / 2f, Main.screenHeight / 3f - fadeoutOffset);

        var title = $"{acceptedMission.Name}";
        var description = $"{acceptedMission.Description}";

        var titleSize = FontAssets.DeathText.Value.MeasureString(title);
        var descSize = FontAssets.MouseText.Value.MeasureString(description);

        var titleScale = UnifiedScale;
        var descScale = UnifiedScale * 0.65f;

        var titlePos = screenCenter - new Vector2(0, titleSize.Y * titleScale / 2f + 10);
        var descPos = screenCenter + new Vector2(0, descSize.Y * descScale * 0.5f + 20);

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.DeathText.Value,
            title,
            titlePos.X,
            titlePos.Y,
            TextColor,
            Color.Black * ElementOpacity,
            titleSize / 2f,
            titleScale
        );

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            description,
            descPos.X,
            descPos.Y,
            TextColor,
            Color.Black * ElementOpacity,
            descSize / 2f,
            descScale
        );

        spriteBatch.End();

        var effect = ShaderLoader.GetShader("SunburstShader").Value;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                          DepthStencilState.None, Main.Rasterizer, effect, Main.UIScaleMatrix);

        if (effect != null)
        {
            effect.Parameters["uTime"]?.SetValue(SunburstRotation);
            effect.Parameters["uIntensity"]?.SetValue((TextColor.R / 255f) * 0.2f);
            effect.Parameters["uColor"]?.SetValue(new Vector3(1f, 1f, 1f)); // White sunburst

            Vector2 shaderCenter = new(
                (screenCenter.X / Main.screenWidth) * 2f - 1f,
                (screenCenter.Y / Main.screenHeight) * 2f - 1.1f
            );

            effect.Parameters["uCenter"]?.SetValue(shaderCenter);
            effect.Parameters["uScale"]?.SetValue(UnifiedScale * 0.7f);
            effect.Parameters["uRayCount"]?.SetValue(18f);
        }

        var noiseTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Perlin").Value;
        spriteBatch.Draw(noiseTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                          DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
    }

    public void Update()
    {
        if (timeLeft == DISPLAY_TIME)
            SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}MissionAccept"));

        timeLeft--;
        timeLeft = Math.Max(0, timeLeft);
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom) { }
}