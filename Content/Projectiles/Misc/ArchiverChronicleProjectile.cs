using Reverie.Content.Items.Misc;
using Reverie.Core.Cinematics;
using Reverie.Core.Loaders;
using Terraria.Audio;

namespace Reverie.Content.Projectiles.Misc;

public class ArchiverChronicleProjectile : ModProjectile
{
    public override string Texture => $"{TEXTURE_DIRECTORY}Items/Misc/ArchiverChronicle";

    private int phase = 0;
    private float shakeIntensity = 0f;
    private float glowIntensity = 0f;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private Vector2 basePosition;
    private bool initialized = false;
    private float sineStartTime;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.width = 36;
        Projectile.height = 32;
        Projectile.tileCollide = false;
        Projectile.alpha = 255;
        Projectile.timeLeft = 420;
        Projectile.aiStyle = -1;
        Projectile.light = 0.75f;
    }

    public override void AI()
    {
        if (!initialized)
        {
            startPosition = Projectile.position;
            targetPosition = startPosition + new Vector2(0, -80);
            basePosition = startPosition;
            initialized = true;
            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop, Main.LocalPlayer.position);

        }

        // Phase 0: Rise and fade in (2 seconds) - original timing
        if (phase == 0)
        {
            float progress = (420f - Projectile.timeLeft) / 120f;

            Vector2 currentPos = Vector2.Lerp(startPosition, targetPosition, EaseFunction.EaseCubicInOut.Ease(progress));
            Projectile.position = currentPos;
            basePosition = currentPos;

            Projectile.alpha = Math.Max(0, (int)(255 * (1f - EaseFunction.EaseQuadOut.Ease(progress))));
            glowIntensity = EaseFunction.EaseQuadIn.Ease(progress) * 1f;

            if (Projectile.timeLeft <= 300)
            {
                phase = 1;
                // Capture the start time for seamless sine transition
                sineStartTime = Main.GlobalTimeWrappedHourly;
            }
        }
        // Phase 1: Hover in sine pattern
        else if (phase == 1)
        {
            // Maintain glow intensity
            glowIntensity = 1f;

            // Sine wave hovering around the target position - starts from 0 for seamless transition
            float time = Main.GlobalTimeWrappedHourly - sineStartTime;
            Vector2 sineOffset = new Vector2(0, (float)Math.Sin(time) * 15f); // 15 pixel amplitude

            basePosition = targetPosition + sineOffset;
            Projectile.position = basePosition;

            // Optional: slight glow pulsing with the sine wave
            glowIntensity = 1f + (float)Math.Sin(time * 1.5f) * 0.3f;

            if (Projectile.timeLeft <= 250)
            {
                phase = 2;
            }
        }
        else if (phase == 2)
        {
            float time = Main.GlobalTimeWrappedHourly - sineStartTime;
            Vector2 sineOffset = new Vector2(0, (float)Math.Sin(time) * 15f);
            basePosition = targetPosition + sineOffset;
            Projectile.position = basePosition;

            // Calculate fade progress (0 to 1 as timeLeft goes from 150 to 0)
            float fadeProgress = (150f - Projectile.timeLeft) / 150f;

            // Smooth fade using easing function
            glowIntensity = 1f * EaseFunction.EaseQuadOut.Ease(1f - fadeProgress);

            // Also fade the projectile alpha
            Projectile.alpha = (int)(255 * EaseFunction.EaseQuadIn.Ease(fadeProgress));
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!initialized) return true;

        var spriteBatch = Main.spriteBatch;
        var texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        Color drawColor = lightColor * (1f - Projectile.alpha / 255f);
        spriteBatch.Draw(texture, drawPos, null, drawColor, Projectile.rotation, texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

        if (glowIntensity > 0f)
        {
            var sunburstShader = ShaderLoader.GetShader("SunburstShader").Value;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, sunburstShader, Main.GameViewMatrix.TransformationMatrix);

            if (sunburstShader != null)
            {
                sunburstShader.Parameters["uCenter"]?.SetValue(Vector2.Zero);
                sunburstShader.Parameters["uIntensity"]?.SetValue(glowIntensity * 0.5f);
                sunburstShader.Parameters["uScale"]?.SetValue(1f + glowIntensity * 0.3f);
                sunburstShader.Parameters["uRayCount"]?.SetValue(10f);
                sunburstShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3.5f);
                sunburstShader.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                sunburstShader.Parameters["uSourceRect"]?.SetValue(new Vector4(0, 0, texture.Width, texture.Height));
                sunburstShader.CurrentTechnique.Passes[0].Apply();
            }

            float sunburstSize = 60f + glowIntensity * 64f;
            Rectangle sunburstRect = new Rectangle(
                (int)(drawPos.X - sunburstSize),
                (int)(drawPos.Y - sunburstSize),
                (int)(sunburstSize * 2),
                (int)(sunburstSize * 2)
            );

            Color sunburstColor = Color.White * (1f - Projectile.alpha / 255f) * 0.8f;
            spriteBatch.Draw(texture, sunburstRect, null, sunburstColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        return false;
    }

    public override bool PreKill(int timeLeft)
    {
        if (initialized)
        {
            Projectile.position = basePosition;

            // Create circular vector dust cloud
            for (int i = 0; i < 16; i++)
            {
                Vector2 dustVelocity = Main.rand.NextVector2Circular(16f, 16f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, dustVelocity);
                dust.scale = Main.rand.NextFloat(1f, 1.8f);
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Item.NewItem(Projectile.GetSource_Death(), (int)Projectile.Center.X, (int)Projectile.Center.Y,
                            0, 0, ModContent.ItemType<ArchiverChronicleI>(), 1);
            }
        }
        SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, Main.LocalPlayer.position);
        return false;
    }
}