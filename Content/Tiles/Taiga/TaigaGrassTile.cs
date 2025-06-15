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
        VanillaFallbackOnModDeletion = TileID.Grass;

        AddMapEntry(new Color(88, 150, 112));
    }
    public override void RandomUpdate(int i, int j)
    {
        if (!Main.rand.NextBool(6)) return;

        int[] directions = { -1, 1 };
        foreach (int xDir in directions)
        {
            int x = i + xDir;
            if (x < 0 || x >= Main.maxTilesX) continue;

            foreach (int yDir in directions)
            {
                int y = j + yDir;
                if (y < 0 || y >= Main.maxTilesY) continue;

                Tile tile = Main.tile[x, y];
                if (tile.HasTile && (tile.TileType == Type || tile.TileType == DirtType))
                {
                    if (!Main.tile[x, y - 1].HasTile || !Main.tileSolid[Main.tile[x, y - 1].TileType])
                    {
                        tile.TileType = (ushort)ModContent.TileType<SnowTaigaGrassTile>();

                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, x, y, 1);
                    }
                }
            }
        }
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
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly)
        {
            fail = true;
            Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<TaigaGrassTile>();
        }
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

        VanillaFallbackOnModDeletion = TileID.CorruptGrass;

        TileID.Sets.CorruptBiome[Type] = Type;
        AddMapEntry(new Color(200, 199, 215));
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.CorruptPlants;
    }
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly)
        {
            fail = true;
            Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<TaigaGrassTile>();
        }
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

        VanillaFallbackOnModDeletion = TileID.CrimsonGrass;

        TileID.Sets.CrimsonBiome[Type] = Type;
        AddMapEntry(new Color(215, 199, 201));
    }

    public override bool HasWalkDust()
    {
        return true;
    }

    public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
    {
        dustType = DustID.CrimsonPlants;
    }
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly)
        {
            fail = true;
            Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<TaigaGrassTile>();
        }
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

        VanillaFallbackOnModDeletion = TileID.HallowedGrass;

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
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly)
        {
            fail = true;
            Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<TaigaGrassTile>();
        }
    }
}