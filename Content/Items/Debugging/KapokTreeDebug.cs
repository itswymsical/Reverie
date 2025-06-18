using Reverie.Content.Tiles.Rainforest.Surface;
using Reverie.Content.Tiles.Rainforest.Surface.Trees;
using System.Collections.Generic;
using Terraria.Audio;

namespace Reverie.Content.Items.Debugging;

public class KapokTreeDebugWand : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.rare = ItemRarityID.Expert;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
    }

    public override bool? UseItem(Player player)
    {
        var mouseX = (int)(Main.MouseWorld.X / 16);
        var mouseY = (int)(Main.MouseWorld.Y / 16);

        if (PlaceKapokTree(mouseX, mouseY))
        {
            Main.NewText("Kapok tree placed!", Color.Green);
            SoundEngine.PlaySound(SoundID.Grass, new Vector2(mouseX * 16, mouseY * 16));
        }
        else
        {
            Main.NewText("Failed to place tree", Color.Red);
        }

        return true;
    }

    private bool PlaceKapokTree(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return false;

        // Clear area above
        for (var clearY = y - 30; clearY < y; clearY++)
        {
            for (var clearX = x - 1; clearX <= x + 1; clearX++)
            {
                if (WorldGen.InWorld(clearX, clearY))
                {
                    WorldGen.KillTile(clearX, clearY, noItem: true);
                }
            }
        }

        // Place ground
        WorldGen.PlaceTile(x, y + 1, ModContent.TileType<CanopyGrassTile>(), mute: true, forced: true);
        WorldGen.PlaceTile(x + 1, y + 1, ModContent.TileType<CanopyGrassTile>(), mute: true, forced: true);

        // Place sapling and grow it
        if (WorldGen.PlaceTile(x, y, ModContent.TileType<KapokSapling>(), mute: true, forced: true))
        {
            return CustomTree.GrowTree<KapokTree>(x, y);
        }

        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "Usage", "Click to place Kapok tree at cursor"));
    }
}