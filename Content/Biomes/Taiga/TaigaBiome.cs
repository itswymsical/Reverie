using Reverie.Common.Tiles;

namespace Reverie.Content.Biomes.Taiga;

public class TaigaBiome : ModBiome
{
    public override int Music => MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}TaigaDay");
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<TaigaBackgroundStyle>();
    public override string BestiaryIcon => "Reverie/Assets/Textures/Bestiary/TaigaIcon";
    public override string BackgroundPath => "Reverie/Assets/Textures/Bestiary/TaigaMapBG";
    public override Color? BackgroundColor => new Color(88, 150, 112);
    //public override ModWaterStyle WaterStyle => ModContent.GetInstance<TaigaWaterStyle>();

    public override bool IsBiomeActive(Player player)
    => TileCounts.Instance.taigaCount > 150;
}
