using Reverie.Content.Tiles.Taiga.Trees;

namespace Reverie.DebugTools;

public class SprucePlacer : ModItem
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

        for (var attempts = 0; attempts < 5 && !success; attempts++)
        {
            var tryX = tileX + Main.rand.Next(-1, 2);
            var tryY = tileY + attempts;

            success = SpruceTree.Grow(tryX, tryY);

            if (success)
            {
                Main.NewText($"Spruce tree grown at ({tryX}, {tryY}) after {attempts + 1} attempts", Color.Green);

                var effectPos = new Vector2(tryX, tryY) * 16;
                for (var i = 0; i < 30; i++)
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
            var groundTile = Framing.GetTileSafely(tileX, tileY + 1);
            var currentTile = Framing.GetTileSafely(tileX, tileY);

            var reason = "Unknown";
            if (!WorldGen.InWorld(tileX, tileY))
                reason = "Out of world bounds";
            else if (!groundTile.HasTile)
                reason = "No ground tile";
            else if (currentTile.HasTile && Main.tileSolid[currentTile.TileType])
                reason = "Solid tile in the way";

            Main.NewText($"Failed to grow Spruce tree at ({tileX}, {tileY}) - {reason}", Color.Red);

            for (var i = 0; i < 15; i++)
            {
                var dust = Dust.NewDustDirect(mouseWorld, 16, 16, DustID.Blood,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                dust.scale = 0.8f;
            }
        }

        return true;
    }
}