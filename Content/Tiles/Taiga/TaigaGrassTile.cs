using Reverie.Common.Tiles;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Taiga;

public class TaigaGrassTile : GrassTile
{
    public override int SoilType => ModContent.TileType<PeatTile>();
    public override int spreadChance => 8;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[TileID.SnowBlock][Type] = true;
        Main.tileMerge[Type][TileID.SnowBlock] = true;

        Main.tileMerge[TileID.IceBlock][Type] = true;
        Main.tileMerge[Type][TileID.IceBlock] = true;

        Main.tileMerge[SoilType][Type] = true;
        Main.tileMerge[Type][SoilType] = true;
        Main.tileBlockLight[Type] = true;

        DustType = DustID.Mud;
        HitSound = SoundID.Dig;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.SnowBiome[Type] = Type;
        AddMapEntry(new Color(88, 150, 112));
    }
}

public class TaigaSnowGrassTile : GrassTile
{
    public override int SoilType => ModContent.TileType<PeatTile>();
    public override int spreadChance => 8;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;

        Main.tileMergeDirt[Type] = true;

        Main.tileMerge[TileID.SnowBlock][Type] = true;
        Main.tileMerge[Type][TileID.SnowBlock] = true;

        Main.tileMerge[ModContent.TileType<TaigaGrassTile>()][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<TaigaGrassTile>()] = true;

        Main.tileMerge[TileID.IceBlock][Type] = true;
        Main.tileMerge[Type][TileID.IceBlock] = true;

        Main.tileMerge[SoilType][Type] = true;
        Main.tileMerge[Type][SoilType] = true;
        Main.tileBlockLight[Type] = true;

        DustType = DustID.Mud;
        HitSound = SoundID.Dig;
        TileID.Sets.SnowBiome[Type] = Type;
        TileID.Sets.Snow[Type] = true;
        VanillaFallbackOnModDeletion = TileID.SnowBlock;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        AddMapEntry(new Color(190, 223, 232));
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.Snow;
    }
}