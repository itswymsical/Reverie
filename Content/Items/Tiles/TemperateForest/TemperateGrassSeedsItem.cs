// Super simple code but I'm still crediting:
// https://github.com/GabeHasWon/SpiritReforged/blob/master/Content/Savanna/Items/SavannaGrassSeeds.cs

using Reverie.Content.Tiles.TemperateForest;
using System.Collections.Generic;
using System.Linq;

namespace Reverie.Content.Items.Tiles.TemperateForest;

public class TemperateGrassSeedsItem : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = Item.height = 18;

        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.maxStack = Item.CommonMaxStack;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.value = Item.sellPrice(copper: 0);
        Item.useTurn = Item.autoReuse = Item.consumable = true;

        Item.rare = ItemRarityID.White;
    }
    public override void HoldItem(Player player)
    {
        if (player.IsTargetTileInItemRange(Item))
        {
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = Type;
        }
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var placeable = tooltips.FirstOrDefault(x => x.Name == "Placeable");
        var consumable = tooltips.FirstOrDefault(x => x.Name == "Consumable");
        if (consumable != null)
            tooltips.Remove(consumable);

        tooltips.Add(placeable);
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            var tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
            if (tile.HasTile && tile.TileType == TileID.Dirt && player.IsTargetTileInItemRange(Item))
            {
                WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, ModContent.TileType<TemperateGrassTile>(), forced: true);

                if (Main.netMode != NetmodeID.SinglePlayer)
                    NetMessage.SendTileSquare(player.whoAmI, Player.tileTargetX, Player.tileTargetY);

                return true;
            }
        }

        return null;
    }
}