using Reverie.Common.Tiles;
using System.Collections.Generic;

namespace Reverie.Content.Tiles.Taiga;

public class TaigaGrassTile : GrassTile
{
    protected override int DirtType => ModContent.TileType<PeatTile>();
    public override List<int> PlantTypes => [ModContent.TileType<TaigaPlants>()];
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.IceBlock, TileID.SnowBlock, Type, ModContent.TileType<SnowTaigaGrassTile>());
        Main.tileBlockLight[Type] = true;

        DustType = DustID.Mud;
        HitSound = SoundID.Dig;
        TileID.Sets.SnowBiome[Type] = Type;
        AddMapEntry(new Color(88, 150, 112));
    }
}

public class SnowTaigaGrassTile : GrassTile
{
    protected override int DirtType => ModContent.TileType<PeatTile>();
    public override List<int> PlantTypes => [ModContent.TileType<SnowTaigaPlants>()];
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.IceBlock, TileID.SnowBlock, Type, ModContent.TileType<TaigaGrassTile>());
        Main.tileBlockLight[Type] = true;

        DustType = DustID.Mud;
        HitSound = SoundID.Dig;

        VanillaFallbackOnModDeletion = TileID.SnowBlock;

        TileID.Sets.SnowBiome[Type] = Type;
        TileID.Sets.Snow[Type] = true;
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

public class CorruptTaigaGrassTile : GrassTile
{
    protected override int DirtType => ModContent.TileType<PeatTile>();
    public override List<int> PlantTypes => [ModContent.TileType<CorruptTaigaPlants>()];
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.IceBlock, TileID.SnowBlock, Type, ModContent.TileType<TaigaGrassTile>());
        Main.tileBlockLight[Type] = true;

        DustType = DustID.CorruptPlants;
        HitSound = SoundID.Dig;

        VanillaFallbackOnModDeletion = TileID.SnowBlock;

        TileID.Sets.CorruptBiome[Type] = Type;
        AddMapEntry(new Color(109, 106, 174));
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.CorruptPlants;
    }
}

public class CrimsonTaigaGrassTile : GrassTile
{
    protected override int DirtType => ModContent.TileType<PeatTile>();
    public override List<int> PlantTypes => [ModContent.TileType<CrimsonTaigaPlants>()];
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.IceBlock, TileID.SnowBlock, Type, ModContent.TileType<TaigaGrassTile>());
        Main.tileBlockLight[Type] = true;

        DustType = DustID.CrimsonPlants;
        HitSound = SoundID.Dig;

        VanillaFallbackOnModDeletion = TileID.SnowBlock;

        TileID.Sets.CrimsonBiome[Type] = Type;
        AddMapEntry(new Color(208, 80, 80));
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.CrimsonPlants;
    }
}

public class HallowTaigaGrassTile : GrassTile
{
    protected override int DirtType => ModContent.TileType<PeatTile>();
    public override List<int> PlantTypes => [ModContent.TileType<HallowTaigaPlants>()];
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Merge(TileID.IceBlock, TileID.SnowBlock, Type, ModContent.TileType<TaigaGrassTile>());
        Main.tileBlockLight[Type] = true;

        DustType = DustID.HallowedPlants;
        HitSound = SoundID.Dig;

        VanillaFallbackOnModDeletion = TileID.SnowBlock;

        TileID.Sets.HallowBiome[Type] = Type;
        AddMapEntry(new Color(77, 153, 191));
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.HallowedPlants;
    }
}