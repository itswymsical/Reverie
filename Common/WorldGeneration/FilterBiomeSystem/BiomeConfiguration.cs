namespace Reverie.Common.WorldGeneration.FilterBiomeSystem;

public class BiomeConfiguration
{
    public float BaseNoiseFreq { get; set; } = 0.015f;
    public float TerrainNoiseFreq { get; set; } = 0.0105f;
    public int MinWidth { get; set; } = 100;
    public int MaxWidth { get; set; } = 300;
    public int BaseHeightOffset { get; set; } = -8;
    public int TerrainHeightVariation { get; set; } = 20;
    public int SurfaceDepth { get; set; } = (int)(Main.maxTilesY * 0.075f);
    public int EdgeTaperZone { get; set; } = 100;
    public int BiomeSeparation { get; set; } = 200;
}
