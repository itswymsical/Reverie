using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Rainforest.Surface.Trees;

public class KapokSapling : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileCut[Type] = true;

        // Configure as a 1x2 sapling
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        // Define what tiles this sapling can grow on
        TileObjectData.newTile.AnchorValidTiles = [
            ModContent.TileType<CanopyGrassTile>(),
            ModContent.TileType<WoodgrassTile>(),
            ModContent.TileType<OxisolTile>()
        ];

        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawFlipHorizontal = true;
        TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleMultiplier = 3;

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Sapling"));

        // Sapling identification sets
        TileID.Sets.TreeSapling[Type] = true;
        TileID.Sets.CommonSapling[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        DustType = DustID.RichMahogany;
        AdjTiles = [TileID.Saplings];
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override void RandomUpdate(int i, int j)
    {
        // Kapok saplings grow based on environmental conditions
        if (CanGrowIntoKapokTree(i, j))
        {
            var growthChance = CalculateGrowthChance(i, j);
            if (WorldGen.genRand.NextBool(growthChance))
            {
                GrowIntoKapokTree(i, j);
            }
        }
    }

    /// <summary>
    /// Calculates the growth chance based on environmental factors
    /// </summary>
    private int CalculateGrowthChance(int i, int j)
    {
        var baseChance = 12; // Base 1/12 chance (slightly faster than vanilla)

        // Environmental modifiers
        if (IsNearWater(i, j, 10))
            baseChance -= 3; // Faster growth near water

        if (HasNearbyCrowding(i, j, 12))
            baseChance += 4; // Slower growth when crowded

        if (IsInOptimalLocation(i, j))
            baseChance -= 2; // Faster growth in good spots

        return Math.Max(baseChance, 4); // Minimum 1/4 chance
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (!closer) return;

        // Handle fertilizer for immediate growth
        for (var k = 0; k < Main.maxProjectiles; k++)
        {
            var proj = Main.projectile[k];
            if (proj.active && proj.type == ProjectileID.Fertilizer)
            {
                var tileRect = new Rectangle(i * 16, j * 16, 32, 32);
                if (proj.Hitbox.Intersects(tileRect))
                {
                    if (CanGrowIntoKapokTree(i, j))
                    {
                        GrowIntoKapokTree(i, j);

                        if (Main.netMode == NetmodeID.SinglePlayer)
                        {
                            Main.NewText("Kapok sapling flourished with fertilizer!", Color.Green);
                        }
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Checks if the sapling can grow into a Kapok tree
    /// </summary>
    private bool CanGrowIntoKapokTree(int i, int j)
    {
        // Get the target height for this potential tree
        var kapokInstance = ModContent.GetInstance<KapokTree>();
        var targetHeight = kapokInstance.DetermineTreeHeight(i, j, false);
        var treeWidth = kapokInstance.GetTreeWidth();

        // Use the CustomTree utility method to check if growth is possible
        return CustomTree.CanGrowAt(i, j, treeWidth, targetHeight, kapokInstance.ValidAnchorTiles);
    }

    /// <summary>
    /// Grows the sapling into a Kapok tree using the custom tree system
    /// </summary>
    private void GrowIntoKapokTree(int i, int j)
    {
        // Use the custom tree growth system
        var success = CustomTree.GrowTree<KapokTree>(i, j);

        if (success)
        {
            // Create growth effects if player can see it
            if (WorldGen.PlayerLOS(i, j))
            {
                CreateGrowthEffects(i, j);
            }

            // Network sync for multiplayer
            if (Main.netMode == NetmodeID.Server)
            {
                // The CustomTree system handles networking, but we can add extra effects
                NetMessage.SendTileSquare(-1, i - 1, j - 30, 4, 32, TileChangeType.None);
            }
        }
    }

    /// <summary>
    /// Creates visual effects when the sapling grows
    /// </summary>
    private void CreateGrowthEffects(int i, int j)
    {
        // Determine the height of the newly grown tree
        var treeHeight = CustomTree.GetTreeHeightAt(i, j, ModContent.TileType<KapokTree>());

        // Create growth particles
        for (var h = 0; h < Math.Min(treeHeight / 3, 8); h++)
        {
            var position = new Vector2(i * 16, (j - h * 3) * 16);

            // Create upward-moving leaf particles
            for (var p = 0; p < 4; p++)
            {
                var leaf = Dust.NewDustDirect(
                    position + new Vector2(Main.rand.Next(-8, 24), Main.rand.Next(-8, 8)),
                    16, 16,
                    DustID.GrassBlades,
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-3f, -1f)
                );
                leaf.fadeIn = 1.5f;
                leaf.scale = 1.2f;
                leaf.noGravity = false;
            }

            // Create some sparkly "growth magic" particles
            if (Main.rand.NextBool(2))
            {
                var magic = Dust.NewDustDirect(
                    position,
                    32, 16,
                    DustID.GoldFlame,
                    0f,
                    Main.rand.NextFloat(-2f, 0f)
                );
                magic.fadeIn = 1.0f;
                magic.scale = 0.8f;
                magic.noGravity = true;
            }
        }

        // Play growth sound
        Terraria.Audio.SoundEngine.PlaySound(SoundID.Grass, new Vector2(i * 16, j * 16));
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects)
    {
        if (i % 2 == 1)
        {
            effects = SpriteEffects.FlipHorizontally;
        }
    }

    // ===============================
    // UTILITY METHODS
    // ===============================

    private static bool IsNearWater(int x, int y, int range)
    {
        for (var checkX = x - range; checkX <= x + range; checkX++)
        {
            for (var checkY = y - range; checkY <= y + range; checkY++)
            {
                if (WorldGen.InWorld(checkX, checkY))
                {
                    var tile = Framing.GetTileSafely(checkX, checkY);
                    if (tile.LiquidAmount > 0)
                        return true;
                }
            }
        }
        return false;
    }

    private static bool HasNearbyCrowding(int x, int y, int range)
    {
        var treeCount = 0;

        for (var checkX = x - range; checkX <= x + range; checkX++)
        {
            for (var checkY = y - range; checkY <= y + range; checkY++)
            {
                if (WorldGen.InWorld(checkX, checkY))
                {
                    var tile = Framing.GetTileSafely(checkX, checkY);
                    if (tile.HasTile)
                    {
                        // Count trees and saplings
                        if (tile.TileType == ModContent.TileType<KapokTree>() ||
                            tile.TileType == ModContent.TileType<KapokSapling>() ||
                            TileID.Sets.TreeSapling[tile.TileType] ||
                            tile.TileType == TileID.Trees)
                        {
                            treeCount++;
                            if (treeCount >= 3) return true; // Crowded if 3+ trees nearby
                        }
                    }
                }
            }
        }
        return false;
    }

    private static bool IsInOptimalLocation(int x, int y)
    {
        // Check if the location has good conditions for Kapok growth

        // Prefer deeper locations (more nutrients)
        if (y > Main.worldSurface + 20) return true;

        // Prefer areas with some moisture but not flooded
        var nearWater = IsNearWater(x, y, 15);
        var notFlooded = Framing.GetTileSafely(x, y - 1).LiquidAmount == 0;

        return nearWater && notFlooded;
    }

    // ===============================
    // DEBUG AND UTILITY (Remove in production)
    // ===============================

    public override bool RightClick(int i, int j)
    {
        // Debug info when right-clicking sapling (remove in production)
        if (Main.netMode == NetmodeID.SinglePlayer && Main.LocalPlayer.HeldItem.type == ItemID.ActuationRod)
        {
            var canGrow = CanGrowIntoKapokTree(i, j);
            var growthChance = CalculateGrowthChance(i, j);

            Main.NewText($"Kapok Sapling Debug:", Color.Yellow);
            Main.NewText($"  Can Grow: {canGrow}", canGrow ? Color.Green : Color.Red);
            Main.NewText($"  Growth Chance: 1/{growthChance}", Color.Cyan);
            Main.NewText($"  Near Water: {IsNearWater(i, j, 10)}", Color.Blue);
            Main.NewText($"  Crowded: {HasNearbyCrowding(i, j, 12)}", Color.Orange);
            Main.NewText($"  Optimal Location: {IsInOptimalLocation(i, j)}", Color.Lime);

            return true;
        }

        return false;
    }
}

// ===============================
// SAPLING INTEGRATION SYSTEM
// ===============================

/// <summary>
/// Handles integration between saplings and the custom tree system
/// </summary>
public class CustomTreeSaplingSystem : ModSystem
{
    /// <summary>
    /// Dictionary mapping sapling types to their corresponding tree types
    /// </summary>
    public static readonly Dictionary<int, int> SaplingToTreeMap = new();

    public override void PostSetupContent()
    {
        // Register sapling-to-tree relationships
        SaplingToTreeMap[ModContent.TileType<KapokSapling>()] = ModContent.TileType<KapokTree>();

        // Add more sapling types here as they're created
        // SaplingToTreeMap[ModContent.TileType<OtherSapling>()] = ModContent.TileType<OtherTree>();
    }

    public override void PostWorldGen()
    {
        // Clear tree caches after world generation
        CustomTree.ClearCache();
    }

    /// <summary>
    /// Utility method to grow any custom tree from its sapling
    /// </summary>
    public static bool TryGrowCustomTree(int saplingType, int x, int y)
    {
        if (!SaplingToTreeMap.TryGetValue(saplingType, out var treeType))
            return false;

        // Use reflection to call the appropriate GrowTree method
        var treeModTile = TileLoader.GetTile(treeType);
        if (treeModTile is CustomTree customTree)
        {
            // Get the generic method and make it specific to this tree type
            var method = typeof(CustomTree).GetMethod("GrowTree");
            var genericMethod = method?.MakeGenericMethod(customTree.GetType());

            var result = genericMethod?.Invoke(null, [x, y]);
            return result is bool success && success;
        }

        return false;
    }
}