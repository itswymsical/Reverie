using Reverie.Common.Tiles;

namespace Reverie.Content.Biomes.Rainforest;

public class SurfaceCanopyBiome : ModBiome
{
    public override int Music => MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}CanopySurfaceNight");
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
    public override ModWaterStyle WaterStyle => ModContent.GetInstance<CanopyWaterStyle>();
    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<CanopySurfaceBackgroundStyle>();
    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => base.BackgroundPath;
    public override Color? BackgroundColor => new Color(95, 143, 65);

    public override bool IsBiomeActive(Player player)
    {
        return player.ZoneOverworldHeight && ModContent.GetInstance<TileCounts>().surfaceCanopyBlockCount >= 200;
    }
}

public class UndergroundCanopyBiome : ModBiome
{
    public override int Music => MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}UndergroundCanopy");
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
    public override ModWaterStyle WaterStyle => ModContent.GetInstance<CanopyWaterStyle>();

    public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<CanopyBackgroundStyle>();
    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => base.BackgroundPath;
    public override Color? BackgroundColor => new Color(95, 143, 65);

    public override bool IsBiomeActive(Player player)
    {
        return (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight) && ModContent.GetInstance<TileCounts>().undergroundCanopyBlockCount >= 200;
    }
}