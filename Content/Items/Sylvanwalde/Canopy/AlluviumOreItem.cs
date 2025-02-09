namespace Reverie.Content.Items.Sylvanwalde.Canopy;

public class AlluviumOreItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        //Item.DefaultToPlaceableTile(ModContent.TileType<AlluviumOreTile>());
        
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(silver: 2);
    }
}