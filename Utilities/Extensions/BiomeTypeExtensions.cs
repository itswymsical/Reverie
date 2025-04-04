namespace Reverie.Utilities.Extensions;

/// <summary>
///     Provides an extension method for <see cref="Player"/> zones, specifically for the use of ObjectiveHandler checks.
/// </summary>
public static class BiomeTypeExtensions
{

    /// <summary>
    ///     Attempts to locate the biome/zone the <see cref="Player"/> is in, specifically for the use of ObjectiveHandler checks.
    /// </summary>
    public static bool IsPlayerInBiome(this BiomeType biome, Player player) => biome switch
    {
        BiomeType.Beach => player.ZoneBeach,
        BiomeType.Dungeon => player.ZoneDungeon,
        BiomeType.Corrupt => player.ZoneCorrupt,
        BiomeType.Crimson => player.ZoneCrimson,
        BiomeType.Desert => player.ZoneDesert,
        BiomeType.Glowshroom => player.ZoneGlowshroom,
        BiomeType.Hallow => player.ZoneHallow,
        BiomeType.Jungle => player.ZoneJungle,
        BiomeType.Meteor => player.ZoneMeteor,
        BiomeType.Snow => player.ZoneSnow,
        BiomeType.UndergroundDesert => player.ZoneUndergroundDesert,
        BiomeType.Rain => player.ZoneRain,
        BiomeType.Sandstorm => player.ZoneSandstorm,
        BiomeType.OldOneArmy => player.ZoneOldOneArmy,
        BiomeType.PeaceCandle => player.ZonePeaceCandle,
        BiomeType.WaterCandle => player.ZoneWaterCandle,
        BiomeType.Forest => player.ZoneForest,
        BiomeType.Underground => player.ZoneRockLayerHeight || player.ZoneDirtLayerHeight,
        BiomeType.Granite => player.ZoneGranite,
        BiomeType.Marble => player.ZoneMarble,
        BiomeType.Underworld => player.ZoneUnderworldHeight,
        _ => false
    };
}
