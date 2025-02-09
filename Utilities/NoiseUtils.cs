using Reverie.lib;

namespace Reverie.Utilities;

/// <summary>
/// Utility class for handling FastNoiseLite operations and common noise patterns.
/// </summary>
public static class NoiseUtils
{
    #region Default Values
    public const float DEFAULT_LACUNARITY = 3f;
    public const float DEFAULT_GAIN = 0.27f;
    public const float DEFAULT_WEIGHTED_STRENGTH = 0.12f;
    #endregion

    #region Noise Configuration
    /// <summary>
    /// Configures basic noise settings with default fractal values.
    /// </summary>
    public static FastNoiseLite ConfigureNoise(FastNoiseLite.NoiseType type, float frequency, int octaves)
    {
        var noise = new FastNoiseLite();
        ConfigureNoiseBase(noise, type, frequency, octaves);
        return noise;
    }

    /// <summary>
    /// Configures a noise instance with custom settings.
    /// </summary>
    public static void ConfigureNoiseBase(FastNoiseLite noise, FastNoiseLite.NoiseType type, float frequency, int octaves)
    {
        noise.SetNoiseType(type);
        noise.SetFrequency(frequency);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(DEFAULT_LACUNARITY);
        noise.SetFractalGain(DEFAULT_GAIN);
        noise.SetFractalWeightedStrength(DEFAULT_WEIGHTED_STRENGTH);
    }

    /// <summary>
    /// Configures advanced noise settings with custom fractal values.
    /// </summary>
    public static FastNoiseLite ConfigureAdvancedNoise(
        FastNoiseLite.NoiseType type,
        float frequency,
        int octaves,
        float lacunarity = DEFAULT_LACUNARITY,
        float gain = DEFAULT_GAIN,
        float weightedStrength = DEFAULT_WEIGHTED_STRENGTH)
    {
        var noise = new FastNoiseLite();
        ConfigureAdvancedNoiseBase(noise, type, frequency, octaves, lacunarity, gain, weightedStrength);
        return noise;
    }

    /// <summary>
    /// Configures a noise instance with custom fractal values.
    /// </summary>
    public static void ConfigureAdvancedNoiseBase(
        FastNoiseLite noise,
        FastNoiseLite.NoiseType type,
        float frequency,
        int octaves,
        float lacunarity = DEFAULT_LACUNARITY,
        float gain = DEFAULT_GAIN,
        float weightedStrength = DEFAULT_WEIGHTED_STRENGTH)
    {
        noise.SetNoiseType(type);
        noise.SetFrequency(frequency);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(gain);
        noise.SetFractalWeightedStrength(weightedStrength);
    }
    #endregion

    #region Common Noise Patterns
    /// <summary>
    /// Creates noise suitable for cave generation.
    /// </summary>
    public static FastNoiseLite CreateCaveNoise(float frequency = 0.19f, int octaves = 2)
        => ConfigureNoise(FastNoiseLite.NoiseType.ValueCubic, frequency, octaves);

    /// <summary>
    /// Creates noise suitable for wall generation.
    /// </summary>
    public static FastNoiseLite CreateWallNoise(float frequency = 0.09f, int octaves = 2)
        => ConfigureNoise(FastNoiseLite.NoiseType.OpenSimplex2S, frequency, octaves);

    /// <summary>
    /// Creates noise suitable for open cave generation.
    /// </summary>
    public static FastNoiseLite CreateOpenCaveNoise(float frequency = 0.056f, int octaves = 3)
        => ConfigureNoise(FastNoiseLite.NoiseType.Perlin, frequency, octaves);

    /// <summary>
    /// Creates noise suitable for mineral/ore generation.
    /// </summary>
    public static FastNoiseLite CreateMineralNoise(float frequency = 0.12f, int octaves = 3)
        => ConfigureNoise(FastNoiseLite.NoiseType.ValueCubic, frequency, octaves);
    #endregion

    #region Noise Sampling
    /// <summary>
    /// Gets a 2D noise value with optional offset.
    /// </summary>
    public static float SampleNoise2D(FastNoiseLite noise, float x, float y, float offset = 0)
        => noise.GetNoise(x, y + offset);

    /// <summary>
    /// Gets a 2D noise value within a specific range.
    /// </summary>
    public static float SampleNoise2DRange(FastNoiseLite noise, float x, float y, float minValue, float maxValue, float offset = 0)
    {
        float noiseValue = SampleNoise2D(noise, x, y, offset);
        return minValue + (noiseValue + 1f) * 0.5f * (maxValue - minValue);
    }

    /// <summary>
    /// Gets multiple noise samples at different offsets.
    /// </summary>
    public static float[] SampleNoiseMultiple(FastNoiseLite noise, float x, float y, params float[] offsets)
    {
        float[] samples = new float[offsets.Length];
        for (int i = 0; i < offsets.Length; i++)
        {
            samples[i] = SampleNoise2D(noise, x, y, offsets[i]);
        }
        return samples;
    }
    #endregion

    #region Noise Modifiers
    /// <summary>
    /// Applies height-based noise modification.
    /// </summary>
    public static float ApplyHeightModifier(float noiseValue, int currentHeight, int minHeight, int maxHeight)
    {
        float heightFactor = (float)(currentHeight - minHeight) / (maxHeight - minHeight);
        return noiseValue * (1f - heightFactor);
    }

    /// <summary>
    /// Combines multiple noise values with weights.
    /// </summary>
    public static float CombineNoise(float[] noiseValues, float[] weights)
    {
        if (noiseValues.Length != weights.Length)
            throw new ArgumentException("Noise values and weights arrays must have the same length");

        float totalValue = 0f;
        float totalWeight = 0f;

        for (int i = 0; i < noiseValues.Length; i++)
        {
            totalValue += noiseValues[i] * weights[i];
            totalWeight += weights[i];
        }

        return totalValue / totalWeight;
    }
    #endregion
}