using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.Audio;

namespace Reverie.Core.Dialogue;

public static class DialogueBuilder
{
    public static List<DialogueData> BuildSequence(string dialogueKey)
    {
        var dialogueLines = new List<DialogueData>();
        int lineIndex = 1;

        // Build each line until we can't find any more
        while (true)
        {
            var lineData = BuildLine(dialogueKey, $"Line{lineIndex}");
            if (lineData == null)
                break;

            dialogueLines.Add(lineData);
            lineIndex++;
        }

        return dialogueLines;
    }

    public static DialogueData BuildLine(string dialogueKey, string lineKey)
    {
        string baseKey = $"DialogueLibrary.{dialogueKey}.{lineKey}";

        string textKey = $"{baseKey}.Text";
        var textLocalization = Reverie.Instance.GetLocalization(textKey);
        if (string.IsNullOrEmpty(textLocalization.Value))
            return null;

        string rawText = textLocalization.Value;

        string speakerType = GetOptionalValue($"{baseKey}.Speaker", "Unknown");
        string displayName = ResolveName(speakerType); // Use resolved name for display
        int emote = GetOptionalInt($"{baseKey}.Emote", 0);

        float speed = GetOptionalFloat($"{baseKey}.Speed", 1.0f);

        // Process speaker name replacements first, then parse effects
        var processedText = ProcessName(rawText);
        var (plainText, effects) = ParseTextAndEffects(processedText);

        return new DialogueData
        {
            PlainText = plainText,
            Effects = effects,
            Speaker = displayName,
            SpeakerType = speakerType,
            SpeakerColor = GetColor(speakerType),
            Emote = emote,
            Speed = speed,
            BaseDelay = 3,
            TextColor = Color.White,
            BackgroundColor = GetColor(speakerType) * 0.5f,
            TypeSound = GetVoice(speakerType)
        };
    }

    private static (string plainText, Dictionary<int, DialogueEffect> effects) ParseTextAndEffects(string rawText)
    {
        var effects = new Dictionary<int, DialogueEffect>();
        var markupRemovals = new List<(int start, int length)>();

        // First pass: Find all pause tags
        var pauseRegex = new Regex(@"<pause(?::(\d+))?>");
        foreach (Match match in pauseRegex.Matches(rawText))
        {
            var duration = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 60;
            var cleanPosition = CleanPosition(rawText, match.Index, markupRemovals);

            effects[cleanPosition] = new DialogueEffect
            {
                Type = EffectType.Pause,
                Duration = duration
            };

            markupRemovals.Add((match.Index, match.Length));
        }

        // Second pass: Find all paired tags including new pitch tag
        var pairedRegex = new Regex(@"<(\w+)(?::([0-9.]+))?>(.*?)</\1>");
        foreach (Match match in pairedRegex.Matches(rawText))
        {
            var effectType = match.Groups[1].Value.ToLower();
            var hasValue = match.Groups[2].Success;
            var valueStr = match.Groups[2].Value;
            var content = match.Groups[3].Value;

            var cleanStartPos = CleanPosition(rawText, match.Index, markupRemovals);

            // Add effect for each character in the content
            for (int i = 0; i < content.Length; i++)
            {
                var effect = effectType switch
                {
                    "shake" => new DialogueEffect { Type = EffectType.Shake, Intensity = hasValue ? int.Parse(valueStr) : 2 },
                    "sine" => new DialogueEffect { Type = EffectType.Sine, Intensity = hasValue ? int.Parse(valueStr) : 2 },
                    "fast" => new DialogueEffect { Type = EffectType.Speed, Duration = hasValue ? int.Parse(valueStr) : 2 },
                    "slow" => new DialogueEffect { Type = EffectType.Speed, Duration = hasValue ? -int.Parse(valueStr) : -2 },
                    "pitch" => new DialogueEffect { Type = EffectType.Pitch, PitchModifier = hasValue ? float.Parse(valueStr) : 1.2f }, // New pitch effect
                    _ => null
                };

                if (effect != null)
                    effects[cleanStartPos + i] = effect;
            }

            // Record the tags for removal
            var openingTagLength = match.Length - content.Length - $"</{effectType}>".Length;
            var closingTagLength = $"</{effectType}>".Length;

            markupRemovals.Add((match.Index, openingTagLength));
            markupRemovals.Add((match.Index + openingTagLength + content.Length, closingTagLength));
        }

        // Create clean text by removing all markup
        var cleanText = Regex.Replace(rawText, @"<[^>]*>", "");
        return (cleanText, effects);
    }

    private static int CleanPosition(string text, int markupIndex, List<(int start, int length)> previousRemovals)
    {
        int totalRemoved = 0;
        foreach (var (start, length) in previousRemovals)
        {
            if (start < markupIndex)
                totalRemoved += length;
        }
        return markupIndex - totalRemoved;
    }

    private static string ResolveName(string speakerType)
    {
        // Find the NPC by type name
        var npc = FindNPCName(speakerType);

        // If found and has a given name, use it. Otherwise use the type name.
        if (npc != null && !string.IsNullOrEmpty(npc.GivenName) && npc.GivenName != npc.TypeName)
        {
            return npc.GivenName;
        }

        return speakerType;
    }

    private static NPC FindNPCName(string typeName)
    {
        string lowerTypeName = typeName.ToLower();

        for (int i = 0; i < Main.npc.Length; i++)
        {
            var npc = Main.npc[i];
            if (npc.active && !string.IsNullOrEmpty(npc.TypeName))
            {
                if (npc.TypeName.ToLower() == lowerTypeName)
                {
                    return npc;
                }
            }
        }

        return null;
    }
    
    private static Color GetColor(string speaker)
    {
        // Simple color mapping without complex registry
        return speaker.ToLower() switch
        {
            "guide" => new Color(64, 109, 164),
            "player" => new Color(100, 150, 200),
            "you" => new Color(100, 150, 200),
            "argie" => new Color(66, 85, 206),
            _ => new Color(180, 180, 180) // Default gray
        };
    }

    private static string GetOptionalValue(string key, string defaultValue)
    {
        try
        {
            var localization = Reverie.Instance.GetLocalization(key);
            return !string.IsNullOrEmpty(localization.Value) ? localization.Value : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private static int GetOptionalInt(string key, int defaultValue)
    {
        try
        {
            var localization = Reverie.Instance.GetLocalization(key);
            if (!string.IsNullOrEmpty(localization.Value) && int.TryParse(localization.Value, out int result))
                return result;
        }
        catch { }
        return defaultValue;
    }

    private static float GetOptionalFloat(string key, float defaultValue)
    {
        try
        {
            var localization = Reverie.Instance.GetLocalization(key);
            if (!string.IsNullOrEmpty(localization.Value) && float.TryParse(localization.Value, out float result))
                return result;
        }
        catch { }
        return defaultValue;
    }

    private static SoundStyle GetVoice(string speaker)
    {
        return speaker.ToLower() switch
        {
            "guide" => new SoundStyle($"{SFX_DIRECTORY}Dialogue/Guide/Speech")
            {
                Volume = 0.5f,
                MaxInstances = 0
            },
            "player" or "you" => SoundID.MenuTick with
            {
                Volume = 0.5f,
                MaxInstances = 0
            },
            "merchant" => new SoundStyle($"{SFX_DIRECTORY}Dialogue/Merchant/Speech")
            {
                Volume = 0.5f,
                MaxInstances = 0
            },
            "nurse" => new SoundStyle($"{SFX_DIRECTORY}Dialogue/Nurse/Speech")
            {
                Volume = 0.5f,
                MaxInstances = 0
            },
            "demolitionist" => new SoundStyle($"{SFX_DIRECTORY}Dialogue/Demolitionist/Speech")
            {
                Volume = 0.5f,
                MaxInstances = 0
            },
            "goblin tinkerer" => new SoundStyle($"{SFX_DIRECTORY}Dialogue/Gobblin")
            {
                Volume = 0.5f,
                MaxInstances = 0
            },
            "mechanic" => new SoundStyle($"{SFX_DIRECTORY}Dialogue/Mechanic")
            {
                Volume = 0.5f,
                MaxInstances = 0
            },
            _ => SoundID.MenuTick with
            {
                Volume = 0.5f,
                MaxInstances = 0
            }
        };
    }

    private static string ProcessName(string text)
    {
        var speakerNameRegex = new Regex(@"\{speakerName:([^}]+)\}", RegexOptions.IgnoreCase);

        return speakerNameRegex.Replace(text, match =>
        {
            var targetType = match.Groups[1].Value.Trim();
            string resolvedName;

            if (targetType.ToLower() == "playername" || targetType.ToLower() == "player")
            {
                resolvedName = Main.LocalPlayer.name;
            }
            else
            {
                resolvedName = ResolveName(targetType);
            }
            return resolvedName;
        });
    }
}

public class DialogueData
{
    public string PlainText { get; init; } = string.Empty;
    public Dictionary<int, DialogueEffect> Effects { get; init; } = new();
    public string Speaker { get; init; } = string.Empty;
    public string SpeakerType { get; init; } = string.Empty;
    public Color SpeakerColor { get; init; } = Color.White;
    public int Emote { get; init; }
    public float Speed { get; init; } = 1.0f;
    public int BaseDelay { get; init; } = 3;
    public Color TextColor { get; init; } = Color.White;
    public Color BackgroundColor { get; init; } = Color.Black;
    public SoundStyle TypeSound { get; init; } = SoundID.MenuTick;
}

public class DialogueEffect
{
    public EffectType Type { get; init; }
    public int Duration { get; init; }
    public int Intensity { get; init; } = 1;
    public float PitchModifier { get; init; } = 1.0f;
    public SoundStyle? Sound { get; init; }
}

public enum EffectType
{
    Pause,
    Shake,
    Speed,
    Sound,
    Sine,
    Pitch
}