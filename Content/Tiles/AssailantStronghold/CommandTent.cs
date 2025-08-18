using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Reverie.Content.Tiles.AssailantStronghold;

public class CommandTent : ModTile
{
    //public override string Texture => PLACEHOLDER;
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileHammer[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 10;
        TileObjectData.newTile.Height = 6;

        TileObjectData.newTile.Origin = new Point16(5, 5);
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16];

        TileObjectData.newTile.UsesCustomCanPlace = true;
        AddMapEntry(new Color(196, 117, 190), CreateMapEntryName());
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;

        TileObjectData.addTile(Type);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

        Tile tile = Framing.GetTileSafely(i, j);
        var lightColor = Lighting.GetColor(i, j);

        if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
            return false;

        var tentTexture = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/CommandTent_Outer").Value;

        int tileWidth = 10 * 16;
        int tileHeight = 6 * 16;

        Vector2 textureOrigin = new Vector2(tentTexture.Width / 2f, tentTexture.Height / 2f);

        Vector2 worldCenter = new Vector2(i * 16 + tileWidth / 2f, j * 16 + tileHeight / 2f);

        Vector2 drawPos = worldCenter - Main.screenPosition + zero;

        bool flipped = (i % 2 == 1);
        SpriteEffects tentEffect = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        var leftPole = ModContent.Request<Texture2D>(
            $"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/CommandTent_RopeL").Value;
        var rightPole = ModContent.Request<Texture2D>(
            $"{TEXTURE_DIRECTORY}Tiles/AssailantStronghold/CommandTent_RopeR").Value;

        Vector2 poleOrigin = new Vector2(leftPole.Width / 2f, leftPole.Height / 2f);

        Vector2 leftOffset = new Vector2(-tentTexture.Width / 2f - 8, tentTexture.Height / 2f - leftPole.Height / 2f + 2);
        Vector2 rightOffset = new Vector2(tentTexture.Width / 2f - 8, tentTexture.Height / 2f - rightPole.Height / 2f + 2);
        if (flipped)
        {
            rightOffset = new Vector2(-tentTexture.Width / 2f + 4, tentTexture.Height / 2f - leftPole.Height / 2f + 2);
            leftOffset = new Vector2(tentTexture.Width / 2f + 8, tentTexture.Height / 2f - rightPole.Height / 2f + 2);
        }    
        spriteBatch.Draw(rightPole, drawPos + rightOffset, null, lightColor, 0f, poleOrigin, 1f, tentEffect, 0f);

        spriteBatch.Draw(tentTexture, new Vector2(drawPos.X, drawPos.Y + 2), null, lightColor, 0f, textureOrigin, 1f, tentEffect, 0f);

        spriteBatch.Draw(leftPole, drawPos + leftOffset, null, lightColor, 0f, poleOrigin, 1f, tentEffect, 0f);
        return false;
    }
}