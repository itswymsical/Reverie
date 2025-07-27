using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Taiga;

public class WinterberryBushTile : ModTile
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
            ModContent.TileType<TaigaGrassTile>(),
            ModContent.TileType<SnowTaigaGrassTile>(),
        ];

        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.WaterDeath = false;

        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newTile.CoordinateWidth = 32;
        TileObjectData.newTile.CoordinateHeights = [34];
        TileObjectData.newTile.DrawYOffset = (-14);
        //TileObjectData.newTile.RandomStyleRange = 12;
        TileObjectData.addTile(Type);

        DustType = DustID.GrassBlades;
        HitSound = SoundID.Grass;

        AddMapEntry(new Color(48, 116, 58));
    }
}
