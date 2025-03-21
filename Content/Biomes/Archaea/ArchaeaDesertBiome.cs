
using Reverie.Common.Subworlds.Archaea;
using SubworldLibrary;


namespace Reverie.Content.Biomes.Archaea;

public class ArchaeaDesertBiome : ModBiome
{
    public override int Music => MusicID.OtherworldlyDesert;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => base.BackgroundPath;
    public override Color? BackgroundColor => Color.SandyBrown;

    public override bool IsBiomeActive(Player player)
        => player.ZoneDesert && SubworldSystem.IsActive<ArchaeaSub>();
}
