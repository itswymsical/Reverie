using Reverie.Common.Tiles;
using static Reverie.Common.WorldGeneration.WoodlandCanopy.CanopyGeneration;

namespace Reverie.Content.Biomes.Canopy;

public class WoodlandCanopyBiome : ModBiome
{
    public override int Music => MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}Woodhaven");
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
    public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<CanopyBackgroundStyle>();
    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => base.BackgroundPath;
    public override Color? BackgroundColor => new Color(95, 143, 65);

    public override bool IsBiomeActive(Player player)
    {
        int canopyTop = canopyY - canopyRadiusV;
        int canopyBottom = canopyY + canopyRadiusV;

        var canopyMiddle = (canopyTop + canopyBottom) / 2;

        var playerTile = player.position.ToTileCoordinates();

        var inCanopyX = Math.Abs(playerTile.X - canopyX) <= canopyRadiusH;

        var inUpperHalfY = playerTile.Y >= canopyTop && playerTile.Y < canopyMiddle;

        var belowDirtLayer = playerTile.Y > Main.worldSurface + 20;

        var enoughCanopyBlocks = ModContent.GetInstance<TileCounts>().canopyBlockCount >= 100;

        return inCanopyX && inUpperHalfY && belowDirtLayer && enoughCanopyBlocks;
    }
}

public class LowerCanopyBiome : ModBiome
{
    public override int Music => MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}LowerCanopy");
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
    public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<CanopyBackgroundStyle>();
    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => base.BackgroundPath;
    public override Color? BackgroundColor => new Color(95, 143, 65);
    public override bool IsBiomeActive(Player player)
    {
        int canopyTop = canopyY - canopyRadiusV;
        int canopyBottom = canopyY + canopyRadiusV;

        var canopyMiddle = (canopyTop + canopyBottom) / 2;

        var playerTile = player.position.ToTileCoordinates();

        var inCanopyX = Math.Abs(playerTile.X - canopyX) <= canopyRadiusH;

        var inLowerHalfY = playerTile.Y >= canopyMiddle && playerTile.Y <= canopyBottom;

        var enoughCanopyBlocks = ModContent.GetInstance<TileCounts>().canopyBlockCount >= 100;

        return inCanopyX && inLowerHalfY && enoughCanopyBlocks;
    }
}