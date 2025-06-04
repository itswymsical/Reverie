using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria.GameContent;
using Reverie.Content.Tiles;

namespace Reverie.Content.Items.Botany;

public class MagnoliaItem : ModItem
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.width = Item.height = 26;
        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(copper: 45);

        Item.maxStack = Item.CommonMaxStack;

        Item.consumable = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 19;

        Item.createTile = ModContent.TileType<MagnoliaTile>();
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Name == "ItemName")
        {
            var pos = new Vector2(line.X, line.Y);
            var text = line.Text;
            var textWidth = FontAssets.MouseText.Value.MeasureString(text).X;

            Main.spriteBatch.DrawString(
                FontAssets.MouseText.Value,
                text,
                pos,
                Color.White
            );

            var time = Main.GlobalTimeWrappedHourly;
            var frameWidth = 10;
            var frameHeight = 14;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);

            for (var i = 0; i < 4; i++)
            {
                var individualTime = time + i * 1.5f;
                var swayAmount = (float)Math.Sin(individualTime * 1.5f) * 15f;
                var verticalDrift = individualTime * 8f % 40f;
                var horizontalDrift = individualTime * 15f % textWidth;

                var offset = new Vector2(
                    swayAmount + horizontalDrift,
                    verticalDrift
                );

                var rotationSpeed = 0.8f;
                var rotation = individualTime * rotationSpeed + i * MathHelper.PiOver2;

                var particleColor = Color.PaleGoldenrod * (0.5f + (float)Math.Sin(time * 1.2f) * 0.3f);

                var frame = (int)(Main.GameUpdateCount / 6 + i) % 8;
                var sourceRect = new Rectangle(
                    0,
                    frameHeight * frame,
                    frameWidth,
                    frameHeight
                );

                Main.spriteBatch.Draw(
                    ModContent.Request<Texture2D>($"{VFX_DIRECTORY}MagnoliaLeaf").Value,
                    pos + offset,
                    sourceRect,
                    particleColor * 1.15f,
                    rotation,
                    new Vector2(frameWidth / 2f, frameHeight / 2f),
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.UIScaleMatrix);

            return false;
        }

        return base.PreDrawTooltipLine(line, ref yOffset);
    }
}
