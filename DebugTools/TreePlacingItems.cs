using Reverie.Content.Tiles.Canopy.Trees;
using Reverie.Content.Tiles.Taiga.Trees;
using Reverie.Content.Tiles.TemperateForest;
using System.Collections.Generic;
using System.Linq;

namespace Reverie.DebugItems;

public class SmallTanglewoodTreeDebugWand : ModItem
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
        if (Main.myPlayer != player.whoAmI)
            return true;

        var mouseWorld = Main.MouseWorld;
        var tileX = (int)(mouseWorld.X / 16f);
        var tileY = (int)(mouseWorld.Y / 16f);

        var success = false;

        // Try multiple times with slight position variations if first attempt fails
        for (var attempts = 0; attempts < 5 && !success; attempts++)
        {
            var tryX = tileX + Main.rand.Next(-1, 2);
            var tryY = tileY + attempts; // Try lower positions

            success = SmallTanglewoodTree.GrowTanglewoodTree(tryX, tryY);

            if (success)
            {
                // Success feedback
                Main.NewText($"Tanglewood tree grown at ({tryX}, {tryY}) after {attempts + 1} attempts", Color.Green);

                // Enhanced visual effect
                var effectPos = new Vector2(tryX, tryY) * 16;
                for (var i = 0; i < 30; i++) // Fewer particles for smaller tree
                {
                    var dust = Dust.NewDustDirect(effectPos, 16, 16, DustID.GrassBlades,
                        Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 1f));
                    dust.scale = 1.2f;
                    dust.fadeIn = 1.5f;
                }
                break;
            }
        }

        if (!success)
        {
            // Enhanced failure feedback with diagnostic info
            var groundTile = Framing.GetTileSafely(tileX, tileY + 1);
            var currentTile = Framing.GetTileSafely(tileX, tileY);

            var reason = "Unknown";
            if (!WorldGen.InWorld(tileX, tileY))
                reason = "Out of world bounds";
            else if (!groundTile.HasTile)
                reason = "No ground tile";
            else if (currentTile.HasTile && Main.tileSolid[currentTile.TileType])
                reason = "Solid tile in the way";

            Main.NewText($"Failed to grow tanglewood tree at ({tileX}, {tileY}) - {reason}", Color.Red);

            // Red dust effect
            for (var i = 0; i < 15; i++)
            {
                var dust = Dust.NewDustDirect(mouseWorld, 16, 16, DustID.Blood,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                dust.scale = 0.8f;
            }
        }

        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "Usage", "[c/00FF00:Left Click:] Grow Small Tanglewood tree at cursor"));
    }
}

public class PineTreeDebugWand : ModItem
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
        if (Main.myPlayer != player.whoAmI)
            return true;

        var mouseWorld = Main.MouseWorld;
        var tileX = (int)(mouseWorld.X / 16f);
        var tileY = (int)(mouseWorld.Y / 16f);

        var success = false;

        // Try multiple times with slight position variations if first attempt fails
        for (var attempts = 0; attempts < 5 && !success; attempts++)
        {
            var tryX = tileX + Main.rand.Next(-1, 2);
            var tryY = tileY + attempts; // Try lower positions

            success = SpruceTree.GrowSpruceTree(tryX, tryY);

            if (success)
            {
                // Success feedback
                Main.NewText($"Pine tree grown at ({tryX}, {tryY}) after {attempts + 1} attempts", Color.Green);

                // Enhanced visual effect
                var effectPos = new Vector2(tryX, tryY) * 16;
                for (var i = 0; i < 30; i++) // Fewer particles for smaller tree
                {
                    var dust = Dust.NewDustDirect(effectPos, 16, 16, DustID.GrassBlades,
                        Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 1f));
                    dust.scale = 1.2f;
                    dust.fadeIn = 1.5f;
                }
                break;
            }
        }

        if (!success)
        {
            // Enhanced failure feedback with diagnostic info
            var groundTile = Framing.GetTileSafely(tileX, tileY + 1);
            var currentTile = Framing.GetTileSafely(tileX, tileY);

            var reason = "Unknown";
            if (!WorldGen.InWorld(tileX, tileY))
                reason = "Out of world bounds";
            else if (!groundTile.HasTile)
                reason = "No ground tile";
            else if (currentTile.HasTile && Main.tileSolid[currentTile.TileType])
                reason = "Solid tile in the way";

            Main.NewText($"Failed to grow tanglewood tree at ({tileX}, {tileY}) - {reason}", Color.Red);

            // Red dust effect
            for (var i = 0; i < 15; i++)
            {
                var dust = Dust.NewDustDirect(mouseWorld, 16, 16, DustID.Blood,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                dust.scale = 0.8f;
            }
        }

        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "Usage", "[c/00FF00:Left Click:] Grow pine tree at cursor"));
    }
}

public class MediumTanglewoodTreeDebugWand : ModItem
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
        if (Main.myPlayer != player.whoAmI)
            return true;

        var mouseWorld = Main.MouseWorld;
        var tileX = (int)(mouseWorld.X / 16f);
        var tileY = (int)(mouseWorld.Y / 16f);

        var success = false;

        // Try multiple times with slight position variations if first attempt fails
        for (var attempts = 0; attempts < 5 && !success; attempts++)
        {
            var tryX = tileX + Main.rand.Next(-1, 2);
            var tryY = tileY + attempts; // Try lower positions

            success = MediumTanglewoodTree.GrowTanglewoodTree(tryX, tryY);

            if (success)
            {
                // Success feedback
                Main.NewText($"Medium Tanglewood tree grown at ({tryX}, {tryY}) after {attempts + 1} attempts", Color.Green);

                // Enhanced visual effect
                var effectPos = new Vector2(tryX, tryY) * 16;
                for (var i = 0; i < 30; i++) // Fewer particles for smaller tree
                {
                    var dust = Dust.NewDustDirect(effectPos, 16, 16, DustID.GrassBlades,
                        Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 1f));
                    dust.scale = 1.2f;
                    dust.fadeIn = 1.5f;
                }
                break;
            }
        }

        if (!success)
        {
            // Enhanced failure feedback with diagnostic info
            var groundTile = Framing.GetTileSafely(tileX, tileY + 1);
            var currentTile = Framing.GetTileSafely(tileX, tileY);

            var reason = "Unknown";
            if (!WorldGen.InWorld(tileX, tileY))
                reason = "Out of world bounds";
            else if (!groundTile.HasTile)
                reason = "No ground tile";
            else if (currentTile.HasTile && Main.tileSolid[currentTile.TileType])
                reason = "Solid tile in the way";

            Main.NewText($"Failed to grow tanglewood tree at ({tileX}, {tileY}) - {reason}", Color.Red);

            // Red dust effect
            for (var i = 0; i < 15; i++)
            {
                var dust = Dust.NewDustDirect(mouseWorld, 16, 16, DustID.Blood,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                dust.scale = 0.8f;
            }
        }

        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "Usage", "[c/00FF00:Left Click:] Grow Small Tanglewood tree at cursor"));
    }
}
