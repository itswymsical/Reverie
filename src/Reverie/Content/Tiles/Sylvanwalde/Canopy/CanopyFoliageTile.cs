using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Sylvanwalde.Canopy;

public class CanopyFoliageTile : ModTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);

        TileObjectData.newTile.AnchorValidTiles =
        [
            ModContent.TileType<WoodgrassTile>()
        ];

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;

        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newTile.CoordinateHeights = [18];

        TileObjectData.addTile(Type);

        DustType = DustID.JunglePlants;
        HitSound = SoundID.Grass;

        AddMapEntry(new Color(100, 190, 8));
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        base.SetSpriteEffects(i, j, ref spriteEffects);

        spriteEffects = i % 2 == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
    }
}