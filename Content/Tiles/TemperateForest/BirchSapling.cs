using Reverie.Common.Tiles;
using Reverie.Content.Dusts;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.TemperateForest;

public class BirchSapling : SaplingTile
{
    public override void PreAddObjectData() => TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<TemperateGrassTile>()];

}
