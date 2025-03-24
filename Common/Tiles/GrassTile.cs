using System.Collections.Generic;

namespace Reverie.Common.Tiles;

public abstract class GrassTile : ModTile
{
    /// <summary>
    /// The tile type that this grass turns into when killed.
    /// </summary>
    public virtual int SoilType => TileID.Dirt;

    /// <summary>
    /// Chance for the grass to spread in a random update. Lower values mean higher chance.
    /// </summary>
    public virtual int spreadRate => 2;

    /// <summary>
    /// Whether plants can grow on this grass.
    /// </summary>
    public virtual bool CanGrowPlants => true;

    /// <summary>
    /// The plant types that can grow on this grass.
    /// </summary>
    public virtual List<int> PlantTypes => new List<int> { TileID.Plants };

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBrick[Type] = true;
        Main.tileBlockLight[Type] = true;

        Main.tileMergeDirt[Type] = true;
        Main.tileMerge[Type][SoilType] = true;
        Main.tileMerge[SoilType][Type] = true;

        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[SoilType] = Type;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.GrassSpecial[Type] = false;
        TileID.Sets.ChecksForMerge[Type] = true;
    }

    public override void RandomUpdate(int i, int j)
    {
        if (!Main.rand.NextBool(spreadRate)) return;

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
                if (tile.HasTile && tile.TileType == SoilType)
                {
                    if (!Main.tile[x, y - 1].HasTile || !Main.tileSolid[Main.tile[x, y - 1].TileType])
                    {
                        tile.TileType = (ushort)Type;

                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, x, y, 1);
                    }
                }
            }
        }

        if (CanGrowPlants && !Main.tile[i, j - 1].HasTile && Main.rand.NextBool(15))
        {
            int plantType = PlantTypes[Main.rand.Next(PlantTypes.Count)];
            WorldGen.PlaceTile(i, j - 1, plantType, true);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, i, j - 1, 1);
        }
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail || effectOnly) return;

        Main.tile[i, j].TileType = (ushort)SoilType;

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendTileSquare(-1, i, j, 1);
    }
}
