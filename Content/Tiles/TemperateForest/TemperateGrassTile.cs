using Reverie.Common.Tiles;

namespace Reverie.Content.Tiles.TemperateForest;

public class TemperateGrassTile : GrassTile
{
    protected override int DirtType => TileID.Dirt;
    //public override List<int> PlantTypes => [ModContent.TileType<TaigaPlants>()];
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.Stone, TileID.ClayBlock, Type, TileID.Grass);
        Main.tileBlockLight[Type] = true;

        DustType = DustID.GrassBlades;
        HitSound = SoundID.Dig;
        VanillaFallbackOnModDeletion = TileID.Grass;

        AddMapEntry(new Color(34, 170, 94));
    }
}