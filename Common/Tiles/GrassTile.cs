// Credits to Spirit: Reforged, by Team Spirit
// https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/TileCommon/PresetTiles/GrassTile.cs

using Terraria.ObjectData;
using System.Linq;

namespace Reverie.Common.Tiles;

public abstract class GrassTile : ModTile
{
    protected virtual int DirtType => TileID.Dirt;

    protected void AllowAnchor(params int[] types)
    {
        foreach (var type in types)
        {
            var data = TileObjectData.GetTileData(type, 0);
            if (data != null)
                data.AnchorValidTiles = data.AnchorValidTiles.Concat([Type]).ToArray();
        }
    }

    /// <summary>
    /// <inheritdoc/><para/>Also automatically controls common grass tile settings.
    /// </summary>
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBlendAll[Type] = true;

        Merge(DirtType, TileID.Grass);
        AllowAnchor(TileID.Sunflower);

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = DirtType;
        TileID.Sets.NeedsGrassFraming[Type] = true;
    }

    public override bool CanExplode(int i, int j)
    {
        WorldGen.KillTile(i, j, false, false, true); //Makes the tile completely go away instead of reverting to dirt
        return true;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly) //Change self into dirt
        {
            fail = true;
            Framing.GetTileSafely(i, j).TileType = (ushort)DirtType;
        }
    }
    public void Merge(params int[] otherIds)
    {
        foreach (int id in otherIds)
        {
            Main.tileMerge[Type][id] = true;
            Main.tileMerge[id][Type] = true;
        }
    }
}