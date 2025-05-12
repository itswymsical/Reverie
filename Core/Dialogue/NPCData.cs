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
        return new Rectangle(0, frameIndex * FrameHeight, FrameWidth, FrameHeight);
    }
}

public class NPCData
{
    public NPCPortrait Portrait { get; }
    public string NpcName { get; }
    public int NpcID { get; }
    public Color BoxColor { get; }
    public SoundStyle TalkSFX { get; }

    // This cache stores frequently used dialogues for this NPC
    private readonly Dictionary<string, DialogueSequence> _dialogueCache = [];

    public NPCData(NPCPortrait portrait, string npcName, int npcID, Color boxColor, SoundStyle talkSFX)
    {
        Portrait = portrait;
        NpcName = npcName;
        NpcID = npcID;
        BoxColor = boxColor;
        TalkSFX = talkSFX;
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

public static class NPCManager
{
    public static NPCData Default { get; private set; } = null!;
    public static NPCData GuideData { get; private set; } = null!;
    public static NPCData MerchantData { get; private set; } = null!;
    public static NPCData DemolitionistData { get; private set; } = null!;
    public static NPCData NurseData { get; private set; } = null!;
    static NPCManager()
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
            "Unknown NPC",
            -1,
             new Color(64, 109, 164),
            SoundID.MenuTick
        );

        var guidePortrait = new NPCPortrait(
            ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Guide"),
            frameCount: 6
        );

        GuideData = new NPCData(
            guidePortrait,
            "Guide",
            NPCID.Guide,
            new Color(63, 82, 151),
            new SoundStyle($"{SFX_DIRECTORY}Dialogue/Guide/Speech")
            {
                MaxInstances = 4,
                Volume = 0.4f
            }
        );

        var merchantPortrait = new NPCPortrait(
            ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/Characters/Merchant"),
            frameCount: 1
        );
        MerchantData = new NPCData(
            merchantPortrait,
            "Merchant",
            NPCID.Merchant,
            new Color(196, 102, 58),
            new SoundStyle($"{SFX_DIRECTORY}Dialogue/Merchant/Speech")
            {
                MaxInstances = 4,
                Volume = 0.4f
            }
        );

        DemolitionistData = new NPCData(
            basicPortrait,
            "Demolitionist",
            NPCID.Demolitionist,
            new Color(223, 213, 106),
            new SoundStyle($"{SFX_DIRECTORY}Dialogue/Demolitionist/Speech")
            {
                MaxInstances = 4,
                Volume = 0.4f
            }
        );

        NurseData = new NPCData(
            basicPortrait,
            "Nurse",
            NPCID.Nurse,
            new Color(255, 87, 126),
            new SoundStyle($"{SFX_DIRECTORY}Dialogue/Nurse/Speech")
            {
                MaxInstances = 4,
                Volume = 0.4f
            }
        );

        DialogueManager.Instance.RegisterNPC("Default", Default);
        DialogueManager.Instance.RegisterNPC("Guide", GuideData);
        DialogueManager.Instance.RegisterNPC("Merchant", MerchantData);
        DialogueManager.Instance.RegisterNPC("Demolitionist", DemolitionistData);
        DialogueManager.Instance.RegisterNPC("Nurse", NurseData);

        // Pre-cache some common dialogues if needed
        Default.CacheDialogue(DialogueKeys.FallingStar.Intro,
            DialogueBuilder.BuildByKey(DialogueKeys.FallingStar.Intro, 1, 1, 0));

        GuideData.CacheDialogue(DialogueKeys.FallingStar.Intro,
            DialogueBuilder.BuildByKey(DialogueKeys.FallingStar.Intro, 1, 1, 0));
    }
}