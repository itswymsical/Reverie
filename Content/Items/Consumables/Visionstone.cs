using Reverie.Common.Players;
using System.Collections.Generic;
using Terraria.Audio;

namespace Reverie.Content.Items.Consumables;

public class VisionstoneItem : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 26;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useTurn = true;
        Item.UseSound = new SoundStyle($"{SFX_DIRECTORY}VisionstoneConsume");
        Item.consumable = true;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 1);
        Item.maxStack = 99;
    }

    public override bool CanUseItem(Player player)
    {
        var modPlayer = player.GetModPlayer<ExperiencePlayer>();
        return modPlayer.expCapacity < 20;
    }

    public override bool? UseItem(Player player)
    {
        var modPlayer = player.GetModPlayer<ExperiencePlayer>();

        if (modPlayer.expCapacity < 20)
        {
            modPlayer.IncreaseCapacity(1);
            return true;
        }

        return false;
    }
}