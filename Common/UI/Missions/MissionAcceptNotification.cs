using Reverie.Core.Loaders;
using Reverie.Core.Missions.Core;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Common.UI.Missions;

public class MissionAcceptNotification(Mission mission) : IInGameNotification
{
    private const float DISPLAY_TIME = 8f * 60f;
    private const float ANIMATION_TIME = 90f;
    private const float FADEOUT_TIME = 60f;

    public bool ShouldBeRemoved => timeLeft <= 0;
    private float timeLeft = DISPLAY_TIME;
    private readonly Mission startedMission = mission;

    private float AnimationProgress => Math.Clamp((DISPLAY_TIME - timeLeft) / ANIMATION_TIME, 0f, 1f);

    private float FadeoutProgress
    {
        get
        {
            if (timeLeft > FADEOUT_TIME) return 1f;
            return timeLeft / FADEOUT_TIME;
        }
    }

    // Unified scale for all elements
    private float UnifiedScale => MathHelper.SmoothStep(0.15f, 1f, AnimationProgress);

    private float ElementOpacity => FadeoutProgress;

    private float SunburstRotation => timeLeft * 0.01f;

    private Color TextColor
    {
        get
        {
            var targetColor = Color.LightGray;
            var whiteColor = Color.White;
            // Start white, transition to light gray during animation
            var currentColor = Color.Lerp(whiteColor, targetColor, AnimationProgress);
            return currentColor * ElementOpacity;
        }
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (timeLeft <= 0) return;

        var fadeoutOffset = MathHelper.Lerp(30f, 0f, FadeoutProgress);
        Vector2 screenCenter = new(Main.screenWidth / 2f, Main.screenHeight / 3f - fadeoutOffset);

        spriteBatch.End();

        var effect = ShaderLoader.GetShader("SunburstShader").Value;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                          DepthStencilState.None, Main.Rasterizer, effect, Main.UIScaleMatrix);

        if (effect != null)
        {
            effect.Parameters["uTime"]?.SetValue(SunburstRotation);
            effect.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["uSourceRect"]?.SetValue(new Vector4(0, 0, Main.screenWidth, Main.screenHeight));
            effect.Parameters["uIntensity"]?.SetValue((TextColor.R / 255f) * 0.35f);
            //effect.Parameters["uImage0"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Perlin").Value);

            Vector2 shaderCenter = new(
                (screenCenter.X / Main.screenWidth) * 2f - 1f,
                (screenCenter.Y / Main.screenHeight) * 2f - 1f
            );
            effect.Parameters["uCenter"]?.SetValue(shaderCenter);
            effect.Parameters["uScale"]?.SetValue(UnifiedScale * 1.2f);
            effect.Parameters["uRayCount"]?.SetValue(8f);
        }

        var noiseTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Perlin").Value;
        spriteBatch.Draw(noiseTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                          DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        var title = startedMission.Name;
        var titleScale = UnifiedScale * 1.05f;
        var titleSize = FontAssets.DeathText.Value.MeasureString(title) * titleScale;
        var titlePos = screenCenter - titleSize / 2f;

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.DeathText.Value,
            title,
            titlePos.X,
            titlePos.Y,
            TextColor,
            Color.Black * ElementOpacity,
            Vector2.Zero,
            titleScale
        );

        var desc = $"{startedMission.Description}";
        var descScale = UnifiedScale * 0.6f; // Apply unified scale
        var descSize = FontAssets.MouseText.Value.MeasureString(desc) * descScale;
        var descPos = titlePos + new Vector2(16, descSize.Y + 50);

        // Description stays white throughout animation
        var descColor = Color.Lerp(Color.White, Color.White, AnimationProgress) * ElementOpacity;

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            desc,
            descPos.X,
            descPos.Y,
            descColor,
            Color.Black * ElementOpacity,
            Vector2.Zero,
            descScale
        );
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