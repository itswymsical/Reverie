using Reverie.Common.Players;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace Reverie.Core.Graphics;

//public class AuraLayer : PlayerDrawLayer
//{
//    public override Position GetDefaultPosition() =>
//        new AfterParent(PlayerDrawLayers.BackAcc);

//    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
//        true;
//    private Texture2D auraTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;
//    protected override void Draw(ref PlayerDrawSet drawInfo)
//    {
//        var player = drawInfo.drawPlayer;

//        var drawPosition = player.Center - Main.screenPosition;
//        if (auraTexture == null)
//            auraTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;

//        DrawReverieAura(drawInfo, drawPosition, 1.0f);
//        SpawnReverieAuraDust(player);
//    }
//    public override void Load()
//    {
//    }

//    private void DrawReverieAura(PlayerDrawSet drawInfo, Vector2 centerPosition, float animationTimer)
//    {
//        auraTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;
//        float pulseValue = (float)Math.Sin(Main.GameUpdateCount * 0.025f);
//        float baseScale = 0.7f + pulseValue * 0.3f;
//        float opacity = animationTimer * 1f;
//        Color baseGlowColor = new Color(37, 93, 185) * opacity * 0.6f;

//        // Add the base glow to the drawing cache
//        DrawData baseGlow = new DrawData(
//            auraTexture,
//            centerPosition,
//            null,
//            baseGlowColor,
//            0f,
//            new Vector2(auraTexture.Width / 2, auraTexture.Height / 2),
//            baseScale,
//            SpriteEffects.None,
//            0
//        );

//        baseGlow.shader = 6;

//        drawInfo.DrawDataCache.Add(baseGlow);

//        int numberOfElements = 8;
//        float elementScale = 0.8f + pulseValue * 0.03f;
//        Color elementColor = new Color(37, 93, 185) * opacity;

//        for (int i = 0; i < numberOfElements; i++)
//        {
//            float angle = i * (MathHelper.TwoPi / numberOfElements) + Main.GameUpdateCount * 0.02f;
//            float distance = 15.0f + pulseValue * 1.0f;

//            Vector2 offset = new Vector2(
//                (float)Math.Cos(angle) * distance,
//                (float)Math.Sin(angle) * distance
//            );

//            DrawData orbitElement = new DrawData(
//                auraTexture,
//                centerPosition + offset,
//                null,
//                elementColor,
//                angle,
//                new Vector2(auraTexture.Width / 2, auraTexture.Height / 2),
//                elementScale,
//                SpriteEffects.None,
//                0
//            );
//            DrawData orbitElement2 = new DrawData(
//                auraTexture,
//                centerPosition + offset,
//                null,
//                elementColor,
//                angle,
//                new Vector2(auraTexture.Width / 2, auraTexture.Height / 2),
//                elementScale,
//                SpriteEffects.None,
//                0
//            );
//            orbitElement.shader = 2;
//            orbitElement2.shader = 7;
//            drawInfo.DrawDataCache.Add(orbitElement);
//            drawInfo.DrawDataCache.Add(orbitElement);
//            drawInfo.DrawDataCache.Add(orbitElement2);
//        }
//    }

//    private void SpawnReverieAuraDust(Player player)
//    {
//        if (Main.netMode == NetmodeID.Server)
//            return;


//        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
//        float distance = Main.rand.NextFloat(10f, 30f);

//        Vector2 dustPos = player.Center + new Vector2(
//            (float)Math.Cos(angle) * distance,
//            (float)Math.Sin(angle) * distance
//        );

//        Dust dust = Dust.NewDustPerfect(
//            dustPos - new Vector2(4, 4),
//            DustID.BlueCrystalShard,
//            new(0f, -1.3f),
//            0,
//            new Color(7, 32, 99) * 0.7f,
//            0.76f
//        );

//        dust.shader = GameShaders.Armor.GetShaderFromItemId(7);
//        dust.noGravity = true;
//        dust.noLightEmittence = false;
//    }

//    private void DrawApathiaAura(PlayerDrawSet drawInfo, Vector2 centerPosition, float animationTimer)
//    {
//        auraTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Glow").Value;
//        float pulseValue = (float)Math.Sin(Main.GameUpdateCount * 0.025f);
//        float baseScale = 0.7f + pulseValue * 0.3f;
//        float opacity = animationTimer * 1f;
//        Color baseGlowColor = new Color(163, 23, 255) * opacity * 0.6f;

//        // Add the base glow to the drawing cache
//        DrawData baseGlow = new DrawData(
//            auraTexture,
//            centerPosition,
//            null,
//            baseGlowColor,
//            0f,
//            new Vector2(auraTexture.Width / 2, auraTexture.Height / 2),
//            baseScale,
//            SpriteEffects.None,
//            0
//        );

//        baseGlow.shader = 6;

//        drawInfo.DrawDataCache.Add(baseGlow);

//        int numberOfElements = 8;
//        float elementScale = 0.8f + pulseValue * 0.03f;
//        Color elementColor = new Color(93, 6, 255) * opacity;

//        for (int i = 0; i < numberOfElements; i++)
//        {
//            float angle = i * (MathHelper.TwoPi / numberOfElements) + Main.GameUpdateCount * 0.02f;
//            float distance = 15.0f + pulseValue * 1.0f;

//            Vector2 offset = new Vector2(
//                (float)Math.Cos(angle) * distance,
//                (float)Math.Sin(angle) * distance
//            );

//            DrawData orbitElement = new DrawData(
//                auraTexture,
//                centerPosition + offset,
//                null,
//                elementColor,
//                angle,
//                new Vector2(auraTexture.Width / 2, auraTexture.Height / 2),
//                elementScale,
//                SpriteEffects.None,
//                0
//            );
//            DrawData orbitElement2 = new DrawData(
//                auraTexture,
//                centerPosition + offset,
//                null,
//                elementColor,
//                angle,
//                new Vector2(auraTexture.Width / 2, auraTexture.Height / 2),
//                elementScale,
//                SpriteEffects.None,
//                0
//            );
//            orbitElement.shader = 10;
//            orbitElement2.shader = 6;
//            drawInfo.DrawDataCache.Add(orbitElement);
//            drawInfo.DrawDataCache.Add(orbitElement2);
//            drawInfo.DrawDataCache.Add(orbitElement);
//        }
//    }

//    private void SpawnApathiaAuraDust(Player player)
//    {
//        if (Main.netMode == NetmodeID.Server)
//            return;


//        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
//        float distance = Main.rand.NextFloat(10f, 30f);

//        Vector2 dustPos = player.Center + new Vector2(
//            (float)Math.Cos(angle) * distance,
//            (float)Math.Sin(angle) * distance
//        );

//        Dust dust = Dust.NewDustPerfect(
//            dustPos - new Vector2(4, 4),
//            DustID.Shadowflame,
//            new(0f, -1.3f),
//            0,
//            new Color(12, 0, 50) * 0.7f,
//            0.76f
//        );

//        dust.noGravity = true;
//        dust.noLightEmittence = false;
//    }
//}