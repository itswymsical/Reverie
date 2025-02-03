using Reverie.Content.Tiles;

namespace Reverie.Content.Items.Canopy;

public class CobblestoneItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.DefaultToPlaceableTile(ModContent.TileType<CobblestoneTile>());
        
        Item.rare = ItemRarityID.Blue;
    }
}