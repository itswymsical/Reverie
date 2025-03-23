using System.Collections.Generic;

namespace Reverie.Core.Dialogue;
public enum DialogueID
{
    CrashLanding_Cutscene,
    CrashLanding_Intro,
    CrashLanding_SettlingIn,
    CrashLanding_GatheringResources,
    CrashLanding_GiveGuideResources,
    CrashLanding_WildlifeWoes,
    CrashLanding_SlimeInfestation,
    CrashLanding_SlimeInfestation_Commentary,
    CrashLanding_SlimeRain,
    CrashLanding_SlimeRain_Commentary,
    CrashLanding_KS_Encounter,
    CrashLanding_KS_Victory,
    ChronicleI_Chapter1,
}
public static class DialogueList
{

    private static Dictionary<DialogueID, DialogueSequence> dialogues;

    static DialogueList() => Initialize();

    private static void Initialize()
    {
        var builder = new DialogueBuilder();
        dialogues = new Dictionary<DialogueID, DialogueSequence>
        {
            [DialogueID.ChronicleI_Chapter1] = DialogueBuilder.Build("Chronicle_01", "Chapter1", 14),

            [DialogueID.CrashLanding_Intro] = DialogueBuilder.Build("CrashLanding", "Intro", 1, 
            modifications: [
                (line: 1, delay: 1, emote: 0),
            ]),

            [DialogueID.CrashLanding_Cutscene] = DialogueBuilder.Build("CrashLanding", "Cutscene", 1,
            modifications: [
                (line: 1, delay: 1, emote: 0),
            ]),

            [DialogueID.CrashLanding_SettlingIn] = DialogueBuilder.Build("CrashLanding", "SettlingIn", 5),

            [DialogueID.CrashLanding_GatheringResources] = DialogueBuilder.Build("CrashLanding", "GatheringResources", 2),

            [DialogueID.CrashLanding_GiveGuideResources] = DialogueBuilder.Build("CrashLanding", "GiveGuideResources", 5),

            [DialogueID.CrashLanding_WildlifeWoes] = DialogueBuilder.Build("CrashLanding", "WildlifeWoes", 3),

            [DialogueID.CrashLanding_SlimeInfestation] = DialogueBuilder.Build("CrashLanding", "SlimeInfestation", 2),

            [DialogueID.CrashLanding_SlimeInfestation_Commentary] = DialogueBuilder.Build("CrashLanding", "SlimeInfestation_Commentary", 2),

            [DialogueID.CrashLanding_SlimeRain] = DialogueBuilder.Build("CrashLanding", "SlimeRain", 2),

            [DialogueID.CrashLanding_SlimeRain_Commentary] = DialogueBuilder.Build("CrashLanding", "SlimeRain_Commentary", 2),

            [DialogueID.CrashLanding_KS_Encounter] = DialogueBuilder.Build("CrashLanding", "KS_Encounter", 3),

            [DialogueID.CrashLanding_KS_Victory] = DialogueBuilder.Build("CrashLanding", "KS_Victory", 4),
        };
    }

    public static DialogueSequence GetDialogueById(DialogueID id)
    => dialogues.TryGetValue(id, out var dialogue) ? dialogue : null;

    public static IEnumerable<DialogueSequence> GetAllDialogues() 
    => dialogues.Values;    
}