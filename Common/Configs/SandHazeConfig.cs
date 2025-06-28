using System.ComponentModel;
using Terraria.ModLoader.Config;

public enum SandHazePreset
{
    OFF,
    Low,
    Medium,
    High
}

public class SandHazeConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("QuickSettings")]

    [Label("Preset")]
    [Tooltip("Quick preset configurations for sand haze intensity")]
    [DefaultValue(SandHazePreset.Medium)]
    public SandHazePreset Preset { get; set; }

    [Header("SandHazeRendering")]

    [Label("Enable Sand Haze")]
    [Tooltip("Toggles sand haze effect on/off")]
    [DefaultValue(true)]
    public bool EnableSandHaze { get; set; }

    [Label("Horizontal Range")]
    [Tooltip("How far horizontally the sand haze extends from the player (in tiles)")]
    [Range(10, 100)]
    [DefaultValue(55)]
    [Increment(5)]
    public int HorizontalRange { get; set; }

    [Label("Vertical Range")]
    [Tooltip("How far vertically the sand haze extends from the player (in tiles)")]
    [Range(10, 60)]
    [DefaultValue(30)]
    [Increment(5)]
    public int VerticalRange { get; set; }

    [Label("Dust Spawn Chance")]
    [Tooltip("Higher values = less dust spawned (1 in X chance)")]
    [Range(10, 200)]
    [DefaultValue(95)]
    [Increment(5)]
    public int DustSpawnChance { get; set; }

    [Label("Dust Size")]
    [Tooltip("Size of individual dust particles")]
    [Range(1, 15)]
    [DefaultValue(5)]
    public int DustSize { get; set; }

    [Header("WindEffects")]

    [Label("Wind Velocity Factor")]
    [Tooltip("How much wind affects dust movement")]
    [Range(0f, 15f)]
    [DefaultValue(5.6f)]
    [Increment(0.2f)]
    public float WindVelocityFactor { get; set; }

    [Label("Sandstorm Upward Velocity")]
    [Tooltip("Additional upward velocity during sandstorms")]
    [Range(0f, 0.2f)]
    [DefaultValue(0.07f)]
    [Increment(0.01f)]
    public float SandstormUpwardVelocity { get; set; }

    [Header("Performance")]

    [Label("Performance Mode")]
    [Tooltip("Reduces dust density for better performance")]
    [DefaultValue(false)]
    public bool PerformanceMode { get; set; }

    // Performance mode modifiers
    public int EffectiveHorizontalRange => GetEffectiveValue(nameof(HorizontalRange));
    public int EffectiveVerticalRange => GetEffectiveValue(nameof(VerticalRange));
    public int EffectiveDustSpawnChance => GetEffectiveValue(nameof(DustSpawnChance));
    public int EffectiveDustSize => GetEffectiveValue(nameof(DustSize));
    public float EffectiveWindVelocityFactor => GetEffectiveFloatValue(nameof(WindVelocityFactor));
    public float EffectiveSandstormUpwardVelocity => GetEffectiveFloatValue(nameof(SandstormUpwardVelocity));
    public bool EffectiveEnableSandHaze => Preset != SandHazePreset.OFF && EnableSandHaze;

    private int GetEffectiveValue(string propertyName)
    {
        if (Preset == SandHazePreset.OFF) return 0;

        var baseValue = Preset switch
        {
            SandHazePreset.Low => propertyName switch
            {
                nameof(HorizontalRange) => 35,
                nameof(VerticalRange) => 20,
                nameof(DustSpawnChance) => 150,
                nameof(DustSize) => 3,
                _ => 0
            },
            SandHazePreset.Medium => propertyName switch
            {
                nameof(HorizontalRange) => 55,
                nameof(VerticalRange) => 30,
                nameof(DustSpawnChance) => 95,
                nameof(DustSize) => 5,
                _ => 0
            },
            SandHazePreset.High => propertyName switch
            {
                nameof(HorizontalRange) => 80,
                nameof(VerticalRange) => 45,
                nameof(DustSpawnChance) => 60,
                nameof(DustSize) => 7,
                _ => 0
            },
            _ => propertyName switch
            {
                nameof(HorizontalRange) => HorizontalRange,
                nameof(VerticalRange) => VerticalRange,
                nameof(DustSpawnChance) => DustSpawnChance,
                nameof(DustSize) => DustSize,
                _ => 0
            }
        };

        return PerformanceMode && propertyName == nameof(DustSpawnChance) ? baseValue * 2 :
               PerformanceMode && (propertyName == nameof(HorizontalRange) || propertyName == nameof(VerticalRange)) ? baseValue / 2 :
               baseValue;
    }

    private float GetEffectiveFloatValue(string propertyName)
    {
        if (Preset == SandHazePreset.OFF) return 0f;

        return Preset switch
        {
            SandHazePreset.Low => propertyName switch
            {
                nameof(WindVelocityFactor) => 3f,
                nameof(SandstormUpwardVelocity) => 0.04f,
                _ => 0f
            },
            SandHazePreset.Medium => propertyName switch
            {
                nameof(WindVelocityFactor) => 5.6f,
                nameof(SandstormUpwardVelocity) => 0.07f,
                _ => 0f
            },
            SandHazePreset.High => propertyName switch
            {
                nameof(WindVelocityFactor) => 8f,
                nameof(SandstormUpwardVelocity) => 0.12f,
                _ => 0f
            },
            _ => propertyName switch
            {
                nameof(WindVelocityFactor) => WindVelocityFactor,
                nameof(SandstormUpwardVelocity) => SandstormUpwardVelocity,
                _ => 0f
            }
        };
    }
}