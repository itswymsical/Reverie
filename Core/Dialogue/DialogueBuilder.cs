using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace Reverie.Core.Dialogue;

public class DialogueBuilder
{
    private const string BASE_KEY = "DialogueLibrary.";

    /// <summary>
    /// Builds a dialogue sequence from a category and sequence name
    /// </summary>
    /// <param name="category">Category of the dialogue (e.g., "Chronicle_01")</param>
    /// <param name="sequence">Sequence name (e.g., "Chapter1")</param>
    /// <param name="lineCount">Number of lines in the sequence</param>
    /// <param name="defaultDelay">Default delay between characters</param>
    /// <param name="defaultEmote">Default emote frame</param>
    /// <param name="musicId">Music ID to play during dialogue</param>
    /// <param name="modifications">Line-specific modifications</param>
    /// <returns>A constructed DialogueSequence</returns>
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

    /// <summary>
    /// Builds a dialogue sequence from a full dialogue key
    /// </summary>
    /// <param name="dialogueKey">The full dialogue key in format "Category.Sequence"</param>
    /// <param name="lineCount">Number of lines in the sequence</param>
    /// <param name="defaultDelay">Default delay between characters</param>
    /// <param name="defaultEmote">Default emote frame</param>
    /// <param name="musicId">Music ID to play during dialogue</param>
    /// <param name="modifications">Line-specific modifications</param>
    /// <returns>A constructed DialogueSequence</returns>
    public static DialogueSequence BuildByKey(
        string dialogueKey,
        int lineCount,
        int defaultDelay = 2,
        int defaultEmote = 0,
        int? musicId = null,
        params (int line, int delay, int emote)[] modifications)
    {
        string[] parts = dialogueKey.Split('.');
        if (parts.Length != 2)
        {
            throw new System.ArgumentException($"Dialogue key must be in format 'Category.Sequence'. Got: {dialogueKey}");
        }

        return Build(parts[0], parts[1], lineCount, defaultDelay, defaultEmote, musicId, modifications);
    }

    /// <summary>
    /// Builds a simple one-line dialogue
    /// </summary>
    /// <param name="category">Category of the dialogue</param>
    /// <param name="sequence">Sequence name</param>
    /// <param name="delay">Character display delay</param>
    /// <param name="emote">Emote frame to display</param>
    /// <param name="musicId">Music ID to play</param>
    /// <returns>A constructed DialogueSequence with one line</returns>
    public static DialogueSequence SimpleLine(
        string category,
        string sequence,
        int delay = 2,
        int emote = 0,
        int? musicId = null)
    {
        return Build(category, sequence, 1, delay, emote, musicId);
    }

    /// <summary>
    /// Builds a simple one-line dialogue from a full key
    /// </summary>
    /// <param name="dialogueKey">The full dialogue key in format "Category.Sequence"</param>
    /// <param name="delay">Character display delay</param>
    /// <param name="emote">Emote frame to display</param>
    /// <param name="musicId">Music ID to play</param>
    /// <returns>A constructed DialogueSequence with one line</returns>
    public static DialogueSequence SimpleLineByKey(
        string dialogueKey,
        int delay = 2,
        int emote = 0,
        int? musicId = null)
    {
        string[] parts = dialogueKey.Split('.');
        if (parts.Length != 2)
        {
            throw new System.ArgumentException($"Dialogue key must be in format 'Category.Sequence'. Got: {dialogueKey}");
        }

        return SimpleLine(parts[0], parts[1], delay, emote, musicId);
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

public class DialogueSequence
{
    public IReadOnlyList<DialogueEntry> Entries { get; }
    public int? MusicID { get; }

    public DialogueSequence(IEnumerable<DialogueEntry> entries, int? musicId = null)
    {
        Entries = new List<DialogueEntry>(entries);
        MusicID = musicId;
    }
}