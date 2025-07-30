using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Desert;

public class DeadBushTile : ModTile
{
    public override void SetStaticDefaults()
    {
        const int height = 34;

        Main.tileSolid[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);

        TileObjectData.newTile.CoordinateWidth = 32;
        TileObjectData.newTile.CoordinateHeights = [height];

        TileObjectData.newTile.DrawYOffset = -(height - 20);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.AnchorValidTiles = [TileID.Sand, TileID.HardenedSand];
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(118, 102, 97));
        DustType = DustID.Hay;
        HitSound = SoundID.Grass;

    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 3 : 6;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 1)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }
}