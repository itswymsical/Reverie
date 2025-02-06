using Reverie.Content.Tiles.Sylvanwalde.Canopy;

namespace Reverie.Content.Items.Sylvanwalde.Canopy;

public class WoodgrassSeedsItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.DefaultToPlaceableTile(ModContent.TileType<WoodgrassTile>());
        
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(silver: 2);
    }
}