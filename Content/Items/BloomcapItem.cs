using Reverie.Content.Tiles;

namespace Reverie.Content.Items;

public class BloomcapItem : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = Item.height = 24;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = Item.CommonMaxStack;
        Item.questItem = true;
    }
}
