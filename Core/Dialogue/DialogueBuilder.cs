using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace Reverie.Core.Dialogue;

public class DialogueBuilder
{
    public static DialogueSequence Build(
        string category,
        string sequence,
        int lineCount,
        int defaultDelay = 2,
        int defaultEmote = 0,
        int? musicId = null,
        params (int line, int delay, int emote)[] modifications)
    {
        var entries = new DialogueEntry[lineCount];
        var modDict = modifications.ToDictionary(x => x.line);

        for (int i = 1; i <= lineCount; i++)
        {
            if (modDict.TryGetValue(i, out var mod))
            {
                entries[i - 1] = new DialogueEntry(category, sequence, i, mod.delay, mod.emote);
            }
            else
            {
                entries[i - 1] = new DialogueEntry(category, sequence, i, defaultDelay, defaultEmote);
            }
        }

        return new DialogueSequence(entries, musicId);
    }
}

public readonly struct DialogueEntry
{
    public readonly string Key;
    public readonly int Delay;
    public readonly int EmoteFrame;
    public readonly Color? EntryTextColor;
    public readonly NPCData SpeakingNPC;
    private readonly LocalizedText _localizedText;
    private const string BASE_KEY = "DialogueLibrary.";

    public DialogueEntry(string category, string sequence, int line, int delay = 2, int emoteFrame = 0,
        Color? entryTextColor = null, NPCData speakingNPC = null)
    {
        Key = $"{BASE_KEY}{category}.{sequence}.Line{line}";
        _localizedText = Reverie.Instance.GetLocalization(Key);
        Delay = delay;
        EmoteFrame = emoteFrame;
        EntryTextColor = entryTextColor;
        SpeakingNPC = speakingNPC;
    }

    public LocalizedText GetText() => _localizedText;
}

public class DialogueSequence(IEnumerable<DialogueEntry> entries, int? musicId = null)
{
    public IReadOnlyList<DialogueEntry> Entries { get; } = new List<DialogueEntry>(entries);
    public int? MusicID { get; } = musicId;
}

public enum DialogueID
{
    CrashLanding_Cutscene,
    CrashLanding_Intro,
    CrashLanding_SettlingIn,
    CrashLanding_GatheringResources,
    CrashLanding_FixHouse,
    CrashLanding_WildlifeWoes,
    CrashLanding_SlimeInfestation,
    CrashLanding_SlimeInfestation_Commentary,
    CrashLanding_SlimeRain,
    CrashLanding_SlimeRain_Commentary,
    CrashLanding_KS_Encounter,
    CrashLanding_KS_Victory
}