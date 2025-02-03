using Reverie.Content.Tiles;

namespace Reverie.Content.Items.Canopy;

public class LoamItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.DefaultToPlaceableTile(ModContent.TileType<LoamTile>());
        
        Item.rare = ItemRarityID.Blue;
    }
}