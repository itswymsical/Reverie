using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;

namespace Reverie.Common.Items.Types;

public sealed class GemStaffGlobal : GlobalItem
{
    public override bool AppliesToEntity(Item item, bool lateInstantiation)
    {
        return item.type == ItemID.AmethystStaff || item.type == ItemID.TopazStaff 
            || item.type == ItemID.SapphireStaff || item.type == ItemID.EmeraldStaff 
            || item.type == ItemID.RubyStaff || item.type == ItemID.DiamondStaff
            || item.type == ItemID.AmberStaff;
    }
    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        //if (item.type != ItemID.AmethystStaff || item.type != ItemID.TopazStaff
        //    || item.type != ItemID.SapphireStaff || item.type != ItemID.EmeraldStaff
        //    || item.type != ItemID.RubyStaff || item.type != ItemID.DiamondStaff
        //    || item.type != ItemID.AmberStaff)
        //    return false;

        if (line.Name == "ItemName")
        {
            var pos = new Vector2(line.X, line.Y);
            var text = line.Text;
            var textWidth = FontAssets.MouseText.Value.MeasureString(text).X;

            var texture = TextureAssets.Item[ItemID.DirtBlock].Value;

            if (item.type == ItemID.AmethystStaff)
                texture = TextureAssets.Item[ItemID.Amethyst].Value;
            
            if (item.type == ItemID.TopazStaff)
                texture = TextureAssets.Item[ItemID.Topaz].Value;
            
            if (item.type == ItemID.SapphireStaff)
                texture = TextureAssets.Item[ItemID.Sapphire].Value;
            
            if (item.type == ItemID.EmeraldStaff)
                texture = TextureAssets.Item[ItemID.Emerald].Value;
            
            if (item.type == ItemID.RubyStaff)
                texture = TextureAssets.Item[ItemID.Ruby].Value;
            
            if (item.type == ItemID.DiamondStaff)
                texture = TextureAssets.Item[ItemID.Diamond].Value;
            
            if (item.type == ItemID.AmberStaff)
                texture = TextureAssets.Item[ItemID.Amber].Value;
            
            var time = Main.GlobalTimeWrappedHourly;

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

                Main.spriteBatch.Draw(
                    texture,
                    pos + offset,
                    null,
                    particleColor * 1.15f,
                    rotation,
                    texture.Size() * 0.5f,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.UIScaleMatrix);

            return true;
        }

        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }
}