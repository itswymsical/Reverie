// Super simple code but I'm still crediting:
// https://github.com/GabeHasWon/SpiritReforged/blob/master/Content/Savanna/Items/SavannaGrassSeeds.cs

using System.Collections.Generic;
using System.Linq;

namespace Reverie.Content.Tiles.Taiga;

public class TaigaGrassSeedsItem : ModItem
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

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            var tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
            if (tile.HasTile && tile.TileType == ModContent.TileType<PeatTile>() && player.IsTargetTileInItemRange(Item))
            {
                WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, ModContent.TileType<TaigaGrassTile>(), forced: true);

                if (Main.netMode != NetmodeID.SinglePlayer)
                    NetMessage.SendTileSquare(player.whoAmI, Player.tileTargetX, Player.tileTargetY);

                return true;
            }
        }

        return null;
    }
}