using Terraria.GameContent;

namespace Reverie.Content.Items.Misc;

public class ArchiverChronicleI : ArchiverChronicle
{
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
}

public abstract class ArchiverChronicle : ModItem
{
    public override string Texture => $"{TEXTURE_DIRECTORY}Items/Misc/ArchiverChronicle";
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Name == "ItemName" && line.Mod == "Terraria")
        {

            var basePosition = new Vector2(line.X, line.Y);

            var time = Main.GlobalTimeWrappedHourly * 3f;
            var amplitude = 2.03f;
            var rarityColor = line.OverrideColor.GetValueOrDefault(line.Color);

            var text = line.Text;
            var fullTextSize = FontAssets.MouseText.Value.MeasureString(text);
            var totalTextWidth = fullTextSize.X * line.BaseScale.X;
            var textHeight = fullTextSize.Y * line.BaseScale.Y;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);

            var starTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star04").Value;
            var starOrigin = starTexture.Size() * 0.5f;

            var starCount = 8;
            for (var f = 0; f < starCount; f++)
            {
                var starTime = time + f * 1.3f;
                var starSpeed = 0.4f + f * 0.1f;

                var angle = starTime * starSpeed + f * MathHelper.TwoPi / starCount;
                var radius = 25f + (float)Math.Sin(starTime * 0.7f + f) * 8f;

                var starOffset = new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius * 0.6f // Flatten Y movement
                );

                var starPos = basePosition + new Vector2(totalTextWidth * 0.5f, textHeight * 0.5f) + starOffset;

                var starScale = (0.3f + (float)Math.Sin(starTime * 1.2f + f) * 0.15f) * line.BaseScale.X;
                var starRotation = starTime * 0.5f + f * 0.8f;
                var flameAlpha = 0.6f + (float)Math.Sin(starTime * 1.5f + f * 0.7f) * 0.3f;

                var starColor = Color.Lerp(Color.LightGoldenrodYellow, Color.Yellow, (float)Math.Sin(starTime + f) * 0.5f + 0.5f);
                starColor *= flameAlpha;

                Main.spriteBatch.Draw(
                    starTexture,
                    starPos,
                    null,
                    starColor,
                    starRotation,
                    starOrigin,
                    starScale,
                    SpriteEffects.None,
                    0f
                );
            }

            var smallStarCount = 6;
            for (var f = 0; f < smallStarCount; f++)
            {
                var starTime = time * 1.8f + f * 1.5f;
                var flameSpeed = 1.2f + f * 0.15f;

                var angle = starTime * flameSpeed + f * MathHelper.TwoPi / smallStarCount;
                var radius = 15f + (float)Math.Sin(starTime * 1.1f + f) * 5f;

                var starOffset = new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius * 0.7f
                );

                var starPos = basePosition + new Vector2(totalTextWidth * 0.5f, textHeight * 0.5f) + starOffset;

                var starScale = (0.15f + (float)Math.Sin(starTime * 1.8f + f) * 0.08f) * line.BaseScale.X;
                var starRotation = starTime * 0.8f + f * 1.2f;
                var starAlpha = 0.4f + (float)Math.Sin(starTime * 2f + f * 0.9f) * 0.2f;

                var starColor = Color.Lerp(Color.Yellow, Color.Orange, (float)Math.Sin(starTime + f) * 0.5f + 0.5f);
                starColor *= starAlpha;

                Main.spriteBatch.Draw(
                    starTexture,
                    starPos,
                    null,
                    starColor,
                    starRotation,
                    starOrigin,
                    starScale,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);

            var posX = basePosition.X;
            for (var i = 0; i < text.Length; i++)
            {
                var sineOffset = (float)Math.Sin(time + i * 0.5f) * amplitude;
                var charPos = new Vector2(posX, basePosition.Y + sineOffset);
                var charStr = text[i].ToString();
                var textSize = FontAssets.MouseText.Value.MeasureString(charStr);
                var charWidth = textSize.X * line.BaseScale.X;

                var glowColor = Color.Lerp(rarityColor, Color.Gray, (float)Math.Sin(time + i * 0.2f) * 0.5f + 0.5f);

                Utils.DrawBorderString(
                    Main.spriteBatch,
                    charStr,
                    charPos,
                    glowColor,
                    line.BaseScale.X * 1.1f);

                posX += charWidth;
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.UIScaleMatrix);

            posX = basePosition.X;
            for (var i = 0; i < text.Length; i++)
            {
                var sineOffset = (float)Math.Sin(time + i * 0.5f) * amplitude;
                var charPos = new Vector2(posX, basePosition.Y + sineOffset);
                var charStr = text[i].ToString();
                var textSize = FontAssets.MouseText.Value.MeasureString(charStr);
                var charWidth = textSize.X * line.BaseScale.X;

                Utils.DrawBorderString(
                    Main.spriteBatch,
                    charStr,
                    charPos,
                    rarityColor,
                    line.BaseScale.X);

                posX += charWidth;
            }

            return false;
        }
        return true;
    }
}
