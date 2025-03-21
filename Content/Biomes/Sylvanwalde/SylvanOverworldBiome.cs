using Reverie.Common.Subworlds.Sylvanwalde;
using SubworldLibrary;

namespace Reverie.Content.Biomes.Sylvanwalde;

public class SylvanOverworldBiome : ModBiome
{
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => base.BackgroundPath;
    public override Color? BackgroundColor => Color.DarkOliveGreen;

    // Simple inline ternary operator for music selection
    public override int Music => Main.dayTime && !Main.raining ?
        MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}SylvanwaldeDay") :
        (Main.raining && Main.dayTime ?
            MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}SylvanwaldeRain") :
            MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}SylvanwaldeNight"));

    public override bool IsBiomeActive(Player player)
    {
        bool surface = player.ZoneSkyHeight || player.ZoneOverworldHeight || player.ZoneRockLayerHeight;
        return surface && SubworldSystem.IsActive<SylvanSub>();
    } 
}