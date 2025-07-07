using MonoMod.Cil;
using Reverie.Content.Tiles.Farming;

namespace Reverie.Common.MonoMod;

public class SeedBedPlantSupport : ModSystem
{
    private static int SeedBedType => ModContent.TileType<SeedBedTile>();

    public override void PostSetupContent()
    {
        // Try IL editing first, fallback to method hooks if needed
        try
        {
            IL_WorldGen.PlaceTile += IL_PlaceTile_SeedBedSupport;
            IL_WorldGen.GrowAlch += IL_GrowAlch_SeedBedSupport;
            IL_WorldGen.CheckAlch += IL_CheckAlch_SeedBedSupport;
            Mod.Logger.Info("Seedbed planter box IL Edit enabled");
        }
        catch (Exception e)
        {
            Mod.Logger.Warn($"IL editing failed ({e.Message}), using fallback method hooks");
            On_WorldGen.PlaceTile += Hook_PlaceTile_SeedBedSupport;
            On_WorldGen.GrowAlch += Hook_GrowAlch_SeedBedSupport;
            On_WorldGen.CheckAlch += Hook_CheckAlch_SeedBedSupport;
        }
    }

    #region IL shit

    private void IL_PlaceTile_SeedBedSupport(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);

            while (c.TryGotoNext(i => i.MatchLdcI4(TileID.ClayPot)))
            {
                c.Index++;
                c.EmitDelegate<Func<int, int, bool>>((tileType, originalConstant) =>
                    tileType == TileID.ClayPot ||
                    tileType == TileID.PlanterBox ||
                    tileType == SeedBedType);
            }

            // Also handle PlanterBox checks
            c.Index = 0;
            while (c.TryGotoNext(i => i.MatchLdcI4(TileID.PlanterBox)))
            {
                c.Index++;
                c.EmitDelegate<Func<int, int, bool>>((tileType, originalConstant) =>
                    tileType == TileID.ClayPot ||
                    tileType == TileID.PlanterBox ||
                    tileType == SeedBedType);

            }

            Mod.Logger.Debug("Successfully patched PlaceTile for SeedBed plant support");
        }
        catch (Exception e)
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<Mod>(), il);
            Mod.Logger.Error(e);
            throw;
        }
    }

    private void IL_GrowAlch_SeedBedSupport(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);

            // Same pattern for herb growth logic
            while (c.TryGotoNext(i => i.MatchLdcI4(TileID.ClayPot)))
            {
                c.Index++;
                c.EmitDelegate<Func<int, int, bool>>((tileType, originalConstant) =>
                    tileType == TileID.ClayPot ||
                    tileType == TileID.PlanterBox ||
                    tileType == SeedBedType);
            }

            c.Index = 0;
            while (c.TryGotoNext(i => i.MatchLdcI4(TileID.PlanterBox)))
            {
                c.Index++;
                c.EmitDelegate<Func<int, int, bool>>((tileType, originalConstant) =>
                    tileType == TileID.ClayPot ||
                    tileType == TileID.PlanterBox ||
                    tileType == SeedBedType);
            }

            Mod.Logger.Debug("Successfully patched GrowAlch for SeedBed herb support");
        }
        catch (Exception e)
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<Mod>(), il);
            Mod.Logger.Error(e);
            throw;
        }
    }

    private void IL_CheckAlch_SeedBedSupport(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);

            while (c.TryGotoNext(i => i.MatchLdcI4(TileID.ClayPot)))
            {
                c.Index++;
                c.EmitDelegate<Func<int, int, bool>>((tileType, originalConstant) =>
                    tileType == TileID.ClayPot ||
                    tileType == TileID.PlanterBox ||
                    tileType == SeedBedType);
            }

            c.Index = 0;
            while (c.TryGotoNext(i => i.MatchLdcI4(TileID.PlanterBox)))
            {
                c.Index++;
                c.EmitDelegate<Func<int, int, bool>>((tileType, originalConstant) =>
                    tileType == TileID.ClayPot ||
                    tileType == TileID.PlanterBox ||
                    tileType == SeedBedType);
            }

            Mod.Logger.Debug("Successfully patched CheckAlch for SeedBed herb support");
        }
        catch (Exception e)
        {
            MonoModHooks.DumpIL(ModContent.GetInstance<Mod>(), il);
            Mod.Logger.Error(e);
            throw;
        }
    }

    #endregion

    #region Method Hooks (Fallback)

    private bool Hook_PlaceTile_SeedBedSupport(On_WorldGen.orig_PlaceTile orig, int i, int j, int type, bool mute, bool forced, int plr, int style)
    {
        // Check if it's a plant tile trying to place on a seedbed
        if (IsPlantTile(type))
        {
            var tileBelow = Framing.GetTileSafely(i, j + 1);
            if (tileBelow.HasTile && tileBelow.TileType == SeedBedType)
            {
                // Temporarily disguise seedbed as clay pot for vanilla logic
                var originalType = tileBelow.TileType;
                tileBelow.TileType = TileID.ClayPot;

                bool result = orig(i, j, type, mute, forced, plr, style);

                // Restore original type
                tileBelow.TileType = (ushort)originalType;
                return result;
            }
        }

        return orig(i, j, type, mute, forced, plr, style);
    }

    private void Hook_GrowAlch_SeedBedSupport(On_WorldGen.orig_GrowAlch orig, int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);
        var tileBelow = Framing.GetTileSafely(i, j + 1);

        if (IsHerbTile(tile.TileType) && tileBelow.HasTile && tileBelow.TileType == SeedBedType)
        {
            var originalType = tileBelow.TileType;
            tileBelow.TileType = TileID.ClayPot;

            // 5x faster growth rate
            for (int attempts = 0; attempts < 5; attempts++)
            {
                orig(i, j);

                var currentTile = Framing.GetTileSafely(i, j);
                if (currentTile.TileType != tile.TileType)
                    break;
            }

            tileBelow.TileType = (ushort)originalType;
            return;
        }

        orig(i, j);
    }
    private void Hook_CheckAlch_SeedBedSupport(On_WorldGen.orig_CheckAlch orig, int x, int y)
    {
        var herb = Framing.GetTileSafely(x, y);
        var tileBelow = Framing.GetTileSafely(x, y + 1);

        // Protect herbs on seedbeds during validation
        if (IsHerbTile(herb.TileType) && tileBelow.HasTile && tileBelow.TileType == SeedBedType)
        {
            var originalType = tileBelow.TileType;
            tileBelow.TileType = TileID.ClayPot;

            orig(x, y);

            tileBelow.TileType = (ushort)originalType;
            return;
        }

        orig(x, y);
    }

    #endregion

    #region Helper Methods

    private static bool IsPlantTile(int tileType)
    {
        return tileType == TileID.Plants ||
               tileType == TileID.Plants2 ||
               tileType == TileID.BloomingHerbs ||
               tileType == TileID.MatureHerbs ||
               tileType == TileID.ImmatureHerbs ||
               tileType == TileID.CorruptPlants ||
               tileType == TileID.CrimsonPlants ||
               tileType == TileID.HallowedPlants ||
               tileType == TileID.HallowedPlants2 ||
               tileType == TileID.JunglePlants ||
               tileType == TileID.JunglePlants2 ||
               tileType == TileID.MushroomPlants;
    }

    private static bool IsHerbTile(int tileType)
    {
        return tileType == TileID.BloomingHerbs ||
               tileType == TileID.MatureHerbs ||
               tileType == TileID.ImmatureHerbs;
    }

    #endregion

    public override void Unload()
    {
        IL_WorldGen.PlaceTile -= IL_PlaceTile_SeedBedSupport;
        IL_WorldGen.GrowAlch -= IL_GrowAlch_SeedBedSupport;
        IL_WorldGen.CheckAlch -= IL_CheckAlch_SeedBedSupport;
        On_WorldGen.PlaceTile -= Hook_PlaceTile_SeedBedSupport;
        On_WorldGen.GrowAlch -= Hook_GrowAlch_SeedBedSupport;
        On_WorldGen.CheckAlch -= Hook_CheckAlch_SeedBedSupport;
    }
}