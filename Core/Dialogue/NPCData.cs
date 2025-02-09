using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using static Reverie.Reverie;

namespace Reverie.Core.Dialogue;

public class NPCPortrait(Asset<Texture2D> texture, int frameCount)
{
    public Asset<Texture2D> Texture { get; } = texture;
    public int FrameCount { get; } = frameCount;
    public int FrameWidth = 92;
    public int FrameHeight = 92;

    public Rectangle GetFrameRect(int frameIndex)
    {
        return new Rectangle(frameIndex * FrameWidth, 0, FrameWidth, FrameHeight);
    }
}

public class NPCData(NPCPortrait portrait, string npcName, int npcID, Color dialogueColor, SoundStyle characterSound)
{
    public NPCPortrait Portrait { get; } = portrait;
    public string NpcName { get; } = npcName;
    public int NpcID { get; } = npcID;
    public Color DialogueColor { get; } = dialogueColor;
    public SoundStyle CharacterSound { get; } = characterSound;
    public Dictionary<DialogueID, DialogueSequence> DialogueSequences { get; } = [];

    public void AddDialogueSequence(DialogueID dialogueId, DialogueSequence sequence)
    {
        DialogueSequences[dialogueId] = sequence;
    }
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


        foreach (DialogueID dialogueId in Enum.GetValues(typeof(DialogueID)))
        {
            var dialogue = DialogueList.GetDialogueById(dialogueId);
            if (dialogue != null)
            {
                Default.AddDialogueSequence(dialogueId, dialogue);
                GuideData.AddDialogueSequence(dialogueId, dialogue);
            }
        }

        DialogueManager.Instance.RegisterNPC("Default", Default);
        DialogueManager.Instance.RegisterNPC("Guide", GuideData);
    }
}