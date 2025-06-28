using Reverie.Content.Tiles.Farming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reverie.Content.Items.Tiles.Farming;

public class SeedBedItem : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<SeedBedTile>());
    }
}
