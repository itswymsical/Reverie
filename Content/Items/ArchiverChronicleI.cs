using Terraria.GameContent;

namespace Reverie.Content.Items;

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
    public override string Texture => "Reverie/Assets/Textures/Items/ArchiverChronicle";
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

            Vector2 basePosition = new Vector2(line.X, line.Y);

            float time = Main.GlobalTimeWrappedHourly * 3f;
            float amplitude = 2.03f;
            Color rarityColor = line.OverrideColor.GetValueOrDefault(line.Color);

            string text = line.Text;
            Vector2 fullTextSize = FontAssets.MouseText.Value.MeasureString(text);
            float totalTextWidth = fullTextSize.X * line.BaseScale.X;
            float textHeight = fullTextSize.Y * line.BaseScale.Y;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);

            Texture2D starTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star04").Value;
            Vector2 starOrigin = starTexture.Size() * 0.5f;

            int starCount = 8;
            for (int f = 0; f < starCount; f++)
            {
                float starTime = time + f * 1.3f;
                float starSpeed = 0.4f + f * 0.1f;

                float angle = starTime * starSpeed + f * MathHelper.TwoPi / starCount;
                float radius = 25f + (float)Math.Sin(starTime * 0.7f + f) * 8f;

                Vector2 starOffset = new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius * 0.6f // Flatten Y movement
                );

                Vector2 starPos = basePosition + new Vector2(totalTextWidth * 0.5f, textHeight * 0.5f) + starOffset;

                float starScale = (0.3f + (float)Math.Sin(starTime * 1.2f + f) * 0.15f) * line.BaseScale.X;
                float starRotation = starTime * 0.5f + f * 0.8f;
                float flameAlpha = 0.6f + (float)Math.Sin(starTime * 1.5f + f * 0.7f) * 0.3f;

                Color starColor = Color.Lerp(Color.LightGoldenrodYellow, Color.Yellow, (float)Math.Sin(starTime + f) * 0.5f + 0.5f);
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

            int smallStarCount = 6;
            for (int f = 0; f < smallStarCount; f++)
            {
                float starTime = time * 1.8f + f * 1.5f;
                float flameSpeed = 1.2f + f * 0.15f;

                float angle = starTime * flameSpeed + f * MathHelper.TwoPi / smallStarCount;
                float radius = 15f + (float)Math.Sin(starTime * 1.1f + f) * 5f;

                Vector2 starOffset = new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius * 0.7f
                );

                Vector2 starPos = basePosition + new Vector2(totalTextWidth * 0.5f, textHeight * 0.5f) + starOffset;

                float starScale = (0.15f + (float)Math.Sin(starTime * 1.8f + f) * 0.08f) * line.BaseScale.X;
                float starRotation = starTime * 0.8f + f * 1.2f;
                float starAlpha = 0.4f + (float)Math.Sin(starTime * 2f + f * 0.9f) * 0.2f;

                Color starColor = Color.Lerp(Color.Yellow, Color.Orange, (float)Math.Sin(starTime + f) * 0.5f + 0.5f);
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

            float posX = basePosition.X;
            for (int i = 0; i < text.Length; i++)
            {
                float sineOffset = (float)Math.Sin(time + i * 0.5f) * amplitude;
                Vector2 charPos = new Vector2(posX, basePosition.Y + sineOffset);
                string charStr = text[i].ToString();
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(charStr);
                float charWidth = textSize.X * line.BaseScale.X;

                Color glowColor = Color.Lerp(rarityColor, Color.Gray, (float)Math.Sin(time + i * 0.2f) * 0.5f + 0.5f);

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
            for (int i = 0; i < text.Length; i++)
            {
                float sineOffset = (float)Math.Sin(time + i * 0.5f) * amplitude;
                Vector2 charPos = new Vector2(posX, basePosition.Y + sineOffset);
                string charStr = text[i].ToString();
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(charStr);
                float charWidth = textSize.X * line.BaseScale.X;

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
