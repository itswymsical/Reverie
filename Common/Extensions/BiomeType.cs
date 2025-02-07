using Terraria;

namespace Reverie.Common.Extensions;

public enum BiomeType
{
    Beach,
    Dungeon,
    Corrupt,
    Crimson,
    Desert,
    Glowshroom,
    Hallow,
    Jungle,
    Meteor,
    Snow,
    UndergroundDesert,
    Rain,
    Sandstorm,
    OldOneArmy,
    PeaceCandle,
    WaterCandle,
    Forest,
    Underground
}

public static class BiomeTypeExtensions
{
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
        BiomeType.Underground => player.ZoneNormalUnderground,
        _ => false
    };
}