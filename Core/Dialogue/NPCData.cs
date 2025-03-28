using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using static Reverie.Reverie;

namespace Reverie.Core.Dialogue;

public class NPCPortrait
{
    public Asset<Texture2D> Texture { get; }
    public int FrameCount { get; }
    public int FrameWidth = 92;
    public int FrameHeight = 92;

    public NPCPortrait(Asset<Texture2D> texture, int frameCount)
    {
        Texture = texture;
        FrameCount = frameCount;
    }

    public Rectangle GetFrameRect(int frameIndex)
    {
        return new Rectangle(frameIndex * FrameWidth, 0, FrameWidth, FrameHeight);
    }
}

public class NPCData
{
    public NPCPortrait Portrait { get; }
    public string NpcName { get; }
    public int NpcID { get; }
    public Color DialogueColor { get; }
    public SoundStyle CharacterSound { get; }

    // This cache stores frequently used dialogues for this NPC
    private readonly Dictionary<string, DialogueSequence> _dialogueCache = [];

    public NPCData(NPCPortrait portrait, string npcName, int npcID, Color dialogueColor, SoundStyle characterSound)
    {
        Portrait = portrait;
        NpcName = npcName;
        NpcID = npcID;
        DialogueColor = dialogueColor;
        CharacterSound = characterSound;
    }

    /// <summary>
    /// Caches a dialogue sequence for this NPC for quicker access
    /// </summary>
    public void CacheDialogue(string dialogueKey, DialogueSequence sequence)
    {
        _dialogueCache[dialogueKey] = sequence;
    }

    /// <summary>
    /// Gets a cached dialogue sequence for this NPC
    /// </summary>
    public DialogueSequence GetCachedDialogue(string dialogueKey)
    {
        return _dialogueCache.TryGetValue(dialogueKey, out var sequence) ? sequence : null;
    }

    /// <summary>
    /// Clears all cached dialogues for this NPC
    /// </summary>
    public void ClearDialogueCache()
    {
        _dialogueCache.Clear();
    }

    /// <summary>
    /// Starts a dialogue directly for this NPC
    /// </summary>
    public bool StartDialogue(string dialogueKey, int lineCount, bool zoomIn = false,
        int defaultDelay = 2, int defaultEmote = 0, int? musicId = null,
        params (int line, int delay, int emote)[] modifications)
    {
        // Check if dialogue is already cached
        DialogueSequence dialogue = GetCachedDialogue(dialogueKey);

        if (dialogue == null)
        {
            // Build and cache the dialogue
            dialogue = DialogueBuilder.BuildByKey(dialogueKey, lineCount, defaultDelay, defaultEmote, musicId, modifications);
            CacheDialogue(dialogueKey, dialogue);
        }

        return DialogueManager.Instance.StartDialogue(this, dialogue, dialogueKey, zoomIn);
    }

    /// <summary>
    /// Starts a simple one-line dialogue for this NPC
    /// </summary>
    public bool StartSimpleDialogue(string dialogueKey, bool zoomIn = false,
        int delay = 2, int emote = 0, int? musicId = null)
    {
        // Check if dialogue is already cached
        DialogueSequence dialogue = GetCachedDialogue(dialogueKey);

        if (dialogue == null)
        {
            // Build and cache the dialogue
            dialogue = DialogueBuilder.SimpleLineByKey(dialogueKey, delay, emote, musicId);
            CacheDialogue(dialogueKey, dialogue);
        }

        return DialogueManager.Instance.StartDialogue(this, dialogue, dialogueKey, zoomIn);
    }

    /// <summary>
    /// Gets the NPC's given name from the game
    /// </summary>
    public string GetNpcGivenName(int npcID)
    {
        var thisNPC = NPC.FindFirstNPC(npcID);
        var npc = Main.npc.FirstOrDefault(n => n.active && n.TypeName == NpcName);
        return npc?.GivenOrTypeName ?? NpcName;
    }
}

public static class NPCDataManager
{
    public static NPCData Default { get; private set; } = null!;
    public static NPCData GuideData { get; private set; } = null!;

    static NPCDataManager()
    {
        Initialize();
    }

    public static void Initialize()
    {
        var basicPortrait = new NPCPortrait(
            ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/PortraitFrame"),
            frameCount: 1
        );

        Default = new NPCData(
            basicPortrait,
            "You",
            -1,
             new Color(64, 109, 164),
            SoundID.MenuTick
        );

        var guidePortrait = new NPCPortrait(
            ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Guide"),
            frameCount: 2
        );

        GuideData = new NPCData(
            guidePortrait,
            "Guide",
            NPCID.Guide,
            new Color(64, 109, 164),
            SoundID.MenuOpen
        );

        DialogueManager.Instance.RegisterNPC("Default", Default);
        DialogueManager.Instance.RegisterNPC("Guide", GuideData);

        // Pre-cache some common dialogues if needed
        Default.CacheDialogue(DialogueKeys.CrashLanding.Intro,
            DialogueBuilder.BuildByKey(DialogueKeys.CrashLanding.Intro, 1, 1, 0));

        GuideData.CacheDialogue(DialogueKeys.CrashLanding.Intro,
            DialogueBuilder.BuildByKey(DialogueKeys.CrashLanding.Intro, 1, 1, 0));
    }
}