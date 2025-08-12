using Reverie.Core.Missions.SystemClasses;

namespace Reverie.Content.Items.Mycology;

public class BloomcapItem : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = Item.height = 24;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = Item.CommonMaxStack;
    }
}
