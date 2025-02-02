using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Missions.Items
{
    public class OldTradeReceipt : ModItem
    {
        public override string Texture => "Terraria/Images/Item_1315";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 99;
            Item.value = Item.sellPrice(silver: 0);
            Item.rare = ItemRarityID.Quest;
            Item.questItem = true;
        }
    }
}