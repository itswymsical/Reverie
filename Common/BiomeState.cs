using Terraria;

namespace Reverie.Common.Players
{
    public partial class MissionPlayer
    {
        public struct BiomeState
        {
            public bool ZoneBeach;
            public bool ZoneDungeon;
            public bool ZoneCorrupt;
            public bool ZoneCrimson;
            public bool ZoneDesert;
            public bool ZoneGlowshroom;
            public bool ZoneHallow;
            public bool ZoneJungle;
            public bool ZoneMeteor;
            public bool ZoneSnow;
            public bool ZoneUndergroundDesert;
            public bool ZoneRain;
            public bool ZoneSandstorm;
            public bool ZoneOldOneArmy;
            public bool ZonePeaceCandle;
            public bool ZoneWaterCandle;

            public static BiomeState FromPlayer(Player player)
            {
                return new BiomeState
                {
                    ZoneBeach = player.ZoneBeach,
                    ZoneDungeon = player.ZoneDungeon,
                    ZoneCorrupt = player.ZoneCorrupt,
                    ZoneCrimson = player.ZoneCrimson,
                    ZoneDesert = player.ZoneDesert,
                    ZoneGlowshroom = player.ZoneGlowshroom,
                    ZoneHallow = player.ZoneHallow,
                    ZoneJungle = player.ZoneJungle,
                    ZoneMeteor = player.ZoneMeteor,
                    ZoneSnow = player.ZoneSnow,
                    ZoneUndergroundDesert = player.ZoneUndergroundDesert,
                    ZoneRain = player.ZoneRain,
                    ZoneSandstorm = player.ZoneSandstorm,
                    ZoneOldOneArmy = player.ZoneOldOneArmy,
                    ZonePeaceCandle = player.ZonePeaceCandle,
                    ZoneWaterCandle = player.ZoneWaterCandle
                };
            }
        }
    }
}